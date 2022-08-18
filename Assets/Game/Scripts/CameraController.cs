using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;

    public static CameraController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CameraController>();
            }
            return _instance;
        }
    }

    [SerializeField] private float ySwipeSpeed = 0.2f;
    [SerializeField] private float xSwipeSpeed = 30;
    private Touch activeTouch;
    private Vector3 FirstPoint;
    private Vector3 SecondPoint;

    private float swipeDirX = 0;
    private float swipeDirY = 0;
    private Vector3 xSwipeAmount;

    [SerializeField] private CinemachineFreeLook freelockCam;

    public CinemachineFreeLook FreeLook => freelockCam;

    private void Start()
    {
        EnhancedTouchSupport.Enable();
        xSwipeAmount = Vector3.zero;
    }

    private bool IsPointerOverUIObject(Vector2 pos)
    {
        PointerEventData eventDataCurrentPosition = new(EventSystem.current);
        eventDataCurrentPosition.position = pos;
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void Update()
    {
        if (Touch.activeTouches.Count > 0)
        {
            for (int i = 0; i < Touch.activeTouches.Count; i++)
            {
                if (Touch.activeTouches[i].began)
                {
                    Vector3 firstPosition = Touch.activeTouches[i].screenPosition;
                    if (IsPointerOverUIObject(firstPosition))
                    {
                        Debug.LogError("OverUI");
                        continue;
                    }
                    else
                    {
                        activeTouch = Touch.activeTouches[i];
                        System.ReadOnlySpan<char> log = string.Format("{0} : {1}", activeTouch.touchId, i);
                        Debug.LogError(log.ToString());
                        break;
                    }
                }
            }
            if (IsPointerOverUIObject(FirstPoint)) return;

            if (activeTouch.phase == TouchPhase.Began)
            {
                FirstPoint = activeTouch.screenPosition;
            }
            else if (activeTouch.phase == TouchPhase.Moved)
            {
                swipeDirX = -activeTouch.delta.x;
                swipeDirY = -activeTouch.delta.y;
            }
            else
            {
                swipeDirX = 0;
                swipeDirY = 0;
                FirstPoint = Vector3.zero;
                SecondPoint = Vector3.zero;
            }
        }
        else
        {
            swipeDirX = 0;
            swipeDirY = 0;
            FirstPoint = Vector3.zero;
            SecondPoint = Vector3.zero;
        }

        xSwipeAmount = swipeDirX * xSwipeSpeed * Time.deltaTime * Vector3.right + swipeDirY * ySwipeSpeed * Time.deltaTime * Vector3.up;
        freelockCam.m_XAxis.m_InputAxisValue = xSwipeAmount.x;
        freelockCam.m_YAxis.m_InputAxisValue = xSwipeAmount.y;
    }
}