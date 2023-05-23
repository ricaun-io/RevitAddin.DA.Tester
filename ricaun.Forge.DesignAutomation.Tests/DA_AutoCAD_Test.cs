using NUnit.Framework;
using ricaun.Forge.DesignAutomation.Services;
using ricaun.Forge.DesignAutomation.Tests.Models;
using ricaun.Forge.DesignAutomation.Tests.Services;
using System.Threading.Tasks;

namespace ricaun.Forge.DesignAutomation.Tests
{
    public class DA_AutoCAD_Test
    {
        [Test]
        public async Task DA_Test()
        {
            IDesignAutomationService service = new AutoCADDesignAutomationService("ListLayers")
            {
                EnableConsoleLogger = true,
                EnableParameterConsoleLogger = true,
                //ForceUpdateAppBundle = true,
                //ForceUpdateActivity = true,
                //ForceCreateWorkItemReport = true,
            };
            await service.Initialize(@".\DA\DA4ACAD\ListLayers.zip");
            var result = await service.Run<AutoCADParameterOptions>(options =>
            {
                options.InputDwg = @".\DA\DA4ACAD\ListLayers.dwg";
                options.Script = "(command \"LISTLAYERS\")\n";
            });
            await service.Delete();

            Assert.IsTrue(result);
        }
    }
}