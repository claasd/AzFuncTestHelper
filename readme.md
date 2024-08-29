# AzFuncTestHelper

[![License](https://img.shields.io/badge/license-MIT-blue)](https://github.com/claasd/caffoa.net/blob/main/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/CdIts.AzFuncTestHelper)](https://www.nuget.org/packages/CdIts.Caffoa.Json.Net/)
[![CI](https://github.com/claasd/AzFuncTestHelper/actions/workflows/build.yml/badge.svg)](https://github.com/claasd/caffoa.net/actions/workflows/build.yml)

Helper for unit testing azure functions and other REST related functions.
Uses [Moq](https://github.com/devlooped/moq) to create requests, and [FluentAssertions](https://github.com/fluentassertions/fluentassertions) to check responses

## RequestBuilder

The request builder is a helper class to create HttpRequest objects for unit testing azure functions or other REST related functions.
Uses Moq under the hood to create the HttpRequest object.

Example:
```csharp
var data = new SomeJsonObject();
HttpRequest request = new RequestBuilder()
    .Header("Content-Type", "application/json")
    .Header("api-key", "FAKE_API_KEY")
    .header("roles",  "admin", "user")
    .Query("name", "SomeName")
    .Query("before", DateTimeOffset.UtcNow)
    .Query("useFancyStuff", true)
    .Content(data).Build();
```
the `Content` can use string, byte, streams or objects. Objects are serialized to json.
By default, Json.NET is used, but you can set `CdIts.AzFuncTestHelper.Settings.DefaultJsonFlavor` to use SystemTextJson instead.
You can also modify the serializer settings by modifying `CdIts.AzFuncTestHelper.Settings.JsonSettings` for Json.NET and  `CdIts.AzFuncTestHelper.Settings.JsonOptions` for System.Text.Json.

## Extensions for Task\<HttpResponseMessage> and Task\<IActionResult>

There are two helper extensions for `HttpResponseMessage`, and `Task<HttpResponseMessage>` to make it easier to work with the response:
* `Check()` checks the status code and throws an exception if it is not successful. You can give several allowed response codes to the method.
* `Json<T>()` checks the status deserializes the content of the response to an object of type `T`. You can pass several allowed response codes to the method.

for both functions, the status codes 200, 201 and 202 are considered okay. You can overwrite this by passing the status codes as parameters.

Example:
```csharp
var response = await client.SendAsync(request).Json<SomeJsonObject>();
var errorResponse = await client.SendAsync(wrongRequest).Json<SomeJsonObject>(HttpStatusCode.BadRequest);
await client.CheckAsync(request).Check(HttpStatusCode.Created, HttpStatusCode.OK);
```

As with the RequestBuilder, you can use `CdIts.AzFuncTestHelper.Settings.DefaultJsonFlavor`, `CdIts.AzFuncTestHelper.Settings.JsonSettings` and `CdIts.AzFuncTestHelper.Settings.JsonOptions` to modify the behavior of the Json deserialization.


### Additional extension for Task\<IActionResult>

* `ToMsg()` transforms a Task<IActionResult> into a HttpResponseMessage.

 










