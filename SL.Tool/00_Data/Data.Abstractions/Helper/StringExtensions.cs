using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Abstractions.Helper
{
    public static class StringExtensions
    {
        /// <summary>
        /// stirng转任意类型
        /// </summary>
        /// <typeparam name="T">需要转的类型</typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T ConvertTo<T>(this string s)
        {
            if (typeof(T).IsEnum)
            {
                return (T)Enum.Parse(typeof(T), s);
            }
            return (T)Convert.ChangeType(s, typeof(T));
        }

        /// <summary>
        /// 判断字符串是否为Null、空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// 判断字符串是否不为Null、空
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool NotNull(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        /// 与字符串进行比较，忽略大小写
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool EqualsIgnoreCase(this string s, string value)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(value);

            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 首字母转小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstCharToLower(this string s)
        {
            //return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s);


            if (string.IsNullOrEmpty(s))
                return s;

            string str = s.First().ToString().ToLower() + s.Substring(1);
            return str;
        }

        /// <summary>
        /// 首字母转大写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string FirstCharToUpper(this string s)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s);
            //if (string.IsNullOrEmpty(s))
            //    return s;

            //string str = s.First().ToString().ToUpper() + s.Substring(1);
            //return str;
        }

        ///// <summary>
        ///// 首字母小写
        ///// </summary>
        ///// <param name="str"></param>
        ///// <returns></returns>
        //public static string GetUpperF(string str)
        //{
        //    return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str);
        //}
       
    }
}
