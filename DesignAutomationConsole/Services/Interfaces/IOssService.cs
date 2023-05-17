using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public interface IOssService
    {
        public Task<string> UploadFileAsync(string localFullName, string fileName);
        public Task<string> CreateUrlReadWriteAsync(string fileName);
    }
}