# veracity-machinelearning-automation

## Overview
This repository contains code for Azure Machine Learning automation cases. 

### Retrain Machine Learning Model - MachineLearningRetrain project
In case when user wants to retrain existing Model its necessary to provide new learning dataset, execute retraining of the Retrain Model and update Predictive Model with new .ilearner data.
This project provides code for doing it programmatically.
User needs to provide all data for connecting to Machine Learning WebServices and Azure Storage. 
That means Retraining WebService and Predictive Web Service needs to be set up. Also Azure Blob Storage needs to be provided.

### Azure Function - RetrainModelFunction project
Sample code showing how to use MachineLearningRetrain algorithm inside Azure Function which is triggered when blob container gets updated with blobs.

### Useful articles:
Azure Functions with Blob Storage trigger

https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob

Rertain Machine Learning overview:

https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-machine-learning-model

Retrain Machine Learning models programmatically:

https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-retrain-models-programmatically

Deploy Machine Learning Web Service:

https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-walkthrough-5-publish-web-service

Consume Machine Learning Web Service:

https://docs.microsoft.com/en-us/azure/machine-learning/machine-learning-consume-web-services

