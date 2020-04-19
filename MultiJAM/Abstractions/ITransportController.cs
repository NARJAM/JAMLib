using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITransportController
{
    public abstract void JoinRoom(PlayerInitModel initData, string gameAuth, OnConnectedEvent onConnectedEvent);
    public abstract void SendToClients(string eventName, string dataString);
    public abstract void SendToServer(string eventName, string dataString);

    public string connectionId;
    Dictionary<string, DataReceiveFromServerEvent> onFromServerDic = new Dictionary<string, DataReceiveFromServerEvent>();
    Dictionary<string, DataReceiveFromClientEvent> onFromClientDic = new Dictionary<string, DataReceiveFromClientEvent>();
    List<OnPlayerJoinedEvent> onPlayerJoinedList = new List<OnPlayerJoinedEvent>();
    List<OnPlayerLeftEvent> onPlayerLeftList = new List<OnPlayerLeftEvent>();

    public delegate void DataReceiveFromServerEvent(string eventName, string connectionId,  DataPackageHistory  eventData);
    public delegate void DataReceiveFromClientEvent(string eventName, string connectionId, DataPackageHistory eventData);
   
    public delegate void OnConnectedEvent();
    public delegate void OnPlayerJoinedEvent(string conId, string auth, PlayerInitModel init);
    public delegate void OnPlayerLeftEvent(string conId);

    
        public void IJoinRoom(PlayerInitModel initData, string gameAuth, OnConnectedEvent onConnectedEvent)
        {
        JoinRoom(initData, gameAuth, onConnectedEvent);
        }

        public void IOnFromServer(string eventName, DataReceiveFromServerEvent onNodeEvent)
        {
            if (onFromServerDic.ContainsKey(eventName))
            {
                onFromServerDic.Remove(eventName);
            }

            onFromServerDic.Add(eventName, onNodeEvent);
        }

        public void IOnFromClient(string eventName, DataReceiveFromClientEvent onNodeEvent)
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

        public void IEmitToClients(string eventName, object eventData)
        {
            SendToClients(eventName,IMultiplayerController.iinstance.serializer.Serialize(eventData));
        }

        public void IEmitToServer(string eventName, object eventData)
        {
            SendToServer(eventName, IMultiplayerController.iinstance.serializer.Serialize(eventData));
        }

        #region AutoResponses
        public void ReceiveFromClient(string eventName, string connectionId, string eventData)
        {
            DataReceiveFromClientEvent callback;
            DataPackageHistory eventObj = (DataPackageHistory)IMultiplayerController.iinstance.serializer.Deserialize(eventData);

            if (onFromClientDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventObj);
            }
        }

        public void ReceiveFromServer(string eventName, string connectionId, string eventData)
        {
            DataReceiveFromServerEvent callback;
            DataPackageHistory eventObj = (DataPackageHistory)IMultiplayerController.iinstance.serializer.Deserialize (eventData);
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
            for (int i = 0; i < onPlayerJoinedList.Count; i++)
            {
                onPlayerLeftList[i].Invoke(conId);
            }
        }

    #endregion

}

    public enum GameAuth
    {
        Server,
        Client
    }