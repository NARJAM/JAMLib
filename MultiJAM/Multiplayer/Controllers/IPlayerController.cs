using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayerController<PSM,IM> : MonoBehaviour
{
    public IMasterController<PSM, IM> masterController;
    private PlayerStatePack<PSM> playerStatePack;
    public PSM currentPlayerState;
    public bool isInitialized;
    public bool isOwner;
    int tick;
    public void Initialize(PSM initialState,bool _isOwner, string connectionId, IMasterController<PSM, IM> _masterController)
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

    public PlayerStatePack<PSM> SamplePlayerState()
    {
        playerStatePack.tick = tick;
        playerStatePack.playerState = currentPlayerState;
        return playerStatePack;
    }

}
