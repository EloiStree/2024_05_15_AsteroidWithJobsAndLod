using Eloi.WatchAndDate;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class AsteroidByteToTransformViewMono : MonoBehaviour
{
    public NativeArray<STRUCT_AsteroidCreationEvent> m_asteroidCreationEventRef;
    public NativeArray<STRUCT_AsteroidMoveConstant> m_asteroidMovement;
    public NativeArray<STRUCT_AsteroidCapsulePosition> m_asteroidPosition;

    public Transform[] m_whatToMove;


    public int m_currentLenght;
    public void SetNativeArray(NativeArray<STRUCT_AsteroidCreationEvent> asteroid)
    {

        m_asteroidCreationEventRef = asteroid;
        if (asteroid.Length != m_currentLenght)
        {
            m_currentLenght = asteroid.Length;
            if (m_asteroidPosition != null && m_asteroidPosition.IsCreated)
            {
                m_asteroidPosition.Dispose();
            }
            if (m_asteroidMovement != null && m_asteroidMovement.IsCreated)
            {
                m_asteroidMovement.Dispose();
            }
            m_asteroidPosition = new NativeArray<STRUCT_AsteroidCapsulePosition>(asteroid.Length, Allocator.Persistent);
            m_asteroidMovement = new NativeArray<STRUCT_AsteroidMoveConstant>(asteroid.Length, Allocator.Persistent);
        }
    }
    private void OnDestroy()
    {
        if (m_asteroidMovement.IsCreated)
        {
            m_asteroidMovement.Dispose();
        }
        if (m_asteroidPosition.IsCreated)
        {
            m_asteroidPosition.Dispose();
        }
    }
    public long m_currentTickServerUtcNow;
    public long m_currentTickServerUtcPrevious;

    public WatchAndDateTimeActionResult m_moveObject;
    public void Update()
    {
        m_moveObject.StartCounting();
        m_currentTickServerUtcPrevious = m_currentTickServerUtcNow;
        m_currentTickServerUtcNow = DateTime.UtcNow.Ticks;
        STRUCTJOB_AsteroideMoveJob moveJob = new STRUCTJOB_AsteroideMoveJob();
        moveJob.m_asteroidInGame = m_asteroidMovement;
        moveJob.m_currentExistance = m_asteroidPosition;
        moveJob.m_currentMaxAsteroide = m_asteroidCreationEventRef.Length;
        moveJob.m_serverCurrentUtcNowTicks = m_currentTickServerUtcNow;
        moveJob.m_serverCurrentUtcPreviousTicks = m_currentTickServerUtcPrevious;
        JobHandle moveJobHandle = moveJob.Schedule(m_asteroidCreationEventRef.Length, 64);
        moveJobHandle.Complete();



        STRUCTJOB_AsteroideMoveApplyToTransform moveApplyToTransform = new STRUCTJOB_AsteroideMoveApplyToTransform();
        moveApplyToTransform.m_currentExistance = m_asteroidPosition;
        moveApplyToTransform.m_currentMaxAsteroide = m_asteroidPosition.Length;
        TransformAccessArray transformAccessArray = new TransformAccessArray(m_whatToMove.Length);
        transformAccessArray.SetTransforms(m_whatToMove);

        JobHandle moveApplyToTransformHandle = moveApplyToTransform.Schedule(transformAccessArray);
        moveApplyToTransformHandle.Complete();
        transformAccessArray.Dispose();

        m_moveObject.StopCounting();
    }
}
