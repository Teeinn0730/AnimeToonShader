using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 외부 오브젝트를 강제로 따라가도록 하는 스크립트, 마치 자식처럼 행동한다.
// FBX의 애니메이션과 이펙트 애니메이션의 애니메이터를 분리하기 위한 필요성에서 제작함
// FBX의 애니메이션 내부의 특정 본의 자식에 이펙트를 달아줘야하지만 애니메이터는 분리해야할 경우 직접 자식으로 연결하지 않고 이렇게 가상으로 연결한다.

// 주의사항 : 타겟의 부모가 비규격 애니메이션이 된 경우 로테이션과 관련하여 정상적인 자식으로 링크된 경우와 다르게 작동할 수 있다.
[ExecuteInEditMode]
public class ConstraintTransform : MonoBehaviour {
    [HideInInspector]
    public float m_version = 1.102f;

    public Transform m_target;
    public enum ScaleMode { LocalScale, HierarchyScale };
    public ScaleMode scaleMode = ScaleMode.LocalScale;

    private Transform m_transform;

	// Use this for initialization
	void Start () {
        // 속도를 위해 미리 트랜스폼을 얻어둔다.
        m_transform = transform;
	}
	
	// Update is called once per frame
	void Update () {
        if (m_target == null)
            return;

        if (Application.isPlaying)
        {
            m_transform.position = m_target.position;
            m_transform.rotation = m_target.rotation;
            if (scaleMode == ScaleMode.LocalScale)
            {
                m_transform.localScale = m_target.localScale;
            }
            else
            {
                m_transform.localScale = m_target.lossyScale;
            }
        }
        else
        {
            transform.position = m_target.position;
            transform.rotation = m_target.rotation;
            if (scaleMode == ScaleMode.LocalScale)
            {
                transform.localScale = m_target.localScale;
            }
            else
            {
                transform.localScale = m_target.lossyScale;
            }
        }
	}
}
