#if UNITY_EDITOR
using UnityEngine;

public class SoxAtkMotionPath : MonoBehaviour {
    [HideInInspector]
    public float m_version = 1.102f;

    [HideInInspector]
    public bool m_initialized = false;

    public Animator m_animator;
    public Transform m_motionPathObject;
    [Tooltip("Reference object for motion path generation (optional)")]
    public Transform m_baseOfPath;
    [HideInInspector]
    public int m_animClipIndex = 0;

    public bool m_autoUpdate = true;

    public Color m_pathColor = Color.green;

    public int m_pathStep = 60;
    [HideInInspector]
    public Vector3[] m_pathPositions = new Vector3[60];

    [HideInInspector]
    public float m_timeStart = 0.0f;
    [HideInInspector]
    public float m_timeEnd = 1.0f;

    [HideInInspector]
    public float m_animationSlider = 0.0f;

    private Matrix4x4 m_meMatrix;
	
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        if (!m_initialized)
            return;

        Gizmos.color = m_pathColor;
        if (m_baseOfPath == null)
        {
            m_meMatrix = transform.localToWorldMatrix;
        }
        else
        {
            m_meMatrix = m_baseOfPath.localToWorldMatrix;
        }

        for (int i = 1; i < m_pathPositions.Length; i++)
        {
            Gizmos.DrawLine(
                m_meMatrix.MultiplyPoint3x4(m_pathPositions[i - 1]),
                m_meMatrix.MultiplyPoint3x4(m_pathPositions[i])
                );
        }
    }
    private void OnValidate()
    {
        m_pathStep = Mathf.Max(2, m_pathStep);
        m_timeStart = Mathf.Min(m_timeStart, m_timeEnd);
    }
}
#endif
