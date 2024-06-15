using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss.DesignAutomation;
using Autodesk.Forge.Oss.DesignAutomation.Extensions;
using Autodesk.Forge.Oss.DesignAutomation.Services;
using DesignAutomationConsole.Models;
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
        }

        private static async Task DA_RevitAddin_DA_Tester()
        {
            var appName = "RevitAddin_DA_Tester";

            IDesignAutomationService designAutomationService = new RevitDesignAutomationService(appName)
            {
                EngineVersions = new[] {
                    //"2019",
                    //"2020",
                    "2021",
                    //"2022",
                    //"2023",
                    //"2024",
                    //"2025",
                },
                EnableConsoleLogger = true,
                //EnableParameterConsoleLogger = true,
                EnableReportConsoleLogger = true,
                RunTimeOutMinutes = 5,
            };

            //var revit = new RevitDesignAutomationService(appName);
            //var engines = await revit.GetEnginesAsync();
            //foreach ( var engine in engines )
            //{
            //    Console.WriteLine(engine);
            //}

            //var status = await revit.GetWorkitemStatusAsync("id");
            //Console.WriteLine(status.Status);

            await designAutomationService.Initialize($".\\Bundle\\RevitAddin.DA.Tester.bundle.zip");

            //const string RequestUri =
            //    "https://github.com/ricaun-io/RevitAddin.DA.Tester/releases/latest/download/RevitAddin.DA.Tester.bundle.zip";
            //await designAutomationService.Initialize(await RequestService.Instance.GetFileAsync(RequestUri));

            var engineVersions = designAutomationService.CoreEngineVersions();

            var options = new List<ParameterOptions>();
            var tasks = new List<Task>();
            foreach (var engineVersion in engineVersions)
            {
                var option = new ParameterOptions()
                {
                    Input = new InputModel()
                    {
                        Text = engineVersion,
                        Sleep = 1000
                    }
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

            await designAutomationService.Delete();
        }
    }
}