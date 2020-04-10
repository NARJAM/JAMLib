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

    public Dictionary<string, IMasterController<PSM,IM>> masterControllerDic = new Dictionary<string, IMasterController<PSM, IM>>();
    public Dictionary<string, PlayerStatePack<PSM>> playerInitDic = new Dictionary<string, PlayerStatePack<PSM>>();

    public Dictionary<string, PlayerStatePack<PSM>> SamplePlayerStates()
    {
        Dictionary<string, PlayerStatePack<PSM>> result = new Dictionary<string, PlayerStatePack<PSM>>();

        foreach (KeyValuePair<string, IMasterController<PSM, IM>> entry in masterControllerDic)
        {
            result.Add(entry.Key, entry.Value.liveController.SamplePlayerState());
        }

        return result;
    }

    public void SpawnMatch(GameStatePack<GSM,PSM> startMatchData)
    {
        SpawnPlayers(startMatchData.playerStates);
        OnSpawnMatch(startMatchData.gameState);
    }

    public abstract GSM GetSpawnGameSate();

    void SpawnPlayers(Dictionary<string, PlayerStatePack<PSM>> players)
    {
        foreach (KeyValuePair<string, PlayerStatePack<PSM>> entry in players)
        {
            GameObject g = Instantiate(masterControllerPrefab);
            IMasterController<PSM, IM> pu = g.GetComponent<IMasterController<PSM, IM>>();
            masterControllerDic.Add(players[entry.Key].conId, pu);

            if (multiplayerConfigModel.gameAuth == GameAuth.Client)
            {
                if (signalRController.connectionId == players[entry.Key].conId)
                {
                    pu.Initialize(signalRController,players[entry.Key], true);
                }
                else
                {
                    pu.Initialize(signalRController, players[entry.Key], false);
                }
            }
            else
            {
                pu.Initialize(signalRController, players[entry.Key], false);
            }
        }
    }

    public void PlayerJoined(string conId, string auth)
    {
        PlayerStatePack<PSM> ps = new PlayerStatePack <PSM>();
        ps.playerState = GetSpawnPlayerState(playerInitDic.Count);
        ps.conId = conId;
        playerInitDic.Add(conId,ps);
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
        foreach (KeyValuePair<string, PlayerStatePack<PSM>> entry in gameStateData.playerStates)
        {
            if (masterControllerDic[entry.Key].isOwner)
            {
                masterControllerDic[entry.Key].SetGhostState(gameStateData.playerStates[entry.Key]);
            }
            else
            {
                masterControllerDic[entry.Key].mirrorPlayer.SetFromModel(gameStateData.playerStates[entry.Key].playerState);
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
    public Dictionary<string,PlayerStatePack<PSM>> playerStates;
    public GSM gameState;
}

public struct InputPack<IM>
{
    public int tick;
    public string connectionId;
    public IM inputData;
    public Dictionary<int,ServerEventRequest> serverEventRequests;
}

public struct ServerEventRequest
{
    public List<object> requestInstances;
}