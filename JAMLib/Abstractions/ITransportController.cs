using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public abstract class ITransportController
    {
        public abstract void JoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom onConnectedEvent);
        public abstract void SendToClients(string eventName, DataPackageHistory<ServerMessagePack> data);
        public abstract void SendToServer(string eventName, DataPackageHistory<ClientMessagePack> data);

        public string connectionId;
        Dictionary<string, StreamServerMessageReceivedEvent> onFromServerDic = new Dictionary<string, StreamServerMessageReceivedEvent>();
        Dictionary<string, StreamClientMessageReceivedEvent> onFromClientDic = new Dictionary<string, StreamClientMessageReceivedEvent>();

        List<OnPlayerJoinedEvent> onPlayerJoinedList = new List<OnPlayerJoinedEvent>();
        List<OnPlayerLeftEvent> onPlayerLeftList = new List<OnPlayerLeftEvent>();

        public delegate void StreamClientMessageReceivedEvent(string eventName, string connectionId, DataPackageHistory<ClientMessagePack> eventData);
        public delegate void StreamServerMessageReceivedEvent(string eventName, string connectionId, DataPackageHistory<ServerMessagePack> eventData);

        public delegate void OnJoinedRoom();
        public delegate void OnPlayerJoinedEvent(string conId, string auth, PlayerInitModel init);
        public delegate void OnPlayerLeftEvent(string conId);


        public void IJoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom onConnectedEvent)
        {
            JoinRoom(initData, gameAuth, onConnectedEvent);
        }

        public void IOnFromServer(string eventName, StreamServerMessageReceivedEvent onNodeEvent)
        {
            if (onFromServerDic.ContainsKey(eventName))
            {
                onFromServerDic.Remove(eventName);
            }

            onFromServerDic.Add(eventName, onNodeEvent);
        }

        public void IOnFromClient(string eventName, StreamClientMessageReceivedEvent onNodeEvent)
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

        public void IEmitToClients(string eventName, DataPackageHistory<ServerMessagePack> eventData)
        {
            SendToClients(eventName, eventData);
        }

        public void IEmitToServer(string eventName, DataPackageHistory<ClientMessagePack> eventData)
        {
            SendToServer(eventName, eventData);
        }

        #region AutoResponses
        public void ReceiveFromClient(string eventName, string connectionId, DataPackageHistory<ClientMessagePack> eventData)
        {
            StreamClientMessageReceivedEvent callback;

            if (onFromClientDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventData);
            }
        }

        public void ReceiveFromServer(string eventName, string connectionId, DataPackageHistory<ServerMessagePack> eventData)
        {
            StreamServerMessageReceivedEvent callback;
            if (eventName == null)
            {
                eventName = "";
            }
            if (onFromServerDic.TryGetValue(eventName, out callback))
            {
                callback.Invoke(eventName, connectionId, eventData);
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