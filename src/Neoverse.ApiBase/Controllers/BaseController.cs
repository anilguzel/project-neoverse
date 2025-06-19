using Microsoft.AspNetCore.Mvc;
using Neoverse.SharedKernel.Entities;
using Neoverse.SharedKernel.Repositories;
using Neoverse.SharedKernel.Exceptions;

namespace Neoverse.ApiBase.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}
