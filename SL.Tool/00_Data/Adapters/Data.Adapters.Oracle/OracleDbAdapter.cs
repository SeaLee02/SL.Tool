using Data.Abstractions.Adapter;
using Data.Abstractions.Entities;
using Data.Abstractions.Helper;
using Data.Adapters.Oracle.Helper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Data.Adapters.Oracle
{
    public class OracleDbAdapter : DbAdapterAbstract<OracleParameter>
    {
        public override DbProvider Provider => DbProvider.Oracle;
        //"Data Source=10.36.0.57/orcl;User ID=C##TSSYHG;Password=1234;";
        public override async Task<DataTable> GetDataTable(string connectionString, string commandText, params OracleParameter[] parms)
        {
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                OracleCommand command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddRange(parms);
                OracleDataAdapter adapter = new OracleDataAdapter(command);
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
            if (!string.IsNullOrEmpty(tables))
            {
                tables = ((!tables.Contains(",")) ? $" AND  utc.table_name like '{tables}%'  " : string.Format(" and utc.table_name in ('{0}')", tables.Replace(",", "','")));
            }
            else
            {
                tables = " AND 1=1 ";
            }

            string sql = $@"SELECT
    utc.TABLE_NAME TableName, utc.COMMENTS TableDesc,
     CAST(case when t.keyname is null then 0 else 1 end as NUMBER(1)) as HasPrimaryKey,
	t.keyname  TablePrimarkeyName,
    t.data_type TablePrimarkeyType,
    t.data_precision Precision,
    t.Data_Scale Scale
FROM
    user_tab_comments utc
    LEFT JOIN(
    SELECT DISTINCT
        cu.COLUMN_name KEYNAME,
        cu.table_name,
	    uc.data_type,
        uc.data_precision,
        uc.Data_Scale 
    FROM
        user_cons_columns cu,
        user_constraints au,
        user_tab_columns uc
    WHERE
        cu.constraint_name = au.constraint_name  and cu.table_name=uc.table_name and cu.COLUMN_name=uc.column_name
        AND au.constraint_type = 'P'
    ) t ON t.table_name = utc.table_name
WHERE
    utc.table_type = 'TABLE'
    AND utc.table_name != 'HELP'
                        AND utc.table_name NOT LIKE '%$%'
                        AND utc.table_name NOT LIKE 'LOGMNRC_%'
                        AND utc.table_name != 'LOGMNRP_CTAS_PART_MAP'
                        AND utc.table_name != 'LOGMNR_LOGMNR_BUILDLOG'
                        AND utc.table_name != 'SQLPLUS_PRODUCT_PROFILE'  {tables} ";        

            DataTable dataTable = await GetDataTable(connectionString, sql);
            List<DbTable> list = DataTableHelper.Mapper<DbTable>(dataTable);
            foreach (DbTable item in list)
            {
                item.EntityName = item.TableName;
                item.CShareType = OracleDbTypeMap.MapCsharpType(item.TablePrimarkeyType,item.Precision,item.Scale);
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
            string sql = $@" select a.Table_name TableName,a.column_name ColumnName,a.data_type  ColumnType,(case a.data_type when 'VARCHAR' then a.data_length/2
when 'VARCHAR2'then a.data_length/2
when 'NVARCHAR2' then a.data_length/2
when 'CHAR' then a.data_length/2
when 'NCHAR'then a.data_length/2
when 'CLOB' then a.data_length/2
 when 'LONG'then a.data_length/2
when 'NCLOB'then a.data_length/2
when 'ROWID'then a.data_length/2
else  a.data_length end) CharLength,
a.data_precision Precision,
a.Data_Scale Scale,
CAST(case a.nullable when 'N' then 0 else 1 end as NUMBER(1)) as IsNullable
,a.column_id ColumnID,b.comments ColumnDesc
    from user_tab_columns a left join user_col_comments b on a.TABLE_NAME = b.table_name and a.COLUMN_NAME = b.column_name   
     where a.table_name = '{tableName}' order by column_id ";
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
                item.CShareType = OracleDbTypeMap.MapCsharpType(item.ColumnType,item.Precision,item.Scale);
                item.CommonType = OracleDbTypeMap.MapCommonType(item.ColumnType, item.Precision, item.Scale);
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
