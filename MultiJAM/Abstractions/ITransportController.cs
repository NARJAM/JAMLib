using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITransportController<GSM, PSM, IM, PIM>
{
    public abstract void JoinRoom(PIM initData, string gameAuth, OnConnectedEvent onConnectedEvent);
    public abstract void SendToClients(string eventName, string dataString);
    public abstract void SendToServer(string eventName, string dataString);

    public string connectionId;
    Dictionary<string, DataReceiveEvent> onFromServerDic = new Dictionary<string, DataReceiveEvent>();
    Dictionary<string, DataReceiveEvent> onFromClientDic = new Dictionary<string, DataReceiveEvent>();
    List<OnPlayerJoinedEvent> onPlayerJoinedList = new List<OnPlayerJoinedEvent>();
    List<OnPlayerLeftEvent> onPlayerLeftList = new List<OnPlayerLeftEvent>();

    public delegate void DataReceiveEvent(string eventName, string connectionId, object eventData);
    public delegate void OnConnectedEvent();
    public delegate void OnPlayerJoinedEvent(string conId, string auth, PIM init);
    public delegate void OnPlayerLeftEvent(string conId, string auth);

    ISerializerController serializer;

        OnConnectedEvent onConnected;
        public void IJoinRoom(PIM initData, string gameAuth, OnConnectedEvent onConnectedEvent)
        {
        JoinRoom(initData, gameAuth, onConnectedEvent);
        }

        public void IOnFromServer(string eventName, DataReceiveEvent onNodeEvent)
        {
            if (onFromServerDic.ContainsKey(eventName))
            {
                onFromServerDic.Remove(eventName);
            }

            onFromServerDic.Add(eventName, onNodeEvent);
        }

        public void IOnFromClient(string eventName, DataReceiveEvent onNodeEvent)
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
            SendToClients(eventName, serializer.Serialize(eventData));
        }

        public void IEmitToServer(string eventName, object eventData)
        {
            SendToServer(eventName, serializer.Serialize(eventData));
        }

        #region AutoResponses
        public void ReceiveFromClient(string eventName, string connectionId, string eventData)
        {
            DataReceiveEvent callback;
            object eventObj = serializer.Deserialize(eventData);

            if (onFromClientDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventObj);
            }
        }

        public void ReceiveFromServer(string eventName, string connectionId, string eventData)
        {
            DataReceiveEvent callback;
            object eventObj = serializer.Deserialize(eventData);

            if (onFromServerDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventObj);
            }
        }

        public void OnPlayerJoined(string conId, string gameAuth, PIM initData)
        {
            for (int i = 0; i < onPlayerJoinedList.Count; i++)
            {
                onPlayerJoinedList[i].Invoke(conId, gameAuth, initData);
            }
        }

        public void OnPlayerLeft(string conId, string gameAuth)
        {
            for (int i = 0; i < onPlayerJoinedList.Count; i++)
            {
                onPlayerLeftList[i].Invoke(conId, gameAuth);
            }
        }

        public void OnConnected()
        {
            onConnected.Invoke();
        }
    #endregion

}

    public enum GameAuth
    {
        Server,
        Client
    }