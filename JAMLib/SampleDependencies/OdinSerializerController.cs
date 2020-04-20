using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace JAMLib
{
    public class OdinSerializerController : ISerializerController
    {

        public override object Deserialize(string dataString)
        {

            string decomstring = DecompressString(dataString);
            object eventObj = Sirenix.Serialization.SerializationUtility.DeserializeValue<object>(Convert.FromBase64String(decomstring), Sirenix.Serialization.DataFormat.Binary);
            return eventObj;
        }

        public override string Serialize(object data)
        {
            string originalString = Convert.ToBase64String(Sirenix.Serialization.SerializationUtility.SerializeValue<object>(data, Sirenix.Serialization.DataFormat.Binary));
            string dataStringCom = CompressString(originalString);
            string decomstring = DecompressString(dataStringCom);
            return dataStringCom;
        }

        public void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public string CompressString(string datastring)
        {
            byte[] buffer = GetBytes(datastring);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return GetString(gZipBuffer);
        }

        byte[] GetBytes(string data)
        {
            return Convert.FromBase64String(data);
        }

        string GetString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }


        public string DecompressString(string compressedText)
        {
            byte[] gZipBuffer = GetBytes(compressedText);
            using (var memoryStream = new MemoryStream())
            {
                int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                var buffer = new byte[dataLength];

                memoryStream.Position = 0;
                using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    gZipStream.Read(buffer, 0, buffer.Length);
                }

                return GetString(buffer);
            }
        }

    }
}