using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JAMLib
{
    public abstract class IMultiplayerController : MonoBehaviour
    {
        GameStateSenderController stateStreamer;
        GameStateReceiverController stateReceiver;
        public GameObject masterControllerPrefab;
        public PlayerStatePack myState;

        public static GameAuth gameAuth = GameAuth.Client;

        public abstract void OnPlayerSpawned(List<IMasterController>masterControllers);

        public abstract void OnSpawnMatch(WorldStateModel gsm);
        public abstract PlayerStateModel GetSpawnPlayerState(int playerIndex);
        public abstract WorldStateModel SampleWorldState();
        public abstract void SetWorldState(WorldStateModel gsm);
        public abstract void OnMatchConnected();
        public static Config config;
        public static IMultiplayerController m_instance;

        public ISerializerController serializer;
        public ITransportController transportController;

        public void Init()
        {
            m_instance = this;
            config = new Config();
            serializer = new MessagePackSerializerController();
            InitTransport();
        }

        void InitTransport()
        {
            if (config.isOffline)
            {
                transportController = new OfflineController();
            }
            else
            {

                transportController = new SignalRController();
            }
        }

        public void ConnectToMatch(PlayerInitModel init)
        {
            Init();

            if (!config.isOffline)
            {
                if (gameAuth == GameAuth.Server)
                {
                    InitializeServer(init, gameAuth);
                }
                else
                {
                    InitializeClient(init, gameAuth);
                }
            }
            else
            {
                InitializeServer(init, GameAuth.Server);
                InitializeClient(init, GameAuth.Client);
                InitiateMatch();
            }
        }

        public void MatchConnected()
        {
            OnMatchConnected();
        }

        void InitializeServer(PlayerInitModel init, GameAuth auth)
        {
            InitTransport();
            serializer = new MessagePackSerializerController();
            transportController.JoinRoom(init, auth.ToString(), OnMatchConnected);
            transportController.IOnPlayerJoined(PlayerJoined);
        }

        void InitializeClient(PlayerInitModel init, GameAuth auth)
        {
            stateReceiver = new GameStateReceiverController();
            stateReceiver.InitStreamReception("gameState");
            transportController.JoinRoom(init, auth.ToString(), OnMatchConnected);
            transportController.IOnFromServer("start", OnStartMatch);
        }

        public List<IMasterController> masterControllerDic = new List<IMasterController>();
        public List<PlayerStatePack> playerInitDic = new List<PlayerStatePack>();

        public List<PlayerStatePack> SamplePlayerStates()
        {
            List<PlayerStatePack> result = new List<PlayerStatePack>();

            for (int i = 0; i < masterControllerDic.Count; i++)
            {
                result.Add(masterControllerDic[i].liveController.SamplePlayerState());
            }

            return result;
        }


        public List<PlayerStatePack> players;

        public void SpawnMatch(ServerMessagePack startMatchData)
        {
            SpawnPlayers(startMatchData.playerStates);
            OnSpawnMatch(startMatchData.worldState);

            players = startMatchData.playerStates;
        }

        public abstract WorldStateModel GetSpawnGameSate();


        void SpawnPlayers(List<PlayerStatePack> players)
        {
            masterControllerDic = new List<IMasterController>();
            for (int i = 0; i < players.Count; i++)
            {
                GameObject g = Instantiate(masterControllerPrefab);

                IMasterController pu = g.GetComponent<IMasterController>();
                masterControllerDic.Add(pu);
                if (config.isOffline)
                {
                    pu.Initialize(transportController, players[i], true);
                }
                else
                {
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
                }
                pu.SetPlayerInit(players[i].playerInit,players[i].isBot);
            }
            OnPlayerSpawned(masterControllerDic);
        }

        public void PlayerJoined(string conId, string auth, PlayerInitModel init)
        {
            PlayerStatePack ps = new PlayerStatePack();
            ps.playerState = GetSpawnPlayerState(playerInitDic.Count);
            ps.conId = conId;
            ps.playerInit = init;
            playerInitDic.Add(ps);
        }

        public void InitiateMatch()
        {
            DataPackageHistory<ServerMessagePack> dh = new DataPackageHistory<ServerMessagePack>();
            DataPackage<ServerMessagePack> dp = new DataPackage<ServerMessagePack>();
            DataInstance<ServerMessagePack> di = new DataInstance<ServerMessagePack>();
            ServerMessagePack smd = new ServerMessagePack();
            smd.worldState = GetSpawnGameSate();

            if (config.isOffline)
            {
                PlayerStatePack psp = new PlayerStatePack();
                PlayerInitModel pim = new PlayerInitModel();
                psp.playerInit = pim;
                psp.conId = "offline";
                playerInitDic.Add(psp);
            }

            for(int i=0; i<config.botCount; i++)
            {
                PlayerStatePack psp = new PlayerStatePack();
                PlayerInitModel pim = new PlayerInitModel();
                psp.playerInit = pim;
                psp.conId = "b"+i;
                psp.isBot = true;
                playerInitDic.Add(psp);
            }

            smd.playerStates = new List<PlayerStatePack>(playerInitDic);
            di.data = smd;
            dp.dataStream = new List<DataInstance<ServerMessagePack>>();
            dp.dataStream.Add(di);
            dh.dataPackageHistory = new List<DataPackage<ServerMessagePack>>();
            dh.dataPackageHistory.Add(dp);
            if (!config.isOffline)
            {
                transportController.IEmitToClients("start", dh);
            }
            stateStreamer = new GameStateSenderController();
            stateStreamer.StartStream("gameState");
            SpawnMatch(smd);
        }

        ServerMessagePack startMatchData;
        public void OnStartMatch(string eventName, string connectionId, DataPackageHistory<ServerMessagePack> eventData)
        {
            startMatchData = eventData.dataPackageHistory[0].dataStream[0].data;
            SpawnMatch(startMatchData);
        }

        public void ProcessGameStatePack(ServerMessagePack gameStateData)
        {
            for (int i = 0; i < gameStateData.playerStates.Count; i++)
            {
                if (masterControllerDic[i].isOwner && IMultiplayerController.config.isClientSidePrediction)
                {
                    masterControllerDic[i].SetGhostState(gameStateData.playerStates[i]);
                }
                else
                {
                    masterControllerDic[i].SetMirrorState(gameStateData.playerStates[i].playerState);
                }
            }
            SetWorldState(gameStateData.worldState);
        }
    }
}