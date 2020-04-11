using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMultiplayerController<GSM,PSM,IM,PIM> : MonoBehaviour
{
    public TransportController<PIM> signalRController;
    GameStateSenderController<GSM, PSM, IM, PIM> stateStreamer;
    GameStateReceiverController<GSM,PSM,IM, PIM> stateReceiver;

    public MultiplayerConfigModel multiplayerConfigModel;

    public GameObject masterControllerPrefab;

    public abstract void OnSpawnMatch(GSM gsm);
    public abstract PSM GetSpawnPlayerState(int playerIndex);
    public abstract GSM SampleGameState();
    public abstract void SetGameState(GSM gsm);
    public abstract void OnMatchConnected();

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    public abstract PSM GetClientPlayerState();

    public void ConnectToMatch(PIM init) {
        signalRController = new TransportController<PIM>(multiplayerConfigModel, init, MatchConnected);

        if (multiplayerConfigModel.gameAuth == GameAuth.Server)
        {
            InitializeServer();
        }
        else
        {
            InitializeClient();
        }
    }

    public void MatchConnected(BestHTTP.SignalR.Connection con)
    {
        OnMatchConnected();
    }

    void InitializeServer() {
        signalRController.OnOtherJoined(PlayerJoined);
        stateStreamer = new GameStateSenderController<GSM, PSM, IM, PIM>(this);
    }

    void InitializeClient() {
        signalRController.OnFromServer("start", OnStartMatch);
        stateReceiver =new GameStateReceiverController<GSM, PSM, IM, PIM>(this);
    }

    public IMasterController<PSM,IM, PIM>[] masterControllerDic = new IMasterController<PSM, IM, PIM>[0];
    public PlayerStatePack<PSM,PIM>[] playerInitDic = new PlayerStatePack<PSM, PIM>[0];

    public PlayerStatePack<PSM, PIM>[] SamplePlayerStates()
    {
        PlayerStatePack<PSM, PIM>[] result = new PlayerStatePack<PSM, PIM>[masterControllerDic.Length];

        for(int i=0; i< masterControllerDic.Length; i++)
        {
            result[i] = masterControllerDic[i].liveController.SamplePlayerState();
        }

        return result;
    }

    public void SpawnMatch(GameStatePack<GSM,PSM, PIM> startMatchData)
    {
        SpawnPlayers(startMatchData.playerStates);
        OnSpawnMatch(startMatchData.gameState);
    }

    public abstract GSM GetSpawnGameSate();

    void SpawnPlayers(PlayerStatePack<PSM, PIM>[] players)
    {
        masterControllerDic = new IMasterController<PSM, IM, PIM>[players.Length];
        for (int i=0; i<players.Length; i++)
        {
            GameObject g = Instantiate(masterControllerPrefab);
            IMasterController<PSM, IM, PIM> pu = g.GetComponent<IMasterController<PSM, IM, PIM>>();
            masterControllerDic[i] = pu;
            pu.SetPlayerInit(players[i].playerInit);
            if (multiplayerConfigModel.gameAuth == GameAuth.Client)
            {
                if (signalRController.connectionId == players[i].conId)
                {
                    pu.Initialize(signalRController, players[i], true);
                }
                else
                {
                    pu.Initialize(signalRController, players[i], false);
                }
            }
            else
            {
                pu.Initialize(signalRController, players[i], false);
            }
        }

    }

    public void PlayerJoined(string conId, string auth,PIM init)
    {
        PlayerStatePack<PSM,PIM> ps = new PlayerStatePack <PSM, PIM>();
        PlayerStatePack<PSM, PIM>[] newPI = new PlayerStatePack<PSM, PIM>[playerInitDic.Length + 1];
        for(int i=0; i<playerInitDic.Length; i++)
        {
            newPI[i] = playerInitDic[i];
        }

        ps.playerState = GetSpawnPlayerState(playerInitDic.Length);
        ps.conId = conId;
        ps.playerInit = init;
        newPI[playerInitDic.Length] =ps;
        playerInitDic = newPI;
    }

    public void InitiateMatch()
    {
        GameStatePack<GSM,PSM, PIM> smd = new GameStatePack<GSM,PSM, PIM>();
        smd.gameState = GetSpawnGameSate();
        smd.playerStates = playerInitDic;
        signalRController.EmitToClients("start", smd);
        SpawnMatch(smd);
        stateStreamer.StartStream("gameState");
    }

    public void OnStartMatch(string eventName, string connectionId, object eventData)
    {
        SpawnMatch((GameStatePack<GSM,PSM,PIM>)eventData);
        stateReceiver.StartReception("gameState");
    }

    public void ProcessGameStatePack(GameStatePack<GSM, PSM, PIM> gameStateData)
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

public struct PlayerStatePack<PSM,PIM>
{
    public int tick;
    public string conId;
    public PSM playerState;
    public PIM playerInit;
}


public struct GameStatePack<GSM,PSM,PIM>
{
    public PlayerStatePack<PSM,PIM>[] playerStates;
    public GSM gameState;
}

public struct InputPack<IM>
{
    public int tick;
    public string connectionId;
    public IM inputData;
    public ServerEventRequest[] serverEventRequests;
}

public struct ServerEventRequest
{
    public List<object> requestInstances;
}