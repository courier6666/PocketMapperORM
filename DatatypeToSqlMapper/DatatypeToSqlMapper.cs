using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace PocketMapperORM.DatatypeToSqlMapper
{
    public class DatatypeToSqlServerMapper
    {
        public static IDictionary<Type, string> clrDtToSqlServerDt;
        public static IDictionary<SqlDbType, string> sqlDbTypeToSqlServerDt;
        private static DatatypeToSqlServerMapper _instance;
        public static DatatypeToSqlServerMapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatatypeToSqlServerMapper();
                }

                return _instance;
            }
        }
        public string this[Type type]
        {
            get
            {
                try
                {
                    return clrDtToSqlServerDt[type];
                }
                catch(KeyNotFoundException ex)
                {
                    throw new ArgumentException("Provided type cannot be translated into a sql server  data type!", nameof(type), ex);
                }
            }
        }
        public string this[SqlDbType sqlDbType]
        {
            get
            {
                try
                {
                    return sqlDbTypeToSqlServerDt[sqlDbType];
                }
                catch (KeyNotFoundException ex)
                {
                    throw new ArgumentException("Provided sqlDbType cannot be translated into a sql server data type!", nameof(sqlDbType), ex);
                }
            }
        }
        static DatatypeToSqlServerMapper()
        {
            clrDtToSqlServerDt = new Dictionary<Type, string>()
            {
                {typeof(int), "int"},
                {typeof(long), "bigint"},
                {typeof(short), "smallint"},
                {typeof(float), "float"},
                {typeof(double), "double"},
                {typeof(decimal), "decimal"},
                {typeof(bool), "bit"},
                {typeof(string), "nvarchar(max)"},
                {typeof(DateTime), "datetime"}
            };
        }
        private DatatypeToSqlServerMapper()
        {

        }
    }
}
