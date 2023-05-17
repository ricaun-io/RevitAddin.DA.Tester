using Autodesk.Forge.DesignAutomation.Model;
using DesignAutomationConsole.Attributes;
using DesignAutomationConsole.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DesignAutomationConsole.Services
{
    public class ParameterArgumentService<T> : IParameterArgumentService<T> where T : class
    {
        #region Variables
        private readonly IRequestService requestService;
        private readonly IOssService ossService;
        private readonly T obj;

        /// <summary>
        /// Parameters for DA activity.
        /// </summary>
        private Dictionary<string, Parameter> Parameters { get; } = new Dictionary<string, Parameter>();
        /// <summary>
        /// Arguments for DA workitem.
        /// </summary>
        private Dictionary<string, IArgument> Arguments { get; } = new Dictionary<string, IArgument>();
        /// <summary>
        /// Download files for DA workitem when finish.
        /// </summary>
        private List<DownloadFile> DownloadFiles { get; } = new List<DownloadFile>();
        #endregion

        public ParameterArgumentService(IOssService ossService, IRequestService requestService, T obj)
        {
            this.ossService = ossService;
            this.requestService = requestService;
            this.obj = obj;
        }

        private void ObjectForEachProperties(Action<PropertyInfo, string, object> actionPropertyNameValue)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var name = StringUtils.ConvertUpperToUnderscore(property.Name);
                var value = property.GetValue(obj);
                actionPropertyNameValue(property, name, value);
            }
        }

        private async Task ObjectForEachProperties(Func<PropertyInfo, string, object, Task> actionPropertyNameValue)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                var name = StringUtils.ConvertUpperToUnderscore(property.Name);
                var value = property.GetValue(obj);
                await actionPropertyNameValue(property, name, value);
            }
        }

        public async Task Initialize()
        {
            WriteLine($"Initialize - {typeof(T).Name}");

            Parameters.Clear();
            Arguments.Clear();
            DownloadFiles.Clear();

            await ObjectForEachProperties(async (property, name, value) =>
            {
                if (property.TryGetAttribute(out ParameterInputAttribute parameterInput))
                {
                    var localName = parameterInput.LocalName;
                    var uploadFileName = localName;

                    var inputParam = parameterInput.ToParameter();
                    Parameters.Add(name, inputParam);

                    IArgument inputArgument = IArgumentUtils.ToJsonArgument(value);

                    if (parameterInput.UploadFile)
                    {
                        value = InputUtils.CreateTempFile(value);
                        WriteLine($"CreateTempFile: {value}");
                    }

                    // http, file
                    if (value is string stringValue)
                    {
                        if (InputUtils.IsFile(stringValue, out string filePath))
                        {
                            stringValue = await ossService.UploadFileAsync(filePath, uploadFileName);
                            WriteLine($"UploadFile: {localName} {stringValue}");
                        }

                        if (InputUtils.IsUrl(stringValue))
                        {
                            inputArgument = IArgumentUtils.ToFileArgument(stringValue);
                        }
                    }

                    // Add ZipPath Feature
                    if (string.IsNullOrEmpty(parameterInput.ZipPath) == false)
                    {
                        if (inputArgument is XrefTreeArgument argument)
                        {
                            argument.PathInZip = parameterInput.ZipPath;
                        }
                    }

                    Arguments.Add(name, inputArgument);
                }
                else if (property.TryGetAttribute(out ParameterOutputAttribute parameterOutput))
                {
                    var localName = parameterOutput.LocalName;
                    var downloadFileName = localName;

                    if (parameterOutput.Zip)
                        downloadFileName += ".zip";

                    var outputParam = parameterOutput.ToParameter();
                    Parameters.Add(name, outputParam);

                    string callbackArgument = value as string;

                    if (value is null || (string.IsNullOrWhiteSpace(callbackArgument)))
                    {
                        callbackArgument = await ossService.CreateUrlReadWriteAsync(downloadFileName);
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
            });
        }

        public void Update(Activity activity)
        {
            WriteLine($"Update Activity - {activity.Id}");
            activity.Parameters = Parameters;
            ObjectForEachProperties((property, name, value) =>
            {
                foreach (var parameterActivity in property.GetAttributes<ParameterActivityAttribute>())
                {
                    parameterActivity.Update(activity, name, value);
                }
            });
        }

        public void Update(WorkItem workItem)
        {
            WriteLine($"Update WorkItem - {workItem.ActivityId}");
            workItem.Arguments = Arguments;
            ObjectForEachProperties((property, name, value) =>
            {
                foreach (var parameterWorkItem in property.GetAttributes<ParameterWorkItemAttribute>())
                {
                    parameterWorkItem.Update(workItem, name, value);
                }
            });
        }

        public async Task<T> Finalize()
        {
            foreach (var downloadFile in DownloadFiles)
            {
                var fileName = downloadFile.FileName;
                try
                {
                    WriteLine($"Download: {fileName} {downloadFile.Property}");

                    if (!PropertyUtils.IsPropertyTypeString(downloadFile.Property))
                    {
                        var jsonObject = await requestService.GetJsonAsync(downloadFile.Url, downloadFile.Property.PropertyType);
                        WriteLine($"DownloadJson: {jsonObject}");
                        downloadFile.Property.SetValue(obj, jsonObject);
                    }
                    else
                    {
                        var filePath = await requestService.GetFileAsync(downloadFile.Url, fileName);
                        WriteLine($"DownloadFile: {filePath}");
                        downloadFile.Property.SetValue(obj, filePath);
                    }
                }
                catch (Exception ex)
                {
                    WriteLine($"DownloadFail: {ex.GetType()}");
                }
            }

            WriteLine("Finalize");
            return this.obj;
        }

        public bool EnableConsoleLogger { get; set; } = false;
        private void WriteLine(object message)
        {
            if (EnableConsoleLogger == false) return;
            Console.WriteLine($"[ParameterArgument] {message}");
        }

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

        #region DownloadFile
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
        #endregion
    }

    public interface IParameterArgumentService<T>
    {
        public Task Initialize();
        public void Update(Activity activity);
        public void Update(WorkItem workItem);
        public Task<T> Finalize();
    }
}