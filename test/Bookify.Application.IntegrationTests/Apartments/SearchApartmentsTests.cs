using Bookify.Application.Apartments.SearchApartments;
using Bookify.Application.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Bookify.Application.IntegrationTests.Apartments;

public class SearchApartmentsTests : BaseIntegrationTest
{
    public SearchApartmentsTests(IntegrationTestWebAppFactory factory) : base(factory)
    { 
    }

    [Fact]
    public async Task SearchApartments_ShouldReturnEmptyList_WhenDataRangeIsInvalid()
    {
        // arrange
        var query = new SearchApartmentsQuery(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 1));

        // act
        var result = await Sender.Send(query);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
    
    [Fact]
    public async Task SearchApartments_ShouldReturnApartments_WhenDataRangeIsValid()
    {
        // arrange
        var query = new SearchApartmentsQuery(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));

        // act
        var result = await Sender.Send(query);

        // assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}