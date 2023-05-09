//using Autodesk.Forge.Oss;
//using System;
//using System.Threading.Tasks;
//using Autodesk.Forge.DesignAutomation;

//internal class Program
//{
//    public static async Task Main(string[] args)
//    {
//        var ossClient = new OssClient();

//        var buckets = await ossClient.GetBucketsAsync();
//        Console.WriteLine(buckets.ToJson());
//    }
//}


// Bundle
// 2018Activity -> Bundle
// 2018WorkItem -> 2018Activity

using Autodesk.Forge.Core;
using Autodesk.Forge.Oss;
using DesignAutomationConsole.Models;
using DesignAutomationConsole.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DesignAutomationConsole
{
    public class Program
    {
        private const string RequestUri0 =
          "https://github.com/ricaun-io/RevitAddin.DA.Tester/releases/download/1.0.0/RevitAddin.DA.Tester.bundle.zip";

        private const string RequestUri =
            "https://github.com/ricaun-io/RevitAddin.DA.Tester/releases/latest/download/RevitAddin.DA.Tester.bundle.zip";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("...");

            //var ForgeConfiguration = new ForgeConfiguration()
            //{
            //    ClientId = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_ID"),
            //    ClientSecret = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_SECRET")
            //};

            var appName = "RevitAddin_DA_Tester";

            var designAutomationService = new RevitDesignAutomationService(appName);

            //await designAutomationService.Initialize($".\\Bundle\\RevitAddin.DA.Tester.bundle.zip");
            //await designAutomationService.Initialize(await RequestService.Instance.GetFileAsync(RequestUri));

            var engineVersions = designAutomationService.CoreEngineVersions();

            foreach (var engineVersion in engineVersions)
                await designAutomationService.Run<ParameterOptions>(engineVersion);

            //await designAutomationService.Run<ParameterOptions>(2021);


            //var parameters = new ParameterOptionsTest()
            //{
            //    InputJson = "{\"Text\": \"Hello World.\"}",
            //    Input = new InputModel() { Text = "Hello." },
            //    Output = "",
            //    InputUpload = "test"
            //};

            //await designAutomationService.Run(parameters);

            //Console.WriteLine($">>> {parameters.Output}");

            //await designAutomationService.Run<ParameterOptions>((parameters) =>
            //{
            //    parameters.Input = new InputModel() { Text = "Hello." };
            //});

            //Console.WriteLine("--------");
            //Console.WriteLine("--------");
            //Console.WriteLine("--------");

            //await designAutomationService.Run<ParameterOptionsDownloadTest>();


            //await designAutomationService.Run<ParameterOptions>(2021);




            //var engineVersions = designAutomationService.CoreEngineVersions().OrderByDescending(e => e);

            //var engineVersions = designAutomationService.CoreEngineVersions();
            ////engineVersions = new[] { "2021" };

            //foreach (var engine in engineVersions)
            //{
            //    var output = await designAutomationService.Run<ParameterOptions>(engine);
            //    Console.WriteLine("--------");
            //    Console.WriteLine($"{output.ToJson()}");
            //    Console.WriteLine("--------");
            //}


            //for (int i = 0; i < 10; i++)
            //{
            //    Console.WriteLine($">>>");
            //    Console.WriteLine($">>> {i}");
            //    Console.WriteLine($">>>");
            //    var engineVersions = designAutomationService.CoreEngineVersions();
            //    foreach (var engine in engineVersions)
            //    {
            //        var output = await designAutomationService.Run<ParameterOptions>(engine);
            //        Console.WriteLine("--------");
            //        Console.WriteLine($"{output.ToJson()}");
            //        Console.WriteLine("--------");
            //    }
            //}

            //var output = await designAutomationService.Run<ParameterOptions>(2021);
            //Console.WriteLine("--------");
            //Console.WriteLine($"{output.ToJson()}");
            //Console.WriteLine("--------");


            //await designAutomationService.Run<ParameterOptions>("2022");

            //var output = await designAutomationService.Run<ParameterOptions>((parameters) =>
            //{
            //}, "2024");


            //var output = await designAutomationService.Run<ParameterOptionsDownload>((parameters) =>
            //{
            //    parameters.Input = new InputModel() { Text = $"" };
            //});

            //var output = await designAutomationService.Run<ParameterOptionsFile>((parameters) =>
            //{
            //    parameters.Input = "input.json";
            //});

            //Console.WriteLine("--------");
            //Console.WriteLine($"{output.ToJson()}");
            //Console.WriteLine("--------");

            return;

            var name = designAutomationService.GetNickname();
            Console.WriteLine($"Nickname: {name}");

            //var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            //foreach (var bundlesName in bundlesNames)
            //{
            //    Console.WriteLine($"Bundle: {bundlesName}");
            //}
            //var activities = await designAutomationService.GetAllActivitiesAsync();
            //foreach (var item in activities)
            //{
            //    Console.WriteLine($"Activity: {item}");
            //}

            //var bb = await designAutomationService.OssClient.GetBucketsAsync();
            //foreach (var item in bb.Items)
            //{
            //    Console.WriteLine($"Delete BucketKey: {item.BucketKey}");
            //    await designAutomationService.OssClient.DeleteBucketAsync(item.BucketKey);
            //}

            //await designAutomationService.CreateNicknameAsync("ricaun2");

            //var appBundleFilePath = await RequestService.Instance.GetFileAsync(RequestUri);
            //await designAutomationService.Initialize(appBundleFilePath);
            //await CreateWorkItem(designAutomationService);



            //await designAutomationService.DeleteAppBundleAndActivities();
            //await CreateBundles(designAutomationService);
            //await CreateActivities(designAutomationService);
        }

        private static async Task CreateWorkItem(
            RevitDesignAutomationService designAutomationService)
        {
            var tasks = new List<Task>();
            foreach (var versionEngine in designAutomationService.CoreEngineVersions())
            {
                var input = "{\"Text\": \"Hello World.\"}";
                var task = designAutomationService.SendWorkItemAndGetResponse(versionEngine, input);
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        private static async Task CreateActivities(
        DesignAutomationService designAutomationService)
        {
            foreach (var versionEngine in designAutomationService.CoreEngineVersions())
            {
                var activity = await designAutomationService.CreateActivityAsync(versionEngine);
                Console.WriteLine($"Created {activity.Id} {activity.Version}");
                Console.WriteLine(activity);

                //var versions = await designAutomationService.GetActivityVersionsAsync(appName, versionEngine);
                //foreach (var version in versions)
                //{
                //    Console.WriteLine($"Activity {appName} - {version}");
                //}

                var deleted = await designAutomationService.DeleteNotUsedActivityVersionsAsync(versionEngine);
                Console.WriteLine($"Deleted not used versions: {string.Join(" ", deleted)}");
            }

            var activities = await designAutomationService.GetAllActivitiesAsync();
            foreach (var item in activities.Where(e => e.Contains(designAutomationService.AppName)))
            {
                Console.WriteLine(item);
            }
        }

        private static async Task CreateBundles(
            DesignAutomationService designAutomationService)
        {

            //var name = await designAutomationService.GetNicknameAsync();
            //Console.WriteLine(name);

            //



            //Console.WriteLine();

            //var engines = await designAutomationService.GetEnginesAsync();
            //foreach (var engine in engines)
            //{
            //    Console.WriteLine(engine);
            //}

            var filePath = await RequestService.Instance.GetFileAsync(RequestUri);




            //var engineData = await designAutomationService.GetEngineAsync("Autodesk.Revit");
            //Console.WriteLine(engineData);


            //await designAutomationService.DeleteAppBundleAsync(appName);


            for (int i = 0; i < 1; i++)
            {
                var appBundle = await designAutomationService.CreateAppBundleAsync(
                    filePath);

                Console.WriteLine(appBundle);
            }

            var deleted = await designAutomationService.DeleteNotUsedAppBundleVersionsAsync();
            Console.WriteLine($"Deleted not used versions: {string.Join(" ", deleted)}");

            Console.WriteLine("-------------");

            var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            foreach (var bundlesName in bundlesNames)
            {
                Console.WriteLine(bundlesName);
            }

            Console.WriteLine("-------------");

            var versions = await designAutomationService.GetAppBundleVersionsAsync();
            foreach (var version in versions)
            {
                Console.WriteLine($"{designAutomationService.AppName}\t {version}");
            }

            //Console.WriteLine("-------------");
            //var app = await designAutomationService.GetBundleAsync(appName);
            //Console.WriteLine(app);
            //await RequestService.GetFileAsync(app.Package, "package.zip");

            Console.WriteLine("-------------");

            //await designAutomationService.DeleteOldAppBundleVersionsAsync(appName);


            //var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            //foreach (var bundlesName in bundlesNames)
            //{
            //    var id = GetName(bundlesName);
            //    Console.WriteLine(bundlesName);
            //    Console.WriteLine(id);
            //    var versions = await designAutomationService.GetAppBundleVersionsAsync(id);

            //    foreach (var version in versions)
            //    {
            //        Console.WriteLine($"\t {version}");
            //    }
            //}

        }


    }
}