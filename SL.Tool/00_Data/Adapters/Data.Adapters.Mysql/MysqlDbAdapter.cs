using Data.Abstractions.Adapter;
using Data.Abstractions.Entities;
using Data.Abstractions.Helper;
using Data.Adapters.Mysql.Helper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Data.Adapters.Mysql
{
    public class MysqlDbAdapter : DbAdapterAbstract<MySqlParameter>
    {
        public override DbProvider Provider => DbProvider.Oracle;

        //"server=127.0.0.1;database=m_admin2;userid=root;password=123456",
        public override async Task<DataTable> GetDataTable(string connectionString, string commandText, params MySqlParameter[] parms)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddRange(parms);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return await Task.FromResult(dt);
            }
        }


        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tables"></param>
        /// <returns></returns>
        public override async Task<List<DbTable>> GetDbTables(string connectionString, string tables = "")
        {
            //查询条件，如果包含,就是要in,否则就是要like查询
            if (!tables.NotNull())
            {
                tables = ((!tables.Contains(",")) ? $" AND  a.TABLE_NAME like '{tables}%'  " : string.Format(" and a.TABLE_NAME in ('{0}')", tables.Replace(",", "','")));
            }
            string sql = $@"SELECT
	a.TABLE_NAME TableName,
	a.TABLE_SCHEMA Schemname,
	a.TABLE_COMMENT TableDesc,
	a.TABLE_ROWS  `Rows`,
	b.column_name TablePrimarkeyName,
CASE
		
		WHEN LEFT ( c.COLUMN_TYPE, LOCATE( '(', c.COLUMN_TYPE ) - 1 ) = '' THEN
		c.COLUMN_TYPE ELSE LEFT ( c.COLUMN_TYPE, LOCATE( '(', c.COLUMN_TYPE ) - 1 ) 
	END AS TablePrimarkeyType,
CASE
		
		WHEN c.COLUMN_KEY = 'PRI' THEN
	TRUE ELSE FALSE 
	END AS HasPrimaryKey 
FROM
	information_schema.TABLES a -- 多主键，只取一个
	LEFT JOIN ( SELECT * FROM INFORMATION_SCHEMA.`KEY_COLUMN_USAGE` GROUP BY TABLE_NAME ) b ON a.TABLE_NAME = b.table_name 
	AND a.table_schema = b.table_schema -- 多主键，只取一个
	LEFT JOIN ( SELECT * FROM Information_schema.COLUMNS c  where   c.ORDINAL_POSITION='1'  GROUP BY c.table_name ) c ON a.TABLE_NAME = c.TABLE_NAME 
	AND a.table_schema = c.table_schema 
WHERE
	a.table_schema = (
	SELECT DATABASE
	())  {tables}
	AND b.constraint_name = 'PRIMARY' 
	AND c.COLUMN_KEY = 'PRI';
";
            DataTable dataTable = await GetDataTable(connectionString, sql);
            List<DbTable> list = DataTableHelper.Mapper<DbTable>(dataTable);
            foreach (DbTable item in list)
            {
                var arry = item.TableName.Split('_').ToList() ;
                if (arry.Count() > 0)
                {
                    arry.RemoveAt(0);
                    foreach (var name in arry)
                    {
                        item.EntityName += name.FirstCharToUpper();
                    }
                }
                else
                {
                    item.EntityName = item.TableName.FirstCharToUpper();
                }
                item.CShareType = MysqlDbTypeMap.MapCsharpType(item.TablePrimarkeyType);
                item.DbColumns = await GetDbColumns(connectionString, item.TableName, item);
            }
            return list;
        }

        /// <summary>
        /// 获取列信息
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override async Task<List<DbColumn>> GetDbColumns(string connectionString, string tableName, DbTable table)
        {
            string sql = $@"SELECT
	TABLE_NAME AS `TableName`,
	column_name AS `ColumnName`,
CASE
		
		WHEN LEFT ( COLUMN_TYPE, LOCATE( '(', COLUMN_TYPE ) - 1 ) = '' THEN
		COLUMN_TYPE ELSE LEFT ( COLUMN_TYPE, LOCATE( '(', COLUMN_TYPE ) - 1 ) 
	END AS `ColumnType`,
	CAST(
		SUBSTRING(
			COLUMN_TYPE,
			LOCATE( '(', COLUMN_TYPE ) + 1,
			LOCATE( ')', COLUMN_TYPE ) - LOCATE( '(', COLUMN_TYPE ) - 1 
		) AS signed
	) AS `CharLength`,
	column_default AS `DefaultValue`,
	column_comment AS ColumnDesc,
 NUMERIC_SCALE as `Scale`,
 DATETIME_PRECISION as `Precision`,
CASE
		
		WHEN COLUMN_KEY = 'PRI' THEN
	TRUE ELSE FALSE 
	END AS IsPrimaryKey,
CASE
		
		WHEN EXTRA = 'auto_increment' THEN
	TRUE ELSE FALSE 
	END AS IsIdentity,
CASE
		
		WHEN is_nullable = 'YES' THEN
	TRUE ELSE FALSE 
	END AS `IsNullable` 
FROM
	Information_schema.COLUMNS 
WHERE
	TABLE_NAME = '{tableName}' and table_schema=(select database()) order by ORDINAL_POSITION  ";
            DataTable dataTable = await GetDataTable(connectionString, sql);
            List<DbColumn> list = DataTableHelper.Mapper<DbColumn>(dataTable);

            foreach (var item in list)
            {
                if (item.ColumnName == table.TablePrimarkeyName)
                {
                    item.IsPrimaryKey = true;
                }
                if (false)
                {
                    string enumStr = GetEnumStr(item.ColumnDesc, item.ColumnName, table);
                    if (!string.IsNullOrEmpty(enumStr))
                    {
                        item.ColumnType = enumStr;
                    }
                }
                item.CShareType = MysqlDbTypeMap.MapCsharpType(item.ColumnType);
                item.CommonType = MysqlDbTypeMap.MapCommonType(item.ColumnType);

                if ((item.ColumnName.Contains("Id") && item.CShareType == "string") || item.ColumnName == "CreatedOrg")
                {
                    item.CShareType = "Guid";
                    item.CommonType = typeof(Guid);
                }

            }
            return list;
        }


        /// <summary>
        /// 获取枚举字符串
        /// </summary>
        /// <param name="columnDesc"></param>
        /// <param name="colName"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private string GetEnumStr(string columnDesc, string colName, DbTable table)
        {
            if (columnDesc == null)
            {
                return "";
            }
            string regex = @"\(([^)]*)\)";
            if (Regex.IsMatch(columnDesc, regex))
            {
                //枚举定义规则: xxxm枚举(value.key[Des];) 
                // 枚举结尾,()中是枚举定义,[]中是描述值可有可无, .分割值和key,  ;分割每个定义
                string enumString = columnDesc;
                // "测试枚举(7.Sunday[星期天];1.Monday[星期一];2.Tuesday[星期二];3.Wednesday[];5.Friday)";
                string math = Regex.Match(enumString, regex).Value; //得到枚举字符串，带()
                string text = math.Replace("(", "").Replace(")", ""); //去除()
                string one = "    ";  //一级空格
                string two = "        "; //二级空格
                string enumName = colName + "Enum";
                StringBuilder enumSb = new StringBuilder();
                //拼接头部
                enumSb.AppendLine($"{one}/// <summary>");
                enumSb.AppendLine($"{one}/// {columnDesc}");
                enumSb.AppendLine($"{one}/// </summary>");
                enumSb.AppendLine($"{one}public enum {enumName}");
                enumSb.AppendLine($"{one}{{");
                int i = 1;
                string[] arry = text.Split(';');
                foreach (string item in arry)
                {
                    if (item.Contains("[") && item.Contains("]"))  //是否包含描述
                    {
                        string[] arry3 = item.Split('[');
                        string[] arry2 = arry3[0].Split('.');
                        //拼接枚举值
                        enumSb.AppendLine($"{two}/// <summary>");
                        enumSb.AppendLine($"{two}/// {arry2[1]}");
                        enumSb.AppendLine($"{two}/// </summary>");
                        enumSb.AppendLine($"{two}[Description(\"{arry3[1].Replace("]", "")}\")]");
                        if (i == arry.Length)  //最后一个不带,
                        {
                            enumSb.AppendLine($"{two}{arry2[1]} = {arry2[0]}");
                        }
                        else   //带,
                        {
                            enumSb.AppendLine($"{two}{arry2[1]} = {arry2[0]},");
                        }
                        enumSb.AppendLine("");

                    }
                    else
                    {
                        string[] arry2 = item.Split('.');
                        enumSb.AppendLine($"{two}/// <summary>");
                        enumSb.AppendLine($"{two}/// {arry2[1]}");
                        enumSb.AppendLine($"{two}/// </summary>");
                        if (i == arry.Length)
                        {
                            enumSb.AppendLine($"{two}{arry2[1]} = {arry2[0]}");
                        }
                        else
                        {
                            enumSb.AppendLine($"{two}{arry2[1]} = {arry2[0]},");
                        }
                        enumSb.AppendLine("");
                    }

                    i++;
                }
                enumSb.AppendLine($"{one}}}");
                table.ListEnumStr.Add(enumSb.ToString());
                return enumName;
            }
            return string.Empty;

        }

    }
}
