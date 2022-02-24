using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Helper
{
    /// <summary>
    /// table帮助类
    /// </summary>
    public static class DataTableHelper
    {
        /// <summary>
        /// dataTable转实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <param name="SF"></param>
        /// <returns></returns>
        public static List<T> Mapper<T>(this DataTable dt, bool SF = false)
        {
            List<T> result = new List<T>();
            List<string> ColumnNameList = new List<string>();
            foreach (DataColumn item in dt.Columns)
            {
                ColumnNameList.Add(item.ColumnName.ToLower());
            }
            foreach (DataRow item in dt.Rows)
            {
                T d = Activator.CreateInstance<T>();
                Type pp = typeof(T);
                PropertyInfo[] ppList = pp.GetProperties();
                foreach (PropertyInfo pro in pp.GetProperties())
                {

                    if (ColumnNameList.Contains(pro.Name.ToLower()) && item[pro.Name] != null && item[pro.Name] != DBNull.Value)
                    {
                        Type type = pro.PropertyType;
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            type = type.GetGenericArguments()[0];
                        }
                        if (!SF)
                        {
                            object value = Convert.ChangeType(item[pro.Name], type);
                            pro.SetValue(d, value, null);
                        }
                        else
                        {
                            if (item[pro.Name].ToString().Equals("否"))
                            {
                                object value = Convert.ChangeType(false, type);
                                pro.SetValue(d, value, null);
                            }
                            else if (item[pro.Name].ToString().Equals("是"))
                            {
                                object value = Convert.ChangeType(true, type);
                                pro.SetValue(d, value, null);
                            }
                            else if (item[pro.Name].ToString() == string.Empty)
                            {

                            }
                            else
                            {
                                object value = Convert.ChangeType(item[pro.Name], type);
                                pro.SetValue(d, value, null);
                            }
                        }
                    }
                }
                result.Add(d);
            }
            return result;
        }
    }
}
