using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JAMLib
{
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
        MonoBehaviour looper;

        public StreamSenderController(MonoBehaviour _looper)
        {
            looper = _looper;
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

        DataPackage<OutgoingData> currentPackageSend;
        List<DataInstance<OutgoingData>> dataStreamCache = new List<DataInstance<OutgoingData>>();
        DataInstance<OutgoingData> di;
        IEnumerator SendDataPackageCor()
        {
            while (true)
            {
                 currentPackageSend = new DataPackage<OutgoingData>();
                 dataStreamCache.Clear();

                for (int i = 0; i <= streamSenderConfig.sendRate; i++)
                {
                    di = new DataInstance<OutgoingData>();
                    di.data = GetData();
                    di.instanceId = instancesSentCount;
                    instancesSentCount++;
                    dataStreamCache.Add(di);
                    yield return new WaitForFixedUpdate();
                }


                currentPackageSend.packageId = packagesSentCount;
                packagesSentCount++;
                currentPackageSend.dataStream = dataStreamCache;
                SendDataPackage(currentPackageSend);
            }
        }

        public void SendDataPackage(DataPackage<OutgoingData> livePackage)
        {
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
            dataHistory.dataPackageHistory = (packageHistory);

            //send it to transport
            EmitToTransport(emitEventName, dataHistory);
        }

        public abstract void EmitToTransport(string eventName, DataPackageHistory<OutgoingData> dataHistory);
    }
}