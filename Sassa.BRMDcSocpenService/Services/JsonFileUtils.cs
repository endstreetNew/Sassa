using System.Text.Json;
using System.Text.Json.Serialization;
using static Sassa.BRM.Services.TimedService;

namespace Sassa.BRM.Services
{
    // Native/JsonFileUtils.cs
    public class JsonFileUtils
    {
        // Create a new Mutex. The creating thread does not own the mutex.
        private static Mutex mutStatus = new Mutex();
        private static Mutex mutLog = new Mutex();

        private static readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        public void WriteJson(object obj, string fileName)
        {
            var jsonString = JsonSerializer.Serialize(obj, _options);
            mutStatus.WaitOne();
            if (!Directory.Exists(fileName.Substring(0, fileName.LastIndexOf("\\"))))
            {
                Directory.CreateDirectory(fileName.Substring(0, fileName.LastIndexOf("\\")));
            }
            File.WriteAllText(fileName, jsonString);
            mutStatus.ReleaseMutex();
            WriteLog((GlobalVars)obj, fileName);
        }
        public void WriteLog(GlobalVars entry, string fileName)
        {
            mutLog.WaitOne();
            File.AppendAllText(fileName.Substring(0, fileName.LastIndexOf("\\")) + "\\logFile.json", entry.Progress + " " + System.DateTime.Now.ToShortTimeString() + Environment.NewLine);
            mutLog.ReleaseMutex();

        }

        public T? ReadJson<T>(string fileName) where T : new()
        {
            if (!File.Exists(fileName)) return new T();
            string jsonString = File.ReadAllText(fileName);
            if (string.IsNullOrEmpty(jsonString)) return new T();
            return JsonSerializer.Deserialize<T>(jsonString);
        }

    }
}
