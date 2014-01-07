using System.Threading.Tasks;

namespace TestableHttpClient
{
    public interface IApiClient
    {
        Task<object> GetAsync(string path, object parameters = null);
    }
}