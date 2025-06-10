using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Sassa.Logging
{
    public class Log
    {
        private static readonly object _fileLock = new();
        private static readonly JsonSerializerOptions _options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        public void WriteJson(object obj, string fileName)
        {
            var jsonString = JsonSerializer.Serialize(obj, _options);

            var dir = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            lock (_fileLock)
            {
                File.AppendAllText(fileName, jsonString);
            }
        }

        public T? ReadJson<T>(string fileName) where T : new()
        {
            if (!File.Exists(fileName)) return new T();
            var jsonString = File.ReadAllText(fileName);
            if (string.IsNullOrWhiteSpace(jsonString)) return new T();
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}