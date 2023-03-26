namespace SomeWebApi.Contracts.APIs;

using Refit;

public interface IDocumentApi
{
    [Get("/document")]
    Task<HttpResponseMessage> GetPdfFileAsync(string template);
}