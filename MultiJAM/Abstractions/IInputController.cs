using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IInputController : MonoBehaviour
{
    public PlayerInputModel currentInputData;

    public PlayerInputModel SampleInput()
    {
        return currentInputData;
    }

}
