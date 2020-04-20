using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace JAMLib
{
    public abstract class IPlayerController : MonoBehaviour
    {
        public IMasterController masterController;
        private PlayerStatePack playerStatePack;
        public ServerEventRequest[] serverEvents = new ServerEventRequest[0];

        public PlayerStateModel currentPlayerState;
        public PlayerInitModel initPlayer;
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
            ServerEventRequest ser;
            if (serverEvents.Length <= requestId)
            {
                ServerEventRequest[] temp = new ServerEventRequest[requestId + 1];
                for (int i = 0; i < serverEvents.Length; i++)
                {
                    temp[i] = serverEvents[i];
                }
                serverEvents = temp;
            }

            if (serverEvents[requestId].requestMessages != null)
            {
                string[] temp = new string[serverEvents[requestId].requestMessages.Length + 1];
                for (int i = 0; i < serverEvents[requestId].requestMessages.Length; i++)
                {
                    temp[i] = serverEvents[requestId].requestMessages[i];
                }
                temp[temp.Length - 1] = requestData;
                serverEvents[requestId].requestMessages = temp;
            }
            else
            {
                ser = new ServerEventRequest();
                ser.requestMessages = new string[1];
                ser.requestMessages[0] = (requestData);
                serverEvents[requestId] = ser;
            }
        }

        public ServerEventRequest[] SampleServerRequests()
        {
            ServerEventRequest[] temp = serverEvents;
            serverEvents = new ServerEventRequest[1];
            return temp;
        }

        public PlayerStatePack SamplePlayerState()
        {
            playerStatePack.tick = tick;
            playerStatePack.playerState = currentPlayerState;
            return playerStatePack;
        }

    }
}