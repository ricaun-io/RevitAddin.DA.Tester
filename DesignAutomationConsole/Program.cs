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
using DesignAutomationConsole.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesignAutomationConsole
{

    public class Program
    {
        private const string RequestUri =
            "https://github.com/ricaun-io/RevitAddin.DA.Tester/releases/latest/download/RevitAddin.DA.Tester.bundle.zip";

        public static async Task Main(string[] args)
        {
            Console.WriteLine("...");
            var ForgeConfiguration = new ForgeConfiguration()
            {
                ClientId = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_ID"),
                ClientSecret = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_SECRET")
            };

            var designAutomationService = new DesignAutomationService(ForgeConfiguration);

            var name = designAutomationService.GetNickname();
            Console.WriteLine($"Nickname: {name}");

            //await designAutomationService.CreateNicknameAsync("ricaun.io");

            //var appName = "RevitAddin_DA_Tester";
            //await CreateBundles(designAutomationService, appName);
            //await CreateActivities(designAutomationService, appName);
            //await CreateWorkItem(designAutomationService, appName);
        }

        private static async Task CreateWorkItem(
            DesignAutomationService designAutomationService,
            string appName)
        {
            await designAutomationService.SendWorkItemAndGetResponse(appName);
        }

        private static async Task CreateActivities(
        DesignAutomationService designAutomationService,
        string appName)
        {
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    var activity = await designAutomationService.CreateActivityAsync(appName);
                    Console.WriteLine($"Created {activity.Id} {activity.Version}");
                    Console.WriteLine(activity);
                }
                catch (Exception)
                {
                    break;
                }
            }


            var versions = await designAutomationService.GetActivityVersionsAsync(appName);
            foreach (var version in versions)
            {
                Console.WriteLine($"Activity {appName} - {version}");
            }

            var deleted = await designAutomationService.DeleteNotUsedActivityVersionsAsync(appName);
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

        private static string GetName(string bundlesName)
        {
            var splitDot = bundlesName.Split('.');
            var split = splitDot.LastOrDefault().Split('+');
            return split[0];
        }

    }
}