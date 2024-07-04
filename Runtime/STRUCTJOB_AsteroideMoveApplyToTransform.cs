using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

public struct STRUCTJOB_AsteroideMoveApplyToTransform : IJobParallelForTransform
{
   
    public NativeArray<STRUCT_AsteroidCapsulePosition> m_currentExistance;
    public int m_currentMaxAsteroide;
    public Vector3 m_unusedWorldPosition;

    public void Execute(int index, TransformAccess transform)
    {
        if (index >= m_currentMaxAsteroide)
        {
            transform.position = m_unusedWorldPosition;
            return;
        }
        transform.position = m_currentExistance[index].m_currentPosition;
    }
}





