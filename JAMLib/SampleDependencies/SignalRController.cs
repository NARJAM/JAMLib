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
        public string hubName = "flow";
        public string serverUrl = "http://localhost:59474/";

        OnJoinedRoom onCon;
        public override void JoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom _onCon)
        {
            onCon = _onCon;
            Uri uri = new Uri(serverUrl + "signalr");
            signalRConnection = new Connection(uri, hubName);

            ObservableDictionary<string, string> queryParams = new ObservableDictionary<string, string>();
            queryParams.Add("gameAuth", gameAuth);
            queryParams.Add("initData", JsonUtility.ToJson(initData));

            signalRConnection.AdditionalQueryParams = queryParams;
            signalRConnection.Open();

            signalRConnection.OnConnected += OnConnected;
            signalRConnection[hubName].On("OnSelfJoined", OnSelfJoined);
            signalRConnection[hubName].On("OnSessionJoined", OnPlayerJoined);
            signalRConnection[hubName].On("OnSessionLeft", OnPlayerLeft);
            signalRConnection[hubName].On("ReceiveFromClient", ReceiveFromClient);
            signalRConnection[hubName].On("ReceiveFromServer", ReceiveFromServer);
        }

        public void OnConnected(Connection con)
        {
            signalRConnection = con;
            onCon.Invoke();
        }

        public override void SendToClients(string eventName, string dataString)
        {
            signalRConnection[hubName].Call("SendToClients", eventName, dataString);
        }

        public override void SendToServer(string eventName, string dataString)
        {
            signalRConnection[hubName].Call("SendToServer", eventName, dataString);
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
            base.ReceiveFromClient(eventName, connectionId, eventData);
        }

        void ReceiveFromServer(Hub hub, MethodCallMessage msg)
        {
            string eventName = msg.Arguments[0].ToString();
            string connectionId = msg.Arguments[1].ToString();
            string eventData = msg.Arguments[2].ToString();
            base.ReceiveFromServer(eventName, connectionId, eventData);
        }

    }
}