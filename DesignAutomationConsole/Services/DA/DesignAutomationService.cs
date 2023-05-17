using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using DesignAutomationConsole.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public abstract class DesignAutomationService : IOssService, IDesignAutomationService
    {
        #region private readonly
        private IRequestService requestService = RequestService.Instance;
        private readonly DesignAutomationClient designAutomationClient;
        private readonly OssClient ossClient;
        private readonly string appName;
        #endregion

        #region Console
        public bool EnableConsoleLogger { get; set; } = true;
        private void WriteLine(object message)
        {
            if (EnableConsoleLogger == false) return;
            //Console.WriteLine($"[DesignAutomation] {message}");
            Console.WriteLine($"[{DateTime.UtcNow}] {message}");
        }
        #endregion

        #region init
        public bool ForceCreateWorkItemReport { get; init; } = false;
        public bool ForceUpdateAppBundle { get; init; } = false;
        public bool ForceUpdateActivity { get; init; } = false;
        public bool ForceDeleteNotUsed { get; init; } = true;
        public string ForgeEnvironment { get; init; } = "dev";
        #endregion

        #region Console Logger

        #endregion

        /// <summary>
        /// WorkItems Id
        /// </summary>
        public Dictionary<string, Status> WorkItems { get; } = new Dictionary<string, Status>();
        #region public
        public string AppName => appName;
        public DesignAutomationClient DesignAutomationClient => designAutomationClient;
        public OssClient OssClient => ossClient;
        #endregion

        #region Constructor
        public DesignAutomationService(string appName, ForgeConfiguration forgeConfiguration = null)
        {
            this.appName = appName;
            forgeConfiguration = forgeConfiguration ?? new ForgeConfiguration();

            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientId))
                forgeConfiguration.ClientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID");
            if (string.IsNullOrWhiteSpace(forgeConfiguration.ClientSecret))
                forgeConfiguration.ClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET");

            var service = GetForgeService(forgeConfiguration);

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

        #endregion

        public async Task Initialize(string packagePath)
        {
            var tempAppBundle = await TryGetBundleAsync();
            if (tempAppBundle is AppBundle)
            {
                WriteLine($"[AppBundle] Id: {tempAppBundle.Id}");
            }

            var updateAppBundle = (tempAppBundle is null) || ForceUpdateAppBundle;
            if (updateAppBundle)
            {
                var appBundle = await CreateAppBundleAsync(packagePath);
                WriteLine($"[AppBundle] Create Id: {appBundle.Id} {appBundle.Version}");
            }

            if (ForceDeleteNotUsed)
            {
                var appBundleDeleted = await DeleteNotUsedAppBundleVersionsAsync();
                if (appBundleDeleted.Any())
                {
                    WriteLine($"[AppBundle] Delete: {string.Join(" ", appBundleDeleted)}");
                }
            }
        }

        public async Task Delete()
        {
            try
            {
                await DeleteAppBundleAsync();
                WriteLine($"[AppBundle] Delete: {AppName}");
            }
            catch { }
            foreach (var engine in CoreEngineVersions())
            {
                try
                {
                    await DeleteActivityAsync(engine);
                    WriteLine($"[Activity] Delete: {engine}");
                }
                catch { }
            }
        }

        #region Run
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

            IParameterArgumentService<T> parameterArgumentService =
                new ParameterArgumentService<T>(this, requestService, options);

            await parameterArgumentService.Initialize();

            // Activity
            {
                var tempActivity = await TryGetActivityAsync(engine);
                if (tempActivity is Activity)
                {
                    WriteLine($"[Activity] Id: {tempActivity.Id}");
                }

                var updateActivity = (tempActivity is null) || ForceUpdateActivity;
                if (updateActivity)
                {
                    var activity = await CreateActivityAsync(engine, (activity) =>
                    {
                        parameterArgumentService.Update(activity);
                    });
                    WriteLine($"[Activity] Created Id: {activity.Id} {activity.Version}");
                    //WriteLine($"[Activity] Created: {activity.ToJson()}");

                    if (ForceDeleteNotUsed)
                    {
                        var activityDeleted = await DeleteNotUsedActivityVersionsAsync(engine);
                        if (activityDeleted.Any())
                        {
                            WriteLine($"[Activity] Delete: {string.Join(" ", activityDeleted)}");
                        }
                    }
                }
            }

            // WorkItem
            {
                var workItemStatus = await CreateWorkItemAsync(engine, (workItem) =>
                {
                    parameterArgumentService.Update(workItem);
                    WriteLine($"[WorkItem] Created: {workItem.ActivityId}");
                    //WriteLine($"[WorkItem] Created: {workItem.ToJson()}");
                });
                WriteLine($"[WorkItem] Wait: {workItemStatus.Id}");
                await WorkItemStatusWait(workItemStatus);
            }

            var result = await parameterArgumentService.Finalize();

            return result;
        }

        #endregion

        #region Get
        private string GetQualifiedId(string packageName, bool enviromentEnable = true)
        {
            var name = GetNickname();
            var enviroment = this.ForgeEnvironment;
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

        public async Task<AppBundle> TryGetBundleAsync()
        {
            try
            {
                var bundleName = this.GetBundleName(appName);
                var qualifiedId = this.GetQualifiedId(bundleName);
                return await this.designAutomationClient.GetAppBundleAsync(qualifiedId);
            }
            catch
            {
                return null;
            }
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
                aliasId = this.ForgeEnvironment;

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
                await this.designAutomationClient.CreateAppBundleAsync(bundle, this.ForgeEnvironment, packagePath);
                bundle.Version = 1;
            }
            else
            {
                var version = await this.designAutomationClient.UpdateAppBundleAsync(bundle, this.ForgeEnvironment, packagePath);
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

        /// <summary>
        /// DeleteActivityAsync
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        public async Task DeleteActivityAsync(string engine = null)
        {
            engine = GetDefaultEngine(engine);
            var activityName = this.GetActivityName(appName, engine);
            await this.designAutomationClient.DeleteActivityAsync(activityName);
        }

        /// <summary>
        /// GetAllActivitiesAsync
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the details of the specified Activity.
        /// </summary>
        public async Task<Activity> TryGetActivityAsync(string engine = null)
        {
            try
            {
                engine = GetDefaultEngine(engine);
                var activityName = GetActivityName(appName, engine);
                var qualifiedId = GetQualifiedId(activityName, true);
                return await this.designAutomationClient.GetActivityAsync(qualifiedId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create or Update the details of the specified Activity.
        /// </summary>
        public async Task<Activity> CreateActivityAsync(string engine = null, Action<Activity> settingActivity = null)
        {
            engine = GetDefaultEngine(engine);

            string activityName = this.GetActivityName(appName, engine);
            Activity activity = CreateActivity(appName, engine, settingActivity);

            var activities = await GetAllActivitiesAsync(false);
            string qualifiedId = GetQualifiedId(activityName, false);
            if (!activities.Contains(qualifiedId))
            {
                await this.designAutomationClient.CreateActivityAsync(activity, this.ForgeEnvironment);
                activity.Version = 1;
            }
            else
            {
                var version = await this.designAutomationClient.UpdateActivityAsync(activity, this.ForgeEnvironment);
                activity.Version = version;
            }
            return activity;
        }

        private Activity CreateActivity(string appName, string engine, Action<Activity> settingActivity = null)
        {
            var bundleName = this.GetBundleName(appName);
            var activityName = this.GetActivityName(appName, engine);
            var bundleId = GetQualifiedId(bundleName);

            var script = string.Empty;

            var activity = new Activity();
            activity.Id = activityName;
            activity.Description = $"Activity {appName}";
            activity.Appbundles = new List<string>() { bundleId };

            activity.CommandLine = new List<string>() {
                $"$(engine.path)\\{CoreConsoleExe()}",
                $"/al \"$(appbundles[{bundleName}].path)\""
            };
            activity.Engine = engine;

            activity.Settings = new Dictionary<string, ISetting>();
            activity.Parameters = new Dictionary<string, Parameter>();

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

        public async Task DeleteWorkItemAsync(string id)
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

            var workItemStatus = await this.designAutomationClient.CreateWorkItemAsync(workItemBundle);

            WorkItems[workItemStatus.Id] = Status.Pending;

            return workItemStatus;
        }

        public async Task<bool> WorkItemStatusWait(WorkItemStatus workItemStatus, CancellationToken cancellationToken = default)
        {
            const int MillisecondsDelay = 10000;

            if (cancellationToken == CancellationToken.None)
            {
                CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromMinutes(10.0));
                cancellationToken = source.Token;
            }

            WriteLine($"[Status]: {workItemStatus.Id}");
            while (workItemStatus.Status == Status.Pending | workItemStatus.Status == Status.Inprogress)
            {
                WriteLine($"[Status]: {workItemStatus.Status} | \t{workItemStatus.GetTimeStarted()}");
                if (cancellationToken.IsCancellationRequested)
                {
                    WriteLine($"[Status]: Cancel | {workItemStatus.Id}");
                    await this.DeleteWorkItemAsync(workItemStatus.Id);
                    workItemStatus.Status = Status.Cancelled;
                    break;
                }
                await Task.Delay(MillisecondsDelay);
                workItemStatus = await this.GetWorkitemStatusAsync(workItemStatus.Id);
                WorkItems[workItemStatus.Id] = workItemStatus.Status;
            }
            WorkItems[workItemStatus.Id] = workItemStatus.Status;

            WriteLine($"[Status]: {workItemStatus.Status}");
            WriteLine($"[Status]: EstimateTime: {workItemStatus.EstimateTime()}");
            WriteLine($"[Status]: EstimateCosts: {workItemStatus.EstimateCosts():0.0000}");

            var report = await CheckWorkItemReportAsync(workItemStatus.Id);

            if (ForceCreateWorkItemReport)
            {
                string fileName = $"report_{workItemStatus.Id}.log";
                WriteLine($"[Status]: File Create: {fileName}");
                File.WriteAllText(fileName, $"{report}");
            }

            if (workItemStatus.Status != Status.Success)
            {
                WriteLine($"[Status]: {report}");
            }

            // Todo: DeleteWorkItem if timeout

            //WriteLine(await CheckWorkItemReportAsync(status.Id));
            return workItemStatus.Status == Status.Success;
        }

        public async Task<WorkItemStatus> GetWorkitemStatusAsync(string id)
        {
            var status = await this.designAutomationClient.GetWorkitemStatusAsync(id);
            status.ProgressEstimateCosts();
            return status;
        }

        public async Task<object> CheckWorkItemReportAsync(string id)
        {
            var status = await GetWorkitemStatusAsync(id);
            if (status.ReportUrl is not null)
            {
                var report = $"{status.Progress}{Environment.NewLine}";
                report += await requestService.GetStringAsync(status.ReportUrl);
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
            var engineId = engines.OrderByDescending(e => e).FirstOrDefault(e => e.StartsWith(startWith));
            if (engineId is not null)
            {
                return await this.designAutomationClient.GetEngineAsync(engineId);
            }
            return null;
        }
        #endregion

        #region Oss
        private async Task<string> CreateOssBucketKey()
        {
            var nickname = await GetNicknameAsync();
            var bucketKey = nickname.ToLower() + "_" + AppName.ToLower();
            var bucket = await OssClient.TryGetBucketDetailsAsync(bucketKey);
            if (bucket is null) bucket = await OssClient.CreateBucketAsync(bucketKey);
            return bucketKey;
        }

        private async Task<string> UploadFile(string localFullName, string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey();
            var objectDetails = await OssClient.UploadFileAsync(bucketKey, fileName, localFullName);
            return await OssClient.CreateSignedFileAsync(bucketKey, fileName);
        }

        private async Task<string> CreateWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey();
            return await OssClient.CreateSignedFileWriteAsync(bucketKey, fileName);
        }

        private async Task<string> CreateReadWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey();
            return await OssClient.CreateSignedFileAsync(bucketKey, fileName, "readwrite");
        }

        public async Task<string> UploadFileAsync(string localFullName, string fileName)
        {
            var bucketKey = await CreateOssBucketKey();
            var objectDetails = await OssClient.UploadFileAsync(bucketKey, fileName, localFullName);
            return await OssClient.CreateSignedFileAsync(bucketKey, fileName);
        }

        public async Task<string> CreateUrlReadWriteAsync(string fileName)
        {
            var bucketKey = await CreateOssBucketKey();
            return await OssClient.CreateSignedFileAsync(bucketKey, fileName, "readwrite");
        }
        #endregion
    }
}