using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public abstract class IPlayerView : MonoBehaviour
    {
        public bool isOwner;
        public abstract void SetInit(PlayerInitModel pim);
        public abstract void SetFromModel(PlayerStateModel psm);
    }
}