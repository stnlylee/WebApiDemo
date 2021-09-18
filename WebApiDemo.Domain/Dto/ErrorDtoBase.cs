using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Dto
{
    [ExcludeFromCodeCoverage]
    public abstract class ErrorDtoBase
    {
        public string ErrorMessage { get; set; }
    }
}
