using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Ceras;
using Ceras.Formatters.AotGenerator;

namespace JAMLib
{
    public abstract class ISerializerController
    {
        public abstract string Serialize<T>(T data);
        public abstract void Deserialize<T>(string data, ref T empty);
    }
}