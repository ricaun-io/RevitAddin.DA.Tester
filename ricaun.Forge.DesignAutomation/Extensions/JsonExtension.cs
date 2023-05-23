﻿using ricaun.Forge.DesignAutomation.Services;
using System;

namespace ricaun.Forge.DesignAutomation.Extensions
{
    /// <summary>
    /// JsonExtension
    /// </summary>
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
}
