using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Adapters.Oracle.Helper
{
    public class OracleDbTypeMap
    {
        public static string MapCsharpType(string dbtype, int precision, int scale)
        {
            if (string.IsNullOrEmpty(dbtype))
            {
                return dbtype;
            }
            //如果是枚举
            if (dbtype.Contains("Enum"))
            {
                return dbtype;
            }
            dbtype = dbtype.ToLower();

            string csharpType = "object";
            switch (dbtype)
            {

                case "varchar": csharpType = "string"; break;
                case "varchar2": csharpType = "string"; break;
                case "nvarchar2": csharpType = "string"; break;
                case "char": csharpType = "string"; break;
                case "nchar": csharpType = "string"; break;
                case "clob": csharpType = "string"; break;
                case "long": csharpType = "string"; break;
                case "nclob": csharpType = "string"; break;
                case "rowid": csharpType = "string"; break;

                case "number":
                    if (scale == 0)
                    {
                        if (precision == 1)
                        {
                            csharpType = "bool";
                        }
                        else if (precision <= 4)
                        {
                            csharpType = "short";
                        }
                        else if (precision <= 7)
                        {
                            csharpType = "int";
                        }
                        else
                        {
                            csharpType = "long";
                        }
                    }
                    else
                    {
                        csharpType = "decimal";
                    }
                    break;



                case "blob": csharpType = "byte[]"; break;
                case "long raw": csharpType = "byte[]"; break;
                case "raw": csharpType = "byte[]"; break;
                case "bfile": csharpType = "byte[]"; break;
                case "varbinary": csharpType = "byte[]"; break;

                default: csharpType = "DateTime"; break;
            }
            return csharpType;
        }

        public static Type MapCommonType(string dbtype, int precision, int scale)
        {
            if (string.IsNullOrEmpty(dbtype))
            {
                return Type.Missing.GetType();
            }
            //如果是枚举
            if (dbtype.Contains("Enum"))
            {
                return typeof(Enum);
            }
            dbtype = dbtype.ToLower();
            Type commonType = typeof(object);
            switch (dbtype)
            {
                case "varchar": commonType = typeof(string); break;
                case "varchar2": commonType = typeof(string); break;
                case "nvarchar2": commonType = typeof(string); break;
                case "char": commonType = typeof(string); break;
                case "nchar": commonType = typeof(string); break;
                case "clob": commonType = typeof(string); break;
                case "long": commonType = typeof(string); break;
                case "nclob": commonType = typeof(string); break;
                case "rowid": commonType = typeof(string); break;

                case "number":
                    if (scale == 0)
                    {
                        if (precision == 1)
                        {
                            commonType = typeof(bool);
                        }
                        else if (precision <= 4)
                        {
                            commonType = typeof(short);
                        }
                        else if (precision <= 7)
                        {
                            commonType = typeof(int);
                        }
                        else
                        {
                            commonType = typeof(long);
                        }
                    }
                    else
                    {
                        commonType = typeof(decimal);
                    }
                    break;

                default: commonType = typeof(DateTime); break;
            }
            return commonType;
        }
    }
}
