using NUnit.Framework;
using ricaun.Forge.DesignAutomation.Services;
using ricaun.Forge.DesignAutomation.Tests.Models;
using ricaun.Forge.DesignAutomation.Tests.Services;
using System.Threading.Tasks;

namespace ricaun.Forge.DesignAutomation.Tests
{
    public class DA_Inventor_Test
    {
        [Test]
        public async Task DA_Test()
        {
            IDesignAutomationService service = new InventorDesignAutomationService("ChangeParam")
            {
                EnableConsoleLogger = true,
                EnableParameterConsoleLogger = true,
            };
            await service.Initialize(@".\DA\DA4Inventor\samplePlugin.bundle.zip");
            var result = await service.Run<InventorParameterOptions>(options =>
            {
                options.InventorDoc = @".\DA\DA4Inventor\box.ipt";
                options.InventorParams = new()
                {
                    height = "16 in",
                    width = "10 in"
                };
            });
            await service.Delete();

            Assert.IsTrue(result);
        }
    }
}