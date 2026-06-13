using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    private SpriteRenderer _cursorSprite;
    private Camera _mainCamera;

    void Awake()
    {
        _cursorSprite = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main;
    }

    void Start()
    {
        Cursor.visible = false;
        if (Application.isPlaying)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    void Update()
    {
        RotateCursor();
    }

    public void RotateCursor()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) return;
        }

        Vector3 currentMousePosition = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        currentMousePosition.z = 0f;
        _cursorSprite.transform.position = currentMousePosition;
    }
}
