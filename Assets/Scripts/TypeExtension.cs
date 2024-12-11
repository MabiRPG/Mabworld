using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TypeExtension
{
    public static class MyExtensions
    {
        public static string DisplayWithSuffix(this int num)
        {
            string number = num.ToString();
            if (number.EndsWith("11"))
                return number + "th";
            if (number.EndsWith("12"))
                return number + "th";
            if (number.EndsWith("13"))
                return number + "th";
            if (number.EndsWith("1"))
                return number + "st";
            if (number.EndsWith("2"))
                return number + "nd";
            if (number.EndsWith("3"))
                return number + "rd";
            return number + "th";
        }
    }

    public class JsonDictionaryIDConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new Exception("Is not implemented");
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            JToken obj = JToken.Load(reader);

            Dictionary<int, T> dict = new Dictionary<int, T>();

            foreach (JToken token in obj)
            {
                JProperty property = (JProperty)token;

                if (property.Name == "$id")
                {
                    continue;
                }

                T t = JsonConvert.DeserializeObject<T>(property.First.ToString());

                // Dark magic reflection to get the ID value of a model inside the class.
                FieldInfo field = t.GetType().GetField("model");
                int ID = (int)field
                    .GetValue(t)
                    .GetType()
                    .GetField("ID")
                    .GetValue(field.GetValue(t));

                dict.Add(ID, t);
            }

            return dict;
        }
    }
}
