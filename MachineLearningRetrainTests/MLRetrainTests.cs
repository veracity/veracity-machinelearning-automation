using System.Collections.Generic;
using System.Linq;
using MachineLearningRetrain;
using NUnit.Framework;

namespace MachineLearningRetrainTests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class MLRetrainTests
    {
        /// <summary>
        /// Provide valid data to below properties before executing tests.
        /// Need to have two Machine Learning WebServices.
        /// 1. Retraining Web Service
        /// 1. Predictive Web Service
        /// </summary>
        private const string RetrainServiceUrl =
                "https://europewest.services.azureml.net/workspaces/<wokrspace>/services/<service>/jobs";
        private const string RetrainApiKey = "<your retrain api key>";

        private const string PredictiveServiceUrl =
                "https://europewest.management.azureml.net/workspaces/<workspace>/webservices/<service>/endpoints/<retrainEndpointOnPredictiveService>";

        // Notice that this key is NOT url for Predictive WebService itself but for 
        // additional endpoint added to that service, used only for updating.
        private const string PredictiveUpdateEndpointApiKey = "<your retrain endpoint api key on predictive service>";

        private const string RetrainServiceName = "<your retrain service name>";

        private readonly AzureStorageData _inputData = new AzureStorageData("<account name>",
            "<account key>", "<container with data>", "<blob with data>");

        private readonly AzureStorageData _outputData = new AzureStorageData("<account name>",
            "<account key>", "<container with data>", "<blob with output .ilearner>");

        [Test]
        public void RetrainerTest()
        {
            var retrainer = new WebServiceRetrainer(RetrainServiceUrl, RetrainApiKey);
            var result = retrainer.Retrain(_inputData, _outputData);
            result.Wait();
            var resultTuple = result.Result;
            Assert.NotNull(resultTuple);
            Assert.NotNull(resultTuple.Item2);
            var resultList = resultTuple.Item2.ToList();
            Assert.AreEqual(1, resultList.Count);
        }
        [Test]
        public void UpdaterTest()
        {
            var updater = new WebServiceUpdater(PredictiveServiceUrl, PredictiveUpdateEndpointApiKey);
            var references = new List<AzureBlobDataReference>
            {
                // fill below data with valid one before running test!
                new AzureBlobDataReference
                {
                    BaseLocation = "https://service.blob.core.windows.net/",
                    RelativeLocation = "<container with data>/<blob with output .ilearner>",
                    SasBlobToken = "?<sastoken>"
                }
            };
            var result = updater.UpdateModel(references, RetrainServiceName);
            result.Wait();
            var resultString = result.Result;
            Assert.NotNull(resultString);
            Assert.AreEqual(1, resultString.Count);
            Assert.AreEqual("Web Service updated successfully with <container with data>/<blob with output.ilearner>", resultString[0]);

        }
        [Test]
        public void RetrainAndUpdateTest()
        {
            var retrainer = new WebServiceRetrainer(RetrainServiceUrl, RetrainApiKey);
            var retrainResult = retrainer.Retrain(_inputData, _outputData);
            retrainResult.Wait();
            var resultTuple = retrainResult.Result;
            Assert.NotNull(resultTuple);
            Assert.NotNull(resultTuple.Item2);
            var resultList = resultTuple.Item2.ToList();
            Assert.AreEqual(1, resultList.Count);

            var updater = new WebServiceUpdater(PredictiveServiceUrl, PredictiveUpdateEndpointApiKey);
            var updateResult = updater.UpdateModel(resultList, RetrainServiceName);
            updateResult.Wait();
            var resultString = updateResult.Result;
            Assert.NotNull(resultString);
            Assert.AreEqual(1, resultString.Count);
            Assert.AreEqual("Web Service updated successfully with <container with data>/<blob with output.ilearner>", resultString[0]);
        }
    }
}