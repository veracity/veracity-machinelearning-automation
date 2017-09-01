using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MachineLearningRetrain
{
    /// <summary>
    /// Based on Batch Execution API Documentation for Retrain Model provided by Microsoft.
    /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-models-programmatically
    /// 
    /// Assumptions:
    /// 1. Two Azure Machine Learning Web Services are deployed
    /// 1.1. Retraining Web Service
    /// 1.2. Predictive Web Service
    /// 
    /// This class uses Retraining Web Service and all configuration is related to it. 
    /// </summary>
    public class WebServiceRetrainer
    {
        /// <summary>
        /// Retrain Service Url
        /// </summary>
        public string ServiceJobsUrl { get; }
        /// <summary>
        /// Retrain service Api key
        /// </summary>
        public string ApiKey { get; }
        // Set a timeout of 2 minutes
        private const int TimeOutInMilliseconds = 120 * 1000; 
        /// <summary>
        /// Constructor accepts Retrain WebService url for jobs and Api Key for Retrain service
        /// </summary>
        /// <param name="serviceUrl"></param>
        /// <param name="apiKey"></param>
        public WebServiceRetrainer(string serviceUrl, string apiKey)
        {
            ServiceJobsUrl = serviceUrl;
            ApiKey = apiKey;
        }
        /// <summary>
        /// Retrain method.
        /// Prepares request based in input and output
        /// With usage of HttpClient POSTs this request to WebService
        /// Starts retrain job, monitors status
        /// Returns Tuple with message string as Item1 and collection of BlobData for each .ilearner file created.
        /// When error appears, message contains error details and collection is null.
        /// </summary>
        /// <param name="inputStorageData"></param>
        /// <param name="outputStorageData"></param>
        /// <returns></returns>
        public async Task<Tuple<string, IEnumerable<AzureBlobDataReference>>> Retrain(AzureStorageData inputStorageData, AzureStorageData outputStorageData)
        {
            var request = PrepareRequest(inputStorageData, outputStorageData);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                // submit
                var response = await client.PostAsJsonAsync(ServiceJobsUrl + "?api-version=2.0", request);
                if (!response.IsSuccessStatusCode)
                    return new Tuple<string, IEnumerable<AzureBlobDataReference>>(await Utilities.GetFailedResponse(response), null);
                
                var jobId = await response.Content.ReadAsAsync<string>();
                var jobLocation = ServiceJobsUrl + "/" + jobId + "?api-version=2.0";

                // if submitted correctly, start retraining job
                response = await client.PostAsync(ServiceJobsUrl + "/" + jobId + "/start?api-version=2.0", null);
                if (!response.IsSuccessStatusCode)
                    return new Tuple<string, IEnumerable<AzureBlobDataReference>>(await Utilities.GetFailedResponse(response), null);

                return await MonitorProgress(client, jobId, jobLocation);
            }
        }
        /// <summary>
        /// With loop monitors progress of learning Model on Retrain WebService
        /// Returns Tuple with message string as Item1 and collection of BlobData for each .ilearner file created.
        /// When error appears, message contains error details and collection is null.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="jobId"></param>
        /// <param name="jobLocation"></param>
        /// <returns></returns>
        private async Task<Tuple<string, IEnumerable<AzureBlobDataReference>>> MonitorProgress(HttpClient client, string jobId, string jobLocation)
        {
            var watch = Stopwatch.StartNew();
            while (true)
            {
                var response = await client.GetAsync(jobLocation);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await Utilities.GetFailedResponse(response);
                    return new Tuple<string, IEnumerable<AzureBlobDataReference>>(message, null);
                }
                var status = await response.Content.ReadAsAsync<BatchScoreStatus>();
                if (watch.ElapsedMilliseconds > TimeOutInMilliseconds)
                {
                    await client.DeleteAsync(jobLocation);
                    return new Tuple<string, IEnumerable<AzureBlobDataReference>>("Timeout reached! Job deleted", null);
                }
                switch (status.StatusCode)
                {
                    case BatchScoreStatusCode.NotStarted:
                    case BatchScoreStatusCode.Running:
                        break;
                    case BatchScoreStatusCode.Failed:
                        var message = $"Job {jobId}failed!\nError details: {status.Details}";
                        return new Tuple<string, IEnumerable<AzureBlobDataReference>>(message, null);
                    case BatchScoreStatusCode.Cancelled:
                        return new Tuple<string, IEnumerable<AzureBlobDataReference>>($"Job {jobId} cancelled!", null);
                    case BatchScoreStatusCode.Finished:
                        return new Tuple<string, IEnumerable<AzureBlobDataReference>>($"Job {jobId} finished!",
                            ProcessResults(status));
                }
                Thread.Sleep(1000); // Wait one second
            }
        }
        /// <summary>
        /// BatchScoreStatus contains results about several created output blobs.
        /// At this point only .ilearner blob is needed.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private IEnumerable<AzureBlobDataReference> ProcessResults(BatchScoreStatus status)
        {
            return status.Results.Values.Where(v =>
            {
                var extension = Path.GetExtension(v.RelativeLocation);
                return extension != null && extension.Equals(".ilearner");
            });
        }
        /// <summary>
        /// Gather data for request. Prepare class structure for serialization.
        /// Names of inputs are NOT random but required like this.
        /// </summary>
        /// <param name="inputdata">points to input dataset, like csv file</param>
        /// <param name="outputData">points to output blob .ilearner file used later to update predictive web service</param>
        private BatchExecutionRequest PrepareRequest(AzureStorageData inputdata, AzureStorageData outputData)
        {
            return new BatchExecutionRequest
            {
                Inputs = new Dictionary<string, AzureBlobDataReference>
                {
                    {
                        "input2",
                        new AzureBlobDataReference
                        {
                            ConnectionString = inputdata.DataConnectionString,
                            RelativeLocation = $"{inputdata.ContainerName}/{inputdata.BlobName}"
                        }
                    },
                },
                Outputs = new Dictionary<string, AzureBlobDataReference>
                {
                    {
                        "output2",
                        new AzureBlobDataReference
                        {
                            ConnectionString = outputData.DataConnectionString,
                            RelativeLocation = $"{outputData.ContainerName}/{outputData.BlobName}"
                        }
                    }
                },
                GlobalParameters = new Dictionary<string, string>()
            };
        }
    }
}