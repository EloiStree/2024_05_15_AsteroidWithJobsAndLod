using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class Job16KMono_IsInCameraView : MonoBehaviour
{
    public Transform m_camera;
    public float m_horizontalAngle=60;
    public float m_verticalAngle=30;
    public SNAM16K_ObjectVector3 m_positions;
    public SNAM16K_ObjectBool m_isInCameraView;


    public List<int> m_objectIndexInView= new List<int>();


    private void Update()
    {
        Job16k_IsInCameraViewBasic job = new Job16k_IsInCameraViewBasic();

        job.m_positions = m_positions.GetNativeArray(); ;
        job.m_isInCameraView = m_isInCameraView.GetNativeArray();
        job.m_cameraPosition = m_camera.position;
        job.m_cameraRotation = m_camera.rotation;
        job.m_horizontalAngle = m_horizontalAngle;
        job.m_verticalAngle = m_verticalAngle;
        JobHandle jobHandle = job.Schedule(m_positions.GetNativeArray().Length, 64);
        jobHandle.Complete();
        m_objectIndexInView.Clear();
        for (int i = 0; i < m_isInCameraView.GetNativeArray().Length; i++)
        {
            if (m_isInCameraView.Get(i))
            {
                m_objectIndexInView.Add(i);
            }
        }


    }
}

