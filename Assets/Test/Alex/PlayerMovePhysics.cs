using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerMovePhysics : MonoBehaviour
{
    public Vector2 movement;
    public float linearSpeed = 5.0f;
    public float angularSpeed = 2.0f;
    public Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(movement.y * linearSpeed * Vector3.forward, ForceMode.Force );
        rb.AddRelativeTorque(movement.x * angularSpeed * Vector3.up, ForceMode.Force );
    }
}
