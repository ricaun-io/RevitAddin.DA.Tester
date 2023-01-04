using Newtonsoft.Json;
using System.IO;

namespace RevitAddin.DA.Tester.Models
{
    public class InputModel
    {
        public string Text { get; set; }

        #region JsonConvert
        private const string JSON_FILE = "input.json";
        public InputModel Load(string jsonPath = JSON_FILE)
        {
            if (File.Exists(jsonPath))
            {
                string jsonContents = File.ReadAllText(jsonPath);
                return JsonConvert.DeserializeObject<InputModel>(jsonContents);
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
