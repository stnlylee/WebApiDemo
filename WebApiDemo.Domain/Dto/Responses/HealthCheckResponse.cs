using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Dto.Responses
{
    [ExcludeFromCodeCoverage]
    public class HealthCheckResponse
    {
        public int IsHealthy { get; set; }
    }
}
