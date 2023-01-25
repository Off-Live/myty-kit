using System.Net.Http;

namespace MYTYKit.Scripts.MetaverseKit.Util
{
    public static class HttpClientUtil
    {
        public static string GetAsync(string url)
        {
            using (var handler = new HttpClientHandler())
            using (var client = new HttpClient(handler))
            {
                var task = client.GetAsync(url, HttpCompletionOption.ResponseContentRead);
                
                return task.Result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }
}