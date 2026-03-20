using UnityEngine;

public class TapInputHandler : MonoBehaviour
{
    public static TapInputHandler Instance { get; private set; }

    public System.Action<SwipeDirection> OnSwipe;

    [SerializeField] private float swipeThreshold = 20f;

    private bool isActive;
    private bool tracking;
    private bool consumed;
    private Vector2 startPos;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (!active)
        {
            tracking = false;
            consumed = false;
        }
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.touchCount > 0)
            HandleTouch();
        else
            HandleMouse();
    }

    private void HandleTouch()
    {
        var touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startPos = touch.position;
                tracking = true;
                consumed = false;
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (tracking && !consumed)
                {
                    Vector2 delta = touch.position - startPos;
                    if (delta.magnitude >= swipeThreshold)
                    {
                        consumed = true;
                        OnSwipe?.Invoke(ResolveDirection(delta));
                    }
                }
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                tracking = false;
                consumed = false;
                break;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            tracking = true;
            consumed = false;
        }
        else if (Input.GetMouseButton(0) && tracking && !consumed)
        {
            Vector2 delta = (Vector2)Input.mousePosition - startPos;
            if (delta.magnitude >= swipeThreshold)
            {
                consumed = true;
                OnSwipe?.Invoke(ResolveDirection(delta));
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            tracking = false;
            consumed = false;
        }
    }

    private SwipeDirection ResolveDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        else
            return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
    }
}
