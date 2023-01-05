﻿using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class RequestService
    {
        /// <summary>
        /// GetJsonAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public async static Task<T> GetJsonAsync<T>(string requestUri)
        {
            var json = await GetStringAsync(requestUri);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }
        /// <summary>
        /// GetStringAsync
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public async static Task<string> GetStringAsync(string requestUri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStringAsync(requestUri);
            }
        }

        /// <summary>
        /// GetFileAsync
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async static Task<string> GetFileAsync(string requestUri, string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = Path.GetFileName(requestUri);

            if (File.Exists(fileName))
                File.Delete(fileName);

            using (HttpClient client = new HttpClient())
            {
                using (var s = await client.GetStreamAsync(requestUri))
                {
                    using (var fs = new FileStream(fileName, FileMode.CreateNew))
                    {
                        await s.CopyToAsync(fs);
                        return fs.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Stream
        /// </summary>
        /// <param name="requestUri"></param>
        /// <returns></returns>
        public async static Task<Stream> GetStreamAsync(string requestUri)
        {
            using (HttpClient client = new HttpClient())
            {
                return await client.GetStreamAsync(requestUri);
            }
        }

        /// <summary>
        /// Post FormData with File
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="formData"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async static Task UploadFormDataAsync(string requestUri, Dictionary<string, string> formData, string filePath)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    foreach (KeyValuePair<string, string> vp in formData)
                    {
                        content.Add(new StringContent(vp.Value), vp.Key);
                    }

                    var streamContent = new StreamContent(new FileStream(filePath, FileMode.Open));
                    content.Add(streamContent, "file", Path.GetFileName(filePath));

                    var response = await client.PostAsync(requestUri, content);
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
