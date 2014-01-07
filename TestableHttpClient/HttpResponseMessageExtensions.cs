using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestableHttpClient
{
    /// <remarks>
    /// http://aspnetwebstack.codeplex.com/discussions/387945
    /// </remarks>
    public static class RawHttpResponseMessageExtensions
    {
        public static async Task<Stream> ToRawHttpResponseStream(this HttpResponseMessage response, Stream stream)
        {
            var httpMessageContent = new HttpMessageContent(response);
            await httpMessageContent.CopyToAsync(stream);
            return stream;
        }

        public static async Task<HttpResponseMessage> ToHttpResponseMessage(this Stream stream)
        {
            var response = new HttpResponseMessage();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            response.Content = new ByteArrayContent(memoryStream.ToArray());
            response.Content.Headers.Add("Content-Type", "application/http;msgtype=response");
            return await response.Content.ReadAsHttpResponseMessageAsync();
        }
    }
}