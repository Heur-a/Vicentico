using UnityEngine;
using UnityEngine.InputSystem;

public class InputPlayer : MonoBehaviour
{
    public Vector2 move;
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 3.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }
    
    void Move(Vector2 movement)
    {
        transform.Translate(movement.y * moveSpeed * Time.deltaTime * Vector3.forward);
        transform.Rotate(Vector3.up, movement.x * rotateSpeed * Time.deltaTime);
    }
    void Update()
    {
        Move(move);
    }  
}
