using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Api.ErrorHandling
{
    [ExcludeFromCodeCoverage]
    internal class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
