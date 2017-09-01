using System.Collections.Generic;

namespace MachineLearningRetrain
{
    /// <summary>
    /// Based on Batch Execution API Documentation for Retrain Model provided by Microsoft.
    /// https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-models-programmatically
    /// </summary>
    
    public enum BatchScoreStatusCode
    {
        NotStarted,
        Running,
        Failed,
        Cancelled,
        Finished
    }
    public class BatchScoreStatus
    {
        // Status code for the batch scoring job
        public BatchScoreStatusCode StatusCode { get; set; }
        // Locations for the potential multiple batch scoring outputs
        public IDictionary<string, AzureBlobDataReference> Results { get; set; }
        // Error details, if any
        public string Details { get; set; }
    }
}