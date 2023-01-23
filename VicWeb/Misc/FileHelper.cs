using System;
using System.IO;
using Newtonsoft.Json;

namespace VicWeb.Misc
{
    public static class FileHelper
    {
        public static bool DeserializeJsonFile<T>(string path,out T jsonObj) where T : new()
        {
            
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                jsonObj = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            else
            {
                jsonObj = new T();
                return false;
            }
           
        }

        public static void SerializeToJsonFile(this object self, string path, JsonSerializerSettings settings = null)
        {
            var folder = Path.GetDirectoryName(path);
            Directory.CreateDirectory(folder);
            var json = JsonConvert.SerializeObject(self, Formatting.Indented, settings);
            try
            {
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {

            }
        }
    }
}
