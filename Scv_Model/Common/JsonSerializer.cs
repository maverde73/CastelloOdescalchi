using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Net;
using System.IO;

namespace Scv_Model.Common
{
    public static class JsonSerializer
    {
        

        public static T Deserialize<T>(string json)
        {
            
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }

        public static string Serialize<T>(object objToSerialize)
        {
            T obj = Activator.CreateInstance<T>();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            MemoryStream mem = new MemoryStream();
            serializer.WriteObject(mem, (T)objToSerialize);
            string serializedString = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);
            return serializedString;
        }
    }
}
