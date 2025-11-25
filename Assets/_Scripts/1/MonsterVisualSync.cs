using UnityEngine;

// File: MonsterVisualSync.cs
public class MonsterVisualSync : MonoBehaviour
{
    [Header("Animation")]
    public Animator Anim;

    [Header("VFX")]
    public ParticleSystem HitEffect; 

    // Hàm này đã được rút gọn, không còn nhận tham số máu nữa
    public void UpdateVisual(bool isHit, bool isAttacking, bool isDead)
    {
        if (Anim == null) return;

        if (isDead)
        {
            Anim.SetBool("IsDead", true);
        }
        else
        {
            // Reset trạng thái chết (phòng hờ)
            Anim.SetBool("IsDead", false); 

            // 1. Animation Trúng đạn & Hiệu ứng
            if (isHit) 
            {
                Anim.SetTrigger("Hit");
                if (HitEffect != null) HitEffect.Play();
            }
            
            // 2. Animation Tấn công
            if (isAttacking) 
            {
                Anim.SetTrigger("Attack");
            }
        }
    }
}