using System;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace TestableHttpClient
{
    public class Tests
    {
        [Fact]
        public async Task RawResponseTests()
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.facebook.com/4");

            var response = await client.SendAsync(request);

            var responseString = await response.Content.ReadAsStringAsync();
            using (var rawResponse = await response.ToRawHttpResponseStream(new MemoryStream()))
            {
                rawResponse.Seek(0, SeekOrigin.Begin);

                var reader = new StreamReader(rawResponse);
                var rawResponseString = await reader.ReadToEndAsync();
                rawResponse.Seek(0, SeekOrigin.Begin);

                var parsedResponse = await rawResponse.ToHttpResponseMessage();
                var parsedResponseString = await parsedResponse.Content.ReadAsStringAsync();

                Assert.Equal(response.Version, parsedResponse.Version);
                Assert.Equal(response.StatusCode, parsedResponse.StatusCode);
                Assert.Equal(response.ReasonPhrase, parsedResponse.ReasonPhrase);
                Assert.Equal(responseString, parsedResponseString);

                Console.WriteLine(rawResponseString);
            }
        }

        [Fact]
        public async Task ActualApiTests()
        {
            var client = new ApiClient();
            dynamic result = await client.GetAsync("4");

            Assert.Equal("4", result.id);
        }

        [Fact]
        public async Task FakeRawResponseTest()
        {
            var response = new HttpResponseMessage();
            var responseContent = new StringContent("{\"id\":\"4\",\"name\":\"Mark Zuckerberg\",\"first_name\":\"Mark\",\"last_name\":\"Zuckerberg\",\"link\":\"http:\\/\\/www.facebook.com\\/zuck\",\"username\":\"zuck\",\"gender\":\"male\",\"locale\":\"en_US\"}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "UTF-8" };
            response.Content = responseContent;

            var client = new ApiClient(new HttpClient(new FakeHttpMessageHandler(response)));
            dynamic result = await client.GetAsync("4");

            Assert.Equal("4", result.id);
        }

        [Fact]
        public async Task RequestTest()
        {
            var response = new HttpResponseMessage();
            var responseContent = new StringContent("{\"id\":\"4\",\"name\":\"Mark Zuckerberg\",\"first_name\":\"Mark\",\"last_name\":\"Zuckerberg\",\"link\":\"http:\\/\\/www.facebook.com\\/zuck\",\"username\":\"zuck\",\"gender\":\"male\",\"locale\":\"en_US\"}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("text/javascript") { CharSet = "UTF-8" };
            response.Content = responseContent;

            var client = new ApiClient(new HttpClient(new FakeHttpMessageHandler(
                (request, ct) => {
                    Assert.Equal("https://graph.facebook.com/4", request.RequestUri.AbsoluteUri);
                    return Task.FromResult(response);
                })));

            dynamic result = await client.GetAsync("4");

            Assert.Equal("4", result.id);
        }

        [Fact]
        public async Task SampleUsingMoq()
        {
            dynamic result = new ExpandoObject();
            result.id = "4";

            var fake = new Mock<IApiClient>();

            fake.Setup(f => f.GetAsync(It.IsAny<string>(), It.IsAny<object>()))
                .Returns(Task.FromResult((object)result));

            var client = fake.Object;

            await client.GetAsync("4");

            fake.VerifyAll();
        }
    }
}
