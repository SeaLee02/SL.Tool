using Data.Abstractions.Adapter;
using Data.Abstractions.Entities;
using Data.Abstractions.Helper;
using Data.Adapters.SqlServer.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DbColumn = Data.Abstractions.Entities.DbColumn;


namespace Data.Adapters.SqlServer
{
    public class SqlServerDbAdapter : DbAdapterAbstract<SqlParameter>
    {
        public override DbProvider Provider => DbProvider.SqlServer;
        //Data Source = 10.1.8.36,1133; Initial Catalog = SAAS_BASE_R; Persist Security Info=True;User ID = saashr; password=890-=iop[];MultipleActiveResultSets=True;Connect Timeout = 21;
        public override async Task<DataTable> GetDataTable(string connectionString, string commandText, params SqlParameter[] parms)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = connection.CreateCommand();
                command.CommandText = commandText;
                command.Parameters.AddRange(parms);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
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
                tables = ((!tables.Contains(",")) ? $" AND  obj.name='{tables}'  " : string.Format(" and obj.name in ('{0}')", tables.Replace(",", "','")));
            }
            string sql = $@"SELECT obj.name Tablename,
                                   schem.name Schemname,
                                   idx.rows,
                                   CAST(CASE
                                            WHEN
                                            (
                                                SELECT COUNT(1)
                                                FROM sys.indexes
                                                WHERE object_id = obj.object_id
                                                      AND is_primary_key = 1
                                            ) >= 1 THEN
                                                1
                                            ELSE
                                                0
                                        END AS BIT) HasPrimaryKey,
                                   b.value TableDesc,
                                   t.*
                            FROM sys.objects obj
                                INNER JOIN sysindexes idx --行数
                                    ON obj.object_id = idx.id
                                       AND idx.indid <= 1
                                INNER JOIN sys.schemas schem --架构
                                    ON obj.schema_id = schem.schema_id
                                LEFT JOIN sys.extended_properties b --描述
                                    ON obj.object_id = b.major_id
                                       AND b.minor_id = 0
                                       AND b.name = 'MS_Description'
                                OUTER APPLY --主键名称和类型
                            (
                                SELECT TOP 1
                                       colm.name AS TablePrimarkeyName,
                                       systype.name AS TablePrimarkeyType
                                FROM sys.columns colm
                                    INNER JOIN sys.types systype
                                        ON colm.system_type_id = systype.system_type_id
                                WHERE colm.object_id = obj.object_id
                                      AND colm.column_id IN
                                          (
                                              SELECT ic.column_id
                                              FROM sys.indexes idx
                                                  INNER JOIN sys.index_columns ic
                                                      ON idx.index_id = ic.index_id
                                                         AND idx.object_id = ic.object_id
                                              WHERE idx.object_id = obj.object_id
                                                    AND idx.is_primary_key = 1
                                          )
                            ) t
                            WHERE obj.type = 'U'  {tables}  
                            ORDER BY obj.name";

            DataTable dataTable = await GetDataTable(connectionString, sql);
            List<DbTable> list = DataTableHelper.Mapper<DbTable>(dataTable);
            foreach (DbTable item in list)
            {
                if (item.TableName.Split('_').Count() > 0)
                {
                    item.EntityName = item.TableName.Split('_')[1];
                }
                else
                {
                    item.EntityName = item.TableName;
                }
               

                item.CShareType = SqlServerDbTypeMap.MapCsharpType(item.TablePrimarkeyType);
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
            string sql = $@"WITH indexCTE
                            AS (SELECT ic.column_id,
                                       ic.index_column_id,
                                       ic.object_id
                                FROM sys.indexes idx
                                    INNER JOIN sys.index_columns ic
                                        ON idx.index_id = ic.index_id
                                           AND idx.object_id = ic.object_id
                                WHERE idx.object_id = OBJECT_ID('{tableName}')   --找到该表的主键信息
                                      AND idx.is_primary_key = 1)
                            SELECT colm.column_id ColumnID,                 --列id
                                   CAST(CASE
                                            WHEN indexCTE.column_id IS NULL THEN
                                                0
                                            ELSE
                                                1
                                        END AS BIT) IsPrimaryKey,
                                   colm.name ColumnName,                    --列名称
                                   systype.name ColumnType,                 --列类型
                                   colm.is_identity IsIdentity,             --是否自增长
                                   colm.is_nullable IsNullable,             --是否为空
                                   CAST(colm.max_length AS INT) ByteLength, -- sys.columns中的max_length是字节
                                   (CASE
                                        WHEN systype.name = 'nvarchar'
                                             AND colm.max_length > 0 THEN
                                            colm.max_length / 2
                                        WHEN systype.name = 'nchar'
                                             AND colm.max_length > 0 THEN
                                            colm.max_length / 2
                                        WHEN systype.name = 'ntext'
                                             AND colm.max_length > 0 THEN
                                            colm.max_length / 2
                                        ELSE
                                            colm.max_length
                                    END
                                   ) CharLength,                            --得到字符类型长度
                                   CAST(colm.precision AS INT) Precision,
                                   CAST(colm.scale AS INT) Scale,
                                   sep.value ColumnDesc  --列描述
                            FROM sys.columns colm
                                INNER JOIN sys.types systype  
                                    ON colm.system_type_id = systype.system_type_id
                                       AND systype.user_type_id = colm.user_type_id   --通过两个关联进行过滤得到用户创建的类型
                                LEFT JOIN sys.extended_properties sep   
                                    ON sep.major_id = colm.object_id  --得到是这个表的
                                       AND colm.column_id = sep.minor_id   AND sep.name='MS_Description'  --这列的
                                LEFT JOIN indexCTE
                                    ON indexCTE.column_id = colm.column_id
                                       AND indexCTE.object_id = colm.object_id 
                            WHERE colm.object_id = OBJECT_ID('{tableName}')";
            DataTable dataTable = await GetDataTable(connectionString, sql);
            List<DbColumn> list = DataTableHelper.Mapper<DbColumn>(dataTable);

            foreach (var item in list)
            {
                if (false)
                {
                    string enumStr = GetEnumStr(item.ColumnDesc, item.ColumnName, table);
                    if (!string.IsNullOrEmpty(enumStr))
                    {
                        item.ColumnType = enumStr;
                    }
                }
                item.CShareType = SqlServerDbTypeMap.MapCsharpType(item.ColumnType);
                item.CommonType = SqlServerDbTypeMap.MapCommonType(item.ColumnType);
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
