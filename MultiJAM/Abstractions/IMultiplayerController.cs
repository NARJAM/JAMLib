using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMultiplayerController : MonoBehaviour
{
    GameStateSenderController stateStreamer;
    GameStateReceiverController stateReceiver;
    public GameObject masterControllerPrefab;

    public static GameAuth gameAuth;

    public abstract void OnSpawnMatch(GameStateModel gsm);
    public abstract PlayerStateModel GetSpawnPlayerState(int playerIndex);
    public abstract GameStateModel SampleGameState();
    public abstract void SetGameState(GameStateModel gsm);
    public abstract void OnMatchConnected();
    public abstract ISerializerController GetSerializer();
    public abstract ITransportController GetTransportController();

    public static IMultiplayerController iinstance;

    public ISerializerController serializer;
    public ITransportController transportController;

    public IMultiplayerController()
    {
        iinstance = this;
        serializer = GetSerializer();
        transportController = GetTransportController();
    }

    public void ConnectToMatch(PlayerInitModel init) {
        if (gameAuth == GameAuth.Server)
        {
            InitializeServer(init, gameAuth);
        }
        else
        {
            InitializeClient(init, gameAuth);
        }
    }

    public void MatchConnected()
    {
        OnMatchConnected();
    }

    void InitializeServer(PlayerInitModel init, GameAuth auth)
    {
        transportController.JoinRoom(init,auth.ToString(),OnMatchConnected);
        transportController.IOnPlayerJoined(PlayerJoined);
    }

    void InitializeClient(PlayerInitModel init, GameAuth auth)
    {
        stateReceiver = new GameStateReceiverController();
        stateReceiver.InitStreamReception("gameState");
        transportController.JoinRoom(init, auth.ToString(), OnMatchConnected);
        transportController.IOnFromServer("start", OnStartMatch);
    }

    public IMasterController[] masterControllerDic = new IMasterController[0];
    public PlayerStatePack[] playerInitDic = new PlayerStatePack[0];

    public PlayerStatePack[] SamplePlayerStates()
    {
        PlayerStatePack[] result = new PlayerStatePack[masterControllerDic.Length];

        for(int i=0; i< masterControllerDic.Length; i++)
        {
            result[i] = masterControllerDic[i].liveController.SamplePlayerState();
        }

        return result;
    }

    public void SpawnMatch(GameStatePack startMatchData)
    {
        SpawnPlayers(startMatchData.playerStates);
        OnSpawnMatch(startMatchData.gameState);
    }

    public abstract GameStateModel GetSpawnGameSate();

    void SpawnPlayers(PlayerStatePack[] players)
    {
        masterControllerDic = new IMasterController[players.Length];
        for (int i=0; i<players.Length; i++)
        {
            GameObject g = Instantiate(masterControllerPrefab);
            IMasterController pu = g.GetComponent<IMasterController>();
            masterControllerDic[i] = pu;
            if (gameAuth == GameAuth.Client)
            {
                if (transportController.connectionId == players[i].conId)
                {
                    pu.Initialize(transportController, players[i], true);
                }
                else
                {
                    pu.Initialize(transportController, players[i], false);
                }
            }
            else
            {
                pu.Initialize(transportController, players[i], false);
            }
            pu.SetPlayerInit(players[i].playerInit);
        }

    }

    public void PlayerJoined(string conId, string auth,PlayerInitModel init)
    {
        PlayerStatePack ps = new PlayerStatePack ();
        PlayerStatePack[] newPI = new PlayerStatePack[playerInitDic.Length + 1];
        for(int i=0; i<playerInitDic.Length; i++)
        {
            newPI[i] = playerInitDic[i];
        }

        ps.playerState = GetSpawnPlayerState(playerInitDic.Length);
        ps.conId = conId;
        Debug.Log("Conid " + conId);
        ps.playerInit = init;
        newPI[playerInitDic.Length] =ps;
        playerInitDic = newPI;
    }

    public void InitiateMatch()
    {
        DataPackageHistory dh = new DataPackageHistory();
        DataPackage dp = new DataPackage();
        DataInstance di = new DataInstance();
        GameStatePack smd = new GameStatePack();
        smd.gameState = GetSpawnGameSate();
        smd.playerStates = playerInitDic;
        di.data = smd;
        dp.dataStream = new DataInstance[1];
        dp.dataStream[0] = di;
        dh.dataPackageHistory = new DataPackage[1];
        dh.dataPackageHistory[0] = (dp);
        transportController.IEmitToClients("start", dh);
        stateStreamer = new GameStateSenderController();
        stateStreamer.StartStream("gameState");
        SpawnMatch(smd);
    }

    public void OnStartMatch(string eventName, string connectionId, DataPackageHistory eventData)
    {
        SpawnMatch((GameStatePack)eventData.dataPackageHistory[0].dataStream[0].data);
    }

    public void ProcessGameStatePack(GameStatePack gameStateData)
    {
        for(int i=0; i<gameStateData.playerStates.Length; i++)
        {
            if (masterControllerDic[i].isOwner)
            {
                masterControllerDic[i].SetGhostState(gameStateData.playerStates[i]);
            }
            else
            {
                masterControllerDic[i].SetMirrorState(gameStateData.playerStates[i].playerState);
            }
        }
        SetGameState(gameStateData.gameState);
    }
}
