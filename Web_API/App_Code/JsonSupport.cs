using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Web_API.App_Code
{
    public static class JsonSupport
    {

        public static T GetObjectFromJSON<T>(string json) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static Object ConvertJObject(dynamic F)
        {
            Object jsonObject = (JObject)JToken.FromObject(F);
            return jsonObject;
        }
        public static string ConvertJson(DataTable dt)
        {
            return JsonConvert.SerializeObject(dt);
        }

        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props =
            TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }
    }
}
