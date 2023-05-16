﻿using Autodesk.Forge.Core;
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

    public abstract class DesignAutomationService
    {
        #region private readonly
        private readonly string forgeEnvironment;
        private readonly DesignAutomationClient designAutomationClient;
        private readonly OssClient ossClient;
        private readonly string appName;
        #endregion

        #region public
        public string AppName => appName;
        public DesignAutomationClient DesignAutomationClient => designAutomationClient;
        public OssClient OssClient => ossClient;
        #endregion

        #region Constructor
        public DesignAutomationService(
            string appName,
            ForgeConfiguration forgeConfiguration = null,
            string forgeEnvironment = "dev")
        {
            this.appName = appName;
            forgeConfiguration = forgeConfiguration ?? new ForgeConfiguration();

            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientId))
                forgeConfiguration.ClientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientSecret))
                forgeConfiguration.ClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");

            var service = GetForgeService(forgeConfiguration);

            this.forgeEnvironment = forgeEnvironment;
            this.designAutomationClient = new DesignAutomationClient(service);

            this.ossClient = new OssClient(new Autodesk.Forge.Oss.Configuration()
            {
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

        #region Consts
        private const string LATEST = "$LATEST";
        private const string BUNDLE_NAME = "Bundle";
        private const string ACTIVITY_NAME = "Activity";
        protected virtual string BundleName() => BUNDLE_NAME;
        protected virtual string ActivityName() => ACTIVITY_NAME;
        #endregion

        #region Names
        public abstract string[] CoreEngineVersions();
        public abstract string CoreEngine();
        public abstract string CoreConsoleExe();
        //protected abstract void CoreCreateActivity(Activity activity);
        //protected abstract void CoreCreateWorkItem(WorkItem workItemBundle, object[] arguments);

        #endregion

        public async Task Initialize(string packagePath)
        {
            var appBundle = await CreateAppBundleAsync(packagePath);
            Console.WriteLine($"Created AppBundle Id: {appBundle.Id} {appBundle.Version}");
            var appBundleDeleted = await DeleteNotUsedAppBundleVersionsAsync();
            if (appBundleDeleted.Any())
                Console.WriteLine($"\tDeleted AppBundles: {string.Join(" ", appBundleDeleted)}");
        }

        public async Task DeleteAppBundleAndActivities()
        {
            try
            {
                await DeleteAppBundleAsync();
                Console.WriteLine($"Deleted AppBundle: {AppName}");
            }
            catch { }
            foreach (var engine in CoreEngineVersions())
            {
                try
                {
                    await DeleteActivity(engine);
                    Console.WriteLine($"Deleted Activity: {engine}");
                }
                catch { }
            }
        }

        public async Task<T> Run<T>(int engine) where T : class
        {
            return await Run<T>(engine.ToString());
        }

        public async Task<T> Run<T>(string engine = null) where T : class
        {
            return await Run<T>((obj) => { }, engine);
        }

        public async Task<T> Run<T>(Action<T> options, string engine = null) where T : class
        {
            var instance = Activator.CreateInstance<T>();
            options?.Invoke(instance);
            return await Run(instance, engine);
        }

        public async Task<T> Run<T>(T options, string engine = null) where T : class
        {
            if (string.IsNullOrEmpty(engine)) engine = CoreEngineVersions().FirstOrDefault();

            if (CoreEngineVersions().Contains(engine) == false)
            {
                throw new Exception($"Engine '{engine}' not found in the CoreEngineVersions");
            }

            var parameterArgumentService = new ParameterArgumentService<T>(this, options);
            await parameterArgumentService.Initialize();

            //try
            //{
            var activity = await CreateActivityAsync(engine, (activity) =>
            {
                activity.Parameters = parameterArgumentService.Parameters;
                parameterArgumentService.UpdateActivity(activity);
            });
            Console.WriteLine($"Created Activity Id: {activity.Id} {activity.Version}");
            Console.WriteLine($"Created Activity: {activity.ToJson()}");

            var activityDeleted = await DeleteNotUsedActivityVersionsAsync(engine);
            if (activityDeleted.Any())
                Console.WriteLine($"\tDeleted Activitys: {string.Join(" ", activityDeleted)}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Created Activity Exception: {ex.Message}");
            //}


            Console.WriteLine($"Created WorkItem: {engine}");
            var workItem = await CreateWorkItemAsync(engine, (workItem) =>
            {
                workItem.Arguments = parameterArgumentService.Arguments;
            });
            await WorkItemStatusWait(workItem, engine);


            //ConsoleParameter(parameterArgumentService);

            return await parameterArgumentService.Finalize();
        }

        //private static void ConsoleParameter<T>(ParameterArgumentService<T> parameterArgumentService) where T : class
        //{
        //    Console.WriteLine("------------------------");
        //    Console.WriteLine($"Parameters");
        //    var activityParameters = parameterArgumentService.Parameters;
        //    foreach (var item in activityParameters)
        //    {
        //        Console.WriteLine($"{item.Key} {item.Value}");
        //    }

        //    Console.WriteLine($"Arguments");
        //    var workItemArguments = parameterArgumentService.Arguments;
        //    foreach (var item in workItemArguments)
        //    {
        //        Console.WriteLine($"{item.Key} {item.Value}");
        //    }

        //    var downloads = parameterArgumentService.DownloadFiles;
        //    foreach (var download in downloads)
        //    {
        //        Console.WriteLine($"DownloadFiles: {download}");
        //    }
        //    Console.WriteLine("------------------------");
        //}

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
                var engine_version = CoreEngine() + "+" + CoreEngineVersions().FirstOrDefault();
                var engine = Task.Run(() => GetEngineAsync(engine_version)).GetAwaiter().GetResult();
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

        public async Task<AppBundle> GetBundleAsync()
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
        public async Task DeleteAppBundleAsync()
        {
            var bundleName = this.GetBundleName(appName);
            await this.designAutomationClient.DeleteAppBundleAsync(bundleName);
        }

        public async Task DeleteAppBundleAliasAsync(string aliasId = null)
        {
            if (string.IsNullOrWhiteSpace(aliasId))
                aliasId = this.forgeEnvironment;

            var bundleName = this.GetBundleName(appName);
            await this.designAutomationClient.DeleteAppBundleAliasAsync(bundleName, aliasId);
        }

        public async Task<AppBundle> CreateAppBundleAsync(string packagePath)
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
        public async Task<IEnumerable<int>> GetAppBundleVersionsAsync()
        {
            var bundleName = this.GetBundleName(appName);

            var data = await PageUtils.GetAllItems(this.designAutomationClient.GetAppBundleVersionsAsync, bundleName);
            return data.OrderBy(e => e);
        }

        public async Task DeleteAppBundleVersionAsync(int version)
        {
            var bundleName = this.GetBundleName(appName);
            await designAutomationClient.DeleteAppBundleVersionAsync(bundleName, version);
        }

        public async Task<IEnumerable<int>> DeleteNotUsedAppBundleVersionsAsync()
        {
            var versions = await GetAppBundleVersionsAsync();
            var data = new List<int>();
            foreach (var version in versions)
            {
                try
                {
                    await DeleteAppBundleVersionAsync(version);
                    data.Add(version);
                }
                catch { }
            }
            return data;
        }

        #endregion

        #region Activity

        public async Task DeleteActivity(string engine = null)
        {
            engine = GetDefaultEngine(engine);

            var activityName = this.GetActivityName(appName, engine);
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

        public async Task<Activity> CreateActivityAsync(string engine = null, Action<Activity> settingActivity = null)
        {
            //if (settingActivity is null)
            //{
            //    settingActivity = CoreCreateActivity;
            //}

            engine = GetDefaultEngine(engine);

            string activityName = this.GetActivityName(appName, engine);
            Activity activity = CreateActivity(appName, engine, settingActivity);

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

        private Activity CreateActivity(string appName, string engine, Action<Activity> settingActivity = null)
        {
            var bundleName = this.GetBundleName(appName);
            var activityName = this.GetActivityName(appName, engine);
            var bundleId = GetQualifiedId(bundleName);

            //var commandInput = "";
            //commandInput = $"/i \"$(args[{FILE_PARAM}].path)\"";
            //var commandLanguage = "";
            //commandLanguage = $"/l $(args[{"read"}].value)";
            //var commandLine = $"$(engine.path)\\{CoreConsoleExe()} {commandInput} /al \"$(appbundles[{bundleName}].path)\" {commandLanguage}";

            var commandLine = $"$(engine.path)\\{CoreConsoleExe()} /al \"$(appbundles[{bundleName}].path)\"";
            var script = string.Empty;

            var activity = new Activity();
            activity.Id = activityName;
            activity.Description = $"Activity {appName}";
            activity.Appbundles = new List<string>() { bundleId };
            //activity.CommandLine = new List<string>() { commandLine };
            activity.CommandLine = new List<string>() {
                $"$(engine.path)\\{CoreConsoleExe()}",
                $"/al \"$(appbundles[{bundleName}].path)\""
            };
            activity.Engine = engine;

            //script = $"$(args[{"engine_over"}].value)";
            //activity.Engine = $"$(args[{"engine_over"}].value)";

            //activity.Settings = new Dictionary<string, ISetting>()
            //{
            //    { "script", new StringSetting() { Value = script } }
            //};

            activity.Settings = new Dictionary<string, ISetting>();
            activity.Parameters = new Dictionary<string, Parameter>();

            //CoreCreateActivity(activity);
            settingActivity?.Invoke(activity);

            activity.CommandLine = new List<string>() { string.Join(" ", activity.CommandLine) };

            return activity;
        }

        #endregion

        #region ActivityVersion

        public async Task<IEnumerable<int>> GetActivityVersionsAsync(string engine = null)
        {
            engine = GetDefaultEngine(engine);
            var activityName = this.GetActivityName(appName, engine);

            var data = await PageUtils.GetAllItems(this.designAutomationClient.GetActivityVersionsAsync, activityName);
            return data.OrderBy(e => e);
        }
        public async Task DeleteActivityVersionAsync(int version, string engine = null)
        {
            engine = GetDefaultEngine(engine);
            var activityName = this.GetActivityName(appName, engine);
            await designAutomationClient.DeleteActivityVersionAsync(activityName, version);
        }

        public async Task<IEnumerable<int>> DeleteNotUsedActivityVersionsAsync(string engine = null)
        {
            var versions = await GetActivityVersionsAsync(engine);
            var data = new List<int>();
            foreach (var version in versions)
            {
                try
                {
                    await DeleteActivityVersionAsync(version, engine);
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

        public async Task<WorkItemStatus> CreateWorkItemAsync(string engine = null, Action<WorkItem> settingWorkItem = null)
        {
            engine = GetDefaultEngine(engine);

            string activityName = this.GetActivityName(appName, engine);
            string activityId = this.GetQualifiedId(activityName);

            var workItemBundle = new WorkItem();
            workItemBundle.ActivityId = activityId;

            workItemBundle.Arguments = new Dictionary<string, IArgument>();

            settingWorkItem?.Invoke(workItemBundle);

            var status = await this.designAutomationClient.CreateWorkItemAsync(workItemBundle);

            return status;
        }

        public async Task WorkItemStatusWait(WorkItemStatus status, string engine = null)
        {
            var number = 0;
            Console.WriteLine($"[{DateTime.Now}]: {status.Id}");
            while (status.Status == Status.Pending | status.Status == Status.Inprogress)
            {
                Console.WriteLine($"[{DateTime.Now}]: {status.Status} | \t{status.GetTimeStarted()}");
                if (number++ > 120) break;
                await Task.Delay(10000);
                status = await this.CheckWorkItemAsync(status.Id);
            }
            Console.WriteLine($"[{DateTime.Now}]: {status.Status}");
            Console.WriteLine($"[{DateTime.Now}]: EstimateTime: {status.EstimateTime()}");
            Console.WriteLine($"[{DateTime.Now}]: EstimateCosts: {status.EstimateCosts()}");

            var report = await CheckWorkItemReportAsync(status.Id);
            File.WriteAllText($"{engine}_report_{status.Id}.log", $"{report}");
            if (status.Status != Status.Success)
            {
                Console.WriteLine(report);
            }

            // Todo: DeleteWorkItem if timeout

            //Console.WriteLine(await CheckWorkItemReportAsync(status.Id));
        }


        //public async Task<WorkItemStatus> CreateWorkItemAsync(string engine,
        //    params object[] arguments)
        //{
        //    engine = GetDefaultEngine(engine);

        //    string activityName = this.GetActivityName(appName, engine);
        //    string activityId = this.GetQualifiedId(activityName);

        //    var workItemBundle = new WorkItem();
        //    workItemBundle.ActivityId = activityId;

        //    workItemBundle.Arguments = new Dictionary<string, IArgument>();

        //    CoreCreateWorkItem(workItemBundle, arguments);

        //    var status = await this.designAutomationClient.CreateWorkItemAsync(workItemBundle);

        //    return status;
        //}


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
                report += await RequestService.Instance.GetStringAsync(status.ReportUrl);
                return report;
            }
            return status;
        }
        #endregion

        #region Account
        public string GetNickname()
        {
            return Task.Run(GetNicknameAsync).GetAwaiter().GetResult();
        }
        private string nickname;
        public async Task<string> GetNicknameAsync()
        {
            nickname = nickname ?? await this.designAutomationClient.GetNicknameAsync("me");
            return nickname;
        }
        public async Task CreateNicknameAsync(string name)
        {
            nickname = null;
            NicknameRecord nicknameRecord = new NicknameRecord() { Nickname = name };
            await this.designAutomationClient.CreateNicknameAsync("me", nicknameRecord);
        }
        public async Task DeleteForgeAppAsync()
        {
            nickname = null;
            await this.designAutomationClient.DeleteForgeAppAsync("me");
        }
        #endregion

        #region Engine
        public async Task<IEnumerable<string>> GetEnginesAsync()
        {
            var engines = await PageUtils.GetAllItems(designAutomationClient.GetEnginesAsync);
            return engines.OrderBy(e => e);
        }
        /// <summary>
        /// Get Last EngineVersion
        /// </summary>
        /// <param name="startWith"></param>
        /// <returns></returns>
        public async Task<Engine> GetEngineAsync(string startWith)
        {
            var engines = await GetEnginesAsync();
            //foreach (var item in engines)
            //{
            //    Console.WriteLine(item);
            //}
            var engineId = engines.OrderByDescending(e => e).FirstOrDefault(e => e.StartsWith(startWith));
            if (engineId is not null)
            {
                return await this.designAutomationClient.GetEngineAsync(engineId);
            }
            return null;
        }
        #endregion
    }
}