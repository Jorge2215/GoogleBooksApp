using System.Net;
using GoogleBooksApp.Configuration;
using GoogleBooksApp.Services.GoogleBooks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GoogleBooksApp.Tests;

public sealed class GoogleBooksServiceFilterTests
{
    #region Language Filter Tests

    [Fact]
    public async Task SearchAsync_WithLanguageFilter_AppendsLangRestrictParameter()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Madame Bovary","language":"fr"}}]}"""));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Madame Bovary", null, 0, 10, language: "fr");

        Assert.Null(result.ErrorMessage);
        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Items);
        Assert.Contains("&langRestrict=fr", handler.LastRequestUri);
    }

    [Fact]
    public async Task SearchAsync_WithoutLanguageFilter_OmitsLangRestrictParameter()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"War and Peace"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("War and Peace", null, 0, 10, language: null);

        Assert.DoesNotContain("langRestrict", handler.LastRequestUri ?? string.Empty);
    }

    [Theory]
    [InlineData("EN", "en")]
    [InlineData("Fr", "fr")]
    [InlineData("ES", "es")]
    public async Task SearchAsync_WithLanguageFilter_NormalizesToLowercase(string inputLanguage, string expectedLanguage)
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Test Book"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Test", null, 0, 10, language: inputLanguage);

        Assert.Contains($"&langRestrict={expectedLanguage}", handler.LastRequestUri ?? string.Empty);
    }

    #endregion

    #region Sort Order Tests

    [Fact]
    public async Task SearchAsync_WithSortOrderNewest_AppendsOrderByNewest()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"New Book","publishedDate":"2024"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("New Book", null, 0, 10, sortOrder: "newest");

        Assert.Contains("&orderBy=newest", handler.LastRequestUri ?? string.Empty);
    }

    [Fact]
    public async Task SearchAsync_WithSortOrderRelevance_OmitsOrderByParameter()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Relevant Book"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Relevant Book", null, 0, 10, sortOrder: "relevance");

        Assert.DoesNotContain("orderBy", handler.LastRequestUri ?? string.Empty);
    }

    [Fact]
    public async Task SearchAsync_WithNullSortOrder_OmitsOrderByParameter()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Default Book"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Default Book", null, 0, 10, sortOrder: null);

        Assert.DoesNotContain("orderBy", handler.LastRequestUri ?? string.Empty);
    }

    #endregion

    #region Year Range Filter Tests (Client-Side)

    [Fact]
    public async Task SearchAsync_WithYearFromFilter_FiltersResultsClientSide()
    {
        const string responseBody = """
            {
              "totalItems": 4,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book from 2019","publishedDate":"2019"}},
                {"id":"2","volumeInfo":{"title":"Book from 2020","publishedDate":"2020-05"}},
                {"id":"3","volumeInfo":{"title":"Book from 2021","publishedDate":"2021-12-31"}},
                {"id":"4","volumeInfo":{"title":"Book without date","publishedDate":null}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: 2020);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Id == "2");
        Assert.Contains(result.Items, b => b.Id == "3");
        Assert.DoesNotContain(result.Items, b => b.Id == "1");
        Assert.DoesNotContain(result.Items, b => b.Id == "4");
    }

    [Fact]
    public async Task SearchAsync_WithYearToFilter_FiltersResultsClientSide()
    {
        const string responseBody = """
            {
              "totalItems": 4,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book from 2019","publishedDate":"2019"}},
                {"id":"2","volumeInfo":{"title":"Book from 2020","publishedDate":"2020-05"}},
                {"id":"3","volumeInfo":{"title":"Book from 2021","publishedDate":"2021-12-31"}},
                {"id":"4","volumeInfo":{"title":"Book without date","publishedDate":null}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearTo: 2020);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Id == "1");
        Assert.Contains(result.Items, b => b.Id == "2");
        Assert.DoesNotContain(result.Items, b => b.Id == "3");
        Assert.DoesNotContain(result.Items, b => b.Id == "4");
    }

    [Fact]
    public async Task SearchAsync_WithYearFromAndYearToFilter_FiltersResultsClientSide()
    {
        const string responseBody = """
            {
              "totalItems": 5,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book from 2019","publishedDate":"2019"}},
                {"id":"2","volumeInfo":{"title":"Book from 2020","publishedDate":"2020-05"}},
                {"id":"3","volumeInfo":{"title":"Book from 2021","publishedDate":"2021-12-31"}},
                {"id":"4","volumeInfo":{"title":"Book from 2022","publishedDate":"2022"}},
                {"id":"5","volumeInfo":{"title":"Book without date","publishedDate":null}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: 2020, yearTo: 2021);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Id == "2");
        Assert.Contains(result.Items, b => b.Id == "3");
        Assert.DoesNotContain(result.Items, b => b.Id == "1");
        Assert.DoesNotContain(result.Items, b => b.Id == "4");
        Assert.DoesNotContain(result.Items, b => b.Id == "5");
    }

    [Fact]
    public async Task SearchAsync_WithYearFilter_ExcludesNullAndMalformedDates()
    {
        const string responseBody = """
            {
              "totalItems": 5,
              "items": [
                {"id":"1","volumeInfo":{"title":"Valid 2020","publishedDate":"2020"}},
                {"id":"2","volumeInfo":{"title":"Null date","publishedDate":null}},
                {"id":"3","volumeInfo":{"title":"Empty string","publishedDate":""}},
                {"id":"4","volumeInfo":{"title":"Invalid format","publishedDate":"unknown"}},
                {"id":"5","volumeInfo":{"title":"Another valid 2020","publishedDate":"2020-06-15"}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: 2020);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, b => b.Id == "1");
        Assert.Contains(result.Items, b => b.Id == "5");
    }

    [Fact]
    public async Task SearchAsync_WithYearFromGreaterThanYearTo_ReturnsEmptyResults()
    {
        // ASSUMPTION: When yearFrom > yearTo, we return empty results rather than swapping them.
        // This is the simplest behavior and alerts users to input errors.
        const string responseBody = """
            {
              "totalItems": 2,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book from 2020","publishedDate":"2020"}},
                {"id":"2","volumeInfo":{"title":"Book from 2021","publishedDate":"2021"}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: 2021, yearTo: 2020);

        Assert.Empty(result.Items);
    }

    [Theory]
    [InlineData("2020", 2020)]
    [InlineData("2020-05", 2020)]
    [InlineData("2020-05-12", 2020)]
    public async Task SearchAsync_WithVariousDateFormats_ParsesYearCorrectly(string publishedDate, int expectedYear)
    {
        var responseBody = @$"{{
              ""totalItems"": 1,
              ""items"": [
                {{""id"":""1"",""volumeInfo"":{{""title"":""Test Book"",""publishedDate"":""{publishedDate}""}}}}
              ]
            }}";

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Test", null, 0, 10, yearFrom: expectedYear, yearTo: expectedYear);

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task SearchAsync_WithoutYearFilter_ReturnsAllResults()
    {
        const string responseBody = """
            {
              "totalItems": 3,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book from 2019","publishedDate":"2019"}},
                {"id":"2","volumeInfo":{"title":"Book from 2020","publishedDate":"2020"}},
                {"id":"3","volumeInfo":{"title":"Book without date","publishedDate":null}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: null, yearTo: null);

        Assert.Equal(3, result.Items.Count);
    }

    #endregion

    #region Combined Filters Tests

    [Fact]
    public async Task SearchAsync_WithAllFilters_AppliesAllCorrectly()
    {
        const string responseBody = """
            {
              "totalItems": 4,
              "items": [
                {"id":"1","volumeInfo":{"title":"French 2019","publishedDate":"2019","language":"fr"}},
                {"id":"2","volumeInfo":{"title":"French 2020","publishedDate":"2020","language":"fr"}},
                {"id":"3","volumeInfo":{"title":"English 2020","publishedDate":"2020","language":"en"}},
                {"id":"4","volumeInfo":{"title":"French 2021","publishedDate":"2021","language":"fr"}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("French", null, 0, 10, language: "fr", sortOrder: "newest", yearFrom: 2020, yearTo: 2020);

        Assert.Contains("&langRestrict=fr", handler.LastRequestUri ?? string.Empty);
        Assert.Contains("&orderBy=newest", handler.LastRequestUri ?? string.Empty);
        Assert.Single(result.Items);
        Assert.Equal("2", result.Items[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithTitleAuthorAndFilters_BuildsCorrectQuery()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("""{"totalItems":1,"items":[{"id":"1","volumeInfo":{"title":"Norwegian Wood","publishedDate":"1987","language":"en"}}]}"""));
        var service = CreateService(handler);

        await service.SearchAsync("Norwegian Wood", "Haruki Murakami", 0, 10, language: "en", sortOrder: "relevance");

        Assert.Contains("intitle%3A\"Norwegian Wood\"", handler.LastRequestUri ?? string.Empty);
        Assert.Contains("inauthor%3A\"Haruki Murakami\"", handler.LastRequestUri ?? string.Empty);
        Assert.Contains("&langRestrict=en", handler.LastRequestUri ?? string.Empty);
        Assert.DoesNotContain("orderBy", handler.LastRequestUri ?? string.Empty);
    }

    #endregion

    #region TotalItems Accuracy Tests

    [Fact]
    public async Task SearchAsync_WithYearFilter_UpdatesTotalItemsToMatchFilteredCount()
    {
        // The API returns totalItems=5, but after client-side year filtering, only 2 match
        const string responseBody = """
            {
              "totalItems": 5,
              "items": [
                {"id":"1","volumeInfo":{"title":"Book 2019","publishedDate":"2019"}},
                {"id":"2","volumeInfo":{"title":"Book 2020","publishedDate":"2020"}},
                {"id":"3","volumeInfo":{"title":"Book 2021","publishedDate":"2021"}},
                {"id":"4","volumeInfo":{"title":"Book 2022","publishedDate":"2022"}},
                {"id":"5","volumeInfo":{"title":"Book null","publishedDate":null}}
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => JsonResponse(responseBody));
        var service = CreateService(handler);

        var result = await service.SearchAsync("Book", null, 0, 10, yearFrom: 2020, yearTo: 2021);

        Assert.Equal(2, result.TotalItems);
        Assert.Equal(2, result.Items.Count);
    }

    #endregion

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
