using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TestableHttpClient
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _responseMessageFunc;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseMessageFunc)
        {
            _responseMessageFunc = responseMessageFunc;
        }

        public FakeHttpMessageHandler(HttpResponseMessage response)
            : this((request, ct) => Task.FromResult(response))
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _responseMessageFunc(request, cancellationToken);
        }
    }
}