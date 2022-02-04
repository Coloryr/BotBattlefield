using Newtonsoft.Json;
using System.IO;

namespace ColoryrSDK
{
    class ConfigUtils
    {
        private static object Locker = new object();
        public static T Config<T>(T obj1, string FilePath) where T : new()
        {
            FileInfo file = new FileInfo(FilePath);
            T obj;
            if (!file.Exists)
            {
                if (obj1 == null)
                    obj = new T();
                else
                    obj = obj1;
                Save(obj, FilePath);
            }
            else
            {
                lock (Locker)
                {
                    obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath));
                }
            }
            return obj;
        }
        public static void Save(object obj, string FilePath)
        {
            lock (Locker)
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }
        }
    }
}
