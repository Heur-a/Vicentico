using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleRotate : MonoBehaviour
{
    public float rotSpeed = 0.4f;
    public float scaleSpeed = 1.0f;
    private float _initialDistance;
    private Vector3 _initialScale;
    private Camera _mainCamera;
    private bool _isDragging = false;
    private Vector2 _lastMousePosition;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        // Para probar en el editor pero lo importante es la de touch
        if (Application.isEditor)
        {
            HandleEditorInput();
        }
        else
        {
            HandleTouchInput();
        }
    }

    private void HandleEditorInput()
    {
        // Rotación con click izquierdo
        if (Input.GetMouseButtonDown(0) && IsMouseOverObject())
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = currentMousePosition - _lastMousePosition;

            transform.Rotate(delta.y * rotSpeed, -delta.x * rotSpeed, 0, Space.World);

            _lastMousePosition = currentMousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
        }

        // Escala con rueda del ratón
        if (IsMouseOverObject())
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                transform.localScale +=  scroll * scaleSpeed * Vector3.one;
            }
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount < 1)
        {
            _isDragging = false;
        }
        
        if (Input.touchCount == 1 && _isDragging)
        {
            MoveObjectTouch();
            return;
        }
        
        
        if (Input.touchCount == 1 && objIsTouched())
        {
            MoveObjectTouch();
            return;
        }

        if (Input.touchCount == 2 && objIsTouched())
        {
            ScaleObjectTouch();
            return;
        }
    }

    private void ScaleObjectTouch()
    {
        var touchZero = Input.GetTouch(0);
        var touchOne = Input.GetTouch(1);

        if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled ||
            touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
        {
            return;
        }

        if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
        {
            _initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
            _initialScale = transform.localScale;
        }
        else
        {
            var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
            if (Mathf.Approximately(_initialDistance, 0)) return;
            var factor = currentDistance / _initialDistance;
            transform.localScale = _initialScale * factor;
        }
    }

    private void MoveObjectTouch()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Quaternion initRotation = transform.rotation;
            transform.Rotate(Input.GetTouch(0).deltaPosition.y * rotSpeed,
                -Input.GetTouch(0).deltaPosition.x * rotSpeed,
                0, Space.World);
            _isDragging = true;
        }
    }

    private bool objIsTouched()
    {
        foreach (Touch t in Input.touches)
        {
            Ray m_ray = _mainCamera.ScreenPointToRay(t.position);
            RaycastHit m_hit;
            if (Physics.Raycast(m_ray, out m_hit))
            {
                if (m_hit.transform == this.transform) // Mejor comparar la referencia directa
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsMouseOverObject()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return hit.transform == this.transform;
        }

        return false;
    }
}