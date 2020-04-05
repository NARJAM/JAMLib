using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SignalR;
using BestHTTP.SignalR.Hubs;
using BestHTTP.SignalR.Messages;
using System;
using UnityEngine.UI;
using PlatformSupport.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using OdinSerializer;

public class TransportController
{
    public string connectionId;

    Connection signalRConnection;
    public MultiplayerConfigModel multiplayerConfig;

    public TransportController(MultiplayerConfigModel _multiJAMConfig)
    {
        multiplayerConfig = _multiJAMConfig;
        Uri uri = new Uri(_multiJAMConfig.serverURL + "signalr");
        signalRConnection = new Connection(uri, _multiJAMConfig.hubName);
        ObservableDictionary<string, string> queryParams = new ObservableDictionary<string, string>();

        queryParams.Add("gameAuth", _multiJAMConfig.gameAuth.ToString());
        signalRConnection.AdditionalQueryParams = queryParams;
        signalRConnection.Open();

        signalRConnection.OnConnected += (con) => Debug.Log("Connected to the SignalR server! " + con.Hubs[0].Name);
        signalRConnection.OnClosed += (con) => Debug.Log("Connection Closed");
        signalRConnection.OnError += (con, err) => Debug.Log("Error: " + err);
        signalRConnection.OnNonHubMessage += (con, data) => Debug.Log("Message from server: " + data.ToString());
        
        signalRConnection[_multiJAMConfig.hubName].On("OnSessionJoined", OnSessionJoined);
        signalRConnection[_multiJAMConfig.hubName].On("OnSelfJoined", OnSelfJoined);
        signalRConnection[_multiJAMConfig.hubName].On("OnSessionLeft", OnSessionLeft);
        signalRConnection[_multiJAMConfig.hubName].On("ReceiveFromClient", ReceiveFromClient);
        signalRConnection[_multiJAMConfig.hubName].On("ReceiveFromServer", ReceiveFromServer);
        signalRConnection[_multiJAMConfig.hubName].On("ErrorReceived", ErrorReceived);
    }

    void OnSelfJoined(Hub hub, MethodCallMessage msg)
    {
        connectionId = msg.Arguments[0].ToString();
        string gameAuth = msg.Arguments[1].ToString();
        Debug.Log("OnSelfJoined : " + connectionId + " GameAuth: " + gameAuth);

        for(int i=0; i<onSelfJoinedList.Count; i++)
        {
            onSelfJoinedList[i].Invoke(connectionId, gameAuth);
        }
    }

    void OnSessionJoined(Hub hub, MethodCallMessage msg)
    {
        string conId = msg.Arguments[0].ToString();
        string gameAuth = msg.Arguments[1].ToString();
        Debug.Log("OnSessionJoined: " + conId + " GameAuth: " + gameAuth);

        for (int i = 0; i < onPlayerJoinedList.Count; i++)
        {
            onPlayerJoinedList[i].Invoke(conId, gameAuth);
        }
    }

    void OnSessionLeft(Hub hub, MethodCallMessage msg)
    {
        Debug.Log("OnSessionLeft: " + msg.Arguments[0]);
    }

    void ReceiveFromClient(Hub hub, MethodCallMessage msg)
    {
        string eventName = msg.Arguments[0].ToString();
        string connectionId = msg.Arguments[1].ToString();
        string eventData = msg.Arguments[2].ToString();

        OnNodeEvent3 callback;

        byte[] bytes = Convert.FromBase64String(eventData);
        object eventObj = SerializationUtility.DeserializeValue<object>(bytes, DataFormat.Binary);

        if (onFromClientDic.TryGetValue(eventName, out callback))
        {
            callback.Invoke(eventName,connectionId, eventObj);
        }
    }

    void ReceiveFromServer(Hub hub, MethodCallMessage msg)
    {
        string evenName = msg.Arguments[0].ToString();
        string connectionId = msg.Arguments[1].ToString();
        string eventData = msg.Arguments[2].ToString();
        OnNodeEvent3 callback;

        byte[] bytes = Convert.FromBase64String(eventData);
        object eventObj = SerializationUtility.DeserializeValue<object>(bytes, DataFormat.Binary);

        if (onFromServerDic.TryGetValue(evenName, out callback))
        {
            callback.Invoke(evenName, connectionId, eventObj);
        }
    }

    byte[] ObjectToByteArray(object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        using (var ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public delegate void OnNodeEvent1();
    OnNodeEvent1 onConnectedNodeEvent;


    public delegate void OnNodeEvent2(string conID, string auth);

    public void OnConnectedToHub(OnNodeEvent1 onNodeEvent)
    {
        onConnectedNodeEvent = onNodeEvent;
    }

    void ErrorReceived(Hub hub, MethodCallMessage msg)
    {
        string message = msg.Arguments[0].ToString();

        Debug.Log("ErrorReceived: " + message);
    }
    
    public delegate void OnNodeEvent3(string eventName, string connectionId, object eventData);

    Dictionary<string, OnNodeEvent3> onFromServerDic = new Dictionary<string, OnNodeEvent3>();
    Dictionary<string, OnNodeEvent3> onFromClientDic = new Dictionary<string, OnNodeEvent3>();
    List<OnNodeEvent2> onPlayerJoinedList = new List<OnNodeEvent2>();
    List<OnNodeEvent2> onSelfJoinedList = new List<OnNodeEvent2>();

    //Set1
    public void EmitToClients(string eventName, object eventData)
    {
        byte[] bytes = SerializationUtility.SerializeValue(eventData, DataFormat.Binary);
        string dataString = Convert.ToBase64String(bytes);
        signalRConnection[multiplayerConfig.hubName].Call("SendToClients", eventName, dataString);
    }

    public void OnFromServer(string eventName, OnNodeEvent3 onNodeEvent)
    {
        if (onFromServerDic.ContainsKey(eventName))
        {
            onFromServerDic.Remove(eventName);
        }

        onFromServerDic.Add(eventName, onNodeEvent);
    }

    //Set2
    public void EmitToServer(string eventName, object eventData)
    {
        byte[] bytes = SerializationUtility.SerializeValue(eventData, DataFormat.Binary);
        string dataString = Convert.ToBase64String(bytes);
        signalRConnection[multiplayerConfig.hubName].Call("SendToServer", eventName, dataString);
    }

    public void OnFromClient(string eventName, OnNodeEvent3 onNodeEvent)
    {
        if (onFromClientDic.ContainsKey(eventName))
        {
            onFromClientDic.Remove(eventName);
        }

        onFromClientDic.Add(eventName, onNodeEvent);
    }

    public void OnOtherJoined(OnNodeEvent2 onNodeEvent)
    {
        onPlayerJoinedList.Add(onNodeEvent);
    }

    public void OnSelfJoined(OnNodeEvent2 onNodeEvent)
    {
        onSelfJoinedList.Add(onNodeEvent);
    }


    public void OffFromServer(string eventName)
    {
        onFromServerDic.Remove(eventName);
    }

    public void OffFromClient(string eventName)
    {
        onFromClientDic.Remove(eventName);
    }

}

public enum GameAuth
{
    Server,
    Client
}
