using UnityEngine;

public class TDD_AsteroidParsingMono: MonoBehaviour { 

    public byte m_createdStartByte;
    public byte m_destroyedStartByte;
    public STRUCT_AsteroidCreationEvent m_asteroidCreationEvent;
    public STRUCT_AsteroidCreationEvent m_asteroidCreationEventParsed;
    public STRUCT_AsteroidDestructionEvent m_asteroidDestructionEvent;
    public STRUCT_AsteroidDestructionEvent m_asteroidDestructionEventParsed;

    private void OnValidate()
    {
        m_asteroidCreationEventParsed = ByteParseAsteroidUtility.ParseToObject_AsteroidCreated(m_createdStartByte, ByteParseAsteroidUtility.ParseToBytes_AsteroidCreated(m_createdStartByte, m_asteroidCreationEvent), ref m_asteroidCreationEventParsed);
        m_asteroidDestructionEventParsed = ByteParseAsteroidUtility.ParseToObject_AsteroidDestroy(m_destroyedStartByte, ByteParseAsteroidUtility.ParseToByte_AsteroidDestroy(m_destroyedStartByte, m_asteroidDestructionEvent), ref m_asteroidDestructionEventParsed);
    }
}
