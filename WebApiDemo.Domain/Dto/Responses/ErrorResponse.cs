using System.Diagnostics.CodeAnalysis;

namespace WebApiDemo.Domain.Dto.Responses
{
    [ExcludeFromCodeCoverage]
    public class ErrorResponse : ErrorDtoBase
    {
        public ErrorResponse(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
