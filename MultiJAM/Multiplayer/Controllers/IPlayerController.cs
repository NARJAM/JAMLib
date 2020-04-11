using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayerController<PSM,IM, PIM> : MonoBehaviour
{
    public IMasterController<PSM, IM, PIM> masterController;
    private PlayerStatePack<PSM, PIM> playerStatePack;
    public ServerEventRequest[] serverEvents = new ServerEventRequest[0];

    public PSM currentPlayerState;
    public PIM initPlayer;
    public bool isInitialized;
    public bool isOwner;
    int tick;

    public void Initialize(PSM initialState,bool _isOwner, string connectionId, IMasterController<PSM, IM, PIM> _masterController)
    {
        isOwner = _isOwner;
        masterController = _masterController;
        playerStatePack.conId = connectionId;
        isInitialized = true;
        currentPlayerState = initialState;
        OnInitialize(initialState);
    }

    public void SetState(PSM initialState) {
        currentPlayerState = initialState;
        OnInitialize(initialState);
    }

    public PSM ProcessPack(InputPack<IM> inputPack) {
        tick = inputPack.tick;
        return ProcessInput(inputPack.inputData);
    }



    public abstract void OnInitialize(PSM initialState);
    public abstract PSM ProcessInput(IM playerInput);
    public abstract void ProcessServerEvents(int requestId, object requestData);

    public void AddServerEventRequest(int requestId, object requestData)
    {
        ServerEventRequest ser;
        if (serverEvents.Length <= requestId)
        {
            ServerEventRequest[] temp = new ServerEventRequest[requestId + 1];
            for(int i=0; i<serverEvents.Length; i++)
            {
                temp[i] = serverEvents[i];
            }
            serverEvents = temp;
        }

        if(serverEvents[requestId].requestInstances!=null)
        {
            serverEvents[requestId].requestInstances.Add(requestData);
        }
        else
        {
            ser = new ServerEventRequest();
            ser.requestInstances = new List<object>();
            ser.requestInstances.Add(requestData);
            serverEvents[requestId] = ser;
        }
    }

    public ServerEventRequest[] SampleServerRequests() {
        ServerEventRequest[] temp = serverEvents;
        serverEvents = new ServerEventRequest[1];
        return temp;
    }

    public PlayerStatePack<PSM, PIM> SamplePlayerState()
    {
        playerStatePack.tick = tick;
        playerStatePack.playerState = currentPlayerState;
        return playerStatePack;
    }

}
