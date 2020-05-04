using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Ceras.Formatters.AotGenerator;

namespace JAMLib
{
    [GenerateFormatter]
    public struct PlayerStatePack
    {
        public int tick;
        public string conId;
        public PlayerStateModel playerState;
        public PlayerInitModel playerInit;
    }

    [GenerateFormatter]
    public struct ServerMessagePack
    {
        public List<PlayerStatePack> playerStates;
        public WorldStateModel worldState;
    }

    [GenerateFormatter]
    public struct ClientMessagePack
    {
        public int tick;
        public string connectionId;
        public PlayerInputModel inputData;
        public ServerEventRequestModel serverEventRequestModel;
    }

    [GenerateFormatter]
    public struct ServerEventRequestModel
    {
        public List<ServerEventRequest> serverEventRequests;

        public void Init()
        {
            serverEventRequests = new List<ServerEventRequest>();
            for (int i = 0; i < 1; i++)
            {
                ServerEventRequest ser = new ServerEventRequest();
                ser.requestMessages = new List<string>();
                this.serverEventRequests.Add(ser);
            }
        }
    }

    [GenerateFormatter]
    public struct ServerEventRequest
    {
        public List<string> requestMessages;
    }

    [GenerateFormatter]
    public struct TickModel
    {
        public int tick;
        public PlayerInputModel input;
        public PlayerStateModel state;
    }

    [GenerateFormatter]
    public struct DataInstance
    {
        public string data;
        public int instanceId;
    }
    [GenerateFormatter]
    public struct DataPackage
    {
        public List<DataInstance> dataStream;
        public int packageId;
    }

    [GenerateFormatter]
    public struct DataPackageHistory
    {
        public List<DataPackage> dataPackageHistory;
    }

    public enum GameAuth
    {
        Server,
        Client
    }


    [GenerateFormatter]
    public struct NetworkQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion GetQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }

        public void SetQuaternion(Quaternion q)
        {
            this.x = q.x;
            this.y = q.y;
            this.z = q.z;
            this.w = q.w;
        }
    }
    [GenerateFormatter]
    public struct NetworkVector2
    {
        public float x;
        public float y;

        public Vector2 GetVector2()
        {
            return new Vector2(x, y);
        }

        public void SetVector2(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
        }
    }
    [GenerateFormatter]
    public struct NetworkVector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3 GetVector3()
        {
            return new Vector3(x, y, z);
        }
        public void SetVector3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }
    }

}
