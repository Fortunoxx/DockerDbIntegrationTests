namespace SomeWebApi.Controllers;

using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using SomeWebApi.Contracts.APIs;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private readonly ILogger<DocumentController> _logger;

    private readonly IDocumentApi documentApi;

    private const string DefaultFileName = "Default.pdf";

    public DocumentController(ILogger<DocumentController> logger, IDocumentApi documentApi)
    {
        _logger = logger;
        this.documentApi = documentApi;
    }

    [HttpGet(Name = "GetDocument")]
    public Task<FileStreamResult> GetDocumentAsync()
    {
        // var base64 = "JVBERi0xLjAKMSAwIG9iajw8L1BhZ2VzIDIgMCBSPj5lbmRvYmogMiAwIG9iajw8L0tpZHNbMyAw\nIFJdL0NvdW50IDE+PmVuZG9iaiAzIDAgb2JqPDwvTWVkaWFCb3hbMCAwIDMgM10+PmVuZG9iagp0\ncmFpbGVyPDwvUm9vdCAxIDAgUj4+Cg=="; // small pdf
        var base64 = "JVBERi0xLjIgCjkgMCBvYmoKPDwKPj4Kc3RyZWFtCkJULyAzMiBUZiggIFlPVVIgVEVYVCBIRVJFICAgKScgRVQKZW5kc3RyZWFtCmVuZG9iago0IDAgb2JqCjw8Ci9UeXBlIC9QYWdlCi9QYXJlbnQgNSAwIFIKL0NvbnRlbnRzIDkgMCBSCj4+CmVuZG9iago1IDAgb2JqCjw8Ci9LaWRzIFs0IDAgUiBdCi9Db3VudCAxCi9UeXBlIC9QYWdlcwovTWVkaWFCb3ggWyAwIDAgMjUwIDUwIF0KPj4KZW5kb2JqCjMgMCBvYmoKPDwKL1BhZ2VzIDUgMCBSCi9UeXBlIC9DYXRhbG9nCj4+CmVuZG9iagp0cmFpbGVyCjw8Ci9Sb290IDMgMCBSCj4+CiUlRU9G";
        var content = Convert.FromBase64String(base64);
        var stream = new MemoryStream(content);
        return Task.FromResult(File(stream, MediaTypeNames.Application.Pdf, "tiny.pdf"));
    }

    [HttpGet("{template}", Name = "GetDocumentByTemplate")]
    public async Task<IActionResult> GetDocumentByTemplateAsync(string template)
    {
        var response = await documentApi.GetPdfFileAsync(template);
        return File(await response.Content.ReadAsStreamAsync(),
            response.Content.Headers.ContentType?.MediaType ?? MediaTypeNames.Application.Pdf,
            response.Content.Headers.ContentDisposition?.FileName ?? DefaultFileName);
    }
}