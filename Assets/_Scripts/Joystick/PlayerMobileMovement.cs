using UnityEngine;

public class PlayerMobileMovement : MonoBehaviour
{
    [Header("Cài đặt")]
    public float MoveSpeed = 8f;
    public MobileJoystick Joystick; // Kéo Joystick vào đây
    public Rigidbody Rb;
    
    // --- [MỚI] THÊM ANIMATOR ---
    public Animator PlayerAnim; 
    // ---------------------------

    void FixedUpdate()
    {
        if (Joystick == null) return;

        // 1. Lấy hướng
        Vector3 moveDir = new Vector3(Joystick.InputDirection.x, 0, Joystick.InputDirection.y);

        // --- [MỚI] CẬP NHẬT ANIMATION ---
        if (PlayerAnim != null)
        {
            // moveDir.magnitude trả về giá trị từ 0 (không kéo) đến 1 (kéo hết cỡ)
            // Gửi giá trị này vào Animator để nó tự chuyển đổi Idle/Run
            PlayerAnim.SetFloat("Speed", moveDir.magnitude);
        }
        // --------------------------------

        // 2. Di chuyển vật lý (Giữ nguyên logic cũ)
        if (moveDir.magnitude > 0.1f)
        {
            Vector3 targetPosition = Rb.position + moveDir * MoveSpeed * Time.fixedDeltaTime;
            Rb.MovePosition(targetPosition);

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            Rb.rotation = Quaternion.Slerp(Rb.rotation, targetRotation, 20f * Time.fixedDeltaTime);
        }
    }
}