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

        public static GameAuth gameAuth;

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
            Debug.Log("INNITED");
            m_instance = this;
            config = new Config();
            serializer = new CerasSerializerController();
            transportController = new SignalRController();
        }

        public void ConnectToMatch(PlayerInitModel init)
        {
            Init();
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
            transportController = new SignalRController();
            serializer = new CerasSerializerController();
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

        public void SpawnMatch(ServerMessagePack startMatchData)
        {
            SpawnPlayers(startMatchData.playerStates);
            OnSpawnMatch(startMatchData.worldState);
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

        public void PlayerJoined(string conId, string auth, PlayerInitModel init)
        {
            PlayerStatePack ps = new PlayerStatePack();
            ps.playerState = GetSpawnPlayerState(playerInitDic.Count);
            ps.conId = conId;
            Debug.Log("Conid " + conId);
            ps.playerInit = init;
            playerInitDic.Add(ps);
        }

        public void InitiateMatch()
        {
            DataPackageHistory dh = new DataPackageHistory();
            DataPackage dp = new DataPackage();
            DataInstance di = new DataInstance();
            ServerMessagePack smd = new ServerMessagePack();
            smd.worldState = GetSpawnGameSate();
            smd.playerStates = new List<PlayerStatePack>(playerInitDic);
            di.data = IMultiplayerController.m_instance.serializer.Serialize<ServerMessagePack>(smd);
            dp.dataStream = new List<DataInstance>();
            dp.dataStream.Add(di);
            dh.dataPackageHistory = new List<DataPackage>();
            dh.dataPackageHistory.Add(dp);
            transportController.IEmitToClients<DataPackageHistory>("start", dh);
            stateStreamer = new GameStateSenderController();
            stateStreamer.StartStream("gameState");
            SpawnMatch(smd);
        }

        ServerMessagePack startMatchData;
        public void OnStartMatch(string eventName, string connectionId, DataPackageHistory eventData)
        {
            IMultiplayerController.m_instance.serializer.Deserialize<ServerMessagePack>(eventData.dataPackageHistory[0].dataStream[0].data, ref startMatchData);
            SpawnMatch(startMatchData);
        }

        public void ProcessGameStatePack(ServerMessagePack gameStateData)
        {
            for (int i = 0; i < gameStateData.playerStates.Count; i++)
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
            SetWorldState(gameStateData.worldState);
        }
    }
}