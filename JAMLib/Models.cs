using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JAMLib
{
    [Serializable]
    public struct PlayerStatePack
    {
        public int tick;
        public string conId;
        public PlayerStateModel playerState;
        public PlayerInitModel playerInit;
    }

    [System.Serializable]
    public struct ServerMessagePack
    {
        public PlayerStatePack[] playerStates;
        public WorldStateModel worldState;
    }

    [System.Serializable]
    public struct ClientMessagePack
    {
        public int tick;
        public string connectionId;
        public PlayerInputModel inputData;
        public ServerEventRequest[] serverEventRequests;
    }

    [System.Serializable]
    public struct ServerEventRequest
    {
        public string[] requestMessages;
    }

    [System.Serializable]
    public struct TickModel
    {
        public int tick;
        public PlayerInputModel input;
        public PlayerStateModel state;
    }

    [Serializable]
    public struct DataInstance
    {
        public object data;
        public int instanceId;
    }

    [Serializable]
    public struct DataPackage
    {
        public DataInstance[] dataStream;
        public int packageId;
    }

    [Serializable]
    public struct DataPackageHistory
    {
        public DataPackage[] dataPackageHistory;
    }

    public enum GameAuth
    {
        Server,
        Client
    }
}

