using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Abstractions.Entities
{
    public class DbTable
    {
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 子集合
        /// </summary>
        public string EntityName
        {
            get; set;
        }

        /// <summary>
        ///  架构名
        /// </summary>
        public string Schemname { get; set; }

        /// <summary>
        /// 行数
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 是否拥有主键
        /// </summary>
        public bool HasPrimaryKey { get; set; }

        /// <summary>
        /// 表描述
        /// </summary>
        public string TableDesc { get; set; }

        public string TableFullName
        {
            get
            {
                return $"{TableName}({TableDesc})";
            }
        }

        /// <summary>
        /// 主键名
        /// </summary>
        public string TablePrimarkeyName { get; set; }

        /// <summary>
        /// 主键类型
        /// </summary>
        public string TablePrimarkeyType { get; set; }

        /// <summary>
        /// c#类型
        /// </summary>
        public string CShareType { get; set; }

        /// <summary>
        /// 整数位
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// 小数位
        /// </summary>
        public int Scale { get; set; }

        public List<DbColumn> DbColumns { get; set; }

        /// <summary>
        /// 枚举字符串集合
        /// </summary>
        public List<string> ListEnumStr { get; set; }

        public DbTable()
        {
            ListEnumStr = new List<string>();
        }

        /// <summary>
        /// 是否选择
        /// </summary>
        public bool Selected { get; set; } = false;

        /// <summary>
        /// 模块配置
        /// </summary>
        public ModuleModel ModuleModel { get; set; }


    }

}
