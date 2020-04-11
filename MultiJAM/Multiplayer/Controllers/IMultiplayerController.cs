using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMultiplayerController<GSM,PSM,IM> : MonoBehaviour
{
    public TransportController signalRController;
    GameStateSenderController<GSM, PSM, IM> stateStreamer;
    GameStateReceiverController<GSM,PSM,IM> stateReceiver;

    public MultiplayerConfigModel multiplayerConfigModel;

    public GameObject masterControllerPrefab;

    public abstract void OnSpawnMatch(GSM gsm);
    public abstract PSM GetSpawnPlayerState(int playerIndex);
    public abstract GSM SampleGameState();
    public abstract void SetGameState(GSM gsm);

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        signalRController = new TransportController(multiplayerConfigModel);

        if (multiplayerConfigModel.gameAuth == GameAuth.Server)
        {
            InitializeServer();
        }
        else
        {
            InitializeClient();
        }
    }

    void InitializeServer() {
        signalRController.OnOtherJoined(PlayerJoined);
        stateStreamer = new GameStateSenderController<GSM, PSM, IM>(this);
    }

    void InitializeClient() {
        signalRController.OnFromServer("start", OnStartMatch);
        stateReceiver =new GameStateReceiverController<GSM, PSM, IM>(this);
    }

    public IMasterController<PSM,IM>[] masterControllerDic = new IMasterController<PSM, IM>[0];
    public PlayerStatePack<PSM>[] playerInitDic = new PlayerStatePack<PSM>[0];

    public PlayerStatePack<PSM>[] SamplePlayerStates()
    {
        PlayerStatePack<PSM>[] result = new PlayerStatePack<PSM>[masterControllerDic.Length];

        for(int i=0; i< masterControllerDic.Length; i++)
        {
            result[i] = masterControllerDic[i].liveController.SamplePlayerState();
        }

        return result;
    }

    public void SpawnMatch(GameStatePack<GSM,PSM> startMatchData)
    {
        SpawnPlayers(startMatchData.playerStates);
        OnSpawnMatch(startMatchData.gameState);
    }

    public abstract GSM GetSpawnGameSate();

    void SpawnPlayers(PlayerStatePack<PSM>[] players)
    {
        masterControllerDic = new IMasterController<PSM, IM>[players.Length];
        for (int i=0; i<players.Length; i++)
        {
            GameObject g = Instantiate(masterControllerPrefab);
            IMasterController<PSM, IM> pu = g.GetComponent<IMasterController<PSM, IM>>();
            masterControllerDic[i] = pu;

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

    public void PlayerJoined(string conId, string auth)
    {
        PlayerStatePack<PSM> ps = new PlayerStatePack <PSM>();
        PlayerStatePack<PSM>[] newPI = new PlayerStatePack<PSM>[playerInitDic.Length + 1];
        for(int i=0; i<playerInitDic.Length; i++)
        {
            newPI[i] = playerInitDic[i];
        }

        ps.playerState = GetSpawnPlayerState(playerInitDic.Length);
        ps.conId = conId;
        newPI[playerInitDic.Length] =ps;
        playerInitDic = newPI;
    }

    public void InitiateMatch()
    {
        GameStatePack<GSM,PSM> smd = new GameStatePack<GSM,PSM>();
        smd.gameState = GetSpawnGameSate();
        smd.playerStates = playerInitDic;
        signalRController.EmitToClients("start", smd);
        SpawnMatch(smd);
        stateStreamer.StartStream("gameState");
    }

    public void OnStartMatch(string eventName, string connectionId, object eventData)
    {
        SpawnMatch((GameStatePack<GSM,PSM>)eventData);
        stateReceiver.StartReception("gameState");
    }

    public void ProcessGameStatePack(GameStatePack<GSM, PSM> gameStateData)
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

public struct PlayerStatePack<PSM>
{
    public int tick;
    public string conId;
    public PSM playerState;
}


public struct GameStatePack<GSM, PSM>
{
    public PlayerStatePack<PSM>[] playerStates;
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