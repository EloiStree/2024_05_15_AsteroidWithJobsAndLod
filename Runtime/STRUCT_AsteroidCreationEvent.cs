using UnityEngine;

[System.Serializable]
public struct STRUCT_AsteroidCreationEvent
{
    public byte m_poolId;
    public int m_poolItemIndex;
    public long m_serverUtcNowTicks;
    public Vector3 m_startPosition;
    public Quaternion m_startRotationEuler;
    public Vector3 m_startDirection;
    public float m_speedInMetersPerSecond;
    public float m_colliderRadius;
}





