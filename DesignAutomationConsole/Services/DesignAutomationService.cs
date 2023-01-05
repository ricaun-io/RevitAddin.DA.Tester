using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation;
using Autodesk.Forge.DesignAutomation.Model;
using Microsoft.Extensions.Options;
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
        private readonly string forgeEnvironment;
        private readonly DesignAutomationClient designAutomationClient;
        public DesignAutomationClient DesignAutomationClient => designAutomationClient;
        #region Constructor
        public DesignAutomationService(
            ForgeConfiguration forgeConfiguration = null,
            string forgeEnvironment = "dev")
        {
            var service = GetForgeService(forgeConfiguration);
            this.forgeEnvironment = forgeEnvironment;
            this.designAutomationClient = new DesignAutomationClient(service);
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

        #region Bundle
        public async Task<IEnumerable<string>> GetAllBundlesAsync(bool account = true)
        {
            var data = await GetAllItems(this.designAutomationClient.GetAppBundlesAsync);
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

        public async Task<AppBundle> CreateAppBundleAsync(string appName, string packagePath, string engine = null)
        {
            if (engine is null)
            {
                var engineData = await GetEngineAsync(CoreEngine());
                if (engineData is null)
                {
                    throw new Exception($"Engine '{CoreEngine()}' not found!");
                }
                engine = engineData.Id;
            }

            string packageName = this.GetBundleName(appName);
            if (!File.Exists(packagePath))
            {
                throw new FileNotFoundException($"Bundle {packagePath} not found!");
            }

            // Check for prev versions of app bundle
            AppBundle bundle = new AppBundle();
            bundle.Package = packageName;
            bundle.Engine = engine;
            bundle.Id = packageName;
            bundle.Description = $"AppBundle: {packageName}";

            string bundleId = GetQualifiedId(packageName, false);
            var bundles = await this.GetAllBundlesAsync(false);
            if (!bundles.Any(e => e.StartsWith(bundleId)))
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
        #endregion

        #region BundleVersion
        public async Task<IEnumerable<int>> GetAppBundleVersionsAsync(string appName)
        {
            var bundleName = this.GetBundleName(appName);

            var versions = await designAutomationClient.GetAppBundleVersionsAsync(bundleName);

            var data = versions.Data;

            var paginationToken = versions.PaginationToken;
            while (!string.IsNullOrEmpty(paginationToken))
            {
                versions = await this.designAutomationClient.GetAppBundleVersionsAsync(bundleName, paginationToken);
                paginationToken = versions.PaginationToken;
                data.AddRange(versions.Data);
            }

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
            var engines = await GetAllItems(designAutomationClient.GetEnginesAsync);
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

        #region Utils
        public async Task<List<T>> GetAllItems<T>(Func<string, Task<Page<T>>> pageGetter)
        {
            var ret = new List<T>();
            string paginationToken = null;
            do
            {
                var resp = await pageGetter(paginationToken);
                paginationToken = resp.PaginationToken;
                ret.AddRange(resp.Data);
            }
            while (paginationToken != null);
            return ret;
        }
        #endregion
    }
}