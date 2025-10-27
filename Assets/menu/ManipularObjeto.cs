using UnityEngine;

public class ManipularObjeto : MonoBehaviour
{
    [Header("Configuración")]
    public float rotSpeed = 0.4f;
    private bool manipulacionActiva = false; // Se activa desde MenuManager
    
    [Header("Referencias")]
    public GameObject objetoAManipular; // El objeto FBX que se va a manipular
    
    private float initialDistance;
    private Vector3 initialScale;
    
    void Update()
    {
        // Solo manipular si está activado y hay objeto asignado
        if (!manipulacionActiva || objetoAManipular == null) return;
        
        if (objIsTouched())
        {
            // ROTAR con 1 dedo
            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    objetoAManipular.transform.Rotate(
                        Input.GetTouch(0).deltaPosition.y * rotSpeed,
                        -Input.GetTouch(0).deltaPosition.x * rotSpeed,
                        0,
                        Space.World
                    );
                }
            }
            
            // ESCALAR con 2 dedos
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);
                
                if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled
                    || touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
                {
                    return;
                }
                
                if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
                {
                    initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                    initialScale = objetoAManipular.transform.localScale;
                }
                else
                {
                    var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);
                    if (Mathf.Approximately(initialDistance, 0)) return;
                    
                    var factor = currentDistance / initialDistance;
                    objetoAManipular.transform.localScale = initialScale * factor;
                }
            }
        }
    }
    
    private bool objIsTouched()
    {
        if (objetoAManipular == null) return false;
        
        foreach (Touch t in Input.touches)
        {
            Ray m_ray = Camera.main.ScreenPointToRay(t.position);
            RaycastHit m_hit;
            
            if (Physics.Raycast(m_ray, out m_hit, 100))
            {
                // Verifica si el toque es en el objeto a manipular o sus hijos
                if (m_hit.transform == objetoAManipular.transform || 
                    m_hit.transform.IsChildOf(objetoAManipular.transform))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Activa el modo manipulación. Llamado desde MenuManager.
    /// </summary>
    public void ActivarManipulacion()
    {
        manipulacionActiva = true;
        Debug.Log("Manipulación del objeto ACTIVADA");
    }
    
    /// <summary>
    /// Desactiva el modo manipulación. Llamado desde MenuManager.
    /// </summary>
    public void DesactivarManipulacion()
    {
        manipulacionActiva = false;
        Debug.Log("Manipulación del objeto DESACTIVADA");
    }
}

