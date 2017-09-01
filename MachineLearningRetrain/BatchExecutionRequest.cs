using System.Collections.Generic;

namespace MachineLearningRetrain
{
    /// <summary>
    /// Based on Batch Execution API Documentation for Retrain Model provided by Microsoft.
    /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-models-programmatically
    /// </summary>
    public class BatchExecutionRequest
    {
        public IDictionary<string, AzureBlobDataReference> Inputs { get; set; }
        public IDictionary<string, string> GlobalParameters { get; set; }
        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Outputs { get; set; }
    }
}