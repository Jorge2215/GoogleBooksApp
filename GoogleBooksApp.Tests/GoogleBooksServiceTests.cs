using System.Net;
using GoogleBooksApp.Configuration;
using GoogleBooksApp.Services.GoogleBooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GoogleBooksApp.Tests;

public sealed class GoogleBooksServiceTests
{
    [Fact]
    public async Task SearchAsync_WithTitleOnly_BuildsExpectedQuery()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"War and Peace"}}]}"""));
        var service = CreateService(handler);

        var result = await service.SearchAsync("War and Peace", null, 0, 10);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            "https://www.googleapis.com/books/v1/volumes?q=intitle%3A\"War and Peace\"&startIndex=0&maxResults=10&key=test-key",
            handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_WithAuthorOnly_BuildsExpectedQuery()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Kafka on the Shore"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync(null, "Haruki Murakami", 0, 10);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            "https://www.googleapis.com/books/v1/volumes?q=inauthor%3A\"Haruki Murakami\"&startIndex=0&maxResults=10&key=test-key",
            handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_WithTitleAndAuthor_BuildsExpectedQuery()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Norwegian Wood"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Norwegian Wood", "Haruki Murakami", 0, 10);

        Assert.Equal(1, handler.CallCount);
        Assert.Equal(
            "https://www.googleapis.com/books/v1/volumes?q=intitle%3A\"Norwegian Wood\"+inauthor%3A\"Haruki Murakami\"&startIndex=0&maxResults=10&key=test-key",
            handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_PassesPaginationParameters()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":25,"items":[{"id":"1","volumeInfo":{"title":"Paged Result"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Dune", null, 20, 5);

        Assert.Equal(1, handler.CallCount);
        Assert.Contains("&startIndex=20&maxResults=5&", handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_WhenApiReturnsZeroResults_ReturnsEmptyResultWithMessage()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":0,"items":[]}"""));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Unknown", null, 0, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal("No books matched your search.", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchAsync_WithoutSearchCriteria_ReturnsValidationMessageWithoutCallingHttp()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1"}]}"""));
        var service = CreateService(handler);

        var result = await service.SearchAsync("   ", null, 0, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal("Enter a title or author to search.", result.ErrorMessage);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_WhenHttpResponseIsNotSuccessful_ReturnsGracefulError()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadGateway));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Dune", null, 0, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal("Unable to load books right now. Please try again later.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("not-json")]
    [InlineData("")]
    public async Task SearchAsync_WithMalformedOrEmptyJson_ReturnsGracefulError(string responseBody)
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Dune", null, 0, 10);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
        Assert.Equal("Received an invalid response from the book service.", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchAsync_WithMissingOptionalFields_DeserializesWithoutThrowing()
    {
        const string responseBody = """
            {
              "totalItems": 1,
              "items": [
                {
                  "id": "book-1",
                  "volumeInfo": {
                    "title": "Minimal Book"
                  }
                }
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Minimal Book", null, 0, 10);

        var item = Assert.Single(result.Items);
        Assert.Equal(1, result.TotalItems);
        Assert.Null(result.ErrorMessage);
        Assert.Equal("book-1", item.Id);
        Assert.Equal("Minimal Book", item.VolumeInfo?.Title);
        Assert.Null(item.VolumeInfo?.Authors);
        Assert.Null(item.VolumeInfo?.Description);
        Assert.Null(item.VolumeInfo?.ImageLinks);
    }

    private static GoogleBooksService CreateService(StubHttpMessageHandler handler, string apiKey = "test-key")
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://www.googleapis.com/books/v1/")
        };

        return new GoogleBooksService(
            httpClient,
            Options.Create(new GoogleBooksOptions { ApiKey = apiKey }),
            NullLogger<GoogleBooksService>.Instance);
    }

    private static HttpResponseMessage JsonResponse(string content) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory = responseFactory;

        public int CallCount { get; private set; }

        public string? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestUri = request.RequestUri?.ToString();
            return Task.FromResult(_responseFactory(request));
        }
    }
}
