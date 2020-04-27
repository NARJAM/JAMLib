using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public abstract class ITransportController
    {
        public abstract void JoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom onConnectedEvent);
        public abstract void SendToClients(string eventName, string dataString);
        public abstract void SendToServer(string eventName, string dataString);

        public string connectionId;
        Dictionary<string, StreamMessageReceivedEvent> onFromServerDic = new Dictionary<string, StreamMessageReceivedEvent>();
        Dictionary<string, StreamMessageReceivedEvent> onFromClientDic = new Dictionary<string, StreamMessageReceivedEvent>();

        List<OnPlayerJoinedEvent> onPlayerJoinedList = new List<OnPlayerJoinedEvent>();
        List<OnPlayerLeftEvent> onPlayerLeftList = new List<OnPlayerLeftEvent>();

        public delegate void StreamMessageReceivedEvent(string eventName, string connectionId, DataPackageHistory eventData);

        public delegate void OnJoinedRoom();
        public delegate void OnPlayerJoinedEvent(string conId, string auth, PlayerInitModel init);
        public delegate void OnPlayerLeftEvent(string conId);


        public void IJoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom onConnectedEvent)
        {
            JoinRoom(initData, gameAuth, onConnectedEvent);
        }

        public void IOnFromServer(string eventName, StreamMessageReceivedEvent onNodeEvent)
        {
            if (onFromServerDic.ContainsKey(eventName))
            {
                onFromServerDic.Remove(eventName);
            }

            onFromServerDic.Add(eventName, onNodeEvent);
        }

        public void IOnFromClient(string eventName, StreamMessageReceivedEvent onNodeEvent)
        {
            if (onFromClientDic.ContainsKey(eventName))
            {
                onFromClientDic.Remove(eventName);
            }

            onFromClientDic.Add(eventName, onNodeEvent);
        }

        public void IOnPlayerJoined(OnPlayerJoinedEvent onNodeEvent)
        {
            onPlayerJoinedList.Add(onNodeEvent);
        }

        public void IOnSelfJoined(OnPlayerJoinedEvent onNodeEvent)
        {
            onPlayerJoinedList.Add(onNodeEvent);
        }

        public void IOnPlayerLeft(OnPlayerLeftEvent onNodeEvent)
        {
            onPlayerLeftList.Add(onNodeEvent);
        }

        public void IOffFromServer(string eventName)
        {
            onFromServerDic.Remove(eventName);
        }

        public void IOffFromClient(string eventName)
        {
            onFromClientDic.Remove(eventName);
        }

        public void IEmitToClients<T>(string eventName, T eventData)
        {
            SendToClients(eventName, IMultiplayerController.m_instance.serializer.Serialize<T>(eventData));
        }

        public void IEmitToServer<T>(string eventName, T eventData)
        {
            SendToServer(eventName, IMultiplayerController.m_instance.serializer.Serialize<T>(eventData));
        }

        #region AutoResponses
        public void ReceiveFromClient(string eventName, string connectionId, string eventData)
        {
            StreamMessageReceivedEvent callback;
            DataPackageHistory eventObj = new DataPackageHistory();

            IMultiplayerController.m_instance.serializer.Deserialize<DataPackageHistory>(eventData,  ref eventObj);

            if (onFromClientDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventObj);
            }
        }

        public void ReceiveFromServer(string eventName, string connectionId, string eventData)
        {
            StreamMessageReceivedEvent callback;
            DataPackageHistory eventObj = new DataPackageHistory();
            IMultiplayerController.m_instance.serializer.Deserialize<DataPackageHistory>(eventData, ref eventObj);
            if (onFromServerDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventObj);
            }
        }

        public void OnPlayerJoined(string conId, string gameAuth, PlayerInitModel initData)
        {
            for (int i = 0; i < onPlayerJoinedList.Count; i++)
            {
                onPlayerJoinedList[i].Invoke(conId, gameAuth, initData);
            }
        }

        public void OnPlayerLeft(string conId)
        {
            for (int i = 0; i < onPlayerLeftList.Count; i++)
            {
                onPlayerLeftList[i].Invoke(conId);
            }
        }

        #endregion

    }

}