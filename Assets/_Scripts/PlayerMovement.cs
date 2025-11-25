using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 6f;
    public Transform cameraTransform;

    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        Move();
        RotateToMoveDirection();
        UpdateRunAnimation();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();

        Vector3 moveDir = forward * v + right * h;
        moveDir.Normalize();

        rb.linearVelocity = new Vector3(
            moveDir.x * moveSpeed,
            rb.linearVelocity.y,
            moveDir.z * moveSpeed
        );
    }

    void RotateToMoveDirection()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = 0;

        if (velocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }

    void UpdateRunAnimation()
    {
        Vector3 flatVel = rb.linearVelocity;
        flatVel.y = 0;

        float speed = flatVel.magnitude;

        animator.SetFloat("Speed", speed); // <= animation chạy
    }
}
