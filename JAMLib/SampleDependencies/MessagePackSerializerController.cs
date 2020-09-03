using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JAMLib;
using MessagePack;
using System.Text;
using System;

namespace JAMLib
{
    public class MessagePackSerializerController : ISerializerController
    {
        MessagePackSerializerOptions lz4Options;

        public MessagePackSerializerController()
        {
         //   MessagePackSerializerOptions finalOptions = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
           // lz4Options = finalOptions;
        }

        public override T Deserialize<T>(string data)
        {
            return MessagePackSerializer.Deserialize<T>(Convert.FromBase64String(data));
        }

        public override string Serialize<T>(T data)
        {
            string dataString = Convert.ToBase64String(MessagePackSerializer.Serialize<T>(data));
            return dataString;
        }
    }
}