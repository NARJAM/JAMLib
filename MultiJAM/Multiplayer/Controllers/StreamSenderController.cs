using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class StreamSenderController<OutgoingData>
{
    [Header("Config")]
    public List<DataPackage<OutgoingData>> packageHistory = new List<DataPackage<OutgoingData>>();
    public StreamSenderConfigModel streamSenderConfig;
    string emitEventName;
    public bool sendMute;

    [Header("Stats")]
    public int instancesSentCount;
    public int packagesSentCount;
    TransportController signalRController;
    MonoBehaviour looper;

    public StreamSenderController(TransportController _signalRController, MonoBehaviour _looper)
    {
        looper = _looper;
        signalRController = _signalRController;
    }

    public abstract OutgoingData GetData();

    Coroutine sdpc;
    public void StartStream(string eventName)
    {
        emitEventName = eventName;

        if (sdpc != null)
        {
            looper.StopCoroutine(sdpc);
        }
        sdpc = looper.StartCoroutine(SendDataPackageCor());
    }

    DataPackage<OutgoingData> currentPackageSend = new DataPackage<OutgoingData>();
    public void SendDataPackageLoop()
    {

        if (signalRController.connectionId != "")
        {
            DataInstance<OutgoingData> di = new DataInstance<OutgoingData>();
            di.data = GetData();
            di.instanceId = instancesSentCount;
            instancesSentCount++;
            currentPackageSend.dataStream.Add(di);

            if (currentPackageSend.dataStream.Count == streamSenderConfig.sendRate)
            {
                currentPackageSend.packageId = packagesSentCount;
                packagesSentCount++;
                SendDataPackage(currentPackageSend);
                currentPackageSend.dataStream.Clear();
            }
        }
    }

    IEnumerator SendDataPackageCor()
    {
        while (true)
        {
            SendDataPackageLoop();
            yield return new WaitForFixedUpdate();
        }
    }

    public void SendDataPackage(DataPackage<OutgoingData> livePackage)
    {
        if (sendMute)
        {
            return;
        }
        //update history
        if (packageHistory.Count < streamSenderConfig.historySize)
        {
            packageHistory.Add(livePackage);
        }
        else
        {
            if (packageHistory.Count > 0)
            {
                packageHistory.RemoveAt(0);
            }
            packageHistory.Add(livePackage);
        }

        //create final package with history added
        DataPackageHistory<OutgoingData> dataHistory = new DataPackageHistory<OutgoingData>();
        dataHistory.dataPackageHistory.AddRange(packageHistory);
        //send it to transport
        if (signalRController.multiplayerConfig.gameAuth ==  GameAuth.Server)
        {
            signalRController.EmitToClients(emitEventName, dataHistory);
        }
        else
        {
            signalRController.EmitToServer(emitEventName, dataHistory);
        }
    }
}

[Serializable]
public class DataInstance<T>
{
    public T data;
    public int instanceId;
}

[Serializable]
public class DataPackage<T>
{
    public List<DataInstance<T>> dataStream = new List<DataInstance<T>>();
    public int packageId;
    public int currentlyProcessed = -1;
}

[Serializable]
public class DataPackageHistory<T>
{
    public List<DataPackage<T>> dataPackageHistory = new List<DataPackage<T>>();
}