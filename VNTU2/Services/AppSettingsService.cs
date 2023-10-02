using System.IO;
using Newtonsoft.Json;
using VNTU2.Models;


namespace VNTU2.Services
{
    public class AppSettingsService
    {
        private const string DefaultPath = @"D:\c#\VNTU2\VNTU2\bin\Debug\appsettings.json";
        
        public AppSetting Read(string path = DefaultPath)
        {
            var json = File.ReadAllText(path);
            var result = JsonConvert.DeserializeObject<AppSetting>(json);
            return result;
        }

        public void Write(AppSetting apps, string path = DefaultPath)
        {
            var json = JsonConvert.SerializeObject(apps);
            File.WriteAllText(path, json);
        }
    }
}