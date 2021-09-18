using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebApiDemo.Domain.Dto.Responses;

namespace WebApiDemo.Api.Controllers
{
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class HealthCheckController : ApiControllerBase
    {
        [HttpGet("/HealthCheck")]
        public async Task<ActionResult<HealthCheckResponse>> HealthCheck()
        {
            // it could use some logic here like check db connection etc
            return Ok(new HealthCheckResponse { IsHealthy = 1 });
        }
    }
}
