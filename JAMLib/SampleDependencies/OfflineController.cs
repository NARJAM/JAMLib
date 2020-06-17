using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JAMLib;

public class OfflineController : ITransportController
{
    public override void JoinRoom(PlayerInitModel initData, string gameAuth, OnJoinedRoom onConnectedEvent)
    {
       //Nothing
    }

    public override void SendToClients(string eventName, string dataString)
    {
        Debug.Log("SendToClients");
        base.ReceiveFromServer(eventName, "", dataString);
    }

    public override void SendToServer(string eventName, string dataString)
    {
        Debug.Log("SendToServer");
        base.ReceiveFromClient(eventName, "", dataString);
    }
}
