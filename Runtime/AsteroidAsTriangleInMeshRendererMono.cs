
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using DroneIMMO;
using UnityEditor.Rendering;
using static UnityEngine.GraphicsBuffer;




public class AsteroidAsTriangleInMeshRendererMono : MonoBehaviour
{
    public Mesh m_meshObject;
    public SNAM16K_AstreroidCapsulePosition m_projectile;

    //[SerializeField]
    //private MeshRenderer m_renderer;
    [SerializeField]
    private SkinnedMeshRenderer m_filter;

    public NativeArray<Vector3> m_vertices;
    public NativeArray<int> m_trianglesOrder;

    public int m_verticesCount = 0;
    public int m_trianglesOrderCount = 0;

    public float m_triangleDepth = 0.1f;
    public float m_triangleSize = 0.05f;
    private void Awake()
    {
        m_vertices = new NativeArray<Vector3>(SNAM16K.ARRAY_MAX_SIZE * 3, Allocator.Persistent);
        m_trianglesOrder = new NativeArray<int>(SNAM16K.ARRAY_MAX_SIZE * 6, Allocator.Persistent);
        m_meshObject = new Mesh();
        m_filter.sharedMesh = m_meshObject;

    }

    private void SetTriangle()
    {
        for (int index = 0; index < SNAM16K.ARRAY_MAX_SIZE; index++)
        {
            int t1, t2, t3, t4, t5, t6;
            int i1, i2, i3;

            i1 = index * 3;
            i2 = index * 3 + 1;
            i3 = index * 3 + 2;

            t1 = index * 6 + 0;
            t2 = index * 6 + 1;
            t3 = index * 6 + 2;
            t4 = index * 6 + 3;
            t5 = index * 6 + 4;
            t6 = index * 6 + 5;
            m_trianglesOrder[t1] = i1;
            m_trianglesOrder[t2] = i2;
            m_trianglesOrder[t3] = i3;
            m_trianglesOrder[t4] = i1;
            m_trianglesOrder[t5] = i3;
            m_trianglesOrder[t6] = i2;
        }

        m_meshObject.SetTriangles(m_trianglesOrder.ToArray(), 0);
    }
    private void OnDestroy()
    {
        m_vertices.Dispose();
        m_trianglesOrder.Dispose();
    }
    private bool m_firstTime = true;
    private void Update()
    {
        Job_SetDroneAsTriangle job = new Job_SetDroneAsTriangle
        {
            m_asteroid = m_projectile.GetNativeArray(),
            m_triangleDepth = m_triangleDepth,
            m_triangleSize = m_triangleSize,
            m_vertices = m_vertices
        };
        job.Schedule(SNAM16K.ARRAY_MAX_SIZE, 64).Complete();

        m_meshObject.SetVertices(m_vertices);
        if (m_firstTime)
        {
            m_firstTime = false;

            SetTriangle();
        }

    }
    private void Reset()
    {

        m_verticesCount = SNAM16K.ARRAY_MAX_SIZE * 3;
        m_trianglesOrderCount = SNAM16K.ARRAY_MAX_SIZE * 6;
    }

}
[BurstCompile]
public struct Job_SetDroneAsTriangle : IJobParallelFor
{


    [ReadOnly]
    public NativeArray<STRUCT_AsteroidCapsulePosition> m_asteroid;

    public float m_triangleDepth;
    public float m_triangleSize;


    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<Vector3> m_vertices;


    public void Execute(int index)
    {
        STRUCT_AsteroidCapsulePosition drone = m_asteroid[index];
        Vector3 direction = drone.m_currentPosition - drone.m_previousPosition;
        if(direction == Vector3.zero)
        {
            return;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        Quaternion right = rotation * Quaternion.Euler(0, 90, 0);
        Vector3 rightDir = (right * (Vector3.forward * m_triangleSize));
        m_vertices[index * 3] = drone.m_currentPosition + (rotation * (Vector3.forward * m_triangleDepth));
        m_vertices[index * 3 + 1] = drone.m_currentPosition + rightDir;
        m_vertices[index * 3 + 2] = drone.m_currentPosition + -rightDir;
    }
}


//[System.Serializable]
//public struct STRUCT_ShieldDroneAsUnity
//{

//    //public float m_gameSate; Removed
//    public Vector3 m_position;
//    public Quaternion m_rotation;
//    public float m_shield;
//}