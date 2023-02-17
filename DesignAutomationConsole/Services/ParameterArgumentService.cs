using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class ParameterArgumentService<T> where T : class
    {
        private readonly DesignAutomationService designAutomationService;
        private readonly T obj;

        public Dictionary<string, Parameter> Parameters { get; } = new Dictionary<string, Parameter>();
        public Dictionary<string, IArgument> Arguments { get; } = new Dictionary<string, IArgument>();

        public Dictionary<string, string> DownloadFiles { get; } = new Dictionary<string, string>();

        public ParameterArgumentService(DesignAutomationService designAutomationService, T obj)
        {
            this.designAutomationService = designAutomationService;
            this.obj = obj;
        }

        public async Task Initialize()
        {
            Parameters.Clear();
            Arguments.Clear();
            DownloadFiles.Clear();

            foreach (var property in obj.GetType().GetProperties())
            {
                var value = property.GetValue(obj);
                var name = property.Name.ToLower();
                if (property.TryGetAttribute<ParameterInputAttribute>(out ParameterInputAttribute parameterInput))
                {
                    var inputParam = new Parameter()
                    {
                        LocalName = parameterInput.Name,
                        Description = parameterInput.Description,
                        Verb = Verb.Get,
                        Required = parameterInput.Required,
                    };
                    Parameters.Add(name, inputParam);
                    IArgument inputArgument = IArgumentUtils.ToJsonArgument(value);

                    // http, file
                    if (value is string str)
                    {
                        bool IsFile(string file)
                        {
                            return File.Exists(file);
                        }
                        bool IsUrl(string url)
                        {
                            return Uri.TryCreate(url, UriKind.Absolute, out var uri);
                        }
                        //if (IsUrl(str))
                        inputArgument = IArgumentUtils.ToJsonArgument(str);
                    }
                    Arguments.Add(name, inputArgument);
                }
                if (property.TryGetAttribute<ParameterOutputAttribute>(out ParameterOutputAttribute parameterOutput))
                {
                    var outputParam = new Parameter()
                    {
                        LocalName = parameterOutput.Name,
                        Description = parameterOutput.Description,
                        Verb = Verb.Put,
                    };
                    Parameters.Add(name, outputParam);

                    string callbackArgument = value as string;

                    if (value is null || (string.IsNullOrWhiteSpace(value as string)))
                    {
                        callbackArgument = await CreateReadWrite(name);
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(obj, callbackArgument);
                        }
                    }

                    if (parameterOutput.DownloadFile)
                    {
                        DownloadFiles.Add(parameterOutput.Name, callbackArgument);
                    }

                    var outputArgument = IArgumentUtils.ToCallbackArgument(callbackArgument);
                    Arguments.Add(name, outputArgument);
                }
            }
        }

        public async Task Finalize()
        {
            foreach (var downloadFile in DownloadFiles)
            {
                var fileName = downloadFile.Key;
                var readSignedUrl = downloadFile.Value;
                try
                {
                    var filePath = await RequestService.GetFileAsync(readSignedUrl, fileName);
                    Console.WriteLine(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task<string> CreateReadWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var nickname = await designAutomationService.GetNicknameAsync();
            var bucketKey = nickname.ToLower() + "_" + designAutomationService.AppName.ToLower();
            var bucket = await designAutomationService.OssClient.TryGetBucketDetailsAsync(bucketKey);
            if (bucket is null) bucket = await designAutomationService.OssClient.CreateBucketAsync(bucketKey);

            return await designAutomationService.OssClient.CreateSignedFileAsync(bucketKey, fileName, "readwrite");
        }

    }
}