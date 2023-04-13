using Newtonsoft.Json;
using System;

namespace DesignAutomationConsole.Services
{
    public static class JsonExtension
    {
        /// <summary>
        /// ToJson using <see cref="JsonService.Instance"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToJson(this object value)
        {
            return JsonService.Instance.Serialize(value);
        }

        /// <summary>
        /// FromJson using <see cref="JsonService.Instance"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object FromJson(this string value, Type type)
        {
            return JsonService.Instance.Deserialize(value, type);
        }

        /// <summary>
        /// FromJson using <see cref="JsonService.Instance"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromJson<T>(this string value)
        {
            return JsonService.Instance.Deserialize<T>(value);
        }
    }

    public class JsonService : IJsonService
    {
        public static JsonService Instance { get; set; } = new JsonService();

        /// <summary>
        /// Serialize <paramref name="value"/> to json string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Serialize(object value)
        {
            if (value is string valueString) return valueString;
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Deserialize <paramref name="value"/> to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Deserialize<T>(string value)
        {
            if (value is T valueString) return valueString;
            return JsonConvert.DeserializeObject<T>(value);
        }

        /// <summary>
        /// Deserialize <paramref name="value"/> to <paramref name="type"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(string value, Type type)
        {
            if (type == typeof(string)) return value;
            return JsonConvert.DeserializeObject(value, type);
        }
    }

    public interface IJsonService
    {
        /// <summary>
        /// Serialize <paramref name="value"/> to json string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Serialize(object value);
        /// <summary>
        /// Deserialize <paramref name="value"/> to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public T Deserialize<T>(string value);
        /// <summary>
        /// Deserialize <paramref name="value"/> to <paramref name="type"/>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object Deserialize(string value, Type type);
    }
}
