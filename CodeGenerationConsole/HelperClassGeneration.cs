using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationConsole
{
    public static class HelperClassGeneration
    {
        public static void StartHelperClassGeneration(DataTable tablesSchema)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"using System;");
            sb.AppendLine($"using System.Collections.Generic;");
            sb.AppendLine($"using System.Linq;");
            sb.AppendLine($"using System.Threading.Tasks;");
            sb.AppendLine($"");
            sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Helpers");
            sb.AppendLine($"{{");
             sb.AppendLine($"    public class QueryObject");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        //public string? SortBy {{ get; set; }} = null;");
             sb.AppendLine($"        //public bool IsDecsending {{ get; set; }} = false;");
             sb.AppendLine($"        public int PageNumber {{ get; set; }} = 1;");
             sb.AppendLine($"        public int PageSize {{ get; set; }} = 20;");
             sb.AppendLine($"    }}");
             sb.AppendLine($"}}");

            //add the following code in program.cs to inject repositories
            sb.AppendLine($"//add the following code in program.cs to inject repositories");
            foreach (DataRow table in tablesSchema.Rows)
            {
                string tableName = table["TABLE_NAME"].ToString();
                string modelName = HelperFunctions.GetSingularTableName(tableName);
                string pluralModelName = HelperFunctions.GetPluralTableName(modelName);
                if (tableName.ToLower().StartsWith("sysdiagram")) continue;

                if (tableName.ToLower().StartsWith("sysdiagram")) continue;
                sb.AppendLine($"        //builder.Services.AddScoped<I{modelName}Repository, {modelName}Repository>();");
            }
            sb.AppendLine();

            string filePath = Path.Combine(HelperFunctions.helperDirectory, $"QueryObject.cs");
            File.WriteAllText(filePath, sb.ToString());

        }

    }
}
