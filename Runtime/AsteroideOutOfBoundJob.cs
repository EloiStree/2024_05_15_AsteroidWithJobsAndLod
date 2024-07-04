using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct AsteroideOutOfBoundJob : IJobParallelFor
{
    public NativeArray<bool> m_destroyEvent;
    public NativeArray<STRUCT_AsteroidCapsulePosition> m_currentExistance;
    public long m_serverCurrentUtcNowTicks;
    public long m_serverCurrentUtcPreviousTicks;

    public int m_currentMaxAsteroide;
    public Vector3 m_centerPosition;
    public float m_maxHeightDistance;
    public float m_maxWidthDistance;

    public void Execute(int index)
    {
        m_destroyEvent[index] = false;
        if (index>=m_currentMaxAsteroide)
        {
            return;
        }
        if(m_currentExistance[index].m_currentPosition.y > m_maxHeightDistance 
            || m_currentExistance[index].m_currentPosition.y <0f)
        {
            m_destroyEvent[index] = true;
            return;
        }
        if (m_currentExistance[index].m_currentPosition.x > m_maxWidthDistance 
            || m_currentExistance[index].m_currentPosition.x < -m_maxWidthDistance)
        {
            m_destroyEvent[index] = true;
            return;
        }
        if (m_currentExistance[index].m_currentPosition.z > m_maxWidthDistance 
            || m_currentExistance[index].m_currentPosition.z < -m_maxWidthDistance)
        {
            m_destroyEvent[index] = true;
            return;
        }


    }
}





