using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = new Vector2(0f, -1f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Simpan arah hadap terakhir jika ada input
        if (moveInput.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }

    void Update()
{
    if (anim != null)
    {
        // Kirim arah hadap
        anim.SetFloat("MoveX", lastMoveDirection.x);
        anim.SetFloat("MoveY", lastMoveDirection.y);

        // KUNCI UTAMA: Jika tombol ditekan, Speed = 1. Jika lepas, Speed = 0
        float speedValue = moveInput.sqrMagnitude > 0.001f ? 1f : 0f;
        anim.SetFloat("Speed", speedValue);
    }
}

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }
}