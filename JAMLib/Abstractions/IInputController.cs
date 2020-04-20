using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public abstract class IInputController : MonoBehaviour
    {
        public PlayerInputModel currentInputData;

        public PlayerInputModel SampleInput()
        {
            return currentInputData;
        }

    }
}