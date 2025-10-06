using PurrNet;
using PurrNet.Transports;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float rotationSmoothness = 10f;  // Higher = snappier
    [SerializeField] private float movementSmoothness = 10f;
    [SerializeField] private float bounceForce = 10f;

    private bool _fire;
    private Vector2 _moveInput;

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        if (asServer)
            return;

        // Only the server simulates physics
        rb.bodyType = isServer ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;

        // Only the owner handles input and Update
        enabled = isOwner;

        if (isOwner)
            networkManager.onTick += OnTick;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        networkManager.onTick -= OnTick;
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _fire = true;

        // Store current frame's input for next tick
        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    private void OnTick(bool asServer)
    {
        if (asServer) return;

        var input = new InputData()
        {
            movement = _moveInput,
            fire = _fire
        };

        _fire = false;
        Move(input);
    }

    [ServerRpc(Channel.Unreliable)]
    private void Move(InputData input)
    {
        // Smooth velocity update
        Vector2 targetVelocity = input.movement * movementSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * movementSmoothness);

        // Smooth rotation toward movement direction
        if (input.movement != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(-input.movement.x, input.movement.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * rotationSmoothness);
            rb.rotation = smoothAngle;
        }

        // You could handle firing input here too
        if (input.fire)
        {
            Debug.Log($"[Server] Player {parent.id} fired!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer) return;

        if (!collision.gameObject.TryGetComponent(out PlayerMovement otherPlayer))
            return;

        var direction = (transform.position - otherPlayer.transform.position).normalized;
        rb.AddForce(direction * bounceForce, ForceMode2D.Impulse);
    }

    private struct InputData
    {
        public Vector2 movement;
        public bool fire;
    }
}
