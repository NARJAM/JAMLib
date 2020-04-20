using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JAMLib
{
    public abstract class StreamSenderController<OutgoingData>
    {
        [Header("Config")]
        public List<DataPackage> packageHistory = new List<DataPackage>();
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
            Debug.Log("StartStream " + eventName);
            emitEventName = eventName;
            if (sdpc != null)
            {
                looper.StopCoroutine(sdpc);
            }
            sdpc = looper.StartCoroutine(SendDataPackageCor());
        }


        IEnumerator SendDataPackageCor()
        {
            while (true)
            {
                DataPackage currentPackageSend = new DataPackage();
                List<DataInstance> dataStreamCache = new List<DataInstance>();

                for (int i = 0; i <= streamSenderConfig.sendRate; i++)
                {
                    DataInstance di = new DataInstance();
                    di.data = GetData();
                    di.instanceId = instancesSentCount;
                    instancesSentCount++;
                    dataStreamCache.Add(di);
                    yield return new WaitForFixedUpdate();
                }

                currentPackageSend.packageId = packagesSentCount;
                packagesSentCount++;
                currentPackageSend.dataStream = dataStreamCache.ToArray();
                SendDataPackage(currentPackageSend);
            }
        }

        public void SendDataPackage(DataPackage livePackage)
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
            DataPackageHistory dataHistory = new DataPackageHistory();
            dataHistory.dataPackageHistory = (packageHistory).ToArray();

            //send it to transport
            if (IMultiplayerController.gameAuth == GameAuth.Server)
            {
                IMultiplayerController.m_instance.transportController.IEmitToClients(emitEventName, dataHistory);
            }
            else
            {
                IMultiplayerController.m_instance.transportController.IEmitToServer(emitEventName, dataHistory);
            }
        }
    }
}