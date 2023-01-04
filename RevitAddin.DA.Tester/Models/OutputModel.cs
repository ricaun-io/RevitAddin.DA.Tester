using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAddin.DA.Tester.Models
{
    public class OutputModel
    {
        public string VersionName { get; set; }
        public string VersionBuild { get; set; }
        public DateTime TimeStart { get; set; } = DateTime.UtcNow;
        public string Text { get; set; }

        #region JsonConvert
        private const string JSON_FILE = "output.json";
        public OutputModel Load(string jsonPath = JSON_FILE)
        {
            if (File.Exists(jsonPath))
            {
                string jsonContents = File.ReadAllText(jsonPath);
                return JsonConvert.DeserializeObject<OutputModel>(jsonContents);
            }
            return this;
        }

        public string Save(string jsonPath = JSON_FILE)
        {
            string text = JsonConvert.SerializeObject(this);
            using (StreamWriter sw = File.CreateText(jsonPath))
            {
                sw.WriteLine(text);
                sw.Close();
            }
            return text;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        #endregion
    }
}
