using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayerView<PSM,PIM> : MonoBehaviour
{
    public abstract void SetInit(PIM pim);
    public abstract void SetFromModel(PSM psm);
}
