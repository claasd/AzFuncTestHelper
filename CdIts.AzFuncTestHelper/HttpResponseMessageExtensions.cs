using System.Net;
using FluentAssertions;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CdIts.AzFuncTestHelper;

public static class HttpResponseMessageExtensions
{
    public static readonly HttpStatusCode[] DefaultStatusCodes =
    {
        HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.NoContent
    };

    public static async Task Check(this Task<HttpResponseMessage> task, params HttpStatusCode[] expectedStatusCodes)
    {
        if (expectedStatusCodes.Length == 0)
            expectedStatusCodes = DefaultStatusCodes;
        var result = await task;
        var content = await result.Content.ReadAsStringAsync();
        result.StatusCode.Should().BeOneOf(expectedStatusCodes, content);
    }

    public static async Task<T> Json<T>(this Task<HttpResponseMessage> result, params HttpStatusCode[] expectedStatusCodes)
        where T : class
    {
        if (expectedStatusCodes.Length == 0)
            expectedStatusCodes = DefaultStatusCodes;
        var response = await result;
        var content = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().BeOneOf(expectedStatusCodes, content);
        if (Settings.DefaultJsonFlavor == Settings.JsonFlavor.SystemTextJson)
            return JsonSerializer.Deserialize<T>(content, Settings.JsonOptions)!;
        else
            return JsonConvert.DeserializeObject<T>(content, Settings.JsonSettings)!;
    }
}