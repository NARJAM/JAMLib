using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JAMLib
{
    public abstract class IPlayerController : MonoBehaviour
    {
        public IMasterController masterController;
        private PlayerStatePack playerStatePack = new PlayerStatePack();
        public ServerEventRequestModel serverEvents = new ServerEventRequestModel();

        public PlayerStateModel currentPlayerState = new PlayerStateModel();
        public PlayerInitModel initPlayer = new PlayerInitModel();
        public bool isInitialized;
        public bool isOwner;
        int tick;

        public void Initialize(PlayerStateModel initialState, bool _isOwner, string connectionId, IMasterController _masterController)
        {
            isOwner = _isOwner;
            masterController = _masterController;
            playerStatePack.conId = connectionId;
            isInitialized = true;
            currentPlayerState = initialState;
            OnInitialize(initialState);

            serverEvents = new ServerEventRequestModel();
            serverEvents.Init();
        }

        public void SetState(PlayerStateModel initialState)
        {
            currentPlayerState = initialState;
            OnInitialize(initialState);
        }

        public PlayerStateModel ProcessPack(ClientMessagePack inputPack)
        {
            tick = inputPack.tick;
            return ProcessInput(inputPack.inputData);
        }



        public abstract void OnInitialize(PlayerStateModel initialState);
        public abstract PlayerStateModel ProcessInput(PlayerInputModel playerInput);
        public abstract void ProcessServerEvents(int requestId, string requestData);

        public void AddServerEventRequest(int requestId, string requestData)
        {
            serverEvents.serverEventRequests[requestId].requestMessages.Add(requestData);
        }

        public ServerEventRequestModel SampleServerRequests()
        {
            ServerEventRequestModel qwe = serverEvents;
            serverEvents = new ServerEventRequestModel();
            serverEvents.Init();
            return qwe;
        }

        public PlayerStatePack SamplePlayerState()
        {
            playerStatePack.tick = tick;
            playerStatePack.playerState = currentPlayerState;
            return playerStatePack;
        }

    }
}