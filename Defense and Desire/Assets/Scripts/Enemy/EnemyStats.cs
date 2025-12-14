using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyTags {Armoured, Flying, Charming, Vehicle, Bleeding}

    public float health;
    public float maxHealth;
    public bool isDead;

    public List<EnemyTags> tags;
    public List<Effect> ActiveEffects;

    public Transform RootPart;
    public int ID;
    public int NodeIndex;
    public float MaxSpeed;
    public float Speed;
    public float SpeedDebuff;
    public float DamageResistance = 1f;
    public bool Stun;
    public bool Slow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitVariables();
    }

    public void Tick()
    {
        Debug.Log("Active Effects: " + ActiveEffects.Count);

        for (int i = 0; i < ActiveEffects.Count; i++)
        {
            if (ActiveEffects[i].ExpireTime > 0f)
            {
                if (ActiveEffects[i].EffectName == "Stun")
                {
                    // Debug.Log("Stun should be applied");
                    Stun = true;
                    Speed = 0;
                }

                if (ActiveEffects[i].EffectName == "Slow")
                {
                    Slow = true;
                    Speed = MaxSpeed / 2;
                    Debug.Log("Current Speed: " + Speed);
                }

                if (ActiveEffects[i].DamageDelay > 0f)
                {
                    ActiveEffects[i].DamageDelay -= Time.deltaTime;
                }
                else
                {
                    GameLoopManager.EnqueueDamageData(new EnemyDamageData(this, ActiveEffects[i].Damage, 1f, ActiveEffects[i].SpeedDebuff));
                    ActiveEffects[i].DamageDelay = 1f / ActiveEffects[i].DamageRate;
                }

                //Speed = Stun ? 0 : MaxSpeed;

                ActiveEffects[i].ExpireTime -= Time.deltaTime;

                
            }

            else if (ActiveEffects[i].EffectName == "Stun")
            {
                Stun = false;
                Speed = MaxSpeed;
            }

            else if (ActiveEffects[i].EffectName == "Slow")
            {
                Slow = false;
                Speed = MaxSpeed;
                Debug.Log("Speed buff should be taken away");
            }

        }

        ActiveEffects.RemoveAll(x => x.ExpireTime <= 0f);
    }

    
    
    public virtual void CheckHealth()
    {
        if (health < 0)
        {
            health = 0;
            Die();
        }
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float HealthLeft()
    {
        return health;
    }

    public virtual void Die()
    {
        isDead = true;
    }

    public void SetHealthTo(float healthToSetTo)
    {
        health = healthToSetTo;
        CheckHealth();
    }

    public void TakeDamage(float damage)
    {
        float healthAfterDamage = health - damage;
        SetHealthTo(healthAfterDamage);
    }

    public void Heal(float heal)
    {
        float healthAfterHeal = health + heal;
        SetHealthTo(healthAfterHeal);
    }

    public virtual void InitVariables()
    {
        SetHealthTo(maxHealth);
        ActiveEffects = new List<Effect>();
        Speed = MaxSpeed;
        Stun = false;

        isDead = false;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;

    }

    public void StripTag(EnemyTags tag)
    {
        tags.Remove(tag);
    }
}
