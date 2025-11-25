using UnityEngine;
using TMPro; // <--- 1. CẦN THÊM CÁI NÀY

public class MonsterVisualSync : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI HealthText; 

    [Header("Animation")]
    public Animator Anim;

    [Header("VFX")]
    public ParticleSystem HitEffect; 

    private Vector3 _lastPosition;
    private bool _deadPlayed = false;

    void Start()
    {
        _lastPosition = transform.position;
    }

    // 3. THÊM THAM SỐ 'float currentHealth' VÀO ĐẦU
    public void UpdateVisual(float currentHealth, bool isHit, bool isAttacking, bool isDead)
    {
        // --- XỬ LÝ HIỂN THỊ MÁU (MỚI) ---
        if (HealthText != null)
        {
            HealthText.text = Mathf.CeilToInt(currentHealth).ToString();

            if (Camera.main != null)
            {
                HealthText.transform.rotation = Camera.main.transform.rotation;
            }

            HealthText.gameObject.SetActive(!isDead);
        }
        // --------------------------------

        if (Anim == null) return;

        // ---- DEAD (Code chuẩn của bạn) ----
        if (isDead)
        {
            if (!_deadPlayed) // chỉ chạy 1 lần duy nhất
            {
                Anim.SetTrigger("Dead"); 
                _deadPlayed = true;
            }
            return; 
        }


        if (_deadPlayed && !isDead)
            _deadPlayed = false;

        if (isHit)
        {
            Anim.SetTrigger("Hit");
            if (HitEffect != null) HitEffect.Play();
        }
        if (isAttacking)
        {
            Anim.SetTrigger("Attack");
        }
        
        // (Tùy chọn) Tính Speed để chạy Animation Run nếu cần
        // float speed = Vector3.Distance(transform.position, _lastPosition) / Time.deltaTime;
        // Anim.SetFloat("Speed", speed);
        // _lastPosition = transform.position;
    }
}