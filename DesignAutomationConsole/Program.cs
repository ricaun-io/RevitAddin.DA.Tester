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

            await DA_Revit_Test();
            return;

            //await DA_AutoCAD_Test();
            //await DA_Revit_Test();
            //await DA_3dsMax_Test();
            //await DA_Inventor_Test();
            //return;

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

        private static async Task DA_3dsMax_Test()
        {
            var service = new MaxDesignAutomationService("ExecuteMaxscript");
            await service.Run<MaxParameterOptions>(options =>
            {
                options.InputMaxScene = @".\DA\DA43dsMax\input.zip";
                options.MaxscriptToExecute = @".\DA\DA43dsMax\TwistIt.ms";
            });
        }

        private static async Task DA_AutoCAD_Test()
        {
            var service = new AutoCADDesignAutomationService("ListLayers");
            await service.Initialize(@".\DA\DA4ACAD\ListLayers.zip");
            await service.Run<AutoCADParameterOptions>(options =>
            {
                options.InputDwg = @".\DA\DA4ACAD\ListLayers.dwg";
                options.Script = "(command \"LISTLAYERS\")\n";
            });
        }

        private static async Task DA_Inventor_Test()
        {
            var service = new InventorDesignAutomationService("ChangeParam");
            await service.Initialize(@".\DA\DA4Inventor\samplePlugin.bundle.zip");
            await service.Run<InventorParameterOptions>(options =>
            {
                options.InventorDoc = @".\DA\DA4Inventor\box.ipt";
                options.InventorParams = new()
                {
                    height = "16 in",
                    width = "10 in"
                };
            });
        }

        private static async Task DA_Revit_Test()
        {
            var service = new RevitDesignAutomationService("DeleteWalls");
            await service.Initialize(@".\DA\DA4Revit\DeleteWalls.zip");
            await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2021.rvt";
            });
            await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2022.rvt";
            }, "2022");
            await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2023.rvt";
            }, "2023");
            await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2024.rvt";
            }, "2024");
        }
    }
}