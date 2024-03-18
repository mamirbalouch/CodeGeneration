<p>Whenever we require to develop a new application, the biggest concern is to write boiler plate (repetitive code). Consider if you have 50 tables in your database and you choose to develop your application with Clean Code Architecture. I have searched the web and cannot find a solution that can help me. So I have written an application to generate Models, Dtos, IRepositories, Repositories, Controllers and even DB Context. 
</p>
If you already have a database, you just need to provide three parameters in <b>HelperFunctions.cs</b> class as described under:
<P>
<code>
  public static string connectionString = "----YOUR CONNECTION STRING GOES HERES----";
  public static string nameSpaceName = "YOURPROJECT.Api";
  public static string outputDirectory = "C:\\techsarena\\GenCode\\";
</code>
</P>
<P>
This code will do the magic and create following folders:
</P>
<p>
<code>
<b>outputDirectory</b>
|--Controlelrs
|--Data
|--Dtos
|--Helpers
|--Interfaces
|--Mappers
|--Models
|--Repositories
</code>
</p>
<p>
Now, lets say you are using code first approach. Still, when you run your migration and update database, you have your database. You might want to generate IRepositories, Repositoreis, Controllers, Dtos for your project. Just run the code and get all your required classes to an "outputDirectory" and copy only required folders / classes to your project to continue. 
</p>
<p>
I am aware, this code is kind of a messy code, but I just needed these classes for my project and was not willing to write this boiler plate code. Therefore, I wrote a routine for myself and now that I am satisfied with the results, I wanted to share it with the community with two things in my mind:
  <ul>
  <li>This code can help developers to focus on realy functionality of the project and save their time</li>
    <li>The community can help me develop a much better product</li>
  </ul>
</p>
<h2>Sample Controller Code (Generated)</h2>
<p>
  <pre>
using SemsCodeGenTest.Api.Dtos.Branch;
using SemsCodeGenTest.Api.Data;
using SemsCodeGenTest.Api.Helpers;
using SemsCodeGenTest.Api.Interfaces;
using SemsCodeGenTest.Api.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SemsCodeGenTest.Api.Controllers;

[Route("api/Branch")]
[ApiController]
public class BranchController : ControllerBase
{
    private readonly ApplicationDBContext _context;
    private readonly IBranchRepository _branchRepo;
    public BranchController(ApplicationDBContext context, IBranchRepository BranchRepo)
    {
        _branchRepo = BranchRepo;
        _context = context;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branches = await _branchRepo.GetAllAsync();

        var BranchDto = branches.Select(s => s.ToBranchDto()).ToList();

        return Ok(BranchDto);
    }

    [Route("pagewise")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branches = await _branchRepo.GetAllAsync(query);

        var BranchDto = branches.Select(s => s.ToBranchDto()).ToList();

        return Ok(BranchDto);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branch = await _branchRepo.GetByIdAsync(id);

        if (branch == null)
        {
            return NotFound();
        }

        return Ok(branch.ToBranchDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpdateBranchDto BranchDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branchModel = BranchDto.ToBranchFromUpdateDto();

        await _branchRepo.CreateAsync(branchModel);

        return CreatedAtAction(nameof(GetById), new { id = branchModel.Id }, branchModel.ToBranchDto());
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateBranchDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branchModel = await _branchRepo.UpdateAsync(id, updateDto);

        if (branchModel == null)
        {
            return NotFound();
        }

        return Ok(branchModel.ToBranchDto());
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var branchModel = await _branchRepo.DeleteAsync(id);

        if (branchModel == null)
        {
            return NotFound();
        }

        return NoContent();
    }
}
    
  </pre>
</p>
</p>
<h2>Sample Repository Code (Generated)</h2>
<p>
  <pre>
using SemsCodeGenTest.Api.Dtos.Branch;
using SemsCodeGenTest.Api.Data;
using SemsCodeGenTest.Api.Helpers;
using SemsCodeGenTest.Api.Interfaces;
using SemsCodeGenTest.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SemsCodeGenTest.Api.Repository;

public class BranchRepository : IBranchRepository
{
    private readonly ApplicationDBContext _context;
    public BranchRepository(ApplicationDBContext context)
    {
        _context = context;
    }
    public async Task<List<Branch>> GetAllAsync()
    {
        return await _context.Branches.ToListAsync();
    }
    public async Task<List<Branch>> GetAllAsync(QueryObject query)
    {
        var Branches = _context.Branches;
        var skipNumber = (query.PageNumber - 1) * query.PageSize;
        return await Branches.Skip(skipNumber).Take(query.PageSize).ToListAsync();
    }
    public async Task<Branch?> GetByIdAsync(int id)
    {
        return await _context.Branches.FirstOrDefaultAsync(i => i.Id == id);
    }
    public async Task<Branch> CreateAsync(Branch BranchModel)
    {
        await _context.Branches.AddAsync(BranchModel);
        await _context.SaveChangesAsync();
        return BranchModel;
    }
    public async Task<Branch?> UpdateAsync(int id, UpdateBranchDto BranchDto)
        {
            var existingBranch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == id);

            if (existingBranch == null)
            {
                return null;
            }

            existingBranch.Name = BranchDto.Name;
            existingBranch.OrganizationId = BranchDto.OrganizationId;
            existingBranch.Description = BranchDto.Description;
            existingBranch.IsActive = BranchDto.IsActive;


            await _context.SaveChangesAsync();

            return existingBranch;
        }
    public async Task<Branch?> DeleteAsync(int id)
    {
        var BranchModel = await _context.Branches.FirstOrDefaultAsync(x => x.Id == id);
        if (BranchModel == null)
            return null;
        _context.Branches.Remove(BranchModel);
        await _context.SaveChangesAsync();
        return BranchModel;
    }
    public async Task<bool> BranchExists(int id)
    {
        return await _context.Branches.AnyAsync(s => s.Id == id);
    }
}
    
  </pre>
  </p>
