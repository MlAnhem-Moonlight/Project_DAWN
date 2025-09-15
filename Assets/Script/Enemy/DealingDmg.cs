using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealingDmg : MonoBehaviour
{
    public float knockbackForce = 5f; // Lực đẩy ngược (có thể tùy chỉnh)
    public float damageAmount = 5f;
    public float skillDamageAmount = 10f;
    public bool usingSkill = false;


    public void setDamageAmount(float basic,float skill)
    {
        damageAmount = basic;
        skillDamageAmount = skill;
        gameObject.SetActive(false);
    }

    public void setUsingSkill(bool val)
    {
        usingSkill = val;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Found " + collision.gameObject.name);
        if (collision.gameObject.layer != gameObject.layer && collision.gameObject.GetComponent<Stats>() != null)
        {
            try
            {
                if(usingSkill) // Skill attack
                {
                    Debug.Log("Skill hit " + collision.gameObject.name);
                    collision.gameObject.GetComponent<Stats>().TakeDamage(skillDamageAmount);
                    // Tính lực đẩy ngược (knockback)
                    ApplyKnockback(collision.gameObject);
                }
                else
                {
                    Debug.Log("Attack " + collision.gameObject.name);
                    collision.gameObject.GetComponent<Stats>().TakeDamage(damageAmount);
                }


            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e.Message);
            }
        }
    }

    void ApplyKnockback(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>(); // Lấy Rigidbody của đối tượng bị đẩy
        Debug.Log("Knockback to " + target.name);
        if (targetRb != null)
        {
            Rigidbody2D attackerRb = GetComponent<Rigidbody2D>(); // Lấy Rigidbody của đối tượng gây damage
            float attackerMass = attackerRb != null ? attackerRb.mass : 1f; // Nếu không có Rigidbody, đặt khối lượng mặc định = 1

            // Hướng đẩy theo hướng nhìn của gameobject (phía trước hoặc phải)
            Vector2 knockbackDirection = transform.right.normalized; // Use the "right" direction of the attacker object

            float massFactor = targetRb.mass / attackerMass; // Hệ số khối lượng: dựa trên mass của đối tượng bị đẩy và gây damage

            // Giảm lực đẩy bằng cách thêm một hệ số giảm
            float scalingFactor = 0.5f; // Bạn có thể điều chỉnh giá trị này để giảm lực đẩy
            float finalForce = knockbackForce * massFactor * scalingFactor;

            //targetRb.AddForce(knockbackDirection * finalForce, ForceMode2D.Impulse); // Áp dụng lực đẩy ngược đã giảm
            float knockbackDistance = finalForce * 0.1f; // 0.1f: hệ số chuyển lực sang quãng đường
            targetRb.MovePosition(targetRb.position + knockbackDirection * knockbackDistance);
        }
    }


}
