using Microsoft.AspNetCore.Mvc;

namespace AgenticBoard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}
