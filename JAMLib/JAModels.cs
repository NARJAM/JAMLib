using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePack;

namespace JAMLib
{
    [MessagePackObject]
    public struct PlayerStatePack
    {
        [Key(0)]
        public int tick;

        [Key(1)]
        public string conId;

        [Key(2)]
        public PlayerStateModel playerState;

        [Key(3)]
        public PlayerInitModel playerInit;

        [Key(4)]
        public bool isBot;
    }

    [MessagePackObject]
    public struct ServerMessagePack
    {
        [Key(0)]
        public List<PlayerStatePack> playerStates;

        [Key(1)]
        public WorldStateModel worldState;
    }

    [MessagePackObject]
    public struct ClientMessagePack
    {
        [Key(0)]
        public int tick;

        [Key(1)]
        public string connectionId;

        [Key(2)]
        public PlayerInputModel inputData;

        [Key(3)]
        public ServerEventRequestModel serverEventRequestModel;
    }

    [MessagePackObject]
    public struct ServerEventRequestModel
    {
        [Key(0)]
        public ServerEventRequest[] serverEventRequests;
    }

    [MessagePackObject]
    public struct ServerEventRequest
    {
        [Key(0)]
        public List<string> requestMessages;
    }

    [MessagePackObject]
    public struct TickModel
    {
        [Key(0)]
        public int tick;

        [Key(1)]
        public PlayerInputModel input;

        [Key(2)]
        public PlayerStateModel state;
    }

    [MessagePackObject]
    public struct DataInstance
    {
        [Key(0)]
        public string data;

        [Key(1)]
        public int instanceId;
    }

    [MessagePackObject]
    public struct DataPackage
    {
        [Key(0)]
        public List<DataInstance> dataStream;

        [Key(1)]
        public int packageId;
    }

    [MessagePackObject]
    public struct DataPackageHistory
    {
        [Key(0)]
        public List<DataPackage> dataPackageHistory;
    }

    public enum GameAuth
    {
        Server,
        Client
    }


    [MessagePackObject]
    public struct NetworkQuaternion
    {
        [Key(0)]
        public float x;

        [Key(1)]
        public float y;

        [Key(2)]
        public float z;

        [Key(3)]
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

    [MessagePackObject]
    public struct NetworkVector2
    {
        [Key(0)]
        public float x;

        [Key(1)]
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

    [MessagePackObject]
    public struct NetworkVector3
    {
        [Key(0)]
        public float x;

        [Key(1)]
        public float y;

        [Key(2)]
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
