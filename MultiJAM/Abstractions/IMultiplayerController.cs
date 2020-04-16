using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class IMultiplayerController<GSM,PSM,IM,PIM> : MonoBehaviour
{
    public static IMultiplayerController<GSM, PSM, IM, PIM> iinstance;
    GameStateSenderController<GSM, PSM, IM, PIM> stateStreamer;
    GameStateReceiverController<GSM,PSM,IM, PIM> stateReceiver;
    public GameObject masterControllerPrefab;

    public static GameAuth gameAuth;

    public abstract void OnSpawnMatch(GSM gsm);
    public abstract PSM GetSpawnPlayerState(int playerIndex);
    public abstract GSM SampleGameState();
    public abstract void SetGameState(GSM gsm);
    public abstract void OnMatchConnected();
    public abstract ISerializerController GetSerializer();
    public abstract ITransportController<GSM, PSM, IM, PIM> GetTransportController();

    public ISerializerController serializer;
    public ITransportController<GSM, PSM, IM, PIM> transportController;

    private void Awake()
    {
        iinstance = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        serializer = GetSerializer();
        transportController = GetTransportController();
    }

    public void ConnectToMatch(PIM init) {

        if (gameAuth == GameAuth.Server)
        {
            InitializeServer(init, GameAuth.Server);
        }
        else
        {
            InitializeClient(init, GameAuth.Client);
        }
    }

    public void MatchConnected()
    {
        OnMatchConnected();
    }

    void InitializeServer(PIM init, GameAuth auth) {
        transportController.JoinRoom(init,auth.ToString(),OnMatchConnected);
        transportController.IOnPlayerJoined(PlayerJoined);
        stateStreamer = new GameStateSenderController<GSM, PSM, IM, PIM>();
    }

    void InitializeClient(PIM init, GameAuth auth) {
        transportController.JoinRoom(init, auth.ToString(), OnMatchConnected);
        transportController.IOnFromServer("start", OnStartMatch);
        stateReceiver =new GameStateReceiverController<GSM, PSM, IM, PIM>();
    }

    public IMasterController<GSM, PSM, IM, PIM>[] masterControllerDic = new IMasterController<GSM, PSM, IM, PIM>[0];
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
        masterControllerDic = new IMasterController<GSM, PSM, IM, PIM>[players.Length];
        for (int i=0; i<players.Length; i++)
        {
            GameObject g = Instantiate(masterControllerPrefab);
            IMasterController<GSM, PSM, IM, PIM> pu = g.GetComponent<IMasterController<GSM, PSM, IM, PIM>>();
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
        transportController.IEmitToClients("start", smd);
        SpawnMatch(smd);
        stateStreamer.StartStream("gameState");
    }

    public void OnStartMatch(string eventName, string connectionId, object eventData)
    {
        SpawnMatch((GameStatePack<GSM, PSM, PIM>)eventData);
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

[Serializable]
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