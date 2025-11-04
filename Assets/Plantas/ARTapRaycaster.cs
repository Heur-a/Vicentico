using UnityEngine;

public class ARTapRaycaster : MonoBehaviour
{
    public Camera cam;
    public LayerMask clickableLayers = ~0; // opcional: limita a la capa de las semillas
    public float maxDistance = 100f;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
            Handle(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            Handle(Input.GetTouch(0).position);
#endif
    }

    void Handle(Vector2 screenPos)
    {
        if (!cam) return;
        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, clickableLayers))
        {
            Debug.Log("[Raycaster] Hit: " + hit.collider.name);
            var clickable = hit.collider.GetComponentInParent<SeedClick>();
            if (clickable != null)
                clickable.OnClicked();
        }
    }
}
