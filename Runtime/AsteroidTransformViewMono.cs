using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class AsteroidTransformViewMono : MonoBehaviour {

    public AsteroideJobManagerMono m_manager;

    public Transform[] m_transformToUsed;

    public Transform m_hideTransform;


    public bool m_createPool = true;
    public int m_poolSize = 128 * 128;
    public GameObject m_prefab;
    public Transform m_poolParent;




    private void Awake()
    {
        if (m_createPool)
        {
            CreatePool(m_poolSize, m_prefab);
        }
        m_manager.AddCreationNewAsteroidIndex(ReactToAsteroidNewPosition);
        //for (int i = 0; i < m_transformToUsed.Length; i++)
        //{
        //    m_transformToUsed[i].gameObject.SetActive(false);
        //    m_transformToUsed[i].position = m_hideTransform.position;
        //    m_transformToUsed[i].rotation = m_hideTransform.rotation;
        //    m_transformToUsed[i].localScale = m_hideTransform.localScale;
        //}
        for (int i = 0; i < m_transformToUsed.Length; i++)
        {
            ReactToAsteroidNewPosition(i);
        }
    }

    private void CreatePool(int m_poolSize, GameObject m_prefab)
    {
        m_transformToUsed = new Transform[m_poolSize];
        for (int i = 0; i < m_poolSize; i++)
        {
            GameObject go = Instantiate(m_prefab);
            go.SetActive(false);
            go.transform.parent=(m_poolParent);
            m_transformToUsed[i] = go.transform;
        }
    }

    private void ReactToAsteroidNewPosition(int index)
    {
        if(index< m_transformToUsed.Length)
        {
            STRUCT_AsteroidCreationEvent c = m_manager.m_asteroidInGame.Get(index);
            Transform t = m_transformToUsed[index];
            t.gameObject.SetActive(true);
            t.position = c.m_startPosition;
            t.rotation = c.m_startRotationEuler;
            t.localScale= Vector3.one * c.m_colliderRadius * 2;
        }
    }

    public void OnDestroy()
    {
        m_manager.RemoveCreationnewAsteroidIndex(ReactToAsteroidNewPosition);
    }

    public void Update()
    {
        STRUCTJOB_AsteroideMoveApplyToTransform moveApplyToTransform = new STRUCTJOB_AsteroideMoveApplyToTransform();
        moveApplyToTransform.m_currentExistance = m_manager.m_asteroidPosition.GetNativeArray();
        moveApplyToTransform.m_currentMaxAsteroide = m_manager.m_numberOfAsteroidsInGame;
        TransformAccessArray transformAccessArray = new TransformAccessArray(m_transformToUsed.Length);
        transformAccessArray.SetTransforms(m_transformToUsed);

        JobHandle moveApplyToTransformHandle = moveApplyToTransform.Schedule(transformAccessArray);
        moveApplyToTransformHandle.Complete();
        transformAccessArray.Dispose();
    }
}





