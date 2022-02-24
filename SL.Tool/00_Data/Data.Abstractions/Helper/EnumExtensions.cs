using Data.Abstractions.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Helper
{
    public static class EnumExtensions
    {
        //并非字典
        private static readonly ConcurrentDictionary<string, string> DescriptionCache = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 不包含UnKnown选项
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, List<OptionModel>> ListCacheNoIgnore = new ConcurrentDictionary<RuntimeTypeHandle, List<OptionModel>>();

        /// <summary>
        /// 获取枚举类型的Description说明
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToDescription(this Enum value)
        {
            if (value == null)
            {
                return "";
            }
            var type = value.GetType();
            var info = type.GetField(value.ToString());
            if (info != null)
            {
                var key = type.FullName + info.Name;
                if (!DescriptionCache.TryGetValue(key, out string desc))
                {
                    var attrs = info.GetCustomAttributes(typeof(DescriptionAttribute), true);
                    if (attrs.Length < 1)
                        desc = string.Empty;
                    else
                        desc = attrs[0] is DescriptionAttribute
                            descriptionAttribute
                            ? descriptionAttribute.Description
                            : value.ToString();

                    DescriptionCache.TryAdd(key, desc);
                }
                return desc;
            }

            return "";
        }


        public static List<OptionModel> ToResult(this Enum value)
        {
            var enumType = value.GetType();

            if (!enumType.IsEnum)
                return null;

            return Enum.GetValues(enumType).Cast<Enum>().Select(x => new OptionModel
                {
                    Label = x.ToDescription(),
                    Value = x.ToInt()
                }).ToList();
        }

        /// <summary>
        /// 枚举转换为返回模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ignoreUnKnown">忽略UnKnown选项</param>
        /// <returns></returns>
        public static List<OptionModel> ToResult<T>()
        {
            var enumType = typeof(T);

            if (!enumType.IsEnum)
                return null;

            #region ==忽略UnKnown属性==

            if (!ListCacheNoIgnore.TryGetValue(enumType.TypeHandle, out List<OptionModel> list))
            {
                list = Enum.GetValues(enumType).Cast<Enum>()
                    .Where(m => !m.ToString().Equals("UnKnown")).Select(x => new OptionModel
                    {
                        Label = x.ToDescription(),
                        Value = x.ToInt()
                    }).ToList();

                ListCacheNoIgnore.TryAdd(enumType.TypeHandle, list);
            }

            return list.Select(m => new OptionModel { Label = m.Label, Value = m.Value }).ToList();

            #endregion ==忽略UnKnown属性==

        }
    }
}
