using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SignalR;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR.Messages;
using System;
using PlatformSupport.Collections.ObjectModel;

namespace JAMLib
{
    public class SignalRController : ITransportController
    {
        Connection signalRConnection;

        OnJoinedRoom onCon;
        public override void JoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom _onCon)
        {
            onCon = _onCon;
            Uri uri = new Uri(IMultiplayerController.config.serverUrl + "signalr");
            signalRConnection = new Connection(uri, IMultiplayerController.config.hubName);

            ObservableDictionary<string, string> queryParams = new ObservableDictionary<string, string>();
            queryParams.Add("gameAuth", gameAuth);
            queryParams.Add("initData", JsonUtility.ToJson(initData));

            signalRConnection.AdditionalQueryParams = queryParams;
            signalRConnection.Open();

            signalRConnection.OnConnected += OnConnected;
            signalRConnection[IMultiplayerController.config.hubName].On("OnSelfJoined", OnSelfJoined);
            signalRConnection[IMultiplayerController.config.hubName].On("OnSessionJoined", OnPlayerJoined);
            signalRConnection[IMultiplayerController.config.hubName].On("OnSessionLeft", OnPlayerLeft);
            signalRConnection[IMultiplayerController.config.hubName].On("ReceiveFromClient", ReceiveFromClient);
            signalRConnection[IMultiplayerController.config.hubName].On("ReceiveFromServer", ReceiveFromServer);
        }

        public void OnConnected(Connection con)
        {
            signalRConnection = con;
            onCon.Invoke();
        }

        public override void SendToClients(string eventName, DataPackageHistory<ServerMessagePack> data)
        {
            string dataString = IMultiplayerController.m_instance.serializer.Serialize(data);
            signalRConnection[IMultiplayerController.config.hubName].Call("SendToClients", eventName, dataString);
        }

        public override void SendToServer(string eventName, DataPackageHistory<ClientMessagePack> data)
        {
            string dataString = IMultiplayerController.m_instance.serializer.Serialize(data);
            signalRConnection[IMultiplayerController.config.hubName].Call("SendToServer", eventName, dataString);
        }

        void OnSelfJoined(Hub hub, MethodCallMessage msg)
        {
            string conId = msg.Arguments[0].ToString();

            Debug.Log("On Self Joined" + conId);
            base.connectionId = conId;
        }

        void OnPlayerJoined(Hub hub, MethodCallMessage msg)
        {
            string conId = msg.Arguments[0].ToString();
            string gameAuth = msg.Arguments[1].ToString();

            PlayerInitModel initData = default;
            initData = LitJson.JsonMapper.ToObject<PlayerInitModel>(msg.Arguments[2].ToString());

            base.OnPlayerJoined(conId, gameAuth, initData);
        }

        void OnPlayerLeft(Hub hub, MethodCallMessage msg)
        {
            string conId = msg.Arguments[0].ToString();

            base.OnPlayerLeft(conId);
        }

        void ReceiveFromClient(Hub hub, MethodCallMessage msg)
        {
            string eventName = msg.Arguments[0].ToString();
            string connectionId = msg.Arguments[1].ToString();
            string eventData = msg.Arguments[2].ToString();

            DataPackageHistory<ClientMessagePack> dh = IMultiplayerController.m_instance.serializer.Deserialize<DataPackageHistory<ClientMessagePack>>(eventData);
            base.ReceiveFromClient(eventName, connectionId, dh);
        }

        void ReceiveFromServer(Hub hub, MethodCallMessage msg)
        {
            string eventName = msg.Arguments[0].ToString();
            string connectionId = msg.Arguments[1].ToString();
            string eventData = msg.Arguments[2].ToString(); 
            
            DataPackageHistory<ServerMessagePack> dh = IMultiplayerController.m_instance.serializer.Deserialize<DataPackageHistory<ServerMessagePack>>(eventData);
            base.ReceiveFromServer(eventName, connectionId, dh);
        }

    }
}