using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using DesignAutomationConsole.Attributes;
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
                var name = StringUtils.ConvertUpperToUnderscore(property.Name);
                if (property.TryGetAttribute(out ParameterInputAttribute parameterInput))
                {
                    var localName = parameterInput.Name;
                    var inputParam = new Parameter()
                    {
                        LocalName = localName,
                        Description = parameterInput.Description,
                        Verb = Verb.Get,
                        Required = parameterInput.Required,
                    };
                    Parameters.Add(name, inputParam);
                    IArgument inputArgument = IArgumentUtils.ToJsonArgument(value);

                    // http, file
                    if (value is string stringValue)
                    {
                        if (parameterInput.UploadFile)
                        {
                            stringValue = InputUtils.CreateTempFile(stringValue);
                            Console.WriteLine($"CreateTempFile: {stringValue}");
                        }

                        if (InputUtils.IsFile(stringValue, out string filePath))
                        {
                            stringValue = await UploadFile(filePath, localName);
                            Console.WriteLine($"UploadFile: {localName} {stringValue}");
                        }

                        if (InputUtils.IsUrl(stringValue))
                        {
                            inputArgument = IArgumentUtils.ToFileArgument(stringValue);
                        }
                    }
                    Arguments.Add(name, inputArgument);
                }
                else if (property.TryGetAttribute(out ParameterOutputAttribute parameterOutput))
                {
                    var localName = parameterOutput.Name;
                    var outputParam = new Parameter()
                    {
                        LocalName = localName,
                        Description = parameterOutput.Description,
                        Verb = Verb.Put,
                    };
                    Parameters.Add(name, outputParam);

                    string callbackArgument = value as string;

                    if (value is null || (string.IsNullOrWhiteSpace(value as string)))
                    {
                        callbackArgument = await CreateReadWrite(localName);
                        if (property.PropertyType == typeof(string))
                        {
                            property.SetValue(obj, callbackArgument);
                        }
                    }

                    if (parameterOutput.DownloadFile)
                    {
                        DownloadFiles.Add(localName, callbackArgument);
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
                    Console.WriteLine($"DownloadFile: {fileName}");
                    var filePath = await RequestService.GetFileAsync(readSignedUrl, fileName);
                    Console.WriteLine($"DownloadFile: {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DownloadFile: {ex.GetType()}");
                }
            }
        }

        private async Task<string> CreateOssBucketKey(string fileName)
        {
            var nickname = await designAutomationService.GetNicknameAsync();
            var bucketKey = nickname.ToLower() + "_" + designAutomationService.AppName.ToLower();
            var bucket = await designAutomationService.OssClient.TryGetBucketDetailsAsync(bucketKey);
            if (bucket is null) bucket = await designAutomationService.OssClient.CreateBucketAsync(bucketKey);
            return bucketKey;
        }

        private async Task<string> UploadFile(string localFullName, string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey(fileName);
            var objectDetails = await designAutomationService.OssClient.UploadFileAsync(bucketKey, fileName, localFullName);
            return await designAutomationService.OssClient.CreateSignedFileAsync(bucketKey, fileName);
        }

        private async Task<string> CreateWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey(fileName);
            return await designAutomationService.OssClient.CreateSignedFileWriteAsync(bucketKey, fileName);
        }

        private async Task<string> CreateReadWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey(fileName);
            return await designAutomationService.OssClient.CreateSignedFileAsync(bucketKey, fileName, "readwrite");
        }

        class StringUtils
        {
            public static string ConvertUpperToUnderscore(string inputString)
            {
                string outputString = "";
                for (int i = 0; i < inputString.Length; i++)
                {
                    char c = inputString[i];
                    if (char.IsUpper(c))
                    {
                        outputString += i == 0 ? char.ToLower(c) : "_" + char.ToLower(c);
                    }
                    else
                    {
                        outputString += c;
                    }
                }
                return outputString;
            }
        }

        class InputUtils
        {
            public static string CreateTempFile(string content, string name = null)
            {
                var fileName = Path.GetTempFileName() + name;
                File.WriteAllText(fileName, content);
                return fileName;
            }

            public static bool IsFile(string file, out string filePath)
            {
                filePath = file;
                try
                {
                    var fileInfo = new FileInfo(file);
                    filePath = fileInfo.FullName;
                    return fileInfo.Exists;
                }
                catch { }
                return false;
            }
            public static bool IsUrl(string url)
            {
                return Uri.TryCreate(url, UriKind.Absolute, out var uri);
            }
        }
    }
}