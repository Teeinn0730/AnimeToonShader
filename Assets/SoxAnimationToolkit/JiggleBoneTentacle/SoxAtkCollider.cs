using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoxAtkCollider : MonoBehaviour {
    [HideInInspector]
    public float m_version = 1.102f;

    public enum ColliderType
    { Sphere }
    public ColliderType m_colliderType = ColliderType.Sphere;

    public enum Axis
    { X, Y, Z }
    public Axis m_referenceAxisOfScale = Axis.X;

    public float m_sphereRadius = 0.1f;
    [HideInInspector]
    public float m_sphereRadiusScaled = 0f;

    [Range(0.0f, 1.0f)]
    public float m_friction = 0f;
    [HideInInspector]
    public float m_frictionInverse = 1f;

    public Color m_gizmoColor = Color.green;
    public bool m_showGizmoAtPlay = false;
    public bool m_showGizmoAtEditor = true;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (m_showGizmoAtPlay)
            {
                DrawGizmo();
            }
        }
        else
        {
            if (m_showGizmoAtEditor)
            {
                DrawGizmo();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        switch (m_referenceAxisOfScale)
        {
            case Axis.X:
                m_sphereRadiusScaled = m_sphereRadius * transform.lossyScale.x;
                break;
            case Axis.Y:
                m_sphereRadiusScaled = m_sphereRadius * transform.lossyScale.y;
                break;
            case Axis.Z:
                m_sphereRadiusScaled = m_sphereRadius * transform.lossyScale.z;
                break;
        }

        Gizmos.color = m_gizmoColor;
        Gizmos.DrawWireSphere(transform.position, m_sphereRadiusScaled);
    }

    private void OnValidate()
    {
        m_sphereRadius = Mathf.Max(0.0f, m_sphereRadius);
        m_frictionInverse = 1f - m_friction;
    }
}
