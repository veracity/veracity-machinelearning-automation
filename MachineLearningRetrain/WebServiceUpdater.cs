using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MachineLearningRetrain
{
    /// <summary>
    /// Based on Update Resource API Documentation for Retrain Model provided by Microsoft.
    /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-a-classic-web-service
    /// 
    /// Assumptions:
    /// 1. Two Azure Machine Learning Web Services are deployed
    /// 1.1. Retraining Web Service
    /// 1.2. Predictive Web Service
    /// 
    /// This class uses .ilearner file created when Retraining WebService to update Predictive WebService Model
    /// </summary>
    public class WebServiceUpdater
    {
        /// <summary>
        /// Predicitive Service Url
        /// </summary>
        public string ServiceEndpointUrl { get; }
        /// <summary>
        /// Predictive PATCH Api key
        /// Notice that this is NOT default api key for WebService
        /// User has to create new endpoint
        /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-a-classic-web-service
        /// </summary>
        public string EndpointApiKey { get; }
        /// <summary>
        /// Constructor accepts Predictive service endPoint url created for updates and this endpoint ApiKey
        /// </summary>
        /// <param name="serviceEndPointUrl"></param>
        /// <param name="endpointApiKey"></param>
        public WebServiceUpdater(string serviceEndPointUrl, string endpointApiKey)
        {
            ServiceEndpointUrl = serviceEndPointUrl;
            EndpointApiKey = endpointApiKey;
        }
        /// <summary>
        /// AzureBlobDataReference is a data about .ilearner blob that we want to use for update.
        /// We can get this from WebServiceRetrainer.
        /// Method contacts Predictive WebService via update endpoint and performs Model update with .ilearner blob
        /// </summary>
        /// <param name="references"></param>
        /// <param name="retrainServiceName"></param>
        /// <returns></returns>
        public async Task<List<string>> UpdateModel(IEnumerable<AzureBlobDataReference> references, string retrainServiceName)
        {
            var results = new List<string>();
            foreach (var reference in references)
            {
                var resourceLocations = new
                {
                    Resources = new[]
                    {
                        new
                        {
                            Name = retrainServiceName,
                            Location = new AzureBlobDataReference
                            {
                                BaseLocation = reference.BaseLocation,
                                RelativeLocation = reference.RelativeLocation,
                                SasBlobToken = reference.SasBlobToken
                            }
                        }
                    }
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", EndpointApiKey);

                    using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), ServiceEndpointUrl))
                    {
                        request.Content = new StringContent(JsonConvert.SerializeObject(resourceLocations),
                            Encoding.UTF8, "application/json");
                        var response = await client.SendAsync(request);

                        if (!response.IsSuccessStatusCode)
                            results.Add(await Utilities.GetFailedResponse(response));
                        else
                            results.Add($"Web Service updated successfully with {reference.RelativeLocation}");
                    }
                }
            }
            return results;
        }
    }
}