using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StreamReceiverController<IncomingDataModel>
{
    public StreamReceiverConfigModel streamReceiverConfig;
    public StreamRecieverLogModel streamRecieverLogModel = new StreamRecieverLogModel {
    currentProcessSpeed=0.015,
    targetProcessSpeed=0.015,
    instancesProcessed=0,
    packagesReceivedCount=0,
    };

    public Dictionary<int, DataPackage> packageBuffer = new Dictionary<int, DataPackage>();

    public abstract void ProcessData(IncomingDataModel data);
    public abstract void InitStreamReception(string eventName);
    public MonoBehaviour looper;

    public StreamReceiverController(MonoBehaviour _looper)
    {
        looper = _looper;
    }

    bool firstPackageReceived;
    public void DataPackageReceived(string eventName, string connectionId, DataPackageHistory eventData)
    {
        streamRecieverLogModel.packagesReceivedCount++;
        DataPackageHistory pack = eventData;

        if (!firstPackageReceived)
        {
            firstPackageReceived = true;
            packageProgress = pack.dataPackageHistory[0].packageId;
        }

        for (int i = 0; i < pack.dataPackageHistory.Length; i++)
        {
            if ((!packageBuffer.ContainsKey(pack.dataPackageHistory[i].packageId)) && pack.dataPackageHistory[i].packageId > packageProgress)
            {
                packageBuffer.Add(pack.dataPackageHistory[i].packageId, pack.dataPackageHistory[i]);
            }
        }
    }

    Coroutine pdpc;
    public void StartReception()
    {
        if (pdpc != null)
        {
            looper.StopCoroutine(pdpc);
        }
        pdpc = looper.StartCoroutine(ProcessDataPackageCor());
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
    
    int packageProgress = -1;
    IEnumerator ProcessDataPackageCor()
    {
        while (true)
        {
            if (!firstPackageReceived || streamRecieverLogModel.packagesReceivedCount < streamReceiverConfig.initialBufferCount)
            {
                yield return new WaitForFixedUpdate();
            }
            else
            {
                DataPackage currentPackageProcessed = new DataPackage();
                if (packageBuffer.TryGetValue(packageProgress + 1, out currentPackageProcessed))
                {
                    int i = 0;
                    while(i< currentPackageProcessed.dataStream.Length) { 
                        UpdateProcessMode();
                        for (int j = 0; j < (int)streamReceiverConfig.processMode;j++)
                        {
                            if (i < currentPackageProcessed.dataStream.Length)
                            {
                                ProcessInstance((IncomingDataModel)currentPackageProcessed.dataStream[i].data);
                                i++;
                                yield return new WaitForFixedUpdate();
                            }
                        }
                    }
                    packageBuffer.Remove(currentPackageProcessed.packageId);
                    packageProgress = currentPackageProcessed.packageId;
                }
                else
                {
                    yield return new WaitForFixedUpdate();
                }
            }
        }
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
    public double currentProcessSpeed;
    public double targetProcessSpeed;
    public int instancesProcessed;
    public int packagesReceivedCount;
}

public enum ProcessMode{
 Lazy,
 Ideal,
 Hyper
 }