namespace MachineLearningRetrain
{
    public class AzureStorageData
    {
        /// <summary>
        /// Storage container name
        /// </summary>
        public string ContainerName { get; }
        /// <summary>
        /// Blob name with extension
        /// </summary>
        public string BlobName { get; }
        /// <summary>
        /// connection string to storage account
        /// </summary>
        public string DataConnectionString { get; }
        public AzureStorageData(string accoundName, string accountKey, string containerName, string blobName)
        {
            DataConnectionString =
                $"DefaultEndpointsProtocol=https;AccountName={accoundName};AccountKey={accountKey}";
            ContainerName = containerName;
            BlobName = blobName;
        }
    }
}
