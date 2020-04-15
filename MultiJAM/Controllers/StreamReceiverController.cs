using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StreamReceiverController<IncomingDataModel, GSM, PSM, IM, PIM>
{
    public StreamReceiverConfigModel streamReceiverConfig;
    public StreamRecieverLogModel streamRecieverLogModel = new StreamRecieverLogModel {
    packagesProcessed = 0,
    currentProcessSpeed=0.015,
    targetProcessSpeed=0.015,
    instancesProcessed=0,
    lastPackageProcessedInitiated=0,
    packagesReceivedCount=0,
    bufferCount=0,
};

public Dictionary<int, DataPackage<IncomingDataModel>> packageBuffer = new Dictionary<int, DataPackage<IncomingDataModel>>();
    string onEventName;
    public bool receptionMute;

    public abstract void ProcessData(IncomingDataModel data);
    public abstract DataPackage<IncomingDataModel> GetPredictedPackage(DataPackage<IncomingDataModel> data);

    public DataInstance<IncomingDataModel> lastDataInstanceProcessed = new DataInstance<IncomingDataModel>();
    public DataPackage<IncomingDataModel> lastPackageProcessed = new DataPackage<IncomingDataModel>();
    public MonoBehaviour looper;

    public StreamReceiverController(MonoBehaviour _looper)
    {
        looper = _looper;
    }

    bool firstDataPackReceived;
    public void DataPackageReceived(string eventName, string connectionId, object eventData)
    {
        if (receptionMute)
        {
            return;
        }

        streamRecieverLogModel.packagesReceivedCount++;
        DataPackageHistory<IncomingDataModel> pack = (DataPackageHistory<IncomingDataModel>)eventData;

        if (lastPackageProcessed.packageId == -1 && pack.dataPackageHistory.Count > 0 && !firstDataPackReceived)
        {
            firstDataPackReceived = true;
            lastPackageProcessed = pack.dataPackageHistory[0];
        }

        for (int i = 0; i < pack.dataPackageHistory.Count; i++)
        {
            if ((!packageBuffer.ContainsKey(pack.dataPackageHistory[i].packageId)) && pack.dataPackageHistory[i].packageId > lastPackageProcessed.packageId)
            {
                if (currentPackageProcessed != null)
                {
                    if (pack.dataPackageHistory[i].packageId > currentPackageProcessed.packageId)
                    {
                        packageBuffer.Add(pack.dataPackageHistory[i].packageId, pack.dataPackageHistory[i]);
                    }
                }
                else
                {
                    packageBuffer.Add(pack.dataPackageHistory[i].packageId, pack.dataPackageHistory[i]);
                }
            }
        }
    }

    Coroutine pdpc;
    public void StartReception(string _onEventName)
    {
        if (MultiplayerController.gameAuth == GameAuth.Server)
        {
            IMultiplayerController<GSM, PSM, IM, PIM>.instance.transportController.IOnFromClient(_onEventName, DataPackageReceived);
        }
        else
        {
            IMultiplayerController<GSM, PSM, IM, PIM>.instance.transportController.IOnFromServer(_onEventName, DataPackageReceived);
        }

        if (pdpc != null)
        {
            looper.StopCoroutine(pdpc);
        }
        pdpc = looper.StartCoroutine(ProcessDataPackageCor());
    }

    public void FindDataToProcess()
    {
        if (IMultiplayerController<GSM, PSM, IM, PIM>.instance.transportController.connectionId != "")
        {
            streamRecieverLogModel.bufferCount = packageBuffer.Count;

            if (streamRecieverLogModel.packagesReceivedCount > streamReceiverConfig.initialBufferCount)
            {
                //try to find package
                if (packageBuffer.TryGetValue(lastPackageProcessed.packageId + 1, out currentPackageProcessed))
                {
                    //process found package 
                    return;
                }
                else if (streamReceiverConfig.predictionEnabled)
                {
                    PredictDataPackage();
                    return;
                }
            }
        }
    }
     
      
    DataPackage<IncomingDataModel> currentPackageProcessed;
    public int loopCount;

    
    public void ProcessDataPackageLoop()
    {
        loopCount++;
        if (currentPackageProcessed == null)
        {
            FindDataToProcess();

            if (currentPackageProcessed == null)
            {
                return;
            }
        }
        streamRecieverLogModel.lastPackageProcessedInitiated = currentPackageProcessed.packageId;
        if (currentPackageProcessed.dataStream.Count > 0) 
        {
            UpdateProcessMode();

            for (int i = 0; i < (int)streamReceiverConfig.processMode; i++)
            {
                if (currentPackageProcessed.currentlyProcessed < (currentPackageProcessed.dataStream.Count-1) && currentPackageProcessed.dataStream.Count > 0)
                {
                    currentPackageProcessed.currentlyProcessed++;
                    ProcessInstance(currentPackageProcessed.dataStream[currentPackageProcessed.currentlyProcessed].data);
                }
            }
        }

        if (currentPackageProcessed.currentlyProcessed == currentPackageProcessed.dataStream.Count - 1)
        {
            lastPackageProcessed = currentPackageProcessed;
            packageBuffer.Remove(currentPackageProcessed.packageId);
            streamRecieverLogModel.packagesProcessed++;
            currentPackageProcessed = null;
        }
    }

    void UpdateProcessMode() {
        if (packageBuffer.Count == 0)
        {
            streamReceiverConfig.processMode = ProcessMode.Lazy;
        }
        else if (streamReceiverConfig.processMode == ProcessMode.Lazy && packageBuffer.Count >= streamReceiverConfig.bufferCountIdeal)
        {
            streamReceiverConfig.processMode = ProcessMode.Ideal;
        }else if (streamReceiverConfig.processMode == ProcessMode.Ideal && packageBuffer.Count > streamReceiverConfig.bufferCountMaxout)
        {
            streamReceiverConfig.processMode = ProcessMode.Hyper;
        }else if(streamReceiverConfig.processMode == ProcessMode.Hyper && packageBuffer.Count <= streamReceiverConfig.bufferCountIdeal)
        {
            streamReceiverConfig.processMode = ProcessMode.Ideal;
        }
    }

    IEnumerator ProcessDataPackageCor() {
        while (true)
        {
            ProcessDataPackageLoop();
            yield return new WaitForFixedUpdate();
        }
    }

    public void PredictDataPackage()
    {
        DataPackage<IncomingDataModel> predictedPackage = new DataPackage<IncomingDataModel>();
        predictedPackage.dataStream = lastPackageProcessed.dataStream;
        predictedPackage.packageId = (lastPackageProcessed.packageId + 1);

        predictedPackage = GetPredictedPackage(predictedPackage);
        currentPackageProcessed = predictedPackage;
    }

    public void ProcessInstance(IncomingDataModel dataInstance)
    {
        ProcessData(dataInstance);
        streamRecieverLogModel.instancesProcessed++;
    }
}

[System.Serializable]
public struct StreamRecieverLogModel
{
    public int packagesProcessed;
    public double currentProcessSpeed;
    public double targetProcessSpeed;
    public int instancesProcessed;
    public int lastPackageProcessedInitiated;
    public int packagesReceivedCount;
    public int bufferCount;
}

public enum ProcessMode{
 Lazy,
 Ideal,
 Hyper
 }