using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningConsumer
{
    /// <summary>
    /// Based on Request Response Execution API Documentation for Retrain Model provided by Microsoft.
    /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-consume-web-services
    /// 
    /// Assumptions:
    /// 1. Azure Machine Learning Predictive Web Service is deployed
    /// </summary>
    public class WebServiceConsumer
    {
        /// <summary>
        /// Predictive Service Url
        /// </summary>
        public string ServiceUrl { get; }
        /// <summary>
        /// Predictive service Api key
        /// </summary>
        public string ApiKey { get; }
        /// <summary>
        /// Constructor accepts Predictive WebService url and Api Key
        /// </summary>
        /// <param name="webServiceUrl"></param>
        /// <param name="apiKey"></param>
        public WebServiceConsumer(string webServiceUrl, string apiKey)
        {
            ServiceUrl = webServiceUrl;
            ApiKey = apiKey;
        }
        /// <summary>
        /// Consume method.
        /// With prepared request ask WebService for Score.
        /// Notice additional parameters in url.
        /// </summary>
        /// <param name="scoreRequest"></param>
        /// <returns>tuple with two strings. first is success/failure message, second is json response</returns>
        public async Task<Tuple<string, string>> RequestScore(Request scoreRequest)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                var response = await client.PostAsJsonAsync(ServiceUrl + "/execute?api-version=2.0&details=true", scoreRequest);
                if (response.IsSuccessStatusCode)
                    return new Tuple<string, string>("Request executed successfully.", await response.Content.ReadAsStringAsync());
                return new Tuple<string, string>(await GetFailedResponse(response), null);
            }
        }
        /// <summary>
        /// In case of failure, compose user friendly error message.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private async Task<string> GetFailedResponse(HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var sb = new StringBuilder();
            sb.AppendLine($"The request failed with status code: {response.StatusCode}");
            sb.AppendLine(response.Headers.ToString());
            sb.AppendLine(responseContent);
            return sb.ToString();
        }
    }
    /// <summary>
    /// Used for proper request preparation
    /// </summary>
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    /// <summary>
    /// Request to be serialized to json.
    /// </summary>
    public class Request
    {
        public Dictionary<string, StringTable> Inputs { get; set; }
        public Dictionary<string, string> GlobalParameters { get; set; }
    }
}