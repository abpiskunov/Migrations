using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Migrations
{
    internal class JsonHelper
    {
        public static JObject LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return LoadfromStream(stream);
            }
        }

        public static JObject LoadfromStream(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonTextReader jsonReader = new JsonTextReader(streamReader))
            {
                return JObject.Load(jsonReader);
            }
        }
    }
}