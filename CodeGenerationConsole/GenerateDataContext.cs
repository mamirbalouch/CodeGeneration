using CodeGenerationConsole;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationConsole
{
    public static class DataConextGenerationClass
    {

        public static void StartDataConextGeneration()
        {
            DataTable tablesSchema = HelperFunctions.GetTablesList(HelperFunctions.connectionString);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Models;");
            sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Data");
            sb.AppendLine($"{{");
            sb.AppendLine($"    public partial class ApplicationDBContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)");
            sb.AppendLine($"    {{");
            sb.AppendLine($"");


            // Iterate through each table
            foreach (DataRow table in tablesSchema.Rows)
            {
                string tableName = table["TABLE_NAME"].ToString();
                string modelName = HelperFunctions.GetSingularTableName(tableName);
                string pluralModelName = HelperFunctions.GetPluralTableName(modelName);

                if (tableName.ToLower().StartsWith("sysdiagram")) continue;
                sb.AppendLine($"        public DbSet<{modelName}> {pluralModelName} {{get; set; }}");
            }
            sb.AppendLine();

            sb.AppendLine($"        protected override void OnModelCreating(ModelBuilder builder)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            base.OnModelCreating(builder);");

            foreach (DataRow table in tablesSchema.Rows)
            {
                string tableName = table["TABLE_NAME"].ToString();
                string modelName = HelperFunctions.GetSingularTableName(tableName);
                string pluralModelName = HelperFunctions.GetPluralTableName(modelName);
               
                if (tableName.ToLower().StartsWith("sysdiagram")) continue;

                DataTable foreignKeysSchema = HelperFunctions.GetForeignKeysList(tableName);

                if (foreignKeysSchema.Rows.Count <= 0) continue;

                sb.AppendLine($"");
                sb.AppendLine($"            //--------{modelName} Foreign Keys--------");

                foreach (DataRow foreignKey in foreignKeysSchema.Rows)
                {
                    string foreignKeyName = foreignKey["ForeignKeyName"].ToString();
                    string parentTable = HelperFunctions.GetSingularTableName(foreignKey["ParentTable"].ToString());
                    string parentColumn = foreignKey["ParentColumn"].ToString();
                    string referencedTable = HelperFunctions.GetSingularTableName(foreignKey["ReferencedTable"].ToString());
                    string referencedColumn = foreignKey["ReferencedColumn"].ToString();
                    sb.AppendLine($"");
                    sb.AppendLine($"            builder.Entity<{modelName}>()");
                    sb.AppendLine($"                .HasOne(u => u.{referencedTable})");
                    sb.AppendLine($"                .WithMany(u => u.{pluralModelName})");
                    sb.AppendLine($"                .HasForeignKey(p => p.{parentColumn}).OnDelete(DeleteBehavior.NoAction);");
                }//foreach foreign key
            }//for each table row
            sb.AppendLine($"        }}");//end of on model creating

            sb.AppendLine($"    }}");//end of appcontext class
            sb.AppendLine($"}}");//end of namespace
            string filePath = Path.Combine(HelperFunctions.dataDirectory, $"ApplicationDbContext.cs");
            File.WriteAllText(filePath, sb.ToString());

            //partial class extenson for dbContext
            filePath = Path.Combine(HelperFunctions.dataDirectory, $"ApplicationDbContext.Extensions.cs");
            if (File.Exists(filePath)) return;//if there is a partial class, don't change it
            sb.Clear();
            sb.AppendLine($"using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Models;");
            sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Data;");
            sb.AppendLine($"");
            sb.AppendLine($"public partial class ApplicationDBContext : DbContext");
            sb.AppendLine($"{{");
            sb.AppendLine($"}}");
            File.WriteAllText(filePath, sb.ToString());

        }//generate


    }//class
}
