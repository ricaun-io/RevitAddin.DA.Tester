using NUnit.Framework;
using ricaun.Forge.DesignAutomation.Services;
using ricaun.Forge.DesignAutomation.Tests.Models;
using ricaun.Forge.DesignAutomation.Tests.Services;
using System.Threading.Tasks;

namespace ricaun.Forge.DesignAutomation.Tests
{
    public class DA_3dMax_Test
    {
        [Test]
        public async Task DA_Test()
        {
            IDesignAutomationService service = new MaxDesignAutomationService("ExecuteMaxscript")
            {
                EnableConsoleLogger = true,
                EnableParameterConsoleLogger = true,
            };
            var result = await service.Run<MaxParameterOptions>(options =>
            {
                options.InputMaxScene = @".\DA\DA43dsMax\input.zip";
                options.MaxscriptToExecute = @".\DA\DA43dsMax\TwistIt.ms";
            });
            await service.Delete();

            Assert.IsTrue(result);
        }
    }
}