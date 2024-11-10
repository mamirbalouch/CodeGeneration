using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace CodeGenerationConsole
{

    public static class HelperFunctions
    {
        public static string connectionString = "Data Source=DESKTOP-CMTOCQ4\\MSSQL2022;Initial Catalog=CodeGen_PfmSems;Integrated Security=True;";
        public static string nameSpaceName = "Api";
        // Directory where you want to save the generated models
        public static string outputDirectory = "C:\\Alpha\\techsarena\\GenCode\\";
        public static string modelDirectory = outputDirectory + "Models";
        public static string interfaceDirectory = outputDirectory + "Interfaces";
        public static string repositoryDirectory = outputDirectory + "Repositories";
        public static string controllerDirectory = outputDirectory + "Controllers";
        public static string dtosDirectory = outputDirectory + "Dtos";
        public static string mapperDirectory = outputDirectory + "Mappers";
        public static string dataDirectory = outputDirectory + "Data";
        public static string helperDirectory = outputDirectory + "Helpers";

        public static void CreateAllDirectories()
        {
            CreateDirectory(outputDirectory);
            CreateDirectory(modelDirectory);
            CreateDirectory(repositoryDirectory);
            CreateDirectory(controllerDirectory);
            CreateDirectory(dtosDirectory);
            CreateDirectory(mapperDirectory);
            CreateDirectory(interfaceDirectory);
            CreateDirectory(dataDirectory);
            CreateDirectory(helperDirectory);

        }

        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static bool IsFieldPrimaryKey(string tableName, string columnName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Get primary key information for the specified table
                string query = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                AND TABLE_NAME = @TableName
                AND COLUMN_NAME = @ColumnName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TableName", tableName);
                    command.Parameters.AddWithValue("@ColumnName", columnName);

                    DataTable primaryKeyData = new DataTable();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(primaryKeyData);
                    }

                    if (primaryKeyData.Rows.Count >= 1)
                        return true;
                    else
                        return false;
                }
            }
        }

        public static string GeneratePrimaryKeyType(string tableName)
        {
            // Get schema information for the columns in the table
            DataTable columnsSchema = GetCollumnsList(tableName);
            StringBuilder sb = new StringBuilder();
            string pkFieldTypeName = "int";
            // Generate properties for each column
            foreach (DataRow column in columnsSchema.Rows)
            {
                string columnName = column["COLUMN_NAME"].ToString();
                string dataType = column["DATA_TYPE"].ToString();
                string isNullable = column["IS_NULLABLE"].ToString();

                if (IsFieldPrimaryKey(tableName, columnName))
                {
                    pkFieldTypeName = GetCSharpType(dataType);
                    return pkFieldTypeName;
                }

            }

            return pkFieldTypeName;
        }

        public static DataTable GetCollumnsList(string tableName)
        {
            DataTable columnsSchema = null;

            using (var connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Get schema information for all tables in the database
                columnsSchema = connection.GetSchema("Columns", new[] { null, null, tableName });
            }

            return columnsSchema;
        }
        public static DataTable GetForeignKeysList(string tableName)
        {
            DataTable foreignKeysSchema = new DataTable();
            string query = @"
            SELECT
                fk.name AS 'ForeignKeyName',
                OBJECT_NAME(fkc.parent_object_id) AS 'ParentTable',
                cpa.name AS 'ParentColumn',
                OBJECT_NAME(fkc.referenced_object_id) AS 'ReferencedTable',
                cref.name AS 'ReferencedColumn'
            FROM 
                sys.foreign_keys fk
            INNER JOIN 
                sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN 
                sys.columns cpa ON fkc.parent_object_id = cpa.object_id AND fkc.parent_column_id = cpa.column_id
            INNER JOIN 
                sys.columns cref ON fkc.referenced_object_id = cref.object_id AND fkc.referenced_column_id = cref.column_id
            WHERE
                OBJECT_NAME(fkc.parent_object_id) = @TableName";

            // Create a SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add the table name parameter to the command
                    command.Parameters.AddWithValue("@TableName", tableName);

                    // Open the connection
                    connection.Open();

                    // Execute the query and fetch the results into a DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(foreignKeysSchema);
                    }
                }
            }


            return foreignKeysSchema;
        }

        public static DataTable GetListFields(string tableName)
        {
            DataTable foreignKeysSchema = new DataTable();
            string query = @"
            SELECT
                fk.name AS 'ForeignKeyName',
                OBJECT_NAME(fkc.parent_object_id) AS 'ParentTable',
                cpa.name AS 'ParentColumn',
                OBJECT_NAME(fkc.referenced_object_id) AS 'ReferencedTable',
                cref.name AS 'ReferencedColumn'
            FROM 
                sys.foreign_keys fk
            INNER JOIN 
                sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            INNER JOIN 
                sys.columns cpa ON fkc.parent_object_id = cpa.object_id AND fkc.parent_column_id = cpa.column_id
            INNER JOIN 
                sys.columns cref ON fkc.referenced_object_id = cref.object_id AND fkc.referenced_column_id = cref.column_id
            WHERE
                OBJECT_NAME(fkc.referenced_object_id) = @TableName";

            // Create a SqlConnection
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Create a SqlCommand
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add the table name parameter to the command
                    command.Parameters.AddWithValue("@TableName", tableName);

                    // Open the connection
                    connection.Open();

                    // Execute the query and fetch the results into a DataTable
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(foreignKeysSchema);
                    }
                }
            }


            return foreignKeysSchema;
        }

        public static DataTable GetTablesList(string connectionString)
        {
            DataTable tableSchema = null;

            using (var connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                // Get schema information for all tables in the database
                tableSchema = connection.GetSchema("Tables");

            }

            return tableSchema;
        }
        public static string GetSingularTableName(string tableName)
        {
            //using Humanizer
            return tableName.Singularize(false);
        }

        public static string GetPluralTableName(string tableName)
        {
            //using Humanizer
            return tableName.Pluralize(false);
        }
        public static string GetCSharpType(string dbType)
        {
            // Map database data types to C# data types
            switch (dbType.ToLower())
            {
                case "int":
                    return "int";
                case "bigint":
                    return "long";
                case "smallint":
                    return "short";
                case "tinyint":
                    return "byte";
                case "float":
                    return "float";
                case "real":
                    return "double";
                case "decimal":
                case "numeric":
                    return "decimal";
                case "money":
                case "smallmoney":
                    return "decimal";
                case "bit":
                    return "bool";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "datetimeoffset":
                    return "DateTime";
                case "time":
                    return "TimeSpan";
                case "char":
                case "varchar":
                case "text":
                case "nchar":
                case "nvarchar":
                case "ntext":
                case "xml":
                    return "string";
                case "binary":
                case "varbinary":
                case "image":
                    return "byte[]";
                case "uniqueidentifier":
                    return "Guid";
                default:
                    return "object";
            }
        }

    }//class
}
