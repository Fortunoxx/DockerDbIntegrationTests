namespace SomeWebApiUnitTests.ProcessControllerUnitTests;
using Polly;
using Xunit;
using Polly.Registry;
using Polly.Timeout;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SomeWebApi.Contracts.APIs;
using Polly.Extensions.Http;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

public class RetryUnitTests
{
    [Fact]
    public async Task HttpClient_Timeout_ShouldRetryAsync2()
    {
        // Arrange
        var (retry, _) = GetDefaultWaitAndRetryConfig();
        var logger = Substitute.For<ILogger<IDocumentApi>>();

        var defaultWaitAndRetryPolicy = ConfigureResiliencePolicies().Get<IAsyncPolicy<HttpResponseMessage>>("DefaultWaitAndRetryPolicy");

        var documentService = Substitute.For<IDocumentApi>();
        documentService.GetPdfFileAsync(string.Empty).ThrowsAsyncForAnyArgs(new TimeoutRejectedException());
        documentService.GetPdfFileAsync("2").Returns(new HttpResponseMessage(System.Net.HttpStatusCode.RequestTimeout));
        documentService.GetPdfFileAsync("3").Returns(new HttpResponseMessage(System.Net.HttpStatusCode.GatewayTimeout));
        // documentService.GetPdfFileAsync("4").Returns(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        documentService.GetPdfFileAsync($"{retry}").Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        int invocations = 0;

        // Act
        var act = await defaultWaitAndRetryPolicy.ExecuteAsync(() => documentService.GetPdfFileAsync($"{invocations++}"));

        // Assert.
        act.EnsureSuccessStatusCode();
    }

    private static (int, int) GetDefaultWaitAndRetryConfig()
        => (5, 100);

    private static PolicyRegistry ConfigureResiliencePolicies()
    {
        var (retry, wait) = GetDefaultWaitAndRetryConfig();

        var defaultWaitAndRetryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(retry, retryAttempt => TimeSpan.FromMilliseconds(wait),
                onRetry: (outcome, timespan, retryAttempt, context) => 
                    Trace.WriteLine($"*** Delaying for {timespan.TotalMilliseconds}ms, then making retry #{retryAttempt}" )
                );

        return new PolicyRegistry {
            { "DefaultWaitAndRetryPolicy", defaultWaitAndRetryPolicy },           
            // { "MySyncRepositoryPolicy", Policy.Handle<TimeoutException>().Retry(1) },
            // { "MyAsyncRepositoryPolicy", Policy.Handle<TimeoutException>().RetryAsync(1) },
        };
    }
}

