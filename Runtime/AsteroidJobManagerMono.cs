using Eloi;
using Eloi.WatchAndDate;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Jobs;
using UnityEngine.UIElements;


public class AsteroideJobManagerMono : MonoBehaviour
{

    public byte m_poolId=1;
     int m_poolItemMax = 128 * 128;

    public int m_numberOfAsteroidsInGame = 20;
    public SNAM16K_ObjectBool m_asteroidDestroyedEvent;
    public SNAM16K_ObjectBool m_asteroidCreationEvent;
    public SNAM16K_AstreroidCreation m_asteroidInGame;
    public SNAM16K_AstreroidMoveConstant m_asteroidMoveUpdateInfo;
    public SNAM16K_AstreroidCapsulePosition m_asteroidPosition;


    public Transform m_centerOfSpace;

    public float m_skyHeight=ushort.MaxValue/2f;
    public float m_squareWidth = ushort.MaxValue / 2f;

    public UnityEvent<STRUCT_AsteroidCreationEvent> m_onAsteroidCreated;
    public UnityEvent<STRUCT_AsteroidDestructionEvent> m_onAsteroidDestroyed;

    public float m_minSpeed=1;
    public float m_maxSpeed=10;
    public float m_minSize = 0.1f;
    public float m_maxSize = 1f;

    public void OnEnable()
    {

        m_poolItemMax = SNAM16K.ARRAY_MAX_SIZE;
        RandomizedAll();
    }

    [ContextMenu("Randomized All")]
    public void RandomizedAll()
    {
        NativeArray<STRUCT_AsteroidCreationEvent> asteroidInGame = m_asteroidInGame.GetNativeArray();
        NativeArray<STRUCT_AsteroidMoveConstant> asteroidMoveUpdateInfo = m_asteroidMoveUpdateInfo.GetNativeArray();

        for (int i = 0; i < m_poolItemMax; i++)
        {
            STRUCT_AsteroidCreationEvent a = asteroidInGame[i] ;
            STRUCT_AsteroidMoveConstant m= asteroidMoveUpdateInfo[i];
            a.m_poolId = m_poolId;
            a.m_poolItemIndex = i;
            SetRandomStartPointTo(ref a,ref m);
            asteroidInGame[i] = a;
            asteroidMoveUpdateInfo[i] = m;
            m_onAsteroidCreated.Invoke(a);
            if(m_onAsteroidChangedIndex!=null)
                m_onAsteroidChangedIndex.Invoke(i);
        }
    }




    private void SetRandomStartPointTo(ref STRUCT_AsteroidCreationEvent asteroidCreationEvent, ref STRUCT_AsteroidMoveConstant moveInfo)
    {
        asteroidCreationEvent.m_startPosition = new Vector3(UnityEngine.Random.Range(-m_squareWidth, m_squareWidth), UnityEngine.Random.Range(0, m_skyHeight), UnityEngine.Random.Range(-m_squareWidth, m_squareWidth));
        asteroidCreationEvent.m_startRotationEuler = Quaternion.Euler(UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360), UnityEngine.Random.Range(0, 360));
        asteroidCreationEvent.m_startDirection = asteroidCreationEvent.m_startRotationEuler * Vector3.forward;
        asteroidCreationEvent.m_speedInMetersPerSecond = UnityEngine.Random.Range(m_minSpeed, m_maxSpeed);
        asteroidCreationEvent.m_colliderRadius = UnityEngine.Random.Range(m_minSize, m_maxSize);
        asteroidCreationEvent.m_serverUtcNowTicks = DateTime.UtcNow.Ticks;


        moveInfo.m_startUtcNowTicks = asteroidCreationEvent.m_serverUtcNowTicks;
        moveInfo.m_speedInMetersPerSecond = asteroidCreationEvent.m_speedInMetersPerSecond;
        moveInfo.m_startPoint = asteroidCreationEvent.m_startPosition;
        moveInfo.m_direction = asteroidCreationEvent.m_startDirection;



    }

    public long m_currentTickServerUtcNow;
    public long m_currentTickServerUtcPrevious;
    public long m_updateTick;
    public WatchAndDateTimeActionResult m_moveObject;
    public WatchAndDateTimeActionResult m_outOfBoxed;
    public WatchAndDateTimeActionResult m_broadcastNewPosition;
    public void Update()
    {
        m_moveObject.StartCounting();
        m_updateTick++;
        m_currentTickServerUtcPrevious = m_currentTickServerUtcNow;
        m_currentTickServerUtcNow = DateTime.UtcNow.Ticks;
        STRUCTJOB_AsteroideMoveJob moveJob = new STRUCTJOB_AsteroideMoveJob();
        moveJob.m_asteroidInGame = m_asteroidMoveUpdateInfo.GetNativeArray();
        moveJob.m_currentExistance = m_asteroidPosition.GetNativeArray();
        moveJob.m_currentMaxAsteroide = m_numberOfAsteroidsInGame;
        moveJob.m_serverCurrentUtcNowTicks = m_currentTickServerUtcNow;
        moveJob.m_serverCurrentUtcPreviousTicks = m_currentTickServerUtcPrevious;
        JobHandle moveJobHandle = moveJob.Schedule(m_numberOfAsteroidsInGame, 64);
        moveJobHandle.Complete();

       m_moveObject.StopCounting();
        m_outOfBoxed.StartCounting();
        AsteroideOutOfBoundJob outOfBoundJob = new AsteroideOutOfBoundJob();
        outOfBoundJob.m_destroyEvent = m_asteroidDestroyedEvent.GetNativeArray();
        outOfBoundJob.m_currentExistance = m_asteroidPosition.GetNativeArray();
        outOfBoundJob.m_currentMaxAsteroide = m_numberOfAsteroidsInGame;
        outOfBoundJob.m_centerPosition = m_centerOfSpace.position;
        outOfBoundJob.m_maxHeightDistance = m_skyHeight;
        outOfBoundJob.m_maxWidthDistance = m_squareWidth;
        JobHandle outOfBoundJobHandle = outOfBoundJob.Schedule(m_numberOfAsteroidsInGame, 64);
        outOfBoundJobHandle.Complete();

        m_outOfBoxed.StopCounting();

        m_broadcastNewPosition.StartCounting();


        NativeArray<bool> asteroidDestroyedEvent = this.m_asteroidDestroyedEvent.GetNativeArray();
        NativeArray<bool> asteroidCreationEvent = this.m_asteroidCreationEvent.GetNativeArray();
        NativeArray<STRUCT_AsteroidCreationEvent> asteroidInGame = m_asteroidInGame.GetNativeArray();
        NativeArray<STRUCT_AsteroidMoveConstant> asteroidMoveUpdateInfo = m_asteroidMoveUpdateInfo.GetNativeArray();

        for (int i = 0; i < m_numberOfAsteroidsInGame; i++)
        {
            if (asteroidDestroyedEvent[i])
            {
                asteroidDestroyedEvent[i] = false;
                asteroidCreationEvent[i] = true;
                STRUCT_AsteroidCreationEvent a = asteroidInGame[i];
                STRUCT_AsteroidMoveConstant m = asteroidMoveUpdateInfo[i];
                SetRandomStartPointTo(ref a, ref m);

                asteroidMoveUpdateInfo[i] = m;
                asteroidInGame[i] = a;
                m_onAsteroidDestroyed.Invoke(new STRUCT_AsteroidDestructionEvent() { 
                    m_poolId = m_poolId, 
                    m_poolItemIndex = i,
                    m_serverUtcNowTicks = 
                    m_currentTickServerUtcNow });
            }
        }
        for (int i = 0; i < m_numberOfAsteroidsInGame; i++) { 
            if (asteroidCreationEvent[i])
            {
                asteroidCreationEvent[i] = false;
                m_onAsteroidCreated.Invoke(asteroidInGame[i]);
                if(m_onAsteroidChangedIndex!=null)
                m_onAsteroidChangedIndex.Invoke(i);
            }
        }
        if (m_debugLine) { 
            NativeArray<STRUCT_AsteroidCapsulePosition> asteroidPosition = m_asteroidPosition.GetNativeArray();
            for(int i = 0;  i < m_numberOfAsteroidsInGame; i++)
            {
                float distance = Vector3.Distance(asteroidPosition[i].m_currentPosition, asteroidPosition[i].m_previousPosition);
                if(distance< m_maxDistanceToDraw)
                Debug.DrawLine(asteroidPosition[i].m_currentPosition, asteroidPosition[i].m_previousPosition, Color.red, 0.1f);
            }
        }
        m_broadcastNewPosition.StopCounting();
    }
    public bool m_debugLine=true;
    public float m_maxDistanceToDraw=5;

    

    public Action<int> m_onAsteroidChangedIndex;

    public void AddCreationNewAsteroidIndex(Action<int> onAsteroidchanged)
    {
        m_onAsteroidChangedIndex += onAsteroidchanged;
    }
    public void RemoveCreationnewAsteroidIndex(Action<int> onAsteroidchanged)
    {
        m_onAsteroidChangedIndex -= onAsteroidchanged;
    }
}


[BurstCompile]
public struct STRUCTJOB_IsProjectileTouchingTarget: IJobParallelFor
{
    [WriteOnly]
    public NativeArray<bool> m_isTouchingTarget;

    [ReadOnly]
    public NativeArray<STRUCT_AsteroidCapsulePosition> m_capsulePosition;

    public Vector3 m_targetPosition;
    public Vector3 m_targetPreviousPosition;
    public float m_targetRadius;
    internal bool m_useCapsuleCollision;

    public void Execute(int index)
    {

        if (m_useCapsuleCollision) { 

            // Verified that the utility exit the collision check if out of range directly.
        
            m_isTouchingTarget[index] = CapsuleLineCollisionUtility.IsTouching(m_capsulePosition[index].m_previousPosition, m_capsulePosition[index].m_currentPosition, m_capsulePosition[index].m_capsuleRadius, m_targetPreviousPosition, m_targetPosition, m_targetRadius);
        }
        else
        { 
            float distanceToTouch = m_capsulePosition[index].m_capsuleRadius + m_targetRadius;
            if (Vector3.Distance(m_capsulePosition[index].m_currentPosition, m_targetPosition) < distanceToTouch)
            {
                m_isTouchingTarget[index] = true;
                return;
            }
            else if (Vector3.Distance(m_capsulePosition[index].m_currentPosition, m_targetPreviousPosition) < distanceToTouch)
            {
                m_isTouchingTarget[index] = true;
                return;
            }
            else if (Vector3.Distance(m_capsulePosition[index].m_previousPosition, m_targetPosition) < distanceToTouch)
            {
                m_isTouchingTarget[index] = true;
                return;
            }
            else if (Vector3.Distance(m_capsulePosition[index].m_previousPosition, m_targetPreviousPosition) < distanceToTouch)
            {
                m_isTouchingTarget[index] = true;
                return;
            }
            m_isTouchingTarget[index] = false;

        }

    }
}





