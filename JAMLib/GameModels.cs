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
    public int foodObjectType;
    
    [Key(3)]
    public bool canTravel;

    [Key(4)]
    public bool isMoving;

    [Key(5)]
    public double initialSize;

    [Key(6)]
    public double floatingSpeed;

    [Key(7)]
    public double rotationSpeed;

    [Key(8)]
    public NetworkQuaternion foodBoomRotation;

    [Key(9)]
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
    public FoodData foodData;
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

    [Key(3)]
    public int cargoColorType;

    [Key(4)]
    public int uid;
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

    [Key(3)]
    public NetworkQuaternion playerSelfRotation;

    [Key(4)]
    public int vykerVisionDuration;
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
    public Vector2 direction;
}

[MessagePackObject]
public struct PlayerInitModel
{
    [Key(0)]
    public int vykerColor;

    [Key(1)]
    public string userName;
}
