using Data.Abstractions.Entities;
using Data.Abstractions.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Judge
{
    public class EntityJudge
    {
        

        public static string  BaseConfig(List<DbColumn> dbColumns)
        {
            string baseEntity = string.Empty;
            var col = new List<string>
            {
                "Remarks","CreatedId","CreatedName","CreatedTime","ModifiedId",
                "ModifiedName","ModifiedTime","IsDeleted","DeletedId","DeletedName","DeletedTime"
            };

            var columns = dbColumns.Select(x => x.ColumnName).ToList();
            bool isExcept = !col.Except(columns).Any();
            if (isExcept)
            {
                baseEntity += "EntityBaseSoftDelete,";
            }

            col = new List<string>
            {
                 "OrgId","CreatedOrg"
            };
            if (!col.Except(columns).Any())
            {
                baseEntity += "IOrg,";
            }
            col = new List<string>
            {
                 "TenantId"
            };
            if (!col.Except(columns).Any())
            {
                baseEntity += "ISLTenant,";
            }
            return baseEntity.TrimEnd(',');
        }


        public static List<string> IgnoreCol(bool isBase = true)
        {
            if (isBase)
            {
                return new List<string>
            {
               "Remarks","CreatedId","CreatedName","CreatedTime","ModifiedId",
                "ModifiedName","ModifiedTime","IsDeleted","DeletedId","DeletedName","DeletedTime"
            };
            }
            else
            {
                return new List<string>();
            }
        }


        public static string GetIsNull(DbColumn column)
        {
            if (column.CommonType.IsValueType && column.IsNullable && column.CommonType != typeof(Enum))
            {
                return column.CShareType + "?";
            }
            else
            {
                return column.CShareType;
            }
        }

        /// <summary>
        /// 首字母转小写
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToLower(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            string[] arry = value.Split('_');
            string str = "";
            foreach (string item in arry)
            {
                string newstr = item.Replace("(", "").Replace(".", "").Replace(")", "");
                string firstLetter = newstr.Substring(0, 1);
                string rest = newstr.Substring(1, newstr.Length - 1);
                str += firstLetter.ToLower() + rest;
            }
            return str;
        }


    }
}
