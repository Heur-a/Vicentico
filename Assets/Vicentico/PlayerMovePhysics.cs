using System.IO;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public class PlayerMovePhysics : MonoBehaviour
{
    public Vector2 movement;
    public float linearSpeed = 5.0f;
    public float angularSpeed = 2.0f;
    public Rigidbody rb;
    public Transform player;
    
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
        if (movement == Vector2.zero) return;
        float degs = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg;
        player.eulerAngles = new Vector3(player.eulerAngles.x, degs, player.eulerAngles.z);
    }

    void FixedUpdate()
    {
        rb.AddRelativeForce(linearSpeed * new Vector3(movement.x, 0, movement.y), ForceMode.Force);
        // rb.AddRelativeTorque(movement.x * angularSpeed * Vector3.up);
    }
}
