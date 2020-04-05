using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IInputController<IM> : MonoBehaviour
{
    public IM currentInputData;

    public IM SampleInput()
    {
        return currentInputData;
    }

}
