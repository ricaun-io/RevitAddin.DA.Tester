using Autodesk.Forge.DesignAutomation.Model;
using Autodesk.Forge.Oss;
using DesignAutomationConsole.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class ParameterArgumentService<T> where T : class
    {
        private readonly DesignAutomationService designAutomationService;
        private readonly T obj;

        /// <summary>
        /// Parameters for DA activity.
        /// </summary>
        public Dictionary<string, Parameter> Parameters { get; } = new Dictionary<string, Parameter>();
        /// <summary>
        /// Arguments for DA workitem.
        /// </summary>
        public Dictionary<string, IArgument> Arguments { get; } = new Dictionary<string, IArgument>();
        /// <summary>
        /// Download files for DA workitem when finish.
        /// </summary>
        public List<DownloadFile> DownloadFiles { get; } = new List<DownloadFile>();

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
                    var uploadFileName = localName;

                    var inputParam = parameterInput.ToParameter();
                    Parameters.Add(name, inputParam);

                    IArgument inputArgument = IArgumentUtils.ToJsonArgument(value);

                    if (parameterInput.UploadFile)
                    {
                        value = InputUtils.CreateTempFile(value);
                        Console.WriteLine($"CreateTempFile: {value}");
                    }

                    // http, file
                    if (value is string stringValue)
                    {
                        if (InputUtils.IsFile(stringValue, out string filePath))
                        {
                            stringValue = await UploadFile(filePath, uploadFileName);
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
                    var downloadFileName = localName;

                    var outputParam = parameterOutput.ToParameter();
                    Parameters.Add(name, outputParam);

                    string callbackArgument = value as string;

                    if (value is null || (string.IsNullOrWhiteSpace(callbackArgument)))
                    {
                        callbackArgument = await CreateReadWrite(downloadFileName);
                        if (PropertyUtils.IsPropertyTypeString(property))
                        {
                            property.SetValue(obj, callbackArgument);
                        }
                    }

                    if (!PropertyUtils.IsPropertyTypeString(property))
                    {
                        parameterOutput.DownloadFile = true;
                    }

                    if (parameterOutput.DownloadFile)
                    {
                        var downloadFile = new DownloadFile(downloadFileName, callbackArgument, property);

                        DownloadFiles.Add(downloadFile);
                    }

                    var outputArgument = IArgumentUtils.ToCallbackArgument(callbackArgument);
                    Arguments.Add(name, outputArgument);
                }
            }
        }

        public async Task<T> Finalize()
        {
            foreach (var downloadFile in DownloadFiles)
            {
                var fileName = downloadFile.FileName;
                try
                {
                    //Console.WriteLine($"DownloadFile: {fileName} {downloadFile.Property}");

                    if (!PropertyUtils.IsPropertyTypeString(downloadFile.Property))
                    {
                        var jsonObject = await RequestService.Instance.GetJsonAsync(downloadFile.Url, downloadFile.Property.PropertyType);
                        //Console.WriteLine($"DownloadJson: {jsonObject}");
                        downloadFile.Property.SetValue(obj, jsonObject);
                    }
                    else
                    {
                        var filePath = await RequestService.Instance.GetFileAsync(downloadFile.Url, fileName);
                        //Console.WriteLine($"DownloadFile: {filePath}");
                        downloadFile.Property.SetValue(obj, filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DownloadFile: {ex.GetType()}");
                }
            }
            return this.obj;
        }

        #region Oss
        private async Task<string> CreateOssBucketKey()
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
            var bucketKey = await CreateOssBucketKey();
            var objectDetails = await designAutomationService.OssClient.UploadFileAsync(bucketKey, fileName, localFullName);
            return await designAutomationService.OssClient.CreateSignedFileAsync(bucketKey, fileName);
        }

        private async Task<string> CreateWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey();
            return await designAutomationService.OssClient.CreateSignedFileWriteAsync(bucketKey, fileName);
        }

        private async Task<string> CreateReadWrite(string name, string engine = null)
        {
            var fileName = name + engine;
            var bucketKey = await CreateOssBucketKey();
            return await designAutomationService.OssClient.CreateSignedFileAsync(bucketKey, fileName, "readwrite");
        }
        #endregion

        #region Utils

        class PropertyUtils
        {
            public static bool IsPropertyTypeString(PropertyInfo property)
            {
                return property.PropertyType == typeof(string);
            }
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
            public static string CreateTempFile<TJson>(TJson content, string name = null)
            {
                return CreateTempFile(content.ToJson(), name);
            }

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

        #endregion
    }

    public class DownloadFile
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public PropertyInfo Property { get; set; }
        public DownloadFile(string fileName, string url, PropertyInfo property)
        {
            FileName = fileName;
            Url = url;
            Property = property;
        }
        public override string ToString()
        {
            return Url;
        }
    }
}