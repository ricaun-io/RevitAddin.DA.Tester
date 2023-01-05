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
            Console.WriteLine("Main");
            var ForgeConfiguration = new ForgeConfiguration()
            {
                ClientId = Environment.GetEnvironmentVariable("FORGE_CLIENT_ID"),
                ClientSecret = Environment.GetEnvironmentVariable("FORGE_CLIENT_SECRET")
            };

            var designAutomationService = new DesignAutomationService(ForgeConfiguration);

            var name = await designAutomationService.GetNicknameAsync();
            Console.WriteLine(name);

            //

            //Console.WriteLine();

            //var engines = await designAutomationService.GetEnginesAsync();
            //foreach (var engine in engines)
            //{
            //    Console.WriteLine(engine);
            //}

            var filePath = await RequestService.GetFileAsync(RequestUri);
            var appName = "RevitAddin_DA_Tester";
            var engine = "Autodesk.Revit+2018";

            //await designAutomationService.DeleteAppBundleAsync(appName);

            try
            {

                for (int i = 0; i < 50; i++)
                {
                    var appBundle = await designAutomationService.CreateAppBundleAsync(
                        appName,
                        filePath,
                        engine);

                    Console.WriteLine(appBundle);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            //var versions = await designAutomationService.DesignAutomationClient.GetAppBundleVersionsAsync(appName);
            //foreach (var version in versions.Data)
            //{
            //    Console.WriteLine($"\t {version}");
            //}


            var bundlesNames = await designAutomationService.GetAllBundlesAsync();
            foreach (var bundlesName in bundlesNames)
            {
                var id = GetName(bundlesName);
                Console.WriteLine(bundlesName);
                Console.WriteLine(id);
                var versions = await designAutomationService.GetAppBundleVersionsAsync(id);

                foreach (var version in versions)
                {
                    Console.WriteLine($"\t {version}");
                }
            }

        }

        private static string GetName(string bundlesName)
        {
            var splitDot = bundlesName.Split('.');
            var split = splitDot.LastOrDefault().Split('+');
            return split[0];
        }

    }
}