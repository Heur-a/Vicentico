using UnityEngine;

public class EfectoMoverTextura : MonoBehaviour
{
    public Renderer targetRenderer;
    public Transform player;
    public float scrollSpeed = 0.1f;

    private Vector3 lastPlayerPos;
    private Vector2 textureOffset;

    void Start()
    {
        lastPlayerPos = player.position;
    }

    void Update()
    {
        Vector3 delta = player.position - lastPlayerPos;
        
        // Desplaça la textura en direcció contrària al moviment
        textureOffset.x += delta.x * scrollSpeed ;
        textureOffset.y += delta.z * scrollSpeed ;

        targetRenderer.material.mainTextureOffset = textureOffset;

        lastPlayerPos = player.position;
    }
}
