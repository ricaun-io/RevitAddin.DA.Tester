using Autodesk.Forge.Core;
using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class RevitDesignAutomationService : DesignAutomationService
    {
        public RevitDesignAutomationService(string appName,
            ForgeConfiguration forgeConfiguration = null,
            string forgeEnvironment = "dev") : base(appName, forgeConfiguration, forgeEnvironment)
        {
        }

        private const string INPUT_PARAM = "input";
        private const string OUTPUT_PARAM = "output";
        private const string READ_PARAM = "read";
        private const string ENGINE_PARAM = "engine_over";

        public override string CoreConsoleExe()
        {
            return "revitcoreconsole.exe";
        }

        public override string CoreEngine()
        {
            return "Autodesk.Revit";
        }

        public override string[] CoreEngineVersions()
        {
            return new[] { "2021" };
            //return new[] { "2018", "2019", "2020", "2021", "2022", "2023" };
        }

        protected override void CoreCreateActivity(Activity activity)
        {
            var inputParam = new Parameter();
            inputParam.Description = "The input json.";
            inputParam.LocalName = $"{INPUT_PARAM}.json";
            inputParam.Verb = Verb.Get;

            var outputParam = new Parameter();
            outputParam.Description = "The output json.";
            outputParam.LocalName = $"{OUTPUT_PARAM}.json";
            outputParam.Verb = Verb.Put;

            var readParam = new Parameter();
            readParam.LocalName = READ_PARAM;
            readParam.Description = "read";
            readParam.Verb = Verb.Read;

            var engineParam = new Parameter();
            engineParam.LocalName = ENGINE_PARAM;
            engineParam.Description = ENGINE_PARAM;
            engineParam.Verb = Verb.Read;

            activity.Parameters = new Dictionary<string, Parameter>()
            {
                { INPUT_PARAM, inputParam },
                { OUTPUT_PARAM, outputParam },
                //{ READ_PARAM, readParam },
                //{ ENGINE_PARAM, engineParam },
            };
        }

        protected override void CoreCreateWorkItem(WorkItem workItemBundle, object[] arguments)
        {
            var inputJson = (string)arguments[0];
            var outputUrl = (string)arguments[1];
            workItemBundle.Arguments = new Dictionary<string, IArgument>()
            {
                { INPUT_PARAM, IArgumentUtils.ToJsonArgument(inputJson) },
                { OUTPUT_PARAM,  IArgumentUtils.ToCallbackArgument(outputUrl) },
                //{ READ_PARAM,  new StringArgument(){ Value = "FRA"} },
                //{ ENGINE_PARAM,  new StringArgument(){ Value = "Autodesk.Revit+2021"} },
            };
        }

        public async Task Initialize(string packagePath)
        {
            var appBundle = await CreateAppBundleAsync(packagePath);
            Console.WriteLine($"Created AppBundle Id: {appBundle.Id} {appBundle.Version}");
            var appBundleDeleted = await DeleteNotUsedAppBundleVersionsAsync();
            if (appBundleDeleted.Any())
                Console.WriteLine($"\tDeleted AppBundles: {string.Join(" ", appBundleDeleted)}");

            foreach (var engine in CoreEngineVersions())
            {
                var activity = await CreateActivityAsync(engine);
                Console.WriteLine($"Created Activity Id: {activity.Id} {activity.Version}");
                var activityDeleted = await DeleteNotUsedActivityVersionsAsync(engine);
                if (activityDeleted.Any())
                    Console.WriteLine($"\tDeleted Activitys: {string.Join(" ", activityDeleted)}");
            }
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

        public async Task<object> SendWorkItemAndGetResponse(string engine, string inputJson)
        {
            //var input = "{\"Text\": \"Hello Youtube.\"}";
            var fileName = OUTPUT_PARAM + engine;
            var nickname = await GetNicknameAsync();
            var bucketKey = nickname.ToLower() + "_" + AppName.ToLower();
            var bucket = await OssClient.TryGetBucketDetailsAsync(bucketKey);
            if (bucket is null) bucket = await OssClient.CreateBucketAsync(bucketKey);

            Console.WriteLine($"Bucket: {bucket.BucketKey}");

            var writeSignedUrl = await OssClient.CreateSignedFileWriteAsync(bucketKey, fileName);

            var status = await this.CreateWorkItemAsync(engine, inputJson, writeSignedUrl);

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
                var readSignedUrl = await OssClient.CreateSignedFileAsync(bucketKey, fileName);
                await RequestService.GetFileAsync(readSignedUrl, fileName);
                var output = await RequestService.GetStringAsync(readSignedUrl);
                Console.WriteLine(output);
            }


            return status;
        }

    }
}