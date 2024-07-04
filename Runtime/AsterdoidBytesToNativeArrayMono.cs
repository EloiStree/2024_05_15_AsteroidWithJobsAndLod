using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;




public class AsterdoidBytesToNativeArrayMono : MonoBehaviour {

    public byte m_bytesStartType=0;
    public byte m_poolId=0;

    public int m_nativeInitialMaxSize = 128 * 128;
    public NativeArray<STRUCT_AsteroidCreationEvent> m_asteroidCreationEvent;

    public UnityEvent<NativeArray<STRUCT_AsteroidCreationEvent>> m_onSizeChanged;
    public void OnEnable()
    {
        m_asteroidCreationEvent = new NativeArray<STRUCT_AsteroidCreationEvent>(m_nativeInitialMaxSize, Allocator.Persistent);
        m_onSizeChanged.Invoke(m_asteroidCreationEvent);


    }
    private void OnDisable()
    {
        m_asteroidCreationEvent.Dispose();
        m_onSizeChanged.Invoke(m_asteroidCreationEvent);
    }

    public STRUCT_AsteroidCreationEvent asteroidCreationEvent;
    public void PushBytesToNativeArray(byte[] bytes)
    {
        if (bytes == null) return;
        if (bytes.Length < 10) return;
        if (bytes[0] != m_bytesStartType) return;
        if (bytes[1] != m_poolId) return;
        try { 
        ByteParseAsteroidUtility.ParseToObject_AsteroidCreated(m_bytesStartType, bytes, ref asteroidCreationEvent);
        }catch(Exception e)
        {
            Debug.LogError("Error parsing bytes to AsteroidCreationEvent: "+e.Message, this);
            return;
        }
        if(m_asteroidCreationEvent.Length <=asteroidCreationEvent.m_poolItemIndex)
        {
            Debug.LogWarning("We have a pool item index that is out of bounds: "+asteroidCreationEvent.m_poolItemIndex, this);
            return;
        }
        m_asteroidCreationEvent[asteroidCreationEvent.m_poolItemIndex] = asteroidCreationEvent;
        
    }
}
