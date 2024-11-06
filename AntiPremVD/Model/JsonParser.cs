using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AntiPremVD.Model
{
    public class JsonParser : INotifyPropertyChanged
    {
        private string _language;
        public string Language 
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged();
                }
            }
        }

        private JsonInfo _jsonInfo;
        public JsonInfo JsonInfo
        {
            get => _jsonInfo;
            set 
            {
                if (_jsonInfo != value) 
                {
                    _jsonInfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public JsonParser() { SetLanguage("EN"); }

        private string path_to_json;
        private string jsonData;

        public void SetLanguage(string language)
        {
            Language = language;
            path_to_json = $@"G:\ARTEM\Проэкты С++, C#\Visual Studio projects\C# Projects\AntiPremVD\AntiPremVD\Model\Resources\ToolTips{Language}.json";
            jsonData = File.ReadAllText(path_to_json);
            JsonInfo = JsonSerializer.Deserialize<JsonInfo>(jsonData);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }    
}
