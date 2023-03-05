namespace SomeWebApi.Controllers;

using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using SomeWebApi.Contracts;
using SomeWebApi.Database;

[ApiController]
[Route("[controller]")]
public class ProcessController : ControllerBase
{
    private readonly ILogger<PersonController> logger;
    private readonly IMapper mapper;
    private readonly IRequestClient<IStartProcessCommand> startProcessCommandConsumer;
    private readonly SqlServerContext sqlServerContext;

    public ProcessController(ILogger<PersonController> logger, IMapper mapper, IRequestClient<IStartProcessCommand> startProcessCommandConsumer, SqlServerContext sqlServerContext)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.startProcessCommandConsumer = startProcessCommandConsumer;
        this.sqlServerContext = sqlServerContext;
    }

    [HttpPost(Name = "StartProcess")]
    public async Task<ActionResult> StartProcess([FromBody] int id)
    {
        var response = await startProcessCommandConsumer.GetResponse<IAcceptedResponse, IFaultedResponse>(new { ProcessName = "StartProcess", Id = id });

        if (response.Is(out Response<IAcceptedResponse>? accepted))
        {
            return Ok(new { response.ConversationId, accepted?.Message.Timestamp });
        }

        return BadRequest();
    }
}
