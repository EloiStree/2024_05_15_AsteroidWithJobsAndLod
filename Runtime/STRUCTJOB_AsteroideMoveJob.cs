using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public struct STRUCTJOB_AsteroideMoveJob : IJobParallelFor
{

    [ReadOnly]
    public NativeArray<STRUCT_AsteroidMoveConstant> m_asteroidInGame;

    public NativeArray<STRUCT_AsteroidCapsulePosition> m_currentExistance;

    public int m_currentMaxAsteroide;
    public long m_serverCurrentUtcNowTicks;
    public long m_serverCurrentUtcPreviousTicks;
    public const float m_tickToSeconds= 1f/(float)TimeSpan.TicksPerSecond;

    public void Execute(int index)
    {
        if (index >= m_currentMaxAsteroide)
        {
            return;
        }
        STRUCT_AsteroidMoveConstant a = m_asteroidInGame[index];
        STRUCT_AsteroidCapsulePosition p = m_currentExistance[index];
     
        p.m_previousPosition = p.m_currentPosition;
        float timeSinceStart = 
            (m_serverCurrentUtcNowTicks - m_asteroidInGame[index].m_startUtcNowTicks)*m_tickToSeconds;
        float distance = m_asteroidInGame[index].m_speedInMetersPerSecond * timeSinceStart;
        p.m_currentPosition= a.m_startPoint+ a.m_direction * distance;
        m_currentExistance[index] = p;

      
    }
}





