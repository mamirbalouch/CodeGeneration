using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using CodeGenerationConsole;
using Humanizer;

class Program
{

    static void Main(string[] args)
    {
        HelperFunctions.CreateAllDirectories();
        //get list of all tables
        DataConextGenerationClass.StartDataConextGeneration();
        HelperClassGeneration.StartHelperClassGeneration();

        DataTable tablesSchema = HelperFunctions.GetTablesList(HelperFunctions.connectionString);

        // Iterate through each table
        foreach (DataRow table in tablesSchema.Rows)
        {
            string tableName = table["TABLE_NAME"].ToString();
            string modelName = HelperFunctions.GetSingularTableName(tableName);

            if (tableName.ToLower().StartsWith("sysdiagram")) continue;

            string dtoDirectoryName = HelperFunctions.dtosDirectory + "\\" + modelName;
            HelperFunctions.CreateDirectory(dtoDirectoryName);

            GenerateModelCode(tableName, modelName);
            GenerateGetDtoCode(tableName, modelName, dtoDirectoryName);
            GenerateUpdateDtoCode(tableName, modelName, dtoDirectoryName);
            GenerateMapper(tableName, modelName);

            GenerateIRepository(tableName, modelName);
            GenerateRepository(tableName, modelName);

            ControllerGenerate.StartControllerGeneration(tableName,  modelName);
        }//foreach table in tables

        Console.WriteLine("Models generated successfully.");
    }



    #region GENERATE MODEL CODE

    static void GenerateModelCode(string tableName, string modelName)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
        sb.AppendLine($"using System.Collections.Generic;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Models;");
        sb.AppendLine();

        sb.AppendLine($"public class {modelName}");
        sb.AppendLine("{");

        sb.AppendLine(GenerateColumnNames(tableName));
        sb.AppendLine(GenerateForeignKeyFields(tableName));
        sb.AppendLine(GenerateListFields(tableName));

        // Close class definition
        sb.AppendLine("}");

        // Save the model code to a .cs file
        string filePath = Path.Combine(HelperFunctions.modelDirectory, $"{modelName}.cs");
        File.WriteAllText(filePath, sb.ToString());
    }
    static void GenerateGetDtoCode(string tableName, string modelName, string dtoDirWithModel)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
        sb.AppendLine($"using System.Collections.Generic;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
        sb.AppendLine();

        sb.AppendLine($"public class {modelName}Dto");
        sb.AppendLine("{");

        sb.AppendLine(GenerateColumnNames(tableName));

        // Close class definition
        sb.AppendLine("}");

        // Save the model code to a .cs file
        string filePath = Path.Combine(dtoDirWithModel, $"{modelName}Dto.cs");
        File.WriteAllText(filePath, sb.ToString());
    }
    static void GenerateUpdateDtoCode(string tableName, string modelName, string dtoDirWithModel)
    {

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
        sb.AppendLine($"using System.Collections.Generic;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
        sb.AppendLine();

        sb.AppendLine($"public class Update{modelName}Dto");
        sb.AppendLine("{");

        sb.AppendLine(GenerateColumnNames(tableName, false));

        // Close class definition
        sb.AppendLine("}");

        // Save the model code to a .cs file
        string filePath = Path.Combine(dtoDirWithModel, $"Update{modelName}Dto.cs");
        File.WriteAllText(filePath, sb.ToString());
    }

    static void GenerateMapper(string tableName, string modelName)
    {

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using System.ComponentModel.DataAnnotations;");
        sb.AppendLine($"using System.Collections.Generic;");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Models;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Mappers;");
        sb.AppendLine();

        sb.AppendLine($"public static class {modelName}Mappers");
        sb.AppendLine("{");

        //code to generate todto code for general dto
        sb.AppendLine($"    public static {modelName}Dto To{modelName}Dto (this {modelName} {modelName}Model)");
        sb.AppendLine( "    {");

        sb.AppendLine($"        return new {modelName}Dto");
        sb.AppendLine("        {");

        sb.AppendLine(GenerateMappersColumnNames(tableName, $"{modelName}Model", true));

        sb.AppendLine("         };");//return statement
        sb.AppendLine("     }");

        //code to generate todto code for update dto
        sb.AppendLine($"    public static {modelName} To{modelName}FromUpdateDto (this Update{modelName}Dto {modelName}Dto)");
        sb.AppendLine("     {");

        sb.AppendLine($"        return new {modelName}");
        sb.AppendLine("         {");

        sb.AppendLine(GenerateMappersColumnNames(tableName,  $"{modelName}Dto", false));

        sb.AppendLine("         };");//return statement
        sb.AppendLine("     }");


        // Close class definition
        sb.AppendLine("}");

        // Save the model code to a .cs file
        string filePath = Path.Combine(HelperFunctions.mapperDirectory, $"{modelName}Mappers.cs");
        File.WriteAllText(filePath, sb.ToString());
    }
    static void GenerateIRepository(string tableName, string modelName)
    {

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Models;");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Helpers;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Interfaces;");
        sb.AppendLine();

        sb.AppendLine($"public interface I{modelName}Repository");
        sb.AppendLine("{");

        string pkFieldTypeName = GeneratePrimaryKeyType(tableName);

        //code to generate todto code for general dto
        sb.AppendLine($"    Task<List<{modelName}>> GetAllAsync();");
        sb.AppendLine($"    Task<List<{modelName}>> GetAllAsync(QueryObject query);");
        sb.AppendLine($"    Task<{modelName}?> GetByIdAsync({pkFieldTypeName} id);");
        sb.AppendLine($"    Task<{modelName}> CreateAsync({modelName} {modelName}Model);");
        sb.AppendLine($"    Task<{modelName}?> UpdateAsync({pkFieldTypeName} id, Update{modelName}Dto {modelName}Dto);");
        sb.AppendLine($"    Task<{modelName}?> DeleteAsync({pkFieldTypeName} id);");
        sb.AppendLine($"    Task<bool> {modelName}Exists({pkFieldTypeName} id);");

        sb.AppendLine("}");


        // Save the model code to a .cs file
        string filePath = Path.Combine(HelperFunctions.interfaceDirectory, $"I{modelName}Repository.cs");
        File.WriteAllText(filePath, sb.ToString());
    }
    static void GenerateRepository(string tableName, string modelName)
    {

        string pkFieldTypeName = GeneratePrimaryKeyType(tableName);
        string pluralModelName = HelperFunctions.GetPluralTableName(modelName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Data;");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Helpers;");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Interfaces;");
        sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Models;");
        sb.AppendLine($"using Microsoft.EntityFrameworkCore;");
        sb.AppendLine();

        sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Repository;");
        sb.AppendLine();

        sb.AppendLine($"public class {modelName}Repository : I{modelName}Repository");
        sb.AppendLine("{");

        sb.AppendLine($"    private readonly ApplicationDBContext _context;");

        sb.AppendLine($"    public {modelName}Repository(ApplicationDBContext context)");
        sb.AppendLine( "    {");
        sb.AppendLine("        _context = context;");
        sb.AppendLine( "    }");

        //get all async
        sb.AppendLine($"    public async Task<List<{modelName}>> GetAllAsync()");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return await _context.{pluralModelName}.ToListAsync();");
        sb.AppendLine($"    }}");
        //get all async

        //get all async (query)
        sb.AppendLine($"    public async Task<List<{modelName}>> GetAllAsync(QueryObject query)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        var {pluralModelName} = _context.{pluralModelName};");
        sb.AppendLine($"        var skipNumber = (query.PageNumber - 1) * query.PageSize;");
        sb.AppendLine($"        return await {pluralModelName}.Skip(skipNumber).Take(query.PageSize).ToListAsync();");
        sb.AppendLine($"    }}");
        //get all async

        //get by id start
        sb.AppendLine($"    public async Task<{modelName}?> GetByIdAsync({pkFieldTypeName} id)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return await _context.{pluralModelName}.FirstOrDefaultAsync(i => i.Id == id);");
        sb.AppendLine($"    }}");
        //get by id end

        //create model start
        sb.AppendLine($"    public async Task<{modelName}> CreateAsync({modelName} {modelName}Model)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        await _context.{pluralModelName}.AddAsync({modelName}Model);");
        sb.AppendLine($"        await _context.SaveChangesAsync();");
        sb.AppendLine($"        return {modelName}Model;");
        sb.AppendLine($"    }}");        
        //create model end

        sb.AppendLine($"    public async Task<{modelName}?> UpdateAsync({pkFieldTypeName} id, Update{modelName}Dto {modelName}Dto)");
        sb.AppendLine($"        {{");
        sb.AppendLine($"            var existing{modelName} = await _context.{pluralModelName}.FirstOrDefaultAsync(x => x.Id == id);");
        sb.AppendLine($"");
        sb.AppendLine($"            if (existing{modelName} == null)");
        sb.AppendLine($"            {{");
        sb.AppendLine($"                return null;");
        sb.AppendLine($"            }}");
        sb.AppendLine($"");

        sb.AppendLine(RepositoryUpdateColumns(tableName, modelName));

        sb.AppendLine($"");
        sb.AppendLine($"            await _context.SaveChangesAsync();");
        sb.AppendLine($"");
        sb.AppendLine($"            return existing{modelName};");
        sb.AppendLine($"        }}");
        
        //delete async start
        sb.AppendLine($"    public async Task<{modelName}?> DeleteAsync({pkFieldTypeName} id)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        var {modelName}Model = await _context.{pluralModelName}.FirstOrDefaultAsync(x => x.Id == id);");
        sb.AppendLine($"        if ({modelName}Model == null)");
        sb.AppendLine($"            return null;");
        sb.AppendLine($"        _context.{pluralModelName}.Remove({modelName}Model);");
        sb.AppendLine($"        await _context.SaveChangesAsync();");
        sb.AppendLine($"        return {modelName}Model;");
        sb.AppendLine($"    }}");

        //delete async end
        sb.AppendLine($"    public async Task<bool> {modelName}Exists({pkFieldTypeName} id)");
        sb.AppendLine($"    {{");
        sb.AppendLine($"        return await _context.{pluralModelName}.AnyAsync(s => s.Id == id);");
        sb.AppendLine($"    }}");

        sb.AppendLine("}");//class closing


        // Save the model code to a .cs file
        string filePath = Path.Combine(HelperFunctions.repositoryDirectory, $"{modelName}Repository.cs");
        File.WriteAllText(filePath, sb.ToString());
    }

    static string RepositoryUpdateColumns(string tableName, string modelName)
    {
        // Get schema information for the columns in the table
        DataTable columnsSchema = HelperFunctions.GetCollumnsList(tableName);
        StringBuilder sb = new StringBuilder();
        // Generate properties for each column
        foreach (DataRow column in columnsSchema.Rows)
        {
            string columnName = column["COLUMN_NAME"].ToString();
            string dataType = column["DATA_TYPE"].ToString();
            string isNullable = column["IS_NULLABLE"].ToString();

            if (!HelperFunctions.IsFieldPrimaryKey(tableName, columnName))
            {
                sb.AppendLine($"            existing{modelName}.{columnName} = {modelName}Dto.{columnName};");

            }

        }

        return sb.ToString();
    }
    static string GeneratePrimaryKeyType(string tableName)
    {
        // Get schema information for the columns in the table
        DataTable columnsSchema = HelperFunctions.GetCollumnsList(tableName);
        StringBuilder sb = new StringBuilder();
        string pkFieldTypeName = "int";
        // Generate properties for each column
        foreach (DataRow column in columnsSchema.Rows)
        {
            string columnName = column["COLUMN_NAME"].ToString();
            string dataType = column["DATA_TYPE"].ToString();
            string isNullable = column["IS_NULLABLE"].ToString();

            if (HelperFunctions.IsFieldPrimaryKey(tableName, columnName))
            {
                pkFieldTypeName = HelperFunctions.GetCSharpType(dataType);
                return pkFieldTypeName;
            }

        }

        return pkFieldTypeName;
    }

    static string GenerateMappersColumnNames(string tableName,string paramObjectName, bool includePrimaryKey=true)
    {
        // Get schema information for the columns in the table
        DataTable columnsSchema = HelperFunctions.GetCollumnsList(tableName);
        StringBuilder sb = new StringBuilder();
        // Generate properties for each column
        foreach (DataRow column in columnsSchema.Rows)
        {
            string columnName = column["COLUMN_NAME"].ToString();
            string dataType = column["DATA_TYPE"].ToString();
            string isNullable = column["IS_NULLABLE"].ToString();

            if (!includePrimaryKey && HelperFunctions.IsFieldPrimaryKey(tableName, columnName)) continue;

            // Map database data types to C# data types
            string cSharpType = HelperFunctions.GetCSharpType(dataType);

            // Generate property definition
            sb.AppendLine($"            {columnName} = {paramObjectName}.{columnName},");
            sb.AppendLine();

        }

        return sb.ToString();
    }

    static string GenerateColumnNames(string tableName, bool includePrimaryKey = true)
    {
        // Get schema information for the columns in the table
        DataTable columnsSchema = HelperFunctions.GetCollumnsList(tableName);
        StringBuilder sb = new StringBuilder();

        // Generate properties for each column
        foreach (DataRow column in columnsSchema.Rows)
        {
            string columnName = column["COLUMN_NAME"].ToString();
            string dataType = column["DATA_TYPE"].ToString();
            string isNullable = column["IS_NULLABLE"].ToString();

            if (!includePrimaryKey && HelperFunctions.IsFieldPrimaryKey(tableName, columnName)) continue;

            // Map database data types to C# data types
            string cSharpType = HelperFunctions.GetCSharpType(dataType);

            if (!includePrimaryKey && isNullable.ToLower() == "no")
                sb.AppendLine($"    [Required]");

            // Generate property definition
            if(cSharpType.ToLower() == "string")
            {
                if (isNullable.ToLower() == "no")
                    sb.AppendLine($"    public {cSharpType} {columnName} {{ get; set; }} = \"\";");
                else
                    sb.AppendLine($"    public {cSharpType}? {columnName} {{ get; set; }}");
            }
            else if (cSharpType.ToLower()=="byte[]")
                sb.AppendLine($"    public {cSharpType}? {columnName} {{ get; set; }}");
            else
                sb.AppendLine($"    public {cSharpType} {columnName} {{ get; set; }}");

            sb.AppendLine();

        }
        return sb.ToString();
    }



    static string GenerateForeignKeyFields(string tableName)
    {
        DataTable foreignKeysSchema = HelperFunctions.GetForeignKeysList(tableName);
        StringBuilder sb = new StringBuilder();

        // Get foreign key constraints for the table
        foreach (DataRow foreignKey in foreignKeysSchema.Rows)
        {
            string foreignKeyName = foreignKey["ForeignKeyName"].ToString();
            string parentTable = HelperFunctions.GetSingularTableName(foreignKey["ParentTable"].ToString());
            string parentColumn = foreignKey["ParentColumn"].ToString();
            string referencedTable = HelperFunctions.GetSingularTableName(foreignKey["ReferencedTable"].ToString());
            string referencedColumn = foreignKey["ReferencedColumn"].ToString();

            sb.AppendLine($"    public {referencedTable}? {referencedTable} {{ get; set; }}");

        }

        return sb.ToString();
    }
    static string GenerateListFields(string tableName)
    {
        DataTable foreignKeysSchema = HelperFunctions.GetListFields(tableName);
        StringBuilder sb = new StringBuilder();

        // Get foreign key constraints for the table
        foreach (DataRow foreignKey in foreignKeysSchema.Rows)
        {
            string foreignKeyName = foreignKey["ForeignKeyName"].ToString();
            string parentTable = HelperFunctions.GetSingularTableName( foreignKey["ParentTable"].ToString());
            string parentColumn = foreignKey["ParentColumn"].ToString();
            string referencedTable = HelperFunctions.GetPluralTableName(foreignKey["ReferencedTable"].ToString());
            string referencedColumn = foreignKey["ReferencedColumn"].ToString();

            sb.AppendLine($"    public List<{parentTable}> {HelperFunctions.GetPluralTableName(parentTable)} {{ get; set; }}=[];");

        }

        return sb.ToString();
    }

    //get list of tables

    //get list of columns in a table

    //get list of foreign keys in a table
    #endregion GENERATE MODEL CODE

}
