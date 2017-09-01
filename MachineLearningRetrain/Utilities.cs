using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningRetrain
{
    public class Utilities
    {
        /// <summary>
        /// Builds nice message when failure occurs
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<string> GetFailedResponse(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var sb = new StringBuilder();
            sb.AppendLine($"The request failed with status code: {response.StatusCode}");
            sb.AppendLine(response.Headers.ToString());
            sb.AppendLine(responseContent);
            return sb.ToString();
        }
    }
}
