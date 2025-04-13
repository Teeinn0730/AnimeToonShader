using UnityEngine;
using System.Collections.Generic;

// 3ds Max의 LookAt constraint 와 같은 기능을 하도록 만들려고 했으나 m_lookAtAxis, m_sourceAxis 기능은 미 구현

[ExecuteInEditMode]
public class SoxAtkLookAt : MonoBehaviour
{
    [HideInInspector]
    public float m_version = 1.102f;

    public bool m_editorLookAt = false;  // 에디터에서도 작동할지를 선택적으로 할 수 있도록 한다.
	public bool m_lookAtOnce = false; // enabled 될 때에 한 번만 LookAt 하도록
	public bool m_dynamicSearchMainCamera = false; // 메인 카메라를 지속적으로 찾을건지 (매 Update마다 찾지는 않음)
	// Camera.main 은 런타임시 느리기때문에 프라이빗 변수를 사용함.
	private Camera m_cameraMain;

	public enum LookType
	{
		Camera, Nodes
	}
	public enum UpType
	{
		Camera, Node, World
	}
	public enum AxisType
	{
		X, Y, Z
	}
	public enum UpCtrType
	{
		LootAt, AxisAlignment
	}

	public LookType m_lookAtType = LookType.Camera;
	public List<Transform> m_lookAtNodeList = new List<Transform>();
	//public AxisType m_lookAtAxis = AxisType.Z;
	public bool m_lookAtFilp = false;

	public UpType m_upAxisType = UpType.World;
	public Transform m_upNode;
	public UpCtrType m_upControl = UpCtrType.AxisAlignment;

	//public AxisType m_sourceAxis = AxisType.Y;
	public bool m_sourceAxisFilp = false;
	public AxisType m_alignedToUpnodeAxis = AxisType.Y;

	// 프리팹 재활용 등에 의해서 Enable 이 수시로 바뀌는 상황에서 Enable 될 때마다 초기화를 해주기 위한 변수.
	// OnEnable 에서 초기화를 하면 게임 로직이 자리를 잡기 전에 초기화되어서 문제될 수 있으니 Update 함수에서 한 번만 초기화 해주기 위해 이 변수를 사용한다.
	private bool m_initialize = false;

	private void OnEnable()
	{
		m_initialize = false;

		// 메인 카메라 변수가 null 이거나 다이나믹메인카메라가 켜져있으면 Camera.main 을 세팅한다
		if (m_cameraMain == null || m_dynamicSearchMainCamera)
		{
			RefreshCameraMain();
		}
	}

	private void Start()
	{
		RefreshCameraMain();
	}

	void Update()
	{
		if (m_initialize == false)
		{
			UpdateLookRotation();
			m_initialize = true;
			return;
		}

		if (m_lookAtOnce == false)
		{
			UpdateLookRotation();
		}
	}

	public void RefreshCameraMain()
	{
		m_cameraMain = Camera.main;
	}

	private void UpdateLookRotation()
	{
		// 에디터 활성이 꺼져있고 에디터상태면 그냥 리턴. 
		if (!m_editorLookAt && !Application.isPlaying)
			return;
		
		//SolveOverlapAxis();
		transform.rotation = Quaternion.LookRotation(GetForwardVec(), GetUpwardVec());
	}

	private Vector3 GetForwardVec()
	{
		Vector3 lookPos = GetLookPos();
		Vector3 lookDir = lookPos - transform.position;
		if (m_lookAtFilp)
			lookDir *= -1f; 

		return lookDir;
	}

	private Vector3 GetUpwardVec()
	{
		Vector3 posFrom = new Vector3(0, 0, 0);
		Vector3 posTo = new Vector3(0, 1, 0);

		Vector3 alignedToAxis = new Vector3(0, 1, 0);
		switch (m_alignedToUpnodeAxis)
		{
			case AxisType.X:
				alignedToAxis = new Vector3(1, 0, 0);
				break;
			case AxisType.Y:
				alignedToAxis = new Vector3(0, 1, 0);
				break;
			case AxisType.Z:
				alignedToAxis = new Vector3(0, 0, 1);
				break;
		}
		if (m_sourceAxisFilp)
		{
			alignedToAxis *= -1;
		}

		switch (m_upControl)
		{
			case UpCtrType.AxisAlignment:
				switch (m_upAxisType)
				{
					case UpType.World:
						posFrom = new Vector3(0, 0, 0);
						posTo = alignedToAxis;
						break;
					case UpType.Node:
						if (m_upNode != null)
						{
							posFrom = m_upNode.transform.position;
							posTo = m_upNode.TransformPoint(alignedToAxis);
						}
						break;
					case UpType.Camera:
						if (Camera.main != null)
						{
							posFrom = m_cameraMain.transform.position;
							posTo = m_cameraMain.transform.TransformPoint(alignedToAxis);
						}
						break;
				}
				break;
			case UpCtrType.LootAt:
				posFrom = transform.position;
				switch (m_upAxisType)
				{
					case UpType.World:
						posTo = new Vector3(0, 0, 0);
						break;
					case UpType.Node:
						if (m_upNode != null)
						{
							posTo = m_upNode.transform.position;
						}
						break;
					case UpType.Camera:
						if (Camera.main != null)
						{
							posTo = m_cameraMain.transform.position;
						}
						break;
				}
				break;
		}

		return (posTo - posFrom);
	}

	// 바라볼 위치 리턴
	private Vector3 GetLookPos()
	{
		Vector3 lookPos = Vector3.zero;
		switch (m_lookAtType)
		{
			case LookType.Camera:
				if (Camera.main)
				{
					lookPos = m_cameraMain.transform.position;
				}
				else
				{
					lookPos = transform.forward;
				}
				break;
			case LookType.Nodes:
				//노드들에 오브젝트가 등록되지 않을 경우도 있으므로 일단 안전 값을 먼저 넣어준다.
				lookPos = transform.forward;
				int tempCount = 0;
				Vector3 tempPos = Vector3.zero;
				foreach (Transform node in m_lookAtNodeList)
				{
					if (node != null)
					{
						tempCount++;
						tempPos += node.position;
					}
				}
				lookPos = tempPos / (float)tempCount;
				break;
		}
		return lookPos;
	}

	/*
	// Look At Axis 와 Source Axis 가 겹치는 상황을 해결한다. (Look At Axis 우선)
	private void SolveOverlapAxis()
	{
		if (m_lookAtAxis == m_sourceAxis)
		{
			switch (m_lookAtAxis)
			{
				case AxisType.X:
					m_sourceAxis = AxisType.Y;
					break;
				case AxisType.Y:
					m_sourceAxis = AxisType.X;
					break;
				case AxisType.Z:
					m_sourceAxis = AxisType.Y;
					break;
			}
		}
	}
	*/
}
