using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JAMLib
{
    public abstract class ISerializerController
    {
        public abstract string Serialize<T>(T data);
        public abstract T Deserialize<T>(string data);
    }
}