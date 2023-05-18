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

using DesignAutomationConsole.Extensions;
using DesignAutomationConsole.Models;
using DesignAutomationConsole.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DesignAutomationConsole
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("...");

            //var ForgeConfiguration = new ForgeConfiguration()
            //{
            //    ClientId = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_ID"),
            //    ClientSecret = Environment.GetEnvironmentVariable("FORGE_RICAUN_CLIENT_SECRET")
            //};

            await DA_RevitAddin_DA_Tester();

            return;

            await DA_AutoCAD_Test();
            await DA_Revit_Test();
            await DA_3dsMax_Test();
            await DA_Inventor_Test();
        }

        private static async Task DA_RevitAddin_DA_Tester()
        {
            const string RequestUri =
                "https://github.com/ricaun-io/RevitAddin.DA.Tester/releases/latest/download/RevitAddin.DA.Tester.bundle.zip";

            var appName = "RevitAddin_DA_Tester";

            var designAutomationService = new RevitDesignAutomationService(appName)
            {
                //EnableParameterConsoleLogger = true
            };

            Console.WriteLine($"Nickname: {designAutomationService.GetNickname()}");
            //await designAutomationService.Initialize($".\\Bundle\\RevitAddin.DA.Tester.bundle.zip");
            await designAutomationService.Initialize(await RequestService.Instance.GetFileAsync(RequestUri));

            var engineVersions = designAutomationService.CoreEngineVersions();

            var options = new List<ParameterOptions>();
            var tasks = new List<Task>();
            foreach (var engineVersion in engineVersions)
            {
                var option = new ParameterOptions()
                {
                    Engine = engineVersion,
                    Input = new InputModel() { Text = engineVersion }
                };
                options.Add(option);
                var daTask = designAutomationService.Run<ParameterOptions>(option, engineVersion);
                tasks.Add(daTask);
            }

            await Task.WhenAll(tasks);

            foreach (var option in options)
            {
                Console.WriteLine(option.ToJson());
            }

        }

        private static async Task DA_3dsMax_Test()
        {
            IDesignAutomationService service = new MaxDesignAutomationService("ExecuteMaxscript");
            await service.Run<MaxParameterOptions>(options =>
            {
                options.InputMaxScene = @".\DA\DA43dsMax\input.zip";
                options.MaxscriptToExecute = @".\DA\DA43dsMax\TwistIt.ms";
            });
        }

        private static async Task DA_AutoCAD_Test()
        {
            IDesignAutomationService service = new AutoCADDesignAutomationService("ListLayers")
            {
                //ForceUpdateAppBundle = true,
                //ForceUpdateActivity = true,
                //ForceCreateWorkItemReport = true,
            };
            await service.Initialize(@".\DA\DA4ACAD\ListLayers.zip");
            await service.Run<AutoCADParameterOptions>(options =>
            {
                options.InputDwg = @".\DA\DA4ACAD\ListLayers.dwg";
                options.Script = "(command \"LISTLAYERS\")\n";
            });
        }

        private static async Task DA_Inventor_Test()
        {
            IDesignAutomationService service = new InventorDesignAutomationService("ChangeParam");
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
            IDesignAutomationService service = new RevitDesignAutomationService("DeleteWalls");
            //await service.Initialize(@".\DA\DA4Revit\DeleteWalls.zip");
            await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2021.rvt";
            }, "2021");
            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2022.rvt";
            //}, "2022");
            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2023.rvt";
            //}, "2023");
            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2024.rvt";
            //}, "2024");
        }
    }
}