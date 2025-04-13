//using System.Collections;m_realHead
//using System.Collections.Generic;
using System;
using UnityEngine;

public class SoxAtkJiggleBoneSimple : MonoBehaviour
{
    [HideInInspector]
    public float m_version = 1.102f;

    // 씬 전체의 지글본을 기억하는 스태틱 배열
    [HideInInspector]
    public static SoxAtkJiggleBoneSimple[] m_jiggleBoneAll;
    [HideInInspector]
    public static bool m_jiggleBoneAllSearched = false; // 씬 전체를 검색한적이 있는지

    private bool m_initialized = false;
    [HideInInspector]
    public bool m_ifHead = true;
    private bool m_treeInit = false;
    [HideInInspector]
    public SoxAtkJiggleBoneSimple[] m_tree;

    private Transform meTrans;

    public enum Axis { X, Y, Z }

    public float m_targetDistance = 3.0f;
    public bool m_targetFlip = false;

    private const float mc_tensionMul = 0.1f;
    private float m_tensionProxy;
    public float m_tension = 30.0f;
    [Range(0.0f, 1.0f)]
    public float m_inercia = 0.85f;

    // 최적화를 위해 옵션이 바뀔 때에만 미리 세팅해두는 값, 업벡터는 월드가 아닌 오브젝트를 참조할 경우 매 프레임 세팅되기도 한다.
    private Vector3 m_upVector;

    public bool m_upWorld = false;
    public Transform m_upNode;
    public Axis m_upNodeAxis = Axis.Y;
    private bool m_upNodeAutoSet = false; // 최초 지글본 적용시 부모를 업노드로 자동 지정했는지 체크(1회만 해야함)

    public enum UpnodeControl { LookAt, AxisAlignment }
    public UpnodeControl m_upnodeControl = UpnodeControl.AxisAlignment;

    public Vector3 m_gravity = Vector3.zero;
    public SoxAtkCollider[] m_colliders;

    private Transform m_target;
    private Transform m_targetRoot;
    private SoxAtkTentacle m_soxAtkTentacle;

    private Vector3 m_forceVec;
    private Vector3 m_resultVec;
    private Vector3 m_lookWPos;

    private Vector3 m_beforTargetWPos;
    private Vector3 m_beforeInerciaVec;

    public bool m_optShowGizmosAtPlaying = false;
    public bool m_optShowGizmosAtEditor = true;
    public float m_optGizmoSize = 0.1f;
    public bool m_optShowHiddenNodes = false;

    // 히든 노드를 보이고 안보이고 하는 변화가 지글본에서 발생하는데
    // 하이러키 다시그리는건 에디터에서 호출해야하기때문에
    // 지글본에서의 변화가 발생했는지 여부를 기록하는 플래그
    // 에디터에서 토글 변화 발생하자마자 다시그리기 해봐야 지글본에서 하이러키 처리하기 전이다.
    // 지글본에서 하이러키 처리 직후 이 플래그를 세팅해주고 에디터에서 플래그 검사하여 강제 다시그리기
    public bool m_hierarchyChanged = false;

    // Tentacle에서 지글본의 헬퍼 노드들을 인식하려면 지글본의 초기화는 Awake에서 하고 텐타클의 초기화는 Start에서 한다.
    void Awake()
    {
        if (m_jiggleBoneAllSearched == false)
        {
            m_jiggleBoneAll = FindObjectsOfType<SoxAtkJiggleBoneSimple>();
            m_jiggleBoneAllSearched = true;

            // 최초로 Awake되는 지글본 하나가 나머지 모두를 Initialize 한다.
            for (int i = 0; i < m_jiggleBoneAll.Length; i++)
            {
                if (m_jiggleBoneAll[i].m_initialized == false && m_jiggleBoneAll[i].gameObject.activeInHierarchy && m_jiggleBoneAll[i].enabled)
                {
                    m_jiggleBoneAll[i].Initialize();
                }
            }

            // SetRealHead는 모든 지글본이 초기화된 상태에서 다시 한번 호출되어서 모든 지글본이 m_tree[0]에서 최상위 헤드를 기억하도록 한다.
            // 이 전단계까지는 모든 지글본의 m_tree[0]는 최상위가 아니다. (초기화 되는 순서가 뒤죽박죽이므로)
            for (int i = 0; i < m_jiggleBoneAll.Length; i++)
            {
                if (m_jiggleBoneAll[i].gameObject.activeInHierarchy && m_jiggleBoneAll[i].enabled)
                {
                    m_jiggleBoneAll[i].SetRealHead();
                }
            }

            m_jiggleBoneAll = null;
        }
    }

    private void OnEnable()
    {
        if (!m_initialized)
        {
            Initialize();
            SetRealHead();
        }

        Clear();
    }

    public void Initialize()
    {
        if (m_initialized)
            return;

        meTrans = transform;

        // tree가 초기화 안되어있으면
        if (!m_treeInit)
        {
            m_tree = new SoxAtkJiggleBoneSimple[] { this };

            // tree 초기화 했음 표시
            m_treeInit = true;
        }

        // 누가 우두머리인지 결정한다. m_ifHead, m_tree 결정
        SetHead();

        m_upVector = Vector3.up;

        // m_targetRoot는 지글본과 형제 계층구조에서 지글본을 따라다니면서 타겟 위치의 부모 역할을 한다. 지글본과 스케일도 동기화 해야함.
        m_targetRoot = new GameObject("SoxAtkJiggleboneTargetRoot_" + transform.name).transform;
        m_targetRoot.parent = meTrans.parent;
        m_targetRoot.hideFlags = HideFlags.HideInHierarchy;
        m_targetRoot.localPosition = meTrans.localPosition;
        m_targetRoot.localRotation = meTrans.localRotation;
        m_targetRoot.localScale = meTrans.localScale;

        // m_target은 지글본의 최후 도착지점 역할
        m_target = new GameObject("SoxAtkJiggleboneTarget_" + transform.name).transform;
        m_target.parent = m_targetRoot;
        m_target.hideFlags = HideFlags.HideInHierarchy;
        SetTargetDistance(); // m_lookAxis의 옵션에 따라서 m_target의 위치를 변경한다. 
        m_target.localRotation = Quaternion.identity;
        m_target.localScale = Vector3.one;

        m_forceVec = Vector3.zero;
        m_resultVec = Vector3.zero;
        m_lookWPos = Vector3.zero;

        m_beforTargetWPos = m_target.position;
        m_beforeInerciaVec = Vector3.zero;

        EnsureGoodVars();

        m_initialized = true;
    }

    // 누가 우두머리인지 결정한다. m_ifHead, m_tree 결정, Awake에서 호출됨
    private void SetHead()
    {
        // 부모가 없거나 부모 지글본이 없으면 내가 우두머리다. 아무것도 안하고 그냥 리턴
        if (meTrans.parent == null)
            return;
        SoxAtkJiggleBoneSimple headJiggleBone = meTrans.parent.GetComponent<SoxAtkJiggleBoneSimple>();
        if (headJiggleBone == null)
            return;
        // 부모 지글본이 꺼져있으면 역시 내가 우두머리다.
        if (headJiggleBone.gameObject.activeInHierarchy == false || headJiggleBone.enabled == false)
            return;

        // 이후 부모 지글본이 있는 경우

        // while 로 진짜 헤드 찾기. 초기화가 무작위로 되기때문에 헤드의 헤드의 헤드의 헤드가 진짜 해드일 수 있다.
        bool found = false;
        while (!found)
        {
            if (headJiggleBone.m_ifHead)
            {
                found = true;
            }
            else
            {
                // 헤드의 헤드 찾기
                headJiggleBone = headJiggleBone.m_tree[0];
            }
        }

        // 자신의 트리를 헤드 트리의 밑으로 더한다.
        // 헤드의 Start 함수가 작동 했을 수도 있고 안했을 수도 있다. m_treeInit을 검사해야함
        if (!headJiggleBone.m_treeInit)
        {
            // 초기화 안했으면 대신 초기화 해준다.
            headJiggleBone.m_tree = new SoxAtkJiggleBoneSimple[] { headJiggleBone };
            headJiggleBone.m_treeInit = true;
        }

        headJiggleBone.m_tree = ArrayAdd(headJiggleBone.m_tree, m_tree);

        // 나는 더이상 헤드가 아니다.
        m_ifHead = false;

        // 내 헤드는 headJiggleBone이다.
        m_tree = new SoxAtkJiggleBoneSimple[] { headJiggleBone };
    }

    // 모든 지글본의 SetHead가 수행된 이후 불리우는 함수. 최상위 진짜 헤드를 세팅한다.
    public void SetRealHead()
    {
        int safetyCheck = 0;  // 무한루프를 위한 안전장치
        bool found = false;
        SoxAtkJiggleBoneSimple jiggleBone = m_tree[0];
        SoxAtkJiggleBoneSimple realHead = m_tree[0];
        while (!found)
        {
            if (jiggleBone.m_ifHead)
            {
                realHead = jiggleBone;
                found = true;
            }
            else
            {
                jiggleBone = jiggleBone.m_tree[0];
                safetyCheck++;
            }

            // 십만번 이상 루프를 돌면 뭔가 문제가 있는 상황
            if (safetyCheck > 100000)
                found = true;
        }
        m_tree[0] = realHead;
    }

    private SoxAtkJiggleBoneSimple[] ArrayAdd(SoxAtkJiggleBoneSimple[] arrA, SoxAtkJiggleBoneSimple[] arrB)
    {
        if (arrA == null && arrB != null)
            return arrB;
        if (arrA != null && arrB == null)
            return arrA;
        if (arrA == null && arrB == null)
            return null;

        SoxAtkJiggleBoneSimple[] returnArr = new SoxAtkJiggleBoneSimple[arrA.Length + arrB.Length];
        Array.Copy(arrA, 0, returnArr, 0, arrA.Length);
        Array.Copy(arrB, 0, returnArr, arrA.Length, arrB.Length);

        return returnArr;
    }

    void Update()
    {
        // 헤드인 경우에만 tree를 업데이트 한다.
        if (m_ifHead)
        {
            JiggleBoneUpdateTree();
        }
    }

    private void JiggleBoneUpdateTree()
    {
        for (int i = 0; i < m_tree.Length; i++)
        {
            m_tree[i].JiggleBoneUpdate();
        }
    }

    public void JiggleBoneUpdate()
    {
        if (!this.gameObject.activeInHierarchy || !this.enabled)
            return;

        m_targetRoot.position = meTrans.position;
        m_targetRoot.localScale = meTrans.localScale;

        Vector3 tartegPos = m_target.position;
        bool collide = false;
        float friction = 1f;
        // Collider 먼저. 포스 벡터를 계산하기 전에 가야할 m_target.position부터 Collider에 의해 정해져야함
        if (m_colliders.Length > 0)
        {
            // 콜라이더가 여러 개 있더라도 먼저 체크되는 콜라이더부터 순차적으로 계산한다.
            // 계산 결과 나중에 처리한 콜라이더에 의하 앞선 콜라이더 안으로 들어가더라도 어쩔 수 없음.
            for (int i = 0; i < m_colliders.Length; i++)
            {
                if (m_colliders[i] != null)
                {
                    // 충돌체 중심에서 타겟을 바라보는 벡터
                    Vector3 lookFromCollider = m_target.position - m_colliders[i].transform.position;
                    float lookFromColliderLength = lookFromCollider.magnitude;
                    // 타겟을 바라보는 벡터의 길이가 충돌체의 반지름보다 작다면 (충돌 구 안에 포함된다면)
                    if (lookFromColliderLength < m_colliders[i].m_sphereRadiusScaled)
                    {
                        tartegPos =
                            m_colliders[i].transform.position +
                            (lookFromCollider.normalized * m_colliders[i].m_sphereRadiusScaled); // m_lookWPos를 충돌 구체 표면 위치로 옮긴다.
                    }

                    // Friction, 포스벡터를 계산하기 전이라서 이전 프레임의 관성으로만 검사한다. 포스벡터 연산 후에 충돌 검사하려면 연산이 더 필요해서 생략
                    // Collider가 여러 개 있을 수 있어서 여기서는 충돌 했는지만 검사
                    if (m_colliders[i].m_frictionInverse > 0 )
                    {
                        if (Vector3.Distance(m_beforTargetWPos, m_colliders[i].transform.position) < m_colliders[i].m_sphereRadiusScaled)
                        {
                            collide = true;
                            friction = m_colliders[i].m_frictionInverse;  // 마찰력은 최종적으로 충돌 검출된 Collider의 것을 사용
                        }
                    }
                }
            }
        }

        // 이번 이동에서 발생한 포스 벡터
        m_forceVec = (tartegPos - m_beforTargetWPos);
        // 이전 관성을 더한 뒤 텐션 적용한 결과 벡터 (DT?? 프레임이 떨어져서 DT가 증가하면 포스가 강해져야하고 inercia는 줄어들어야한다)
        m_resultVec = (m_forceVec * m_tensionProxy * Time.smoothDeltaTime) + m_beforeInerciaVec * (Mathf.Lerp(m_inercia, 0.0f, Time.smoothDeltaTime));
        // Gravity
        m_resultVec += m_gravity * Time.smoothDeltaTime;

        // 충돌했을 경우 마찰력 반영
        if (collide)
        {
            m_resultVec *= friction;
            m_beforeInerciaVec *= friction;
        }

        m_lookWPos = m_beforTargetWPos + m_resultVec;

        // m_upVector변수 세팅
        if (m_upnodeControl == UpnodeControl.AxisAlignment)
        {
            // Upnode Control : AxisAlignment
            // 업벡터의 기준이 월드가 아닌 오브젝트일 때에만 매 프레임 업데이트 한다. (오브젝트가 회전할 수 있으므로)
            if (!m_upWorld && m_upNode != null)
            {
                switch (m_upNodeAxis)
                {
                    case Axis.X:
                        m_upVector = m_upNode.right;
                        break;
                    case Axis.Y:
                        m_upVector = m_upNode.up;
                        break;
                    case Axis.Z:
                        m_upVector = m_upNode.forward;
                        break;
                }
            }
        }
        else
        {
            // Upnode Control : LookAt
            if (!m_upWorld && m_upNode != null)
            {
                m_upVector = m_upNode.position - meTrans.position;
            }
        }

        try
        {
            // LookAt은 같은 위치를 바라보는 등의 상황에서 다양한 에러가 발생한다.
            meTrans.LookAt(m_lookWPos, m_upVector);
        }
        catch { }

        m_beforTargetWPos = m_lookWPos;
        m_beforeInerciaVec = m_resultVec;
    }

    public bool Clear()
    {
        if (m_ifHead)
        {
            for (int i = 0; i < m_tree.Length; i++)
            {
                m_tree[i].m_beforTargetWPos = m_tree[i].m_target.position;
                m_tree[i].m_beforeInerciaVec = Vector3.zero;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        if (!this.gameObject.activeInHierarchy || !this.enabled)
            return;

        float gizmoSize = m_optGizmoSize * transform.lossyScale.x;
        if (Application.isPlaying)
        {
            // Playing
            if (m_optShowGizmosAtPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(meTrans.position, m_target.position);
                Gizmos.DrawWireSphere(m_target.position, gizmoSize);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(meTrans.position, m_lookWPos);
                Gizmos.DrawWireSphere(m_lookWPos, gizmoSize * 0.6f);
            }
        }
        else
        {
            // Editor
            if (m_optShowGizmosAtEditor)
            {
                Vector3 targetPos = Vector3.zero;
                float targetFlip = m_targetFlip ? -1.0f : 1.0f;
                targetPos = transform.TransformPoint(new Vector3(0.0f, 0.0f, m_targetDistance * targetFlip));

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetPos);
                Gizmos.DrawWireSphere(targetPos, gizmoSize);
            }
        }
    }

    //m_target의 위치를 변경한다. m_targetDistance의 거리가 달라질 경우에도 사용된다.
    public void SetTargetDistance()
    {
        if (m_target == null)
            return;

        float targetFlip = m_targetFlip ? -1.0f : 1.0f;

        m_target.localPosition = new Vector3(0.0f, 0.0f, m_targetDistance * targetFlip);
    }

    public void SetHiddenNodes()
    {
        if (!Application.isPlaying)
            return;

        if (m_optShowHiddenNodes)
        {
            m_targetRoot.hideFlags = HideFlags.None;
            m_target.hideFlags = HideFlags.None;
        }
        else
        {
            m_targetRoot.hideFlags = HideFlags.HideInHierarchy;
            m_target.hideFlags = HideFlags.HideInHierarchy;
        }
        m_hierarchyChanged = true;
    }

    // OnValidate 는 프로젝트 윈도우에서 선택시에도 실행되는 등 문제가 많아서 봉인하고 수동으로 호출
    public void MyValidate()
    {
        EnsureGoodVars();
        // Awake에서 m_target m_targetRoot m_proxyLook m_proxyAlign을 생성하기 전에 다음 함수들이 실행되는 것을 막기 위해 m_targetRoot만 하나 검사
        if (m_targetRoot != null)
        {
            SetTargetDistance();
            SetHiddenNodes();
        }
    }

    private void OnDestroy()
    {
        if (m_target != null)
            GameObject.DestroyImmediate(m_target.gameObject);
        if (m_targetRoot != null)
            GameObject.DestroyImmediate(m_targetRoot.gameObject);
    }

    // 옵션 등의 변수가 적절하도록 강제함
    // 원래 에디터 스크립트에 있던 함수인데 여러 오브젝트를 동시에 편집하기 위해서 시리얼라이즈를 하다보니
    // 에디터에서는 변수에 직접 접근을 하면 그 순간 전체가 한꺼번에 바뀌는 부작용이 생긴다.(시리얼라이즈 관련 작업을 잘 몰라서일지도)
    // 그래서 변수의 무결성 검사는 각자에게 위임하도록 방식을 변경했음.
    // Start 없이 데이터에서 불리는 함수이므로 meTrans 사용하지 않고 transform 사용함
    public void EnsureGoodVars()
    {
        m_tensionProxy = Mathf.Max(0.0f, m_tension) * mc_tensionMul;
        //m_inercia = Mathf.Max(0.0f, m_inercia);
        m_targetDistance = Mathf.Max(0.0f, m_targetDistance);
        m_optGizmoSize = Mathf.Max(0.0f, m_optGizmoSize);

        if (m_upNodeAutoSet == false)
        {
            if (transform.parent != null)
            {
                m_upNode = transform.parent;
            }
            m_upNodeAutoSet = true; // 업노드가 등록 되던 말던 이 다시 이 기능을 하면 안됨
        }

        // 자기 자신을 업노드로 등록했는지 검사
        // 컴포넌트를 Copy & Paste 한다거나 여러 이유로 자기 자신이 들어갈 수 있음
        if (m_upNode == transform)
        {
            m_upNode = null;
        }
    }
}
