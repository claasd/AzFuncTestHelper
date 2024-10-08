﻿using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CdIts.AzFuncTestHelper;

public static class ActionResultExtensions
{
    public static async Task<HttpResponseMessage> ToMsg(this Task<IActionResult> task)
    {
        try
        {
            var result = await task;
            return result.ToMsg();
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.Message);
            await Console.Error.WriteLineAsync(e.StackTrace);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(e.Message)
            };
        }
    }

    public static HttpResponseMessage ToMsg(this IActionResult res)
    {
        return res switch
        {
            JsonResult { SerializerSettings: JsonSerializerSettings settings } jsonResult => new HttpResponseMessage((HttpStatusCode)(jsonResult.StatusCode ?? 200))
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(jsonResult.Value, null,
                        settings), Encoding.UTF8,
                    "application/json")
            },
            JsonResult { SerializerSettings: JsonSerializerOptions options } jsonResult => new HttpResponseMessage((HttpStatusCode)(jsonResult.StatusCode ?? 200))
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonResult.Value, options), Encoding.UTF8,
                    "application/json")
            },
            JsonResult jsonResult when Settings.DefaultJsonFlavor == Settings.JsonFlavor.SystemTextJson => new HttpResponseMessage((HttpStatusCode)(jsonResult.StatusCode ?? 200))
            {
                Content = new StringContent(JsonSerializer.Serialize(jsonResult.Value, Settings.JsonOptions), Encoding.UTF8,
                    "application/json")
            },
            JsonResult jsonResult => new HttpResponseMessage((HttpStatusCode)(jsonResult.StatusCode ?? 200))
            {
                Content = new StringContent(JsonConvert.SerializeObject(jsonResult.Value, Settings.JsonSettings), Encoding.UTF8,
                    "application/json")
            },
            ContentResult contentResult => new HttpResponseMessage(
                (HttpStatusCode)(contentResult.StatusCode ?? 200))
            {
                Content = new StringContent(contentResult.Content ?? "", Encoding.UTF8, contentResult.ContentType ?? MediaTypeNames.Text.Plain)
            },
            FileStreamResult fileStreamResult => new HttpResponseMessage()
            {
                Content = new StreamContent(fileStreamResult.FileStream)
            },
            StatusCodeResult statusCodeResult => new HttpResponseMessage(
                (HttpStatusCode)statusCodeResult.StatusCode) { Content = new ByteArrayContent(Array.Empty<byte>()) },
            _ => new HttpResponseMessage() { Content = new ByteArrayContent(Array.Empty<byte>()) }
        };
    }

    public static async Task Check(this Task<IActionResult> result, params HttpStatusCode[] expectedCodes) => await result.ToMsg().Check(expectedCodes);
    public static Task<T> Json<T>(this Task<IActionResult> result, params HttpStatusCode[] expectedCodes) where T: class => result.ToMsg().Json<T>(expectedCodes);

}