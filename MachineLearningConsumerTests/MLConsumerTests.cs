using System.Collections.Generic;
using MachineLearningConsumer;
using NUnit.Framework;

namespace MachineLearningConsumerTests
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class MLConsumerTests
    {
        /// <summary>
        /// Url to Machine Learning Web Service
        /// </summary>
        private const string ServiceUrl =
                "https://europewest.services.azureml.net/workspaces/<workspace>/services/<service>";

        /// <summary>
        /// ApiKey for MachineLearning We Service
        /// </summary>
        private const string ApiKey = "<api-key>";

        /// <summary>
        /// Test data based on Microsoft Sample 5:
        /// Train, Test, Evaluate for Binary Classification: Adult Dataset
        /// </summary>
        private readonly Request _scoreRequest = new Request
        {
            Inputs = new Dictionary<string, StringTable>
            {
                {
                    "input1",
                    new StringTable
                    {
                        ColumnNames = new[]
                        {
                            "age", "workclass", "fnlwgt", "education", "education-num", "marital-status", "occupation",
                            "relationship", "race", "sex", "capital-gain", "capital-loss", "hours-per-week",
                            "native-country", "income"
                        },
                        Values = new[,]
                        {
                            {
                                "0", "value", "0", "value", "0", "value", "value", "value", "value", "value", "0", "0",
                                "0",
                                "value", "value"
                            },
                            {
                                "0", "value", "0", "value", "0", "value", "value", "value", "value", "value", "0", "0",
                                "0",
                                "value", "value"
                            }
                        }
                    }
                }
            },
            GlobalParameters = new Dictionary<string, string>()
        };
        [Test]
        public void TestConsume()
        {
            var consumer = new WebServiceConsumer(ServiceUrl, ApiKey);
            var response = consumer.RequestScore(_scoreRequest);
            response.Wait();
            Assert.NotNull(response.Result.Item2);
        }
    }
}
