using FluentValidation;
using System.Diagnostics.CodeAnalysis;
using WebApiDemo.Domain.Dto.Requests;

namespace WebApiDemo.Domain.Validators
{
    [ExcludeFromCodeCoverage]
    public class AddMovieRequestMovieValidator : AbstractValidator<AddMovieRequest>
    {
        public AddMovieRequestMovieValidator()
        {
            RuleFor(m => m.Title)
                .NotEmpty()
                .WithMessage("Title is required");

            RuleFor(m => m.Genre)
                .NotEmpty()
                .WithMessage("Genre is required");

            RuleFor(m => m.Classification)
                .NotEmpty()
                .WithMessage("Classification is required");

            RuleFor(m => m.ReleaseDate)
                .InclusiveBetween(1800, 9999)
                .WithMessage("Release Date must be a year");

            RuleFor(m => m.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be in a range of 1 - 5");
        }
    }

    public class UpdateMovieRequestMovieValidator : AbstractValidator<UpdateMovieRequest>
    {
        public UpdateMovieRequestMovieValidator()
        {
            RuleFor(m => m.MovieId)
                .GreaterThan(0)
                .WithMessage("Movie Id is not valid");

            RuleFor(m => m.Title)
                .NotEmpty()
                .WithMessage("Title is required");

            RuleFor(m => m.Genre)
                .NotEmpty()
                .WithMessage("Genre is required");

            RuleFor(m => m.Classification)
                .NotEmpty()
                .WithMessage("Classification is required");

            RuleFor(m => m.ReleaseDate)
                .InclusiveBetween(1800, 9999)
                .WithMessage("Release Date must be a year");

            RuleFor(m => m.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be in a range of 1 - 5");
        }
    }
}
