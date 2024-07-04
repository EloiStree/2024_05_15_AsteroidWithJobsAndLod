using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class AsteroidToByteEventMono : MonoBehaviour
{
    public byte m_createdStartByte;
    public byte m_destroyedStartByte;

    public UnityEvent<byte[]> m_onByteToPushed;

    public void PushAsteroidCreated(STRUCT_AsteroidCreationEvent created)
    {
        byte[] bytes = ByteParseAsteroidUtility.ParseToBytes_AsteroidCreated(m_createdStartByte, created);
        m_onByteToPushed.Invoke(bytes);
    }

    public void PushAsteroidDestroy(STRUCT_AsteroidDestructionEvent destroyed) { 
        byte[] bytes = ByteParseAsteroidUtility.ParseToByte_AsteroidDestroy(m_destroyedStartByte, destroyed);
        m_onByteToPushed.Invoke(bytes);
    }
}


public class ByteParseAsteroidUtility {
    public static byte[] ParseToBytes_AsteroidCreated(byte startByteType, STRUCT_AsteroidCreationEvent created)
    {
        byte[] bytes = new byte[1 + 1 + 4 + 9 * 4 + 2 * 4 + 8];
        bytes[0] = startByteType;
        Vector3 euler= created.m_startRotationEuler.eulerAngles;
        bytes[1]= created.m_poolId;
        BitConverter.GetBytes(created.m_poolItemIndex).CopyTo(bytes, 2);
        BitConverter.GetBytes(created.m_serverUtcNowTicks).CopyTo(bytes, 6);
        BitConverter.GetBytes(created.m_startPosition.x).CopyTo(bytes, 14);
        BitConverter.GetBytes(created.m_startPosition.y).CopyTo(bytes, 18);
        BitConverter.GetBytes(created.m_startPosition.z).CopyTo(bytes, 22);
        BitConverter.GetBytes(euler.x).CopyTo(bytes, 26);
        BitConverter.GetBytes(euler.y).CopyTo(bytes, 30);
        BitConverter.GetBytes(euler.z).CopyTo(bytes, 34);
        BitConverter.GetBytes(created.m_startDirection.x).CopyTo(bytes, 38);
        BitConverter.GetBytes(created.m_startDirection.y).CopyTo(bytes, 42);
        BitConverter.GetBytes(created.m_startDirection.z).CopyTo(bytes, 46);
        BitConverter.GetBytes(created.m_speedInMetersPerSecond).CopyTo(bytes, 50);
        BitConverter.GetBytes(created.m_colliderRadius).CopyTo(bytes, 54);
        return bytes;
    }

    public static STRUCT_AsteroidCreationEvent ParseToObject_AsteroidCreated(byte startBytetype, byte[] bytes, ref STRUCT_AsteroidCreationEvent asteroid)
    {
        if (bytes.Length != 58) throw new Exception();
        asteroid.m_poolId = bytes[1];
        asteroid.m_poolItemIndex = BitConverter.ToInt32(bytes, 2);
        asteroid.m_serverUtcNowTicks = BitConverter.ToInt64(bytes, 6);
        asteroid.m_startPosition = new Vector3(BitConverter.ToSingle(bytes, 14), BitConverter.ToSingle(bytes, 18), BitConverter.ToSingle(bytes, 22));
        asteroid.m_startRotationEuler = Quaternion.Euler(BitConverter.ToSingle(bytes, 26), BitConverter.ToSingle(bytes, 30), BitConverter.ToSingle(bytes, 34));
        asteroid.m_startDirection = new Vector3(BitConverter.ToSingle(bytes, 38), BitConverter.ToSingle(bytes, 42), BitConverter.ToSingle(bytes, 46));
        asteroid.m_speedInMetersPerSecond = BitConverter.ToSingle(bytes, 50);
        asteroid.m_colliderRadius = BitConverter.ToSingle(bytes, 54);
        return asteroid;
    }

    public static STRUCT_AsteroidDestructionEvent ParseToObject_AsteroidDestroy(byte startBytetype, byte[] bytes , ref STRUCT_AsteroidDestructionEvent asteroid)
    {
        if (bytes.Length != 14) throw new Exception("AsteroidDestructionEvent ParseToObject_AsteroidDestroy bytes.Length != 14");
        asteroid.m_poolId = bytes[1];
        asteroid.m_poolItemIndex = BitConverter.ToInt32(bytes, 2);
        asteroid.m_serverUtcNowTicks = BitConverter.ToInt64(bytes, 6);
        return asteroid;
    }
    public static byte[] ParseToByte_AsteroidDestroy(byte startByteType, STRUCT_AsteroidDestructionEvent destroyed)
    {
        byte[] bytes = new byte[1 + 1 + 4 + 8];
        bytes[0] = startByteType;
        BitConverter.GetBytes(destroyed.m_poolId).CopyTo(bytes, 1);
        BitConverter.GetBytes(destroyed.m_poolItemIndex).CopyTo(bytes, 2);
        BitConverter.GetBytes(destroyed.m_serverUtcNowTicks).CopyTo(bytes, 6);
        return bytes;
    }



}