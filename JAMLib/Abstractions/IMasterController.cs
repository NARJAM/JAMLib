using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace JAMLib
{
    public abstract class IMasterController : MonoBehaviour
    {
        public IPlayerController liveController;
        public IInputController inputController;
        public InputReceiverController inputReceiverController;
        public InputSenderController inputSenderController;
        public IPlayerView mirrorPlayer;
        public PlayerStateModel ghostState;
        public PlayerStateModel projectedState;
        public PlayerStateModel pastState;
        public ITransportController transportController;
        public bool isOwner;
        public string connectionId;

        public abstract bool CheckForCorrection(PlayerStateModel serverState, PlayerStateModel localState);

        public void Initialize(ITransportController _transportController, PlayerStatePack psp, bool _isOwner)
        {
            transportController = _transportController;
            liveController.initPlayer = psp.playerInit;
            isOwner = _isOwner;
            connectionId = psp.conId;
            mirrorPlayer.isOwner = _isOwner;
            liveController.Initialize(psp.playerState, _isOwner, connectionId, this);

            if (IMultiplayerController.config.isOffline)
            {
                Debug.Log("Initialized Master");
                inputSenderController = new InputSenderController(this);
                inputSenderController.StartStream(connectionId);
                inputController.enabled = true; 
                inputReceiverController = new InputReceiverController(this);
                inputReceiverController.InitStreamReception(connectionId);
            }
            else
            {
                if (isOwner)
                {
                    inputSenderController = new InputSenderController(this);
                    inputSenderController.StartStream(connectionId);
                    inputController.enabled = true;
                }
                else
                {
                    inputReceiverController = new InputReceiverController(this);
                    inputReceiverController.InitStreamReception(connectionId);
                    inputController.enabled = false;
                }
            }

            if (psp.isBot)
            {
                inputController.InitBot(psp.conId);
            }
        }

        public void SetMirrorState(PlayerStateModel psp)
        {
            mirrorPlayer.SetFromModel(psp);
            OnMirrorStateSet(psp);
        }

        public void SetPlayerInit(PlayerInitModel pim)
        {
            mirrorPlayer.SetInit(pim);
        }

        public void SetGhostState(PlayerStatePack psp)
        {
                ghostState = psp.playerState;
                OnGhostStateSet(psp.playerState);
                TickModel pastTick = new TickModel();
                if (inputSenderController.tickHistory.TryGetValue(psp.tick, out pastTick))
                {
                    pastState = pastTick.state;
                    OnPastStateSet(pastState);
                    if (CheckForCorrection(psp.playerState, pastTick.state))
                    {
                        Debug.Log("Correction Needed");
                        ProjectState(psp);
                    }
                }
        }

        public abstract void OnMirrorStateSet(PlayerStateModel playerState);
        public abstract void OnGhostStateSet(PlayerStateModel playerState);
        public abstract void OnPastStateSet(PlayerStateModel playerState);

        public void ProjectState(PlayerStatePack psp)
        {
            liveController.SetState(psp.playerState);
            for (int i = psp.tick; i < inputSenderController.tickTrack; i++)
            {
                TickModel newTick = new TickModel();
                projectedState = liveController.ProcessInput(inputSenderController.tickHistory[i].input);
                newTick.state = projectedState;
                newTick.input = inputSenderController.tickHistory[i].input;
                newTick.tick = inputSenderController.tickHistory[i].tick;
                inputSenderController.tickHistory[i] = newTick;
            }
        }

        public void ProcessServerRequests(ServerEventRequestModel requests)
        {
            if (requests.serverEventRequests == null)
            {
                return;
            }

            for (int i = 0; i < requests.serverEventRequests.Length; i++)
            {
                if (requests.serverEventRequests[i].requestMessages != null)
                {
                    for (int j = 0; j < requests.serverEventRequests[i].requestMessages.Count; j++)
                    {
                        liveController.ProcessServerEvents(i, requests.serverEventRequests[i].requestMessages[j]);
                    }
                }
            }
        }
    }
}