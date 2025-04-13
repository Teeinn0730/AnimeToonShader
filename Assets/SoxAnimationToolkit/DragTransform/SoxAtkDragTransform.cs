using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 부모 자식 관계와 상관 없이 작동하기 위해 모든 연산은 World 기반으로.
// Scale은 대상에서 제외. 부모의 스케일 변화시 자식인가 아닌가에 의해 형태가 완전히 달라지는 요소라서 Scale은 곤란.
public class SoxAtkDragTransform : MonoBehaviour {
    [HideInInspector]
    public float m_version = 1.102f;

    [Header("Equal Tension - Tension equal to Element 0")]
    public bool m_equalTension = true;

    // This variable was created because Unity could not animate the Tension array.
    public float m_tensionMultiplier = 1f;

    private bool m_initialized = false;

    [System.Serializable]
    public struct DragTransformSet
    {
        public Transform m_sourceObject;
        public Transform m_dragObject;
        public float m_positionTension;
        public float m_rotationTension;

        // 최초 Start시 로컬값 기억 (부모, 자식 관계와 상관 없음)
        [HideInInspector]
        public Vector3 m_localPosBak;
        [HideInInspector]
        public Quaternion m_localRotBak;

        // 이전 프레임에서 나의 상태 기록 (월드)
        [HideInInspector]
        public Vector3 m_posBefore;
        [HideInInspector]
        public Quaternion m_rotBefore;

        public DragTransformSet(Transform sourceObject, Transform dragObject, float positionTension, float rotationTension, Vector3 localPosBak, Quaternion localRotBak, Vector3 posBefore, Quaternion rotBefore)
        {
            m_sourceObject = sourceObject;
            m_dragObject = dragObject;
            m_positionTension = positionTension;
            m_rotationTension = rotationTension;
            m_localPosBak = localPosBak;
            m_localRotBak = localRotBak;
            m_posBefore = posBefore;
            m_rotBefore = rotBefore;
        }
    }

    [SerializeField]
    public DragTransformSet[] m_dragSet = new DragTransformSet[1]
    {
        new DragTransformSet(null, null, 5f, 5f, Vector3.zero, Quaternion.identity, Vector3.zero, Quaternion.identity)
    };

    private void OnValidate()
    {
        m_tensionMultiplier = Mathf.Max(0f, m_tensionMultiplier);

        if (m_dragSet == null)
            return;

        if (m_dragSet.Length <= 0)
            return;

        for (int i = 0; i < m_dragSet.Length; i++)
        {
            m_dragSet[i].m_positionTension = Mathf.Max(0f, m_dragSet[i].m_positionTension);
            m_dragSet[i].m_rotationTension = Mathf.Max(0f, m_dragSet[i].m_rotationTension);
        }

        if (m_equalTension && m_dragSet.Length >= 2)
        {
            for (int i = 1; i < m_dragSet.Length; i++)
            {
                m_dragSet[i].m_positionTension = m_dragSet[0].m_positionTension;
                m_dragSet[i].m_rotationTension = m_dragSet[0].m_rotationTension;
            }
        }
    }

    private void OnEnable()
    {
        if (!m_initialized)
        {
            Initialize();
        }
        Clear();
    }

    private void Initialize ()
    {
        if (m_dragSet == null)
            return;

        if (m_dragSet.Length <= 0)
            return;
        for (int i = 0; i < m_dragSet.Length; i++)
        {
            if (m_dragSet[i].m_sourceObject != null && m_dragSet[i].m_dragObject != null)
            {
                m_dragSet[i].m_localPosBak = m_dragSet[i].m_sourceObject.InverseTransformPoint(m_dragSet[i].m_dragObject.position);

                // 자식 역할의 로컬 로테이션 Inverse로 구하기 (자식 로테이션에서 부모 로테이션을 빼주는건데 쿼터니언은 빼기가 안되므로 Inverse 곱한다.
                // Quaternion.Inverse(부모역할로테이션) * 자식역할로테이션
                m_dragSet[i].m_localRotBak = Quaternion.Inverse(m_dragSet[i].m_sourceObject.rotation) * m_dragSet[i].m_dragObject.rotation;

                m_dragSet[i].m_posBefore = m_dragSet[i].m_dragObject.position;
                m_dragSet[i].m_rotBefore = m_dragSet[i].m_dragObject.rotation;
            }
        }

        m_initialized = true;
    }

    private void Update () {
        if (m_dragSet == null)
            return;

        if (m_dragSet.Length <= 0)
            return;

        for (int i = 0; i < m_dragSet.Length; i++)
        {
            if (m_dragSet[i].m_sourceObject != null && m_dragSet[i].m_dragObject != null)
            {
                Vector3 newPos = m_dragSet[i].m_sourceObject.TransformPoint(m_dragSet[i].m_localPosBak);
                m_dragSet[i].m_dragObject.position = Vector3.Lerp(m_dragSet[i].m_posBefore, newPos, m_dragSet[i].m_positionTension * Time.smoothDeltaTime * m_tensionMultiplier);

                Quaternion newRot = m_dragSet[i].m_sourceObject.rotation * m_dragSet[i].m_localRotBak;
                m_dragSet[i].m_dragObject.rotation = Quaternion.Lerp(m_dragSet[i].m_rotBefore, newRot, m_dragSet[i].m_rotationTension * Time.smoothDeltaTime * m_tensionMultiplier);

                // 다음 프레임을 위해 기록
                m_dragSet[i].m_posBefore = m_dragSet[i].m_dragObject.position;
                m_dragSet[i].m_rotBefore = m_dragSet[i].m_dragObject.rotation;
            }
        }
    }

    // Resets all transforms from the DragTransform. Useful for restarting a DragTransform from a new position.
    public void Clear ()
    {
        for (int i = 0; i < m_dragSet.Length; i++)
        {
            if (m_dragSet[i].m_sourceObject != null && m_dragSet[i].m_dragObject != null)
            {
                m_dragSet[i].m_dragObject.position = m_dragSet[i].m_sourceObject.TransformPoint(m_dragSet[i].m_localPosBak);
                m_dragSet[i].m_dragObject.rotation = m_dragSet[i].m_sourceObject.rotation * m_dragSet[i].m_localRotBak;

                m_dragSet[i].m_posBefore = m_dragSet[i].m_dragObject.position;
                m_dragSet[i].m_rotBefore = m_dragSet[i].m_dragObject.rotation;
            }
        }
    }
}
