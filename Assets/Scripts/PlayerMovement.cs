using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkTransform))]
public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float movementSmoothness = 10f;
    [SerializeField] private float bounceForce = 10f;

    private Vector2 _moveInput;
    private bool _fire;

    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to flip visuals

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = IsServer ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;

        enabled = IsOwner;

        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        _moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
            _fire = true;

        // Flip visual locally for instant feedback
        if (_moveInput.x != 0)
            spriteRenderer.flipX = _moveInput.x < 0;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        var input = new InputData
        {
            movement = _moveInput,
            fire = _fire
        };

        _fire = false;
        MoveServerRpc(input);
    }

    [ServerRpc(RequireOwnership = true)]
    private void MoveServerRpc(InputData input)
    {
        if (!IsServer) return;

        Vector2 targetVelocity = input.movement * movementSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * movementSmoothness);

        if (input.movement != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(-input.movement.x, input.movement.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(rb.rotation, targetAngle, Time.fixedDeltaTime * rotationSmoothness);
            rb.rotation = smoothAngle;
        }

        if (input.fire)
        {
            Debug.Log($"[Server] Player {OwnerClientId} fired!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (!collision.gameObject.TryGetComponent(out PlayerMovement otherPlayer))
            return;

        Vector2 direction = (transform.position - otherPlayer.transform.position).normalized;
        rb.AddForce(direction * bounceForce, ForceMode2D.Impulse);
    }

    [System.Serializable]
    public struct InputData : INetworkSerializable
    {
        public Vector2 movement;
        public bool fire;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref movement);
            serializer.SerializeValue(ref fire);
        }
    }
}
