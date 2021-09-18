using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public abstract class ApiControllerBase : Controller
    {
        protected IConfiguration _config;
    }
}
