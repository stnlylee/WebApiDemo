using WebApiDemo.Domain.Search;
using Xunit;

namespace WebApiDemo.UnitTests.SearchTests
{

    public class SearchOptionTests
    {
        private readonly SearchOption _searchOption;

        public SearchOptionTests()
        {
            _searchOption = new SearchOption();
        }

        [Theory]
        [InlineData("invalid-number", int.MinValue)]
        [InlineData("", int.MinValue)]
        [InlineData(null, int.MinValue)]
        [InlineData("-33", int.MinValue)]
        [InlineData("0", int.MinValue)]
        public void SearchOption_Should_Validate_Invalid_NumberFields(string input, int expected)
        {
            // Arrange
            _searchOption.MovieId = input;
            _searchOption.MinReleaseDate = input;
            _searchOption.MaxReleaseDate = input;
            _searchOption.MinRating = input;
            _searchOption.MaxRating = input;

            // Action
            // no action needed

            // Assert
            Assert.Equal(expected, _searchOption.MovieIdInt);
            Assert.Equal(expected, _searchOption.MinReleaseDateInt);
            Assert.Equal(expected, _searchOption.MaxReleaseDateInt);
            Assert.Equal(expected, _searchOption.MinRatingInt);
            Assert.Equal(expected, _searchOption.MaxRatingInt);
        }

        [Theory]
        [InlineData("54", 54)]
        [InlineData("3443", 3443)]
        public void SearchOption_Should_Validate_Valid_NumberFields(string input, int expected)
        {
            // Arrange
            _searchOption.MovieId = input;
            _searchOption.MinReleaseDate = input;
            _searchOption.MaxReleaseDate = input;
            _searchOption.MinRating = input;
            _searchOption.MaxRating = input;

            // Action
            // no action needed

            // Assert
            Assert.Equal(expected, _searchOption.MovieIdInt);
            Assert.Equal(expected, _searchOption.MinReleaseDateInt);
            Assert.Equal(expected, _searchOption.MaxReleaseDateInt);
            Assert.Equal(expected, _searchOption.MinRatingInt);
            Assert.Equal(expected, _searchOption.MaxRatingInt);
        }

        [Fact]
        public void SearchOption_Should_Be_Valid()
        {
            // Arrange
            SetupValidSearchOption();

            // Action
            // no action needed

            // Assert
            Assert.True(_searchOption.Valid);
        }

        [Theory]
        [InlineData("2019", "2")]
        [InlineData("2012", "5")]
        public void SearchOption_Should_Be_Invalid_When_MinField_GreatThan_MaxField(string minReleaseDate,
            string minRating)
        {
            // Arrange
            SetupValidSearchOption();
            _searchOption.MinReleaseDate = minReleaseDate;
            _searchOption.MinRating = minRating;

            // Action
            // no action needed

            // Assert
            Assert.False(_searchOption.Valid);
        }

        [Fact]
        public void SearchOption_Should_Be_Invalid_When_Rating_OutRange()
        {
            // Arrange
            SetupValidSearchOption();
            _searchOption.MinRating = "6";

            // Action
            // no action needed

            // Assert
            Assert.False(_searchOption.Valid);
        }

        private void SetupValidSearchOption()
        {
            _searchOption.MovieId = "12";
            _searchOption.Title = "some title";
            _searchOption.Genre = "some genre";
            _searchOption.Classification = "some classification";
            _searchOption.MinReleaseDate = "2012";
            _searchOption.MaxReleaseDate = "2018";
            _searchOption.MinRating = "2";
            _searchOption.MaxRating = "4";
            _searchOption.Cast = "some cast";
        }
    }
}
