using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class DesignAutomationService
    {
        private const string INPUT_PARAM = "input";
        private const string OUTPUT_PARAM = "output";

        private readonly string forgeEnvironment;
        private readonly DesignAutomationClient designAutomationClient;
        private readonly OssClient ossClient;
        public DesignAutomationClient DesignAutomationClient => designAutomationClient;
        public OssClient OssClient => ossClient;
        #region Constructor
        public DesignAutomationService(
            ForgeConfiguration forgeConfiguration = null,
            string forgeEnvironment = "dev")
        {
            forgeConfiguration = forgeConfiguration ?? new ForgeConfiguration();

            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientId)) 
                forgeConfiguration.ClientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientSecret)) 
                forgeConfiguration.ClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");

            var service = GetForgeService(forgeConfiguration);

            this.forgeEnvironment = forgeEnvironment;
            this.designAutomationClient = new DesignAutomationClient(service);

            this.ossClient = new OssClient(new Autodesk.Forge.Oss.Configuration() { 
                ClientId = forgeConfiguration.ClientId,
                ClientSecret = forgeConfiguration.ClientSecret
            });
        }
        private ForgeService GetForgeService(ForgeConfiguration forgeConfiguration)
        {
            var client = new HttpClient(new ForgeHandler(Options.Create(forgeConfiguration))
            {
                InnerHandler = new HttpClientHandler()
            });

            return new ForgeService(
                client
            );
        }
        #endregion

        #region Names
        private const string LATEST = "$LATEST";
        private const string BUNDLE_NAME = "Bundle";
        private const string ACTIVITY_NAME = "Activity";
        private const string CORE_ENGINE_REVIT = "Autodesk.Revit";
        private const string CORE_CONSOLE_EXE_REVIT = "revitcoreconsole.exe";
        public virtual string BundleName() => BUNDLE_NAME;
        public virtual string ActivityName() => ACTIVITY_NAME;
        public virtual string CoreEngine() => CORE_ENGINE_REVIT;
        public virtual string CoreConsoleExe() => CORE_CONSOLE_EXE_REVIT;
        #endregion

        #region Get
        private string GetQualifiedId(string packageName, bool enviromentEnable = true)
        {
            var name = GetNickname();
            var enviroment = this.forgeEnvironment;
            var qualifiedId = $"{name}.{packageName}";
            if (enviromentEnable)
                qualifiedId += $"+{enviroment}";
            else
                qualifiedId += $"+{LATEST}";

            return qualifiedId;
        }
        private string GetBundleName(string appName)
        {
            return appName + BundleName();
        }
        private string GetActivityName(string appName)
        {
            return appName + ActivityName();
        }
        private string GetActivityName(string appName, string engine)
        {
            return GetActivityName(appName + GetEngineVersion(engine));
        }
        private string GetEngineVersion(string engine)
        {
            var split = engine.Split('+');
            return split.LastOrDefault();
        }
        #endregion

        #region DefaultEngine
        private string DefaultEngine { get; set; }
        private string GetDefaultEngine()
        {
            if (DefaultEngine is null)
            {
                var engine = Task.Run(() => GetEngineAsync(CoreEngine())).GetAwaiter().GetResult();
                if (engine is null)
                {
                    throw new Exception($"Engine '{CoreEngine()}' not found!");
                }
                DefaultEngine = engine.Id;
            }
            return DefaultEngine;
        }
        private string GetDefaultEngine(string engine)
        {
            if (engine is null) return GetDefaultEngine();

            return $"{CoreEngine()}+{GetEngineVersion(engine)}";
        }
        #endregion

        #region Bundle

        public async Task<AppBundle> GetBundleAsync(string appName)
        {
            var bundleName = this.GetBundleName(appName);
            var qualifiedId = this.GetQualifiedId(bundleName);
            return await this.designAutomationClient.GetAppBundleAsync(qualifiedId);
        }

        public async Task<IEnumerable<string>> GetAllBundlesAsync(bool account = true)
        {
            var data = await PageUtils.GetAllItems(this.designAutomationClient.GetAppBundlesAsync);
            if (account)
            {
                var user = await this.GetNicknameAsync();
                return data
                    .Where(e => e.StartsWith(user))
                    .Where(e => !e.EndsWith(LATEST));
            }

            return data.OrderBy(e => e);
        }
        public async Task DeleteAppBundleAsync(string appName)
        {
            var bundleName = this.GetBundleName(appName);
            await this.designAutomationClient.DeleteAppBundleAsync(bundleName);
        }

        public async Task DeleteAppBundleAliasAsync(string appName, string aliasId = null)
        {
            if (string.IsNullOrWhiteSpace(aliasId))
                aliasId = this.forgeEnvironment;

            var bundleName = this.GetBundleName(appName);
            await this.designAutomationClient.DeleteAppBundleAliasAsync(bundleName, aliasId);
        }

        public async Task<AppBundle> CreateAppBundleAsync(string appName, string packagePath)
        {
            var engine = GetDefaultEngine();

            string packageName = this.GetBundleName(appName);
            if (!File.Exists(packagePath))
            {
                throw new FileNotFoundException($"Bundle {packagePath} not found!");
            }

            AppBundle bundle = CreateAppBundle(appName, engine);

            string bundleId = GetQualifiedId(packageName, false);
            var bundles = await this.GetAllBundlesAsync(false);

            if (!bundles.Contains(bundleId))
            {
                await this.designAutomationClient.CreateAppBundleAsync(bundle, this.forgeEnvironment, packagePath);
                bundle.Version = 1;
            }
            else
            {
                var version = await this.designAutomationClient.UpdateAppBundleAsync(bundle, this.forgeEnvironment, packagePath);
                bundle.Version = version;
            }

            bundle.UploadParameters = null;
            return bundle;
        }

        /// <summary>
        /// Creates an <see cref="AppBundle"/> with the specified name and engine.
        /// </summary>
        /// <param name="appName">The name of the app for which the bundle will be created.</param>
        /// <param name="engine">The engine for which the bundle will be created.</param>
        /// <returns>An <see cref="AppBundle"/> with the specified name and engine.</returns>
        private AppBundle CreateAppBundle(string appName, string engine)
        {
            string packageName = this.GetBundleName(appName);

            AppBundle bundle = new AppBundle();
            bundle.Package = packageName;
            bundle.Engine = engine;
            bundle.Id = packageName;
            bundle.Description = $"AppBundle: {packageName}";

            return bundle;
        }
        #endregion

        #region BundleVersion

        public async Task<IEnumerable<int>> GetAppBundleVersionsAsync(string appName)
        {
            var bundleName = this.GetBundleName(appName);

            var data = await PageUtils.GetAllItems(this.designAutomationClient.GetAppBundleVersionsAsync, bundleName);
            return data.OrderBy(e => e);
        }

        public async Task DeleteAppBundleVersionAsync(string appName, int version)
        {
            var bundleName = this.GetBundleName(appName);
            await designAutomationClient.DeleteAppBundleVersionAsync(bundleName, version);
        }

        public async Task<IEnumerable<int>> DeleteNotUsedAppBundleVersionsAsync(string appName)
        {
            var versions = await GetAppBundleVersionsAsync(appName);
            var data = new List<int>();
            foreach (var version in versions)
            {
                try
                {
                    await DeleteAppBundleVersionAsync(appName, version);
                    data.Add(version);
                }
                catch { }
            }
            return data;
        }

        #endregion


        #region Activity

        public async Task DeleteActivity(string appName)
        {
            var activityName = this.GetActivityName(appName);
            await this.designAutomationClient.DeleteActivityAsync(activityName);
        }

        public async Task<IEnumerable<string>> GetAllActivitiesAsync(bool account = true)
        {
            var data = await PageUtils.GetAllItems(designAutomationClient.GetActivitiesAsync);

            if (account)
            {
                var user = await this.GetNicknameAsync();
                return data
                    .Where(e => e.StartsWith(user))
                    .Where(e => !e.EndsWith(LATEST));
            }

            return data.OrderBy(e => e);
        }

        public async Task<Activity> CreateActivityAsync(string appName, string engine = null)
        {
            engine = GetDefaultEngine(engine);

            string activityName = this.GetActivityName(appName, engine);
            Activity activity = CreateActivity(appName, engine);

            var activities = await GetAllActivitiesAsync(false);
            string qualifiedId = GetQualifiedId(activityName, false);
            if (!activities.Contains(qualifiedId))
            {
                await this.designAutomationClient.CreateActivityAsync(activity, this.forgeEnvironment);
                activity.Version = 1;
            }
            else
            {
                var version = await this.designAutomationClient.UpdateActivityAsync(activity, this.forgeEnvironment);
                activity.Version = version;
            }
            return activity;
        }

        private async Task<Activity> CreateNewActivityAsync(string appName, string engine)
        {
            string activityName = this.GetActivityName(appName, engine);
            Activity activity = this.CreateActivity(appName, engine);

            activity = await designAutomationClient.CreateActivityAsync(activity);

            var alias = new Alias();
            alias.Id = this.forgeEnvironment;
            alias.Version = 1;

            await designAutomationClient.CreateActivityAliasAsync(activityName, alias);

            return activity;
        }

        private async Task<Activity> UpdateActivityAsync(string appName, string engine)
        {
            string activityName = this.GetActivityName(appName, engine);
            Activity activity = this.CreateActivity(appName, engine);

            // Set Id = null to Update Version
            activity.Id = null;

            activity = await designAutomationClient.CreateActivityVersionAsync(activityName, activity);
            if (activity == null)
            {
                throw new Exception("Error trying to update existing activity version");
            }

            var alias = new AliasPatch();
            alias.Version = (int)activity.Version;

            await this.designAutomationClient.ModifyActivityAliasAsync(activityName, this.forgeEnvironment, alias);

            return activity;
        }

        private Activity CreateActivity(string appName, string engine)
        {
            var bundleName = this.GetBundleName(appName);
            var activityName = this.GetActivityName(appName, engine);
            var bundleId = GetQualifiedId(bundleName);

            var commandInput = "";
            //commandInput = $"/i \"$(args[{FILE_PARAM}].path)\"";

            var commandLine = $"$(engine.path)\\{CoreConsoleExe()} {commandInput} /al \"$(appbundles[{bundleName}].path)\"";
            var script = string.Empty;

            var inputParam = new Parameter();
            inputParam.Description = "The input json.";
            inputParam.LocalName = $"{INPUT_PARAM}.json";
            inputParam.Verb = Verb.Get;

            var outputParam = new Parameter();
            outputParam.Description = "The output json.";
            outputParam.LocalName = $"{OUTPUT_PARAM}.json";
            outputParam.Verb = Verb.Put;

            var activity = new Activity();
            activity.Id = activityName;
            activity.Description = $"Activity {appName}";
            activity.Appbundles = new List<string>() { bundleId };
            activity.CommandLine = new List<string>() { commandLine };
            activity.Engine = engine;
            //activity.Settings = new Dictionary<string, ISetting>()
            //{
            //    { "script", new StringSetting() { Value = script } }
            //};
            activity.Parameters = new Dictionary<string, Parameter>()
            {
                { INPUT_PARAM, inputParam },
                { OUTPUT_PARAM, outputParam },
            };
            return activity;
        }
        #endregion

        #region ActivityVersion

        public async Task<IEnumerable<int>> GetActivityVersionsAsync(string appName, string engine = null)
        {
            engine = GetDefaultEngine(engine);
            var activityName = this.GetActivityName(appName, engine);

            var data = await PageUtils.GetAllItems(this.designAutomationClient.GetActivityVersionsAsync, activityName);
            return data.OrderBy(e => e);
        }
        public async Task DeleteActivityVersionAsync(string appName, int version, string engine = null)
        {
            engine = GetDefaultEngine(engine);
            var activityName = this.GetActivityName(appName, engine);
            await designAutomationClient.DeleteActivityVersionAsync(activityName, version);
        }

        public async Task<IEnumerable<int>> DeleteNotUsedActivityVersionsAsync(string appName, string engine = null)
        {
            var versions = await GetActivityVersionsAsync(appName, engine);
            var data = new List<int>();
            foreach (var version in versions)
            {
                try
                {
                    await DeleteActivityVersionAsync(appName, version, engine);
                    data.Add(version);
                }
                catch { }
            }
            return data;
        }

        #endregion


        #region WorkItem

        public async Task DeleteWorkItem(string id)
        {
            await this.designAutomationClient.DeleteWorkItemAsync(id);
        }

        public async Task<WorkItemStatus> CreateWorkItemAsync(string appName, string engine,
            string inputJson, string outputUrl)
        {
            engine = GetDefaultEngine(engine);

            string activityName = this.GetActivityName(appName, engine);
            string activityId = this.GetQualifiedId(activityName);

            var workItemBundle = new WorkItem();
            workItemBundle.ActivityId = activityId;

            workItemBundle.Arguments = new Dictionary<string, IArgument>()
            {
                { INPUT_PARAM, ToJsonArgument(inputJson) },
                { OUTPUT_PARAM,  ToCallbackArgument(outputUrl) },
            };

            var status = await this.designAutomationClient.CreateWorkItemAsync(workItemBundle);

            return status;
        }

        #region Argument
        protected IArgument ToJsonArgument(string json)
        {
            var argument = new XrefTreeArgument();
            argument.Url = $"data:application/json,{json}";
            argument.Verb = Verb.Get;
            return argument;
        }

        protected IArgument ToJsonArgument<T>(T value)
        {
            var json = JsonConvert.SerializeObject(value);
            var argument = new XrefTreeArgument();
            argument.Url = $"data:application/json,{json}";
            argument.Verb = Verb.Get;
            return argument;
        }

        protected IArgument ToFileArgument(string filePathUrl)
        {
            var argument = new XrefTreeArgument();
            argument.Url = filePathUrl;
            return argument;
        }

        protected IArgument ToCallbackArgument(string callback, Verb verb = Verb.Put)
        {
            var argument = new XrefTreeArgument();
            argument.Url = callback;
            argument.Verb = verb;
            return argument;
        }

        #endregion

        public async Task<WorkItemStatus> CheckWorkItemAsync(string id)
        {
            var status = await this.designAutomationClient.GetWorkitemStatusAsync(id);
            status.ProgressEstimateCosts();
            return status;
        }

        public async Task<object> CheckWorkItemReportAsync(string id)
        {
            var status = await CheckWorkItemAsync(id);
            if (status.ReportUrl is not null)
            {
                var report = $"{status.Progress}{Environment.NewLine}";
                report += await RequestService.GetStringAsync(status.ReportUrl);
                return report;
            }
            return status;
        }

        public async Task<object> SendWorkItemAndGetResponse(string appName, string engine, string inputJson)
        {
            //var input = "{\"Text\": \"Hello Youtube.\"}";
            var nickname = await GetNicknameAsync();
            var bucketKey = nickname.ToLower()+"_"+ appName.ToLower();
            var fileName = OUTPUT_PARAM;
            var bucket = await ossClient.TryGetBucketDetailsAsync(bucketKey);
            if (bucket is null) bucket = await ossClient.CreateBucketAsync(bucketKey);

            Console.WriteLine($"Bucket: {bucket.BucketKey}");

            var writeSignedUrl = await ossClient.CreateSignedFileWriteAsync(bucketKey, fileName);

            var status = await this.CreateWorkItemAsync(appName, engine, inputJson, writeSignedUrl);

            var number = 0;
            while (status.Status == Status.Pending | status.Status == Status.Inprogress)
            {
                Console.WriteLine(status);
                if (number++ > 120) break;
                await Task.Delay(5000);
                status = await this.CheckWorkItemAsync(status.Id);
            }

            //if (status.Status == Status.Success)
            //    return await RequestService.GetStringAsync(callbackUrl);
            Console.WriteLine(status);
            Console.WriteLine(await CheckWorkItemReportAsync(status.Id));

            if (status.Status == Status.Success)
            {

                var readSignedUrl = await ossClient.CreateSignedFileAsync(bucketKey, fileName);
                await RequestService.GetFileAsync(readSignedUrl, fileName);
                var output = await RequestService.GetStringAsync(readSignedUrl);
                Console.WriteLine(output);
            }


            return status;
        }


        #endregion

        #region Account
        public string GetNickname()
        {
            return Task.Run(GetNicknameAsync).GetAwaiter().GetResult();
        }
        public async Task<string> GetNicknameAsync()
        {
            return await this.designAutomationClient.GetNicknameAsync("me");
        }
        public async Task CreateNicknameAsync(string name)
        {
            NicknameRecord nicknameRecord = new NicknameRecord() { Nickname = name };
            await this.designAutomationClient.CreateNicknameAsync("me", nicknameRecord);
        }
        public async Task DeleteForgeAppAsync()
        {
            await this.designAutomationClient.DeleteForgeAppAsync("me");
        }
        #endregion

        #region Engine
        public async Task<IEnumerable<string>> GetEnginesAsync()
        {
            var engines = await PageUtils.GetAllItems(designAutomationClient.GetEnginesAsync);
            return engines.OrderBy(e => e);
        }
        public async Task<Engine> GetEngineAsync(string startWith)
        {
            var engines = await GetEnginesAsync();
            var engineId = engines.FirstOrDefault(e => e.StartsWith(startWith));
            if (engineId is not null)
            {
                return await this.designAutomationClient.GetEngineAsync(engineId);
            }
            return null;
        }
        #endregion
    }
}