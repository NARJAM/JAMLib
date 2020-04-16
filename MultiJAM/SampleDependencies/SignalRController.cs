using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SignalR;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR.Messages;
using System;
using PlatformSupport.Collections.ObjectModel;

public class SignalRController : ITransportController<GameStateModel, PlayerStateModel, PlayerInputModel, PlayerInitModel>
{
    Connection signalRConnection;
    public string hubName="flow";
    public string serverUrl= "http://localhost:59474/";

    OnConnectedEvent onCon;
    public override void JoinRoom(PlayerInitModel initData,string gameAuth,OnConnectedEvent _onCon)
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
        string gameAuth = msg.Arguments[1].ToString();

        base.OnPlayerLeft(conId, gameAuth);
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
