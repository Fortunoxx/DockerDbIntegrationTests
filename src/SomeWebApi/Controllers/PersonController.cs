namespace SomeWebApi.Controllers;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SomeWebApi.Database;
using SomeWebApi.Model;

[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly ILogger<PersonController> logger;
    private readonly IMapper mapper;
    private readonly SqlServerContext sqlServerContext;

    public PersonController(ILogger<PersonController> logger, IMapper mapper, SqlServerContext sqlServerContext)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.sqlServerContext = sqlServerContext;
    }

    [HttpGet(Name = "GetAllPersons")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllPersons()
    {
        var query = await sqlServerContext.Users.ToListAsync();
        return query is not null ? new OkObjectResult(query) : NoContent();
    }

    [HttpGet("{id}", Name = "GetPerson")]
    public ActionResult<User> GetPerson(int id)
    {
        var query = sqlServerContext.Users.FirstOrDefault(x => x.Id == id);
        return query is not null ? new OkObjectResult(query) : NoContent();
    }

    [HttpPost(Name = "CreatePerson")]
    public async Task<ActionResult> CreatePerson([FromBody] UpsertUser person)
    {
        var user = mapper.Map<User>(person);
        sqlServerContext.Users.Add(user);
        await sqlServerContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPerson), new { id = user.Id }, user);
    }

    [HttpDelete("{id}", Name = "DeletePerson")]
    public ActionResult DeletePerson(int id)
    {
        var user = sqlServerContext.Find<User>(id);
        if (user is null)
        {
            return NotFound(id);
        }

        sqlServerContext.Remove(user);
        sqlServerContext.SaveChanges();
        return NoContent();
    }

    [HttpPut("{id}", Name = "UpdatePerson")]
    public ActionResult UpdatePerson([FromBody] UpsertUser user, int id)
    {
        var dbuser = sqlServerContext.Find<User>(id);
        if (dbuser is null)
        {
            return NotFound(id);
        }

        dbuser = mapper.Map(user, dbuser);
        sqlServerContext.Update(dbuser);
        sqlServerContext.SaveChanges();
        return NoContent();
    }
}