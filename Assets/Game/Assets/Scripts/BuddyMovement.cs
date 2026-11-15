
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
public class BuddyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Animation Settings")]
    public Animator animator;

    private Rigidbody2D rb;
    private float horizontal;
    private bool jumpRequested;
    private string currentAnimation = "";

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        horizontal = 0f;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed ||
                keyboard.leftArrowKey.isPressed)
                horizontal -= 1f;

            if (keyboard.dKey.isPressed ||
                keyboard.rightArrowKey.isPressed)
                horizontal += 1f;

            // Jump with Space
            if (keyboard.spaceKey.wasPressedThisFrame)
                jumpRequested = true;
        }

#elif ENABLE_LEGACY_INPUT_MANAGER
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpRequested = true;
#endif

        // Animations
        if (horizontal > 0.1f)
        {
            PlayAnimation("Buddy_Right");
        }
        else if (horizontal < -0.1f)
        {
            PlayAnimation("Buddy_Left");
        }
        else
        {
            PlayAnimation("Idle_Buddy");
        }
    }

    void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        Vector2 velocity = rb.linearVelocity;
#else
        Vector2 velocity = rb.velocity;
#endif

        // Horizontal movement
        velocity.x = horizontal * moveSpeed;

        // Jump only when touching the ground
        if (jumpRequested && IsGrounded())
        {
            velocity.y = jumpForce;
        }

        jumpRequested = false;

#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    bool IsGrounded()
    {
        int count = rb.GetContacts(contacts);

        for (int i = 0; i < count; i++)
        {
            // Only surfaces below Buddy count as ground
            if (contacts[i].normal.y > 0.5f)
                return true;
        }

        return false;
    }

    void PlayAnimation(string animationName)
    {
        if (animator == null)
            return;

        if (currentAnimation == animationName)
            return;

        currentAnimation = animationName;
        animator.Play(animationName, 0, 0f);
    }
}
