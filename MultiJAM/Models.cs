using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct PlayerStatePack
{
    public int tick;
    public string conId;
    public PlayerStateModel playerState;
    public PlayerInitModel playerInit;
}

[System.Serializable]
public struct GameStatePack
{
    public PlayerStatePack[] playerStates;
    public GameStateModel gameState;
}

[System.Serializable]
public struct InputPack
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

[Serializable]
public struct FoodData
{
    public int id;
    public int foodColorType;
    public bool isMoving;
    public double initialSize;
    public double floatingSpeed;
    public double rotationSpeed;
    public Quaternion foodBoomRotation;
    public Quaternion foodSelfRotation;
}

[System.Serializable]
public struct GameStateModel
{
    public FoodData[] foodDatas;
}

[System.Serializable]
public struct PlayerInitModel
{
    public int vykerColor;
    public string userName;
}
[Serializable]
public struct PlayerStateModel
{
    public Quaternion playerBoomRot;
    public Quaternion cameraBoomRot;
    public CargoTrainStateModel cargoTrainModel;
}

[Serializable]
public struct CargoTrainStateModel
{
    public CargoStateModel[] cargoDataList;
}


[Serializable]
public struct CargoStateModel
{
    public Vector3 position;
    public Vector3 forward;
    public int pathIndex;
    public EnumData.CargoColorType cargoColorType;
}

[Serializable]
public struct PlayerInputModel
{
    public string connectionId;
    public double keyboardAxis;
    public double joyStickAxis;
    public bool isMobile;
}

public enum SERTypes
{
    FoodCollection = 0
}

[Serializable]
public struct CargoCollectionRequest
{
    public int cargoId;
    public int cargoColor;
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