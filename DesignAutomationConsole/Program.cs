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
using DesignAutomationConsole.Services;
using System;
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

        private const string versionEngine = "2018";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("...");
            //var ForgeConfiguration = new ForgeConfiguration()
            //{
            //    ClientId = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_ID"),
            //    ClientSecret = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_SECRET")
            //};

            var designAutomationService = new DesignAutomationService();

            var name = designAutomationService.GetNickname();
            Console.WriteLine($"Nickname: {name}");

            var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            foreach (var bundlesName in bundlesNames)
            {
                Console.WriteLine($"Bundle: {bundlesName}");
            }
            var activities = await designAutomationService.GetAllActivitiesAsync();
            foreach (var item in activities)
            {
                Console.WriteLine($"Activity: {item}");
            }

            //var bb = await designAutomationService.OssClient.GetBucketsAsync();
            //foreach (var item in bb.Items)
            //{
            //    Console.WriteLine($"Delete BucketKey: {item.BucketKey}");
            //    await designAutomationService.OssClient.DeleteBucketAsync(item.BucketKey);
            //}

            //await designAutomationService.CreateNicknameAsync("ricaun2");

            var appName = "RevitAddin_DA_Tester";
            await CreateBundles(designAutomationService, appName);
            await CreateActivities(designAutomationService, appName);
            await CreateWorkItem(designAutomationService, appName);
        }

        private static async Task CreateWorkItem(
            DesignAutomationService designAutomationService,
            string appName)
        {
            var input = "{\"Text\": \"Hello World.\"}";
            await designAutomationService.SendWorkItemAndGetResponse(appName, versionEngine, input);
        }

        private static async Task CreateActivities(
        DesignAutomationService designAutomationService,
        string appName)
        {
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    var activity = await designAutomationService.CreateActivityAsync(appName, versionEngine);
                    Console.WriteLine($"Created {activity.Id} {activity.Version}");
                    Console.WriteLine(activity);
                }
                catch (Exception)
                {
                    break;
                }
            }


            var versions = await designAutomationService.GetActivityVersionsAsync(appName, versionEngine);
            foreach (var version in versions)
            {
                Console.WriteLine($"Activity {appName} - {version}");
            }

            var deleted = await designAutomationService.DeleteNotUsedActivityVersionsAsync(appName, versionEngine);
            Console.WriteLine($"Deleted not used versions: {string.Join(" ", deleted)}");

            var activities = await designAutomationService.GetAllActivitiesAsync();
            foreach (var item in activities.Where(e => e.Contains(appName)))
            {
                Console.WriteLine(item);
            }
        }

        private static async Task CreateBundles(
            DesignAutomationService designAutomationService,
            string appName)
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

            var filePath = await RequestService.GetFileAsync(RequestUri);
            



            //var engineData = await designAutomationService.GetEngineAsync("Autodesk.Revit");
            //Console.WriteLine(engineData);


            //await designAutomationService.DeleteAppBundleAsync(appName);


            for (int i = 0; i < 1; i++)
            {
                var appBundle = await designAutomationService.CreateAppBundleAsync(
                    appName,
                    filePath);

                Console.WriteLine(appBundle);
            }

            var deleted = await designAutomationService.DeleteNotUsedAppBundleVersionsAsync(appName);
            Console.WriteLine($"Deleted not used versions: {string.Join(" ", deleted)}");

            Console.WriteLine("-------------");

            var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            foreach (var bundlesName in bundlesNames)
            {
                Console.WriteLine(bundlesName);
            }

            Console.WriteLine("-------------");

            var versions = await designAutomationService.GetAppBundleVersionsAsync(appName);
            foreach (var version in versions)
            {
                Console.WriteLine($"{appName}\t {version}");
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

        public static string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

    }
}