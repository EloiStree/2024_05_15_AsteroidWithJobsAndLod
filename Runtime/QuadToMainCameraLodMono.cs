using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadToMainCameraLodMono : MonoBehaviour
{

    public Camera m_toTurnTo;
    public Transform m_whatToRotate;

    public void Update()
    {
        if (m_toTurnTo == null)
        {
            m_toTurnTo = Camera.main;
            return;
        }

        if (m_toTurnTo == null)
        {
            return;
        }

        TurnToCamera();
    }

    private void TurnToCamera()
    {
        if (m_whatToRotate != null && m_toTurnTo != null) { 
            m_whatToRotate.LookAt(m_toTurnTo.transform);
            m_whatToRotate.Rotate(0, 180, 0, Space.Self);
        }
    }

    public void OnValidate()
    {
        TurnToCamera();
    }

    private void Reset()
    {
        m_toTurnTo = Camera.main;
        m_whatToRotate = transform;
    }
}
