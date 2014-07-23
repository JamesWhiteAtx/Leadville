using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Configuration;
using Reeb.SqlOM;
using Reeb.SqlOM.Render;

namespace CST.ISIS.Data
{
    public static class SqlHelper 
    {
        public static void AddParm(this DbCommand cmd, string parameterName, object value)
        {
            DbParameter dbParameter = cmd.CreateParameter();
            dbParameter.IsNullable = true;
            dbParameter.ParameterName = parameterName;
            if (value != null)
            {
                dbParameter.Value = value;
            }
            else
            {
                dbParameter.Value = DBNull.Value;
            }

            cmd.Parameters.Add(dbParameter);
        }
    }

    public class IsisEntities
    {
        public static string SetId = "SALES";
        String connectionString = "";

        public IsisEntities(string connectionStr)
        {
            connectionString = connectionStr;
        }

        public string OraConnectionString()
        {
            return ConfigurationManager.ConnectionStrings[connectionString].ConnectionString; ;
        }

        public DbConnection GetDbConnection()
        {
            return new OracleConnection(OraConnectionString());
        }

        public string DBName()
        {
            string dbName = null;

            using (DbConnection conn = GetDbConnection())
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT NAME FROM V$DATABASE";

                    conn.Open();
                    using (DbDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            dbName = rdr["NAME"].ToString();
                        }
                    }
                }
            }
            return dbName;
        }

        public string DBNameObfuscate()
        {
            string dbName = DBName();
            if (dbName == null)
            {
                return "null";
            }
            else if (dbName.ToUpper() == "FSYS")
            {
                return "Production";
            }
            else if (dbName.ToUpper() == "TEST")
            {
                return "Testing";
            }
            else if (dbName.ToUpper() == "FTST")
            {
                return "Eff. Testing";
            }
            else if (dbName.ToUpper() == "FDEV")
            {
                return "Eff. Development";
            }
            else
            {
                char[] array = dbName.ToLower().ToCharArray();
                Array.Reverse(array);
                return "Unkn. " + new string(array);
            }
        }

        #region Object Lists

        public List<T> Entities<T>(EntityQuery query=null) where T : SqlEntity, new()
        {
            if (query == null)
            {
                query = QueryForEntity<T>();    
            }

            var list = new List<T>();
            using (DbConnection conn = GetDbConnection())
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    query.loadCmdText(cmd);
                    conn.Open();
                    using (DbDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var ent = new T();
                            ent.LoadFromDataRecord(rdr);
                            list.Add(ent);
                        }
                    }
                }
            }
            return list;
        }

        public List<T> Entitiesx<T>(T ent=null) where T : SqlEntity, new()
        {
            var list = new List<T>();
            using (DbConnection conn = GetDbConnection())
            {
                using (DbCommand cmd = conn.CreateCommand())
                {
                    if (ent==null) {
                        ent = new T();
                    }
                    ent.loadCmdText(cmd);
                    conn.Open();
                    using (DbDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            ent = new T();
                            ent.LoadFromDataRecord(rdr);
                            list.Add(ent);
                        }
                    }
                }
            }
            return list;
        }

        public EntityQuery QueryForEntity<T>() where T : SqlEntity
        {
            Type entityType = typeof(T);
            if (entityType == typeof(ProductType)) {
                return new ProductTypeQry();
            } else
                return null; 
        }

        #endregion
    }

    #region Entities

    public abstract class EntityQuery
    {
        protected SelectQuery query = new SelectQuery();
        public bool RenderNewLine { get; set; }

        public void loadCmdText(DbCommand cmd)
        {
            cmd.CommandText = GetSelectSql(RenderNewLine);
            LoadCmdParms(cmd);
        }

        virtual public void LoadCmdParms(DbCommand cmd) { }

        public string GetSelectSql(bool renderNewLine = false)
        {
            return query.SelectSql<OracleRenderer>(renderNewLine);
        }
    }

    public class ProductTypeQry : EntityQuery
    {
        public ProductTypeQry()
        {
            query
            .FROM("FG_PROD_TYPE", "PT")
            .SELECT("FG_PROD_TYPE_CD")
            .SELECT("FG_DESCRIPTION")
            .WHERE("FG_STATUS", "A")
            .ORDERBY("FG_PROD_TYPE_CD")
            ;
        }
    }

    public abstract class SqlEntity
    {
        abstract public string ListSql();

        abstract public void LoadFromDataRecord(IDataRecord dataRecord);

        public void loadCmdText(DbCommand cmd)
        {
            cmd.CommandText = ListSql();
            loadCmdParms(cmd);
        }

        virtual public void loadCmdParms(DbCommand cmd) { }
    }

    public class ProductType : SqlEntity 
    {
        override public void LoadFromDataRecord(IDataRecord dataRecord)
        {
            ProdTypeCd = dataRecord["FG_PROD_TYPE_CD"].ToString();
            Description = dataRecord["FG_DESCRIPTION"].ToString();
        }

        override public string ListSql()
        {
            return @"SELECT PT.FG_PROD_TYPE_CD, PT.FG_DESCRIPTION
FROM FG_PROD_TYPE PT 
WHERE PT.FG_STATUS = 'A' 
ORDER BY PT.FG_PROD_TYPE_CD";
        }

        public string ProdTypeCd { get; set; }
        public string Description { get; set; }

    }

    public class Product : SqlEntity
    {
        override public void LoadFromDataRecord(IDataRecord dataRecord)
        {
            ProdTypeCd = dataRecord["FG_PROD_TYPE_CD"].ToString();
            ProdCd = dataRecord["FG_PROD_CD"].ToString();
            Pattern = dataRecord["FG_PATTERN"].ToString();
            Color = dataRecord["FG_COLOR"].ToString();
            Description = dataRecord["FG_PROD_DESC"].ToString();
        }

        override public string ListSql()
        {
            return
@"SELECT FG_PROD_TYPE_CD, FG_PROD_CD, FG_PATTERN, FG_COLOR, FG_PROD_DESC, FG_PATTERN_DESC
FROM FG_PROD_VW P
WHERE FG_PROD_SETID = 'SALES'";
        }

        public string ProdTypeCd { get; set; }
        public string ProdCd { get; set; }
        public string Pattern { get; set; }
        public string Color { get; set; }
        public string Description { get; set; }
    }

    public class ProductTypeAndCode : Product
    {
        const string parmPrdCd = "&P_PROD_CD";
        const string parmPrdTypCd = "&P_PROD_TYPE_CD";

        string prdTypCd;
        string prdCd;

        public ProductTypeAndCode()
        {
            prdTypCd = null;
            prdCd = null;
        }

        public ProductTypeAndCode(string prodTypeCd, string prodCd)
        {
            prdTypCd = prodTypeCd;
            prdCd = prodCd;
        }

        override public string ListSql()
        {
            string sql = base.ListSql();
            sql = sql + " AND FG_PROD_TYPE_CD = " + parmPrdTypCd + " AND FG_PROD_CD LIKE " + parmPrdCd
                + " ORDER BY FG_PROD_CD";
            return sql;
        }

        override public void loadCmdParms(DbCommand cmd) 
        {
            cmd.AddParm(parmPrdTypCd, prdTypCd);
            cmd.AddParm(parmPrdCd, prdCd.Trim()+"%");
        }
    }

    #endregion
}
