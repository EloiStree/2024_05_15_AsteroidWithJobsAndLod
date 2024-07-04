using System;
using System.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Job16kMono_CheckCollisionWith4Targets : MonoBehaviour
{

    public Transform m_target;

    public SNAM16KGet_ObjectBoolean m_isCollisionEnable;
    public SNAM16KGet_ObjectVector3 m_asteroidPositions;
    public SNAM16KGet_ObjectBoolean m_isHadCollisionWith;


    public void RefreshState() { 
    


    }
}



[BurstCompile]
public struct Job16K_ResetBooleanToZero : IJobParallelFor
{
    [WriteOnly]
    public NativeArray<bool> m_array;
    public void Execute(int index)
    {
        m_array[index] = false;
    }
}



[BurstCompile]
public struct Job16k_CheckCollisionWithTargets : IJobParallelFor
{

    [ReadOnly]
    public NativeArray<bool> m_colliderIsEnable;

    [ReadOnly]
    public NativeArray<Vector3> m_asteroidPositions;

    [WriteOnly]
    public NativeArray<bool> m_isInCollisionWith;

    public Vector3 m_position;
    public float m_radius;

    public void Execute(int index)
    {
        if(!m_colliderIsEnable[index])
        {
            m_isInCollisionWith[index] = false;
            return;
        }
        Vector3 asteroidPosition = m_asteroidPositions[index];
        m_isInCollisionWith[index] = m_colliderIsEnable[index] && Vector3.Distance(asteroidPosition, m_position) < m_radius;    
    }
}


[BurstCompile]
public struct Job16k_IsInCameraViewBasic: IJobParallelFor
{
    [ReadOnly]
    public NativeArray<Vector3> m_positions;

    [WriteOnly]
    public NativeArray<bool> m_isInCameraView;

    public Vector3 m_cameraPosition;
    public Quaternion  m_cameraRotation;
    public float m_horizontalAngle;
    public float m_verticalAngle;
    public float m_maxDistance;

    public void Execute(int index)
    {
          //GetWorldToLocal_Point Eloi Lib
          Vector3  localPosition = Quaternion.Inverse(m_cameraRotation) * (m_positions[index] - m_cameraPosition);
            if (localPosition.z > m_maxDistance)
            {
                m_isInCameraView[index] = false;
                return;
            }
          Vector3  flatHorizontal = new Vector3(localPosition.x, 0, localPosition.z);
          Vector3  flatVertical = new Vector3(0, localPosition.y, localPosition.z);
          float horizontalAngle = Vector3.Angle(Vector3.forward, flatHorizontal);  
          float verticalAngle = Vector3.Angle(Vector3.forward, flatVertical);
          m_isInCameraView[index] = horizontalAngle < m_horizontalAngle && verticalAngle < m_verticalAngle;
    }
}


