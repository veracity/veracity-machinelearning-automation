using System.IO;
using System.Threading.Tasks;
using MachineLearningRetrain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;

namespace RetrainModelFunction
{
    /// <summary>
    /// This function listens for blob changes inside specified container.
    /// When new blob or update found, function starts to retrain Machine Learning Model with this new data
    /// There is no validation prior to retraining so if data does not fit model, exception will occur.
    /// One function is configured to update one Machine Learning Web Service.
    /// </summary>
    public static class RetrainModelOnBlobTrigger
    {
        /// <summary>
        /// Url to Retrain Machine Learning Web Service
        /// </summary>
        private const string RetrainServiceUrl =
            "https://europewest.services.azureml.net/workspaces/<wokrspace>/services/<service>/jobs";
        /// <summary>
        /// Api Key of Retrain Web Service
        /// </summary>
        private const string RetrainApiKey = "<your retrain api key>";
        /// <summary>
        /// Url to Update Endpoint of Predictive WebService.
        /// NOTE: This is not default url to WebService for sending jobs!
        /// </summary>
        private const string PredictiveServiceUrl =
            "https://europewest.management.azureml.net/workspaces/<workspace>/webservices/<service>/endpoints/<retrainEndpointOnPredictiveService>";
        /// <summary>
        /// Api Key of Update Endpoint of Predictive WebService
        /// NOTE: This is NOT default api key for Predictive WebService.
        /// It is required to create new EndPoint only for updates.
        /// </summary>
        private const string PredictiveUpdateEndpointApiKey = "<your retrain endpoint api key on predictive service>";
        /// <summary>
        /// Name of Retrain Web Service. To be found in Machine Learning Studio.
        /// </summary>
        private const string RetrainServiceName = "<your retrain service name>";
        /// <summary>
        /// Storage access data.
        /// </summary>
        private const string AzureStorageName = "<account name>";
        private const string AzureStorageKey = "<account key>";
        /// <summary>
        /// Container that function listens to. Source of new data.
        /// </summary>
        private const string StorageContainerName = "container";
        /// <summary>
        /// Container used to store .ilearner files.
        /// </summary>
        private const string AzureLearningContainerName = "container-learnings";

        /// <summary>
        /// Function entry point. Triggered by blob update.
        /// </summary>
        /// <param name="myBlob"></param>
        /// <param name="name"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("RetrainModelOnBlobTrigger")]
        public static async Task Run(
            [BlobTrigger("container/{name}", Connection = "AzureWebJobsStorage")] CloudBlockBlob myBlob,
            string name, TraceWriter log)
        {
            log.Info($"C# Blob trigger function Initialized by blob\n Name:{name}\nURI:{myBlob.StorageUri}");

            var inputData = new AzureStorageData(AzureStorageName, AzureStorageKey, StorageContainerName, name);
            var outputData = new AzureStorageData(AzureStorageName, AzureStorageKey,
                AzureLearningContainerName, Path.GetFileNameWithoutExtension(name)+"_outputresults.ilearner");
            
            log.Info("Retraining of Machine Learning started...");

            var retrainer = new WebServiceRetrainer(RetrainServiceUrl, RetrainApiKey);
            var retrainResults = await retrainer.Retrain(inputData, outputData);

            if (retrainResults.Item2 == null)
            {
                log.Info("Retraining finished with an error.");
                log.Info(retrainResults.Item1);
            }
            else
            {
                log.Info("Retraining finished successfully.");
                log.Info("Model update started...");
                var updater = new WebServiceUpdater(PredictiveServiceUrl, PredictiveUpdateEndpointApiKey);
                var updateResult = await updater.UpdateModel(retrainResults.Item2, RetrainServiceName);
                foreach(var info in updateResult)
                    log.Info(info);
            }
        }
    }
}