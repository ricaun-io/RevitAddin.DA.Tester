using System;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    /// <summary>
    /// IRequestService
    /// </summary>
    public interface IRequestService
    {
        /// <summary>
        /// GetJsonAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public Task<T> GetJsonAsync<T>(string requestUri);

        /// <summary>
        /// GetJsonAsync
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Task<object> GetJsonAsync(string requestUri, Type type);
        /// <summary>
        /// GetStringAsync
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public Task<string> GetStringAsync(string requestUri);

        /// <summary>
        /// GetFileAsync
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Task<string> GetFileAsync(string requestUri, string fileName = null);

        ///// <summary>
        ///// Asynchronously retrieves a stream from the specified request URI.
        ///// </summary>
        ///// <param name="requestUri">The URI from which to retrieve the stream.</param>
        ///// <returns>A task that represents the asynchronous operation. The task result contains the stream retrieved from the request URI.</returns>
        //public Task<Stream> GetStreamAsync(string requestUri);

        ///// <summary>
        ///// Asynchronously uploads form data to the specified request URI.
        ///// </summary>
        ///// <param name="requestUri">The URI to which the form data will be sent.</param>
        ///// <param name="formData">A dictionary containing the form data to send. The key is the name of the form field and the value is the field value.</param>
        ///// <param name="filePath">The file path of the file to include in the form data.</param>
        //public Task UploadFormDataAsync(string requestUri, Dictionary<string, string> formData, string filePath);
    }
}
