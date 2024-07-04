using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class Job16KMono_IsProjectileTouchingTarget : MonoBehaviour {



    public SNAM16K_AstreroidCapsulePosition m_capsulePosition;
    public SNAM16K_ObjectBool m_isTouchingTarget;

    public Transform m_target;

    public float m_targetRadius;
    public Vector3 m_position;
    public Vector3 m_previousPosition;

    public bool m_useCapsuleCollision = false;

    public void Update()
    {
        m_previousPosition = m_position;
        m_position = m_target.position;


        STRUCTJOB_IsProjectileTouchingTarget job = new STRUCTJOB_IsProjectileTouchingTarget();
        job.m_capsulePosition = m_capsulePosition.GetNativeArray();
        job.m_isTouchingTarget = m_isTouchingTarget.GetNativeArray();
        job.m_targetPosition = m_position;
        job.m_targetPreviousPosition = m_previousPosition;
        job.m_targetRadius = m_targetRadius;
        job.m_useCapsuleCollision = m_useCapsuleCollision;
        JobHandle jobHandle = job.Schedule(m_capsulePosition.GetNativeArray().Length, 64);
        jobHandle.Complete();

        for (int i = 0; i < job.m_isTouchingTarget.Length; i++)
        {
            if (job.m_isTouchingTarget[i])
            {
               // Debug.Log("Touching Target");
                Debug.DrawLine(m_previousPosition, m_position, Color.magenta, 5);
                Debug.DrawLine(job.m_capsulePosition[i].m_previousPosition, job.m_capsulePosition[i].m_currentPosition, Color.blue, 1);
            }
        }
    
    }
}





