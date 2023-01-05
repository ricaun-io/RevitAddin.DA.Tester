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
        private const string REVIT_CORE_CONSOLE_EXE = "revitcoreconsole.exe";
        public virtual string BundleName() => BUNDLE_NAME;
        public virtual string ActivityName() => ACTIVITY_NAME;
        public virtual string CoreConsoleExe() => REVIT_CORE_CONSOLE_EXE;
        #endregion

        #region Get
        private string GetQualifiedId(string packageName)
        {
            var name = GetNickname();
            var enviroment = this.forgeEnvironment;
            return $"{name}.{packageName}+{enviroment}";
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
            var bundles = await this.designAutomationClient.GetAppBundlesAsync();
            var data = bundles.Data;

            var paginationToken = bundles.PaginationToken;
            while (!string.IsNullOrEmpty(paginationToken))
            {
                bundles = await this.designAutomationClient.GetAppBundlesAsync(paginationToken);
                paginationToken = bundles.PaginationToken;
                data.AddRange(bundles.Data);
            }

            if (account)
            {
                var user = await this.GetNicknameAsync();
                return data
                    .Where(e => e.StartsWith(user))
                    .Where(e => !e.EndsWith(LATEST));
            }

            return data.OrderBy(e => e);
        }

        public async Task<AppBundle> CreateAppBundleAsync(string appName, string fileName, string engine)
        {
            string packageName = this.GetBundleName(appName);

            string zipPath = fileName;
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException($"Bundle {fileName} not found!");
            }

            var bundles = await this.GetAllBundlesAsync(false);

            // Check for prev versions of app bundle
            AppBundle bundleVersion;
            string bundleId = GetQualifiedId(packageName);

            if (!bundles.Contains(bundleId))
            {
                bundleVersion = await CreateNewBundleAsync(packageName, engine);
            }
            else
            {
                bundleVersion = await UpdateBundleAsync(packageName, engine);
            }

            await UploadParametersBundleAsync(bundleVersion, zipPath);

            bundleVersion.UploadParameters = null;
            return bundleVersion;
            //var appBundle = new DesignAutoBundleModel();
            //appBundle.Id = bundleId;
            //appBundle.Engine = engine;
            //appBundle.Version = (int)bundleVersion.Version;

            //return appBundle;
        }

        private async Task UploadParametersBundleAsync(AppBundle bundleVersion, string zipPath)
        {
            await RequestService.UploadFormDataAsync(
                bundleVersion.UploadParameters.EndpointURL,
                bundleVersion.UploadParameters.FormData,
                zipPath);
        }

        private async Task<AppBundle> UpdateBundleAsync(string packageName, string engine)
        {
            var bundle = new AppBundle();
            bundle.Engine = engine;
            bundle.Description = $"AppBundle: {packageName}";

            var bundleVersion = await designAutomationClient.CreateAppBundleVersionAsync(packageName, bundle);
            if (bundleVersion == null)
            {
                throw new Exception("Error trying to update existing bundle version");
            }

            var alias = new AliasPatch();
            alias.Version = (int)bundleVersion.Version;

            await this.designAutomationClient.ModifyAppBundleAliasAsync(packageName, this.forgeEnvironment, alias);
            return bundleVersion;
        }

        private async Task<AppBundle> CreateNewBundleAsync(string packageName, string engine)
        {
            var bundle = new AppBundle();
            bundle.Package = packageName;
            bundle.Engine = engine;
            bundle.Id = packageName;
            bundle.Description = $"AppBundle: {packageName}";

            var bundleVersion = await this.designAutomationClient.CreateAppBundleAsync(bundle);
            if (bundleVersion == null)
            {
                throw new Exception("Error trying to create new first bundle version");
            }

            var alias = new Alias();
            alias.Id = this.forgeEnvironment;
            alias.Version = 1;
            await designAutomationClient.CreateAppBundleAliasAsync(packageName, alias);
            return bundleVersion;
        }

        public async Task DeleteAppBundleAsync(string appName)
        {
            var bundleName = this.GetBundleName(appName);
            await this.designAutomationClient.DeleteAppBundleAsync(bundleName);
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
            var engines = await designAutomationClient.GetEnginesAsync();

            var data = engines.Data;

            var paginationToken = engines.PaginationToken;
            while (!string.IsNullOrEmpty(paginationToken))
            {
                engines = await this.designAutomationClient.GetEnginesAsync(paginationToken);
                paginationToken = engines.PaginationToken;
                data.AddRange(engines.Data);
            }

            return data.OrderBy(e => e);
        }
        #endregion

        #region BundleVersion

        public async Task<IEnumerable<int>> GetAppBundleVersionsAsync(string id)
        {
            var versions = await designAutomationClient.GetAppBundleVersionsAsync(id);

            var data = versions.Data;

            var paginationToken = versions.PaginationToken;
            while (!string.IsNullOrEmpty(paginationToken))
            {
                versions = await this.designAutomationClient.GetAppBundleVersionsAsync(id, paginationToken);
                paginationToken = versions.PaginationToken;
                data.AddRange(versions.Data);
            }

            return data.OrderBy(e => e);
        }
        #endregion
    }
}