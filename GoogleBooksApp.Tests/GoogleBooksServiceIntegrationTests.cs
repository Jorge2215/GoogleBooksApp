using GoogleBooksApp.Configuration;
using GoogleBooksApp.Services.GoogleBooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GoogleBooksApp.Tests;

[Collection(GoogleBooksApiIntegrationCollection.Name)]
[Trait("Category", "Integration")]
public sealed class GoogleBooksServiceIntegrationTests
{
    [RequiresGoogleBooksApiKeyFact]
    public async Task SearchAsync_WithRealApi_ReturnsResultsWithExpectedShape()
    {
        var service = CreateService();

        var result = await service.SearchAsync("Dune", null, 0, 5);

        if (result.ErrorMessage is not null)
        {
            AssertGracefulTransientError(result.ErrorMessage);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalItems);
            return;
        }

        Assert.Null(result.ErrorMessage);
        Assert.True(result.TotalItems > 0, "Expected the real Google Books API to report at least one result.");
        Assert.NotEmpty(result.Items);
        var item = result.Items.First();
        Assert.False(string.IsNullOrWhiteSpace(item.Id));
        Assert.False(string.IsNullOrWhiteSpace(item.VolumeInfo?.Title));
    }

    [RequiresGoogleBooksApiKeyFact]
    public async Task SearchAsync_WithNonsenseQuery_ReturnsEmptyResultWithoutThrowing()
    {
        var service = CreateService();
        var nonsenseQuery = $"unlikely-book-query-{Guid.NewGuid():N}";

        var result = await service.SearchAsync(nonsenseQuery, null, 0, 5);

        if (result.ErrorMessage is not null && result.ErrorMessage != "No books matched your search.")
        {
            AssertGracefulTransientError(result.ErrorMessage);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalItems);
            return;
        }

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal("No books matched your search.", result.ErrorMessage);
    }

    [RequiresGoogleBooksApiKeyFact]
    public async Task SearchAsync_WithRealApiAndVeryShortTimeout_ReturnsGracefulTimeout()
    {
        var service = CreateService(TimeSpan.FromMilliseconds(1));

        var result = await service.SearchAsync("Dune", null, 0, 5);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        var errorMessage = Assert.IsType<string>(result.ErrorMessage);
        Assert.Contains(
            errorMessage,
            new[]
            {
                "The book service took too long to respond. Please try again later.",
                "Unable to reach the book service right now. Please try again later."
            });
    }

    private static GoogleBooksService CreateService(TimeSpan? timeout = null)
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://www.googleapis.com/books/v1/"),
            Timeout = timeout ?? TimeSpan.FromSeconds(10)
        };

        return new GoogleBooksService(
            httpClient,
            Options.Create(new GoogleBooksOptions { ApiKey = GoogleBooksApiIntegrationConfiguration.ApiKey! }),
            NullLogger<GoogleBooksService>.Instance);
    }

    private static void AssertGracefulTransientError(string errorMessage) =>
        Assert.Contains(
            errorMessage,
            new[]
            {
                "Unable to load books right now. Please try again later.",
                "Unable to reach the book service right now. Please try again later.",
                "The book service took too long to respond. Please try again later."
            });

    private sealed class RequiresGoogleBooksApiKeyFactAttribute : FactAttribute
    {
        public RequiresGoogleBooksApiKeyFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(GoogleBooksApiIntegrationConfiguration.ApiKey))
            {
                Skip =
                    "Integration test skipped. Set GoogleBooks__ApiKey (or GOOGLEBOOKS_API_KEY / GoogleBooks:ApiKey) to run real Google Books API tests.";
            }
        }
    }
}

internal static class GoogleBooksApiIntegrationConfiguration
{
    public static string? ApiKey =>
        Environment.GetEnvironmentVariable("GoogleBooks__ApiKey")
        ?? Environment.GetEnvironmentVariable("GOOGLEBOOKS_API_KEY")
        ?? Environment.GetEnvironmentVariable("GoogleBooks:ApiKey");
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GoogleBooksApiIntegrationCollection
{
    public const string Name = "Google Books API integration";
}
