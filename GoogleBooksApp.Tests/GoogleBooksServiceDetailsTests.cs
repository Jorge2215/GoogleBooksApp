using System.Net;
using GoogleBooksApp.Configuration;
using GoogleBooksApp.Services.GoogleBooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GoogleBooksApp.Tests;

public sealed class GoogleBooksServiceDetailsTests
{
    #region GetByIdAsync - Success Cases

    [Fact]
    public async Task GetByIdAsync_WithValidVolumeId_ReturnsDeserializedBookResult()
    {
        const string responseBody = """
            {
              "id": "zyTCAlFPjgYC",
              "volumeInfo": {
                "title": "The Google story",
                "authors": ["David A. Vise", "Mark Malseed"],
                "description": "The definitive story of the creation of Google.",
                "imageLinks": {
                  "thumbnail": "http://books.google.com/books/content?id=zyTCAlFPjgYC&printsec=frontcover&img=1&zoom=1"
                },
                "publishedDate": "2005-11-15",
                "language": "en",
                "categories": ["Business & Economics", "Computers"],
                "pageCount": 207,
                "industryIdentifiers": [
                  {
                    "type": "ISBN_10",
                    "identifier": "055380457X"
                  },
                  {
                    "type": "ISBN_13",
                    "identifier": "9780553804577"
                  }
                ]
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("zyTCAlFPjgYC");

        Assert.NotNull(result);
        Assert.Equal("zyTCAlFPjgYC", result.Id);
        Assert.NotNull(result.VolumeInfo);
        Assert.Equal("The Google story", result.VolumeInfo.Title);
        Assert.Equal(2, result.VolumeInfo.Authors?.Count);
        Assert.Contains("David A. Vise", result.VolumeInfo.Authors);
        Assert.Contains("Mark Malseed", result.VolumeInfo.Authors);
        Assert.Equal("The definitive story of the creation of Google.", result.VolumeInfo.Description);
        Assert.Equal("2005-11-15", result.VolumeInfo.PublishedDate);
        Assert.Equal("en", result.VolumeInfo.Language);
        Assert.NotNull(result.VolumeInfo.ImageLinks);
        Assert.Equal("http://books.google.com/books/content?id=zyTCAlFPjgYC&printsec=frontcover&img=1&zoom=1", result.VolumeInfo.ImageLinks.Thumbnail);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidVolumeId_DeserializesCategories()
    {
        const string responseBody = """
            {
              "id": "book-1",
              "volumeInfo": {
                "title": "Test Book",
                "categories": ["Fiction", "Science Fiction", "Fantasy"]
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-1");

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeInfo?.Categories);
        Assert.Equal(3, result.VolumeInfo.Categories.Count);
        Assert.Contains("Fiction", result.VolumeInfo.Categories);
        Assert.Contains("Science Fiction", result.VolumeInfo.Categories);
        Assert.Contains("Fantasy", result.VolumeInfo.Categories);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidVolumeId_DeserializesPageCount()
    {
        const string responseBody = """
            {
              "id": "book-1",
              "volumeInfo": {
                "title": "Test Book",
                "pageCount": 352
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-1");

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeInfo);
        Assert.Equal(352, result.VolumeInfo.PageCount);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidVolumeId_DeserializesIndustryIdentifiers()
    {
        const string responseBody = """
            {
              "id": "book-1",
              "volumeInfo": {
                "title": "Test Book",
                "industryIdentifiers": [
                  {
                    "type": "ISBN_10",
                    "identifier": "0451524934"
                  },
                  {
                    "type": "ISBN_13",
                    "identifier": "9780451524935"
                  },
                  {
                    "type": "OTHER",
                    "identifier": "OCLC:12345678"
                  }
                ]
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-1");

        Assert.NotNull(result);
        Assert.NotNull(result.VolumeInfo?.IndustryIdentifiers);
        Assert.Equal(3, result.VolumeInfo.IndustryIdentifiers.Count);

        var isbn10 = result.VolumeInfo.IndustryIdentifiers.FirstOrDefault(id => id.Type == "ISBN_10");
        Assert.NotNull(isbn10);
        Assert.Equal("0451524934", isbn10.Identifier);

        var isbn13 = result.VolumeInfo.IndustryIdentifiers.FirstOrDefault(id => id.Type == "ISBN_13");
        Assert.NotNull(isbn13);
        Assert.Equal("9780451524935", isbn13.Identifier);

        var other = result.VolumeInfo.IndustryIdentifiers.FirstOrDefault(id => id.Type == "OTHER");
        Assert.NotNull(other);
        Assert.Equal("OCLC:12345678", other.Identifier);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidVolumeId_CallsCorrectEndpoint()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test-id","volumeInfo":{"title":"Test"}}"""));
        var service = CreateService(handler);

        await service.GetByIdAsync("zyTCAlFPjgYC");

        Assert.Equal(1, handler.CallCount);
        Assert.Equal("https://www.googleapis.com/books/v1/volumes/zyTCAlFPjgYC?key=test-key", handler.LastRequestUri);
    }

    [Fact]
    public async Task GetByIdAsync_WithMissingOptionalFields_DeserializesWithoutThrowing()
    {
        const string responseBody = """
            {
              "id": "minimal-book",
              "volumeInfo": {
                "title": "Minimal Book"
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("minimal-book");

        Assert.NotNull(result);
        Assert.Equal("minimal-book", result.Id);
        Assert.NotNull(result.VolumeInfo);
        Assert.Equal("Minimal Book", result.VolumeInfo.Title);
        Assert.Null(result.VolumeInfo.Authors);
        Assert.Null(result.VolumeInfo.Description);
        Assert.Null(result.VolumeInfo.ImageLinks);
        Assert.Null(result.VolumeInfo.Categories);
        Assert.Null(result.VolumeInfo.PageCount);
        Assert.Null(result.VolumeInfo.IndustryIdentifiers);
    }

    [Fact]
    public async Task GetByIdAsync_WithNullCategories_DeserializesCorrectly()
    {
        const string responseBody = """
            {
              "id": "book-1",
              "volumeInfo": {
                "title": "Book Without Categories",
                "pageCount": 100
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-1");

        Assert.NotNull(result);
        Assert.Null(result.VolumeInfo?.Categories);
        Assert.Equal(100, result.VolumeInfo?.PageCount);
    }

    [Fact]
    public async Task GetByIdAsync_WithNullPageCount_DeserializesCorrectly()
    {
        const string responseBody = """
            {
              "id": "book-1",
              "volumeInfo": {
                "title": "Book Without Page Count",
                "categories": ["Fiction"]
              }
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-1");

        Assert.NotNull(result);
        Assert.Null(result.VolumeInfo?.PageCount);
        Assert.NotNull(result.VolumeInfo?.Categories);
        Assert.Single(result.VolumeInfo.Categories);
    }

    #endregion

    #region GetByIdAsync - Invalid Input Cases

    [Fact]
    public async Task GetByIdAsync_WithNullVolumeId_ReturnsNullWithoutHttpCall()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test"}"""));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync(null!);

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyVolumeId_ReturnsNullWithoutHttpCall()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test"}"""));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync(string.Empty);

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(handler.LastRequestUri);
    }

    [Fact]
    public async Task GetByIdAsync_WithWhitespaceVolumeId_ReturnsNullWithoutHttpCall()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test"}"""));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("   ");

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
        Assert.Null(handler.LastRequestUri);
    }

    #endregion

    #region GetByIdAsync - Error Cases

    [Fact]
    public async Task GetByIdAsync_WhenApiReturns404_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("non-existent-book");

        Assert.Null(result);
        Assert.Equal(1, handler.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenApiReturns500_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenApiReturns403_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("not-valid-json")]
    [InlineData("")]
    [InlineData("{invalid")]
    [InlineData("null")]
    public async Task GetByIdAsync_WithMalformedJson_ReturnsNull(string malformedJson)
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(malformedJson));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenApiKeyIsMissing_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test","volumeInfo":{"title":"Test"}}"""));
        var service = CreateService(handler, apiKey: string.Empty);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenApiKeyIsWhitespace_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"id":"test","volumeInfo":{"title":"Test"}}"""));
        var service = CreateService(handler, apiKey: "   ");

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
        Assert.Equal(0, handler.CallCount);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHttpRequestFails_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => throw new HttpRequestException("Network error"));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRequestTimesOut_ReturnsNull()
    {
        var handler = new StubHttpMessageHandler(_ => throw new TaskCanceledException("Request timeout"));
        var service = CreateService(handler);

        var result = await service.GetByIdAsync("book-id");

        Assert.Null(result);
    }

    #endregion

    #region Helper Methods

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

    #endregion
}
