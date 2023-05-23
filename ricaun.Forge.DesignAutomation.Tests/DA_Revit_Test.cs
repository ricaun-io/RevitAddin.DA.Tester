using NUnit.Framework;
using ricaun.Forge.DesignAutomation.Services;
using ricaun.Forge.DesignAutomation.Tests.Models;
using ricaun.Forge.DesignAutomation.Tests.Services;
using System.Threading.Tasks;

namespace ricaun.Forge.DesignAutomation.Tests
{
    public class DA_Revit_Test
    {
        [Test]
        public async Task DA_Test()
        {
            IDesignAutomationService service = new RevitDesignAutomationService("DeleteWalls")
            {
                EnableConsoleLogger = true,
                EnableParameterConsoleLogger = true,
            };
            await service.Initialize(@".\DA\DA4Revit\DeleteWalls.zip");
            var result = await service.Run<RevitParameterOptions>(options =>
            {
                options.RvtFile = @".\DA\DA4Revit\DeleteWalls2021.rvt";
                options.Result = @"Result2021.rvt";
            }, "2021");

            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2022.rvt";
            //    options.Result = @"Result2022.rvt";
            //}, "2022");
            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2023.rvt";
            //    options.Result = @"Result2023.rvt";
            //}, "2023");
            //await service.Run<RevitParameterOptions>(options =>
            //{
            //    options.RvtFile = @".\DA\DA4Revit\DeleteWalls2024.rvt";
            //    options.Result = @"Result2024.rvt";
            //}, "2024");

            await service.Delete();

            Assert.IsTrue(result);
        }
    }
}