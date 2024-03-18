using CodeGenerationConsole;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationConsole
{
    public static class ControllerGenerate
    {

        public static void StartControllerGeneration(string tableName, string modelName)
        {
            string pkFieldTypeName = HelperFunctions.GeneratePrimaryKeyType(tableName);
            string pluralModelName = HelperFunctions.GetPluralTableName(modelName);
            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Dtos.{modelName};");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Data;");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Helpers;");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Interfaces;");
            sb.AppendLine($"using {HelperFunctions.nameSpaceName}.Mappers;");
            sb.AppendLine($"using Microsoft.EntityFrameworkCore;");
            sb.AppendLine($"using Microsoft.AspNetCore.Authorization;");
            sb.AppendLine($"using Microsoft.AspNetCore.Mvc;");
            sb.AppendLine();

            sb.AppendLine($"namespace {HelperFunctions.nameSpaceName}.Controllers;");
            sb.AppendLine();

            sb.AppendLine($"[Route(\"api/{modelName.ToLower()}\")]");
            sb.AppendLine($"[ApiController]");
            sb.AppendLine($"public class {modelName}Controller : ControllerBase");
            sb.AppendLine($"{{");

            sb.AppendLine($"    private readonly ApplicationDBContext _context;");
            sb.AppendLine($"    private readonly I{modelName}Repository _{modelName.ToLower()}Repo;");
            sb.AppendLine($"    public {modelName}Controller(ApplicationDBContext context, I{modelName}Repository {modelName}Repo)");
            sb.AppendLine($"    {{");
             sb.AppendLine($"        _{modelName.ToLower()}Repo = {modelName}Repo;");
             sb.AppendLine($"        _context = context;");
             sb.AppendLine($"    }}");
             sb.AppendLine($"");

            sb.AppendLine($"    [HttpGet]");
            sb.AppendLine($"    [Authorize(\"all\")]");
            sb.AppendLine($"    public async Task<IActionResult> GetAll()");
            sb.AppendLine($"    {{");
            sb.AppendLine($"        if (!ModelState.IsValid)");
            sb.AppendLine($"            return BadRequest(ModelState);");
            sb.AppendLine($"");
            sb.AppendLine($"        var {pluralModelName.ToLower()} = await _{modelName.ToLower()}Repo.GetAllAsync();");
            sb.AppendLine($"");
            sb.AppendLine($"        var {modelName}Dto = {pluralModelName.ToLower()}.Select(s => s.To{modelName}Dto()).ToList();");
            sb.AppendLine($"");
            sb.AppendLine($"        return Ok({modelName}Dto);");
            sb.AppendLine($"    }}");
            sb.AppendLine($"");


            sb.AppendLine($"    [HttpGet(\"pagewise\")]");
             sb.AppendLine($"    [Authorize]");
             sb.AppendLine($"    public async Task<IActionResult> GetAll([FromQuery] QueryObject query)");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        if (!ModelState.IsValid)");
             sb.AppendLine($"            return BadRequest(ModelState);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {pluralModelName.ToLower()} = await _{modelName.ToLower()}Repo.GetAllAsync(query);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {modelName}Dto = {pluralModelName.ToLower()}.Select(s => s.To{modelName}Dto()).ToList();");
             sb.AppendLine($"");
             sb.AppendLine($"        return Ok({modelName}Dto);");
             sb.AppendLine($"    }}");
             sb.AppendLine($"");
             sb.AppendLine($"    [HttpGet(\"{{id:{pkFieldTypeName}}}\")]");
             sb.AppendLine($"    public async Task<IActionResult> GetById([FromRoute] {pkFieldTypeName} id)");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        if (!ModelState.IsValid)");
             sb.AppendLine($"            return BadRequest(ModelState);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {modelName.ToLower()} = await _{modelName.ToLower()}Repo.GetByIdAsync(id);");
             sb.AppendLine($"");
             sb.AppendLine($"        if ({modelName.ToLower()} == null)");
             sb.AppendLine($"        {{");
             sb.AppendLine($"            return NotFound();");
             sb.AppendLine($"        }}");
             sb.AppendLine($"");
             sb.AppendLine($"        return Ok({modelName.ToLower()}.To{modelName}Dto());");
             sb.AppendLine($"    }}");
             sb.AppendLine($"");
             sb.AppendLine($"    [HttpPost]");
             sb.AppendLine($"    public async Task<IActionResult> Create([FromBody] Update{modelName}Dto {modelName}Dto)");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        if (!ModelState.IsValid)");
             sb.AppendLine($"            return BadRequest(ModelState);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {modelName.ToLower()}Model = {modelName}Dto.To{modelName}FromUpdateDto();");
             sb.AppendLine($"");
             sb.AppendLine($"        await _{modelName.ToLower()}Repo.CreateAsync({modelName.ToLower()}Model);");
             sb.AppendLine($"");
             sb.AppendLine($"        return CreatedAtAction(nameof(GetById), new {{ id = {modelName.ToLower()}Model.Id }}, {modelName.ToLower()}Model.To{modelName}Dto());");
             sb.AppendLine($"    }}");
             sb.AppendLine($"");
             sb.AppendLine($"    [HttpPut]");
             sb.AppendLine($"    [Route(\"{{id:{pkFieldTypeName}}}\")]");
             sb.AppendLine($"    public async Task<IActionResult> Update([FromRoute] {pkFieldTypeName} id, [FromBody] Update{modelName}Dto updateDto)");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        if (!ModelState.IsValid)");
             sb.AppendLine($"            return BadRequest(ModelState);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {modelName.ToLower()}Model = await _{modelName.ToLower()}Repo.UpdateAsync(id, updateDto);");
             sb.AppendLine($"");
             sb.AppendLine($"        if ({modelName.ToLower()}Model == null)");
             sb.AppendLine($"        {{");
             sb.AppendLine($"            return NotFound();");
             sb.AppendLine($"        }}");
             sb.AppendLine($"");
             sb.AppendLine($"        return Ok({modelName.ToLower()}Model.To{modelName}Dto());");
             sb.AppendLine($"    }}");
             sb.AppendLine($"");
             sb.AppendLine($"    [HttpDelete]");
             sb.AppendLine($"    [Route(\"{{id:{pkFieldTypeName}}}\")]");
             sb.AppendLine($"    public async Task<IActionResult> Delete([FromRoute] {pkFieldTypeName} id)");
             sb.AppendLine($"    {{");
             sb.AppendLine($"        if (!ModelState.IsValid)");
             sb.AppendLine($"            return BadRequest(ModelState);");
             sb.AppendLine($"");
             sb.AppendLine($"        var {modelName.ToLower()}Model = await _{modelName.ToLower()}Repo.DeleteAsync(id);");
             sb.AppendLine($"");
             sb.AppendLine($"        if ({modelName.ToLower()}Model == null)");
             sb.AppendLine($"        {{");
             sb.AppendLine($"            return NotFound();");
             sb.AppendLine($"        }}");
             sb.AppendLine($"");
             sb.AppendLine($"        return NoContent();");
             sb.AppendLine($"    }}");

            sb.AppendLine($"}}");//class closing

            // Save the model code to a .cs file
            string filePath = Path.Combine(HelperFunctions.controllerDirectory, $"{modelName}Controller.cs");
            File.WriteAllText(filePath, sb.ToString());
        }//generate

        static string ControllerUpdateColumns(string tableName, string modelName)
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
    }//class
}
