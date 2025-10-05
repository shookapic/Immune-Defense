using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class PlayerMovement : NetworkIdentity
{
    private Rigidbody2D rb;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float rotationSmoothness = 10f; // Higher = snappier
    [SerializeField] private float movementSmoothness = 10f;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        enabled = isOwner;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float verticalDirection = Input.GetAxisRaw("Vertical");
        float horizontalDirection = Input.GetAxisRaw("Horizontal");

        Vector2 moveDirection = new Vector2(horizontalDirection, verticalDirection).normalized;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, moveDirection * movementSpeed, Time.fixedDeltaTime * movementSmoothness);


        // Rotate to face movement direction
        if (moveDirection != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(-moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * rotationSmoothness);
            rb.rotation = smoothAngle;
        }
    }
}
