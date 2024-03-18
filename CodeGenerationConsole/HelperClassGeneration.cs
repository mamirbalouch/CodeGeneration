using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationConsole
{
    public static class HelperClassGeneration
    {
        public static void StartHelperClassGeneration()
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

            string filePath = Path.Combine(HelperFunctions.helperDirectory, $"QueryObject.cs");
            File.WriteAllText(filePath, sb.ToString());

        }

    }
}
