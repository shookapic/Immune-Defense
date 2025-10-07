using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float moveSmooth = 10f;
    [SerializeField] private float rotSmooth = 10f;
    [SerializeField] private float bounceForce = 10f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool fire;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = IsServer ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;

        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        enabled = IsOwner;
    }

    void Update()
    {
        if (!IsOwner) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // Flip sprite
        if (input.x != 0)
            spriteRenderer.flipX = input.x < 0;

        if (Input.GetKeyDown(KeyCode.Space))
            fire = true;
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 targetVelocity = input * moveSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, moveSmooth * Time.fixedDeltaTime);

        if (input.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(-input.x, input.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotSmooth * Time.fixedDeltaTime);
            rb.MoveRotation(smoothAngle);
        }

        if (fire)
        {
            fire = false;
            FireServerRpc();
        }
    }

    [ServerRpc]
    void FireServerRpc()
    {
        Debug.Log($"[Server] Player {OwnerClientId} used Fire!");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (!collision.gameObject.TryGetComponent(out PlayerMovement other)) return;

        Vector2 direction = (transform.position - other.transform.position).normalized;
        rb.AddForce(direction * bounceForce, ForceMode2D.Impulse);
    }
}
