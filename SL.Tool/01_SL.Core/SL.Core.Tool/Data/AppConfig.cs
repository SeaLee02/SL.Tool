using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.Core.Tool.Data
{
    public class AppConfig
    {
        public static readonly string SavePath = $"{AppDomain.CurrentDomain.BaseDirectory}AppConfig.json";

        public string Lang { get; set; } = "zh-cn";

        public SkinType Skin { get; set; }

        /// <summary>
        /// 数据库类型
        ///SqlServer = 0,
		///MySql = 1,
		///Oracle = 2
        /// </summary>
        public int DbType { get; set; } = 0;

        /// <summary>
        /// 数据库链接地址
        /// </summary>
        public string DataConnection { get; set; } = "Data Source=10.1.0.131;Initial Catalog=SAAS5;Persist Security Info=True;User ID=sa;password=90-=op[]";

        /// <summary>
        /// 模块化地址
        /// </summary>
        public string Module { get; set; } = "00_Admin";

       

        /// <summary>
        /// 仓储层地址
        /// </summary>
        public string SaveCodePath { get; set; } = @"F:\Code\Work\20210726\HRMS\Core";

        /// <summary>
        /// 命名空间前缀
        /// </summary>
        public string PrefixSpace { get; set; } = "SL.Mkh";

    }
}
