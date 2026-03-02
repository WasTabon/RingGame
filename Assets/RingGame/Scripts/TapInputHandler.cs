using UnityEngine;
using UnityEngine.EventSystems;

public class TapInputHandler : MonoBehaviour
{
    public static TapInputHandler Instance { get; private set; }

    public System.Action OnTap;

    private bool isActive;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void Update()
    {
        if (!isActive) return;

        bool tapped = false;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (!IsPointerOverUI(Input.GetTouch(0).fingerId))
                tapped = true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI(-1))
                tapped = true;
        }

        if (tapped)
            OnTap?.Invoke();
    }

    private bool IsPointerOverUI(int fingerId)
    {
        return false;
    }
}
