﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace JAMLib
{
    public abstract class IInputController : MonoBehaviour
    {
        public PlayerInputModel currentInputData = new PlayerInputModel();

        public abstract void InitBot(string botId);

        public PlayerInputModel SampleInput()
        {
            return currentInputData;
        }

    }
}