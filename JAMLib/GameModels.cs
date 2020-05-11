using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JAMLib;
using MessagePack;

[MessagePackObject]
public struct FoodData
{
    [Key(0)]
    public int id;

    [Key(1)]
    public int foodColorType;

    [Key(2)]
    public bool isMoving;

    [Key(3)]
    public double initialSize;

    [Key(4)]
    public double floatingSpeed;

    [Key(5)]
    public double rotationSpeed;

    [Key(6)]
    public NetworkQuaternion foodBoomRotation;

    [Key(7)]
    public NetworkQuaternion foodSelfRotation;
}

public enum SERTypes
{
    FoodCollection = 0
}

[MessagePackObject]
public struct CargoCollectionRequest
{
    [Key(0)]
    public int cargoId;

    [Key(1)]
    public int cargoColor;
}

[MessagePackObject]
public struct CargoTrainStateModel
{
    [Key(0)]
    public List<CargoStateModel> cargoDataList;
}

[MessagePackObject]
public struct CargoStateModel
{
    [Key(0)]
    public NetworkVector3 position;

    [Key(1)]
    public NetworkVector3 forward;

    [Key(2)]
    public int pathIndex;

    [Key(3)]
    public int cargoColorType;
}

[MessagePackObject]
public struct PlayerStateModel
{
    [Key(0)]
    public NetworkQuaternion playerBoomRot;

    [Key(1)]
    public NetworkQuaternion cameraBoomRot;

    [Key(2)]
    public CargoTrainStateModel cargoTrainModel;
}

[MessagePackObject]
public struct WorldStateModel
{
    [Key(0)]
    public List<FoodData> foodDatas;
}

[MessagePackObject]
public struct PlayerInputModel
{
    [Key(0)]
    public string connectionId;

    [Key(1)]
    public double keyboardAxis;

    [Key(2)]
    public double joyStickAxis;

    [Key(3)]
    public bool isMobile;
}

[MessagePackObject]
public struct PlayerInitModel
{
    [Key(0)]
    public int vykerColor;

    [Key(1)]
    public string userName;
}
