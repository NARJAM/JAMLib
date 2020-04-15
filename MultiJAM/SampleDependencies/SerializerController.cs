using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SerializerController : ISerializerController
{
    public override object Deserialize(string data)
    {
        object eventObj = new object();
        return eventObj;
    }

    public override string Serialize(object data)
    {
        string dataString="";
        return dataString;
    }
}
