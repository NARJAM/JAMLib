using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayerView<PSM> : MonoBehaviour
{
    public abstract void SetFromModel(PSM psm);
}
