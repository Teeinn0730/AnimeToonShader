//using System.Collections;
using System.Collections.Generic;
//using System;
//using System.Reflection;
using UnityEngine;

public class SoxAtkTentacle : MonoBehaviour {
    [HideInInspector]
    public float m_version = 1.102f;

    private bool m_initialized = false;

    // 애니메이션 업데이트를 반영한다. 확실히 애니메이션 업데이트가 존재할 때에만 사용해야한다.
    // 애니메이션 업데이트가 없는데 사용하면 m_keepInitialRotation 기준점이 계속 바뀌는 듯한 효과가 발생하여 계속 빙글빙글 돈다
    // Update에서 애니메이션이 작동하고, LateUpdate에서 텐터클이 작동한다는 가정에서 사용되는 기능이다. Update에서의 애니메이션이 m_keepInitialRotation 역할을 한다.
    [HideInInspector]
    public bool m_animated = false;

    [HideInInspector]
    public bool m_keepInitialRotation = true;

    // 지글본과 연동을 위해서 에디터에서 보이는 m_nodes가 있고 실제 회전에 사용되는 m_nodes이 있다.
    // m_nodes를 검사해서 지글본이 없으면 m_nodes에 원래 노드를 넣고, 지글본이 있으면 지글본의 헬퍼 본을 m_nodes에 넣는다.
    // 지글본에 텐타클이 적영되면 지글본의 숨겨진 기준 본을 텐터클이 흔들게 된다.
    public Transform[] m_nodes = new Transform[5];

    [HideInInspector]
    public List<SoxAtkJiggleBone> m_jiggleboneHeads = new List<SoxAtkJiggleBone>();

    // 텐터클이 지글본과 연계하여 사용될 때에는 m_nodes가 지글본 내부의 더미로 대체된다.
    // Animation 혹은 m_keepInitialRotation의 기준은 원래 노드여야하므로 원래 노드를 기억하고있어야함.
    private Transform[] m_nodesOriginal = new Transform[5];

    private Quaternion[] m_nodesSaveLocalRotations = new Quaternion[5];

    private float[] m_nodesSaveDistances = new float[5];
    private float m_distanceAll = 0.0f;
    private Vector3[] m_wavesetMixEuler = new Vector3[5];

    private Transform m_rootNode;
    private Quaternion m_rootRotation;

    public enum Axis { X, Y, Z }

    public const float mc_freqMul = 0.01f;

    [System.Serializable]
    public struct Waveset
    {
        public Axis m_rotateAxis;
        public float m_frequency;
        [HideInInspector]
        public float m_frequencyProxy;
        public float m_strengthStart;
        public float m_strengthEnd;
        public float m_speed;
        public float m_offset;
        [HideInInspector]
        public float[] m_nodesSaveStrengths;

        public Waveset(Axis rotateAxis, float frequency, float strengthStart, float strengthEnd, float speed, float offset, float[] nodesSaveStrengths)
        {
            m_rotateAxis = rotateAxis;
            m_frequency = frequency;
            m_frequencyProxy = frequency * mc_freqMul;
            m_strengthStart = strengthStart;
            m_strengthEnd = strengthEnd;
            m_speed = speed;
            m_offset = offset;
            m_nodesSaveStrengths = nodesSaveStrengths;
        }
    }

    public Waveset[] wavesets = new Waveset[1]
    {
        new Waveset(Axis.X, -20.0f, 5.0f, 40.0f, 5.0f, 0.0f, new float[]{0, 0, 0, 0, 0})
    };

#if UNITY_EDITOR
    [HideInInspector]
    public float m_bakeTime = 1f;
    [HideInInspector]
    public int m_bakeSamples = 30;
    [HideInInspector]
    public bool m_bakeLoopTime = true;
    [HideInInspector]
    public bool m_bakeDisableTentacle = true;
    [HideInInspector]
    public float m_optimizeThresholdTangent = 1f;
    [HideInInspector]
    public float m_optimizeThresholdDefaultStrength = 10f;
    [HideInInspector]
    public float m_optimizeThresholdStrengthRate = 0f; // Strength 를 적용하는 정도 (0 ~ 1)
#endif

    // Tentacle에서 지글본의 헬퍼 노드들을 인식하려면 지글본의 초기화는 Awake에서 하고 텐타클의 초기화는 Start에서 한다.
    void Start () {
        if (!m_initialized && this.gameObject.activeInHierarchy && this.enabled)
            Initialize();
    }

    private void OnEnable()
    {
        if (!m_initialized)
        {
            // 초기화 안되있으면 초기화
            Initialize();
        }
        else
        {
            // 초기화 되어있으면 SaveNodesJigglebone()만 수행 (초기화에 포함된 것임)
            SaveNodesJigglebone();
        }
    }

    private void OnDisable()
    {
        RevertNodesJigglebone();
    }

    public void Initialize()
    {
        InitArrays();

        if (m_nodes[0] != null)
        {
            m_rootNode = m_nodes[0].parent;
        }

        // SaveLocalRotations에서 사용할 m_rootRotation 준비 
        if (m_rootNode != null)
        {
            m_rootRotation = m_rootNode.rotation;
        }
        else
        {
            m_rootRotation = Quaternion.identity;
        }

        for (int o = 0; o < m_nodes.Length; o++)
        {
            m_nodesOriginal[o] = m_nodes[o];
        }

        SaveLocalRotations();
        SaveDistances();
        SaveStrengths();

        SaveNodesJigglebone();

        // m_wavesetMixEuler 초기화
        for (int p = 0; p < m_wavesetMixEuler.Length; p++)
        {
            m_wavesetMixEuler[p] = Vector3.zero;
        }

        m_initialized = true;
    }
	
    // Update()에서의 애니메이션을 반영하기 위해 텐터클은 LateUpdate에서 작동한다.
	void LateUpdate ()
    {
        if (wavesets == null)
            return;

        // 루트노드 로테이션 업데이트
        if (m_rootNode != null)
        {
            m_rootRotation = m_rootNode.rotation;
        }

        // Update()에서 발생한 애니메이션 변화를 마치 m_keepInitialRotation용의 회전값에 반영한다.
        if (m_animated)
            SaveLocalRotations();

        UpdateTentalces(Time.time, false);

        // JiggleBone으로부터 넘겨받은 지글본 업데이트 수행
        if (m_jiggleboneHeads.Count > 0)
        {
            for (int i = 0; i < m_jiggleboneHeads.Count; i++)
            {
                m_jiggleboneHeads[i].JiggleBoneUpdateTree();
            }
        }
    }

    public Vector3[] UpdateTentalces(float time, bool bake)
    {
        for (int p = 0; p < m_nodes.Length; p++)
        {
            m_wavesetMixEuler[p] = Vector3.zero;
        }

        // wavesets 들을 합산한 m_wavesetMixEuler를 계산한다.
        for (int i = 0; i < wavesets.Length; i++)
        {
            for (int p = 0; p < m_nodes.Length; p++)
            {
                float tempAngle = Mathf.Sin(
                    (m_nodesSaveDistances[p] + wavesets[i].m_offset) * wavesets[i].m_frequencyProxy
                    + (time * wavesets[i].m_speed)
                    ) * wavesets[i].m_nodesSaveStrengths[p];

                switch (wavesets[i].m_rotateAxis)
                {
                    case Axis.X:
                        m_wavesetMixEuler[p].x += tempAngle;
                        break;
                    case Axis.Y:
                        m_wavesetMixEuler[p].y += tempAngle;
                        break;
                    case Axis.Z:
                        m_wavesetMixEuler[p].z += tempAngle;
                        break;
                }
            }
        }

        // m_keepInitialRotation 옵션에 따른 각 노드의 로테이션 반영
        for (int p = 0; p < m_nodes.Length; p++)
        {
            if (m_nodes[p] != null)
            {
                if (m_keepInitialRotation || m_animated)
                {
                    m_nodes[p].rotation = m_rootRotation * m_nodesSaveLocalRotations[p] * Quaternion.Euler(m_wavesetMixEuler[p]);

                    #if UNITY_EDITOR
                    if (bake)
                    {
                        Quaternion localRot = Quaternion.identity;
                        if (p == 0)
                        {
                            localRot = m_nodes[p].rotation * Quaternion.Inverse(m_rootRotation);
                        }
                        else
                        {
                            localRot = m_nodes[p].rotation * Quaternion.Inverse(m_nodes[p - 1].rotation);
                            
                        }
                        m_wavesetMixEuler[p] = localRot.eulerAngles;
                        m_wavesetMixEuler[p] = Restrict180Euler(m_wavesetMixEuler[p]);
                    }
                    #endif

                }
                else
                {
                    m_nodes[p].rotation = m_rootRotation * Quaternion.Euler(m_wavesetMixEuler[p]);
                }
            }
        }

        return m_wavesetMixEuler;
    }

#if UNITY_EDITOR
    private Vector3 Restrict180Euler(Vector3 euler)
    {
        Vector3 result = euler;
        result.x = (result.x > 180f) ? result.x - 360f : result.x;
        result.y = (result.y > 180f) ? result.y - 360f : result.y;
        result.z = (result.z > 180f) ? result.z - 360f : result.z;
        return result;
    }
#endif

    private void InitArrays()
    {
        // m_nodes 의 숫자를 변경할 때 Euler 저장 배열과 Distance 저장 배열의 숫자도 같이 변경한다.
        if (wavesets != null)
        {
            
            // 노드는 최소 1개 이상이 되도록 강제한다.
            if (m_nodes.Length < 1)
            {
                m_nodes = new Transform[1];
            }

            if (m_nodes.Length != m_nodesOriginal.Length)
            {
                m_nodesOriginal = new Transform[m_nodes.Length];
            }

            for (int o = 0; o < m_nodes.Length; o++)
            {
                m_nodesOriginal[o] = m_nodes[o];
            }

            // m_nodesSaveLocalRotations 하나만 숫자가 달라도 나머지 다른 배열들 모두 숫자를 재설정하려고 했으나 중간에 변수가 추가되는 등 예외상황에서 배열 수가 안맞는 일이 있어서 매 번 죄다 체크
            if (m_nodes.Length != m_nodesSaveLocalRotations.Length)
            {
                m_nodesSaveLocalRotations = new Quaternion[m_nodes.Length];
            }

            if (m_nodes.Length != m_nodesSaveDistances.Length)
            {
                m_nodesSaveDistances = new float[m_nodes.Length];
            }

            for (int i = 0; i < wavesets.Length; i++)
            {
                if (m_nodes.Length != wavesets[i].m_nodesSaveStrengths.Length)
                {
                    wavesets[i].m_nodesSaveStrengths = new float[m_nodes.Length];
                }
            }

            if (m_nodes.Length != m_wavesetMixEuler.Length)
            {
                m_wavesetMixEuler = new Vector3[m_nodes.Length];
            }

            SaveFrequencies();
        }
    }

    // OnValidate 는 프로젝트 윈도우에서 선택시에도 실행되는 등 문제가 많아서 봉인하고 수동으로 호출
    public void MyValidate()
    {
        if (!m_initialized && this.gameObject.activeInHierarchy && this.enabled)
            Initialize();
        SaveFrequencies();
        SaveStrengths();
    }

    // 지정된 노드들의 로컬 회전을 Start에서 기억시킨다.
    public void SaveLocalRotations()
    {
        if (wavesets == null)
            return;

        for (int p = 0; p < m_nodesOriginal.Length; p++)
        {
            if (m_nodesOriginal[p] != null)
            {
                m_nodesSaveLocalRotations[p] = Quaternion.Inverse(m_rootRotation) * m_nodesOriginal[p].rotation;
            }
            else
            {
                m_nodesSaveLocalRotations[p] = Quaternion.identity;
            }
        }
    }

    // 지정된 노드들의 Distance 를 Start에서 기억시킨다. 자신의 바로 앞 노드로부터의 거리이고 가장 앞 노드는 0
    public void SaveDistances()
    {
        if (wavesets == null)
            return;

        m_distanceAll = 0.0f;
        m_nodesSaveDistances[0] = 0.0f;
        for (int p = 1; p < m_nodes.Length; p++)
        {
            if (m_nodes[p] != null && m_nodes[p - 1] != null)
            {
                float avrScale = (Mathf.Abs(m_nodes[p].lossyScale.x) + Mathf.Abs(m_nodes[p].lossyScale.y) + Mathf.Abs(m_nodes[p].lossyScale.z)) / 3.0f;
                m_nodesSaveDistances[p] = m_distanceAll + Vector3.Distance(m_nodes[p].position, m_nodes[p-1].position) / avrScale;
            }
            else
            {
                m_nodesSaveDistances[p] = m_distanceAll;
            }
            m_distanceAll = m_nodesSaveDistances[p];
        }
    }

    private void SaveFrequencies()
    {
        for (int i = 0; i < wavesets.Length; i++)
        {
            wavesets[i].m_frequencyProxy = wavesets[i].m_frequency * mc_freqMul;
        }
    }

    // SaveStrengths는 외부에서 세팅할 일이 없어서 private
    // strength 시작과 끝 값을 매 Update마다 lerp 연산하지 않기 위해서 노드마다 각자의 strength를 미리 계산해둔다.
    // SaveDistances가 먼저 연산된 이후에 이것을 해야한다. distance 기반이기때문.
    private void SaveStrengths()
    {
        if (wavesets == null)
            return;

        for (int i = 0; i < wavesets.Length; i++)
        {
            float strengthGap = wavesets[i].m_strengthEnd - wavesets[i].m_strengthStart;
            wavesets[i].m_nodesSaveStrengths[0] = wavesets[i].m_strengthStart; // 최초 노드는 Strength 시작값과 일치
            for (int p = 1; p < m_nodes.Length; p++)
            {
                float bias = m_nodesSaveDistances[p] / m_distanceAll;
                wavesets[i].m_nodesSaveStrengths[p] = wavesets[i].m_strengthStart + (strengthGap * bias);
            }
        }
    }

    private void SaveNodesJigglebone()
    {
        for (int i = 0; i < m_nodes.Length; i++)
        {
            if (m_nodes[i] != null)
            {
                SoxAtkJiggleBone jiggleBone = m_nodes[i].GetComponent<SoxAtkJiggleBone>();
                if (jiggleBone != null)
                {
                    if (jiggleBone.gameObject.activeInHierarchy && jiggleBone.enabled)
                    {
                        m_nodes[i] = jiggleBone.SetMixedTentacle(this);
                    }
                }
            }
        }

        /* 리플랙션으로 구현했던...
        // SoxAtkJiggleBone클래스가 설치되지 않은 곳에서도 SoxAtkJiggleBone의 함수를 실행하고 값을 얻어오기 위해 리플랙션으로 구현했던 코드.
        // 지글본과 텐타클의 긴밀한 통신이 필요해지면서 두 클래스가 모두 있다는 전제를 하면서 봉인
        Type jiggleBonetype = Type.GetType("SoxAtkJiggleBone");
        if (jiggleBonetype == null)
            return; // 지글본이 없는 환경에서는 그냥 리턴

        MethodInfo setMixedTentacle = jiggleBonetype.GetMethod("SetMixedTentacle");
        MethodInfo getThisEnabled = jiggleBonetype.GetMethod("GetThisEnabled");

        for (int i = 0; i < m_nodes.Length; i++)
        {
            if (m_nodes[i] != null)
            {
                object jiggleBone = m_nodes[i].GetComponent(jiggleBonetype);
                if (jiggleBone != null)
                {
                    if ((bool)getThisEnabled.Invoke(jiggleBone, null))
                    {
                        Transform tentacle = (Transform)setMixedTentacle.Invoke(jiggleBone, null);
                        if (tentacle != null)
                            m_nodes[i] = tentacle;
                    }
                }
            }
        }
        */
    }

    // 지글본용으로 세팅되었던 노드들을 원래대로 되돌린다.
    private void RevertNodesJigglebone()
    {

        for (int i = 0; i < m_nodes.Length; i++)
        {
            if (m_nodes[i] != null)
            {
                SoxAtkJiggleBone jiggleBone = m_nodesOriginal[i].GetComponent<SoxAtkJiggleBone>();
                if (jiggleBone != null)
                {
                    if (jiggleBone.gameObject.activeInHierarchy && jiggleBone.enabled)
                    {
                        jiggleBone.RemoveMixedTentacle();
                    }
                }
                m_nodes[i] = m_nodesOriginal[i];
            }
        }

        /* 리플랙션 방식 봉인
        // SoxAtkJiggleBone클래스가 설치되지 않은 곳에서도 SoxAtkJiggleBone의 함수를 실행하고 값을 얻어오기 위해 리플랙션의 복잡한 과정을 거친다.
        Type jiggleBonetype = Type.GetType("SoxAtkJiggleBone");
        if (jiggleBonetype == null)
            return;  // 지글본이 없는 환경에서는 그냥 리턴
       
        for (int i = 0; i < m_nodes.Length; i++)
        {
            m_nodes[i] = m_nodesOriginal[i];
        }

        MethodInfo getThisEnabled = jiggleBonetype.GetMethod("GetThisEnabled");
        MethodInfo removeMixedTentacle = jiggleBonetype.GetMethod("RemoveMixedTentacle");

        for (int i = 0; i < m_nodes.Length; i++)
        {
            if (m_nodes[i] != null)
            {
                object jiggleBone = m_nodes[i].GetComponent(jiggleBonetype);
                if (jiggleBone != null)
                {
                    if ((bool)getThisEnabled.Invoke(jiggleBone, null))
                    {
                        removeMixedTentacle.Invoke(jiggleBone, null);
                    }
                }
            }
        }
        */
    }
}
