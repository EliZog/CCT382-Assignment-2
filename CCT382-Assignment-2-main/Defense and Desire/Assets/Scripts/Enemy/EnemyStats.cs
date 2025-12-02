using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyTags {Armoured, Flying, Charming, Vehicle, Bleeding}

    [SerializeField] public float health;
    [SerializeField] public float maxHealth;
    [SerializeField] public bool isDead;
    [SerializeField] public List<EnemyTags> tags;

    [SerializeField] public List<Effect> ActiveEffects;

    public Transform RootPart;
    public int ID;
    public int NodeIndex;
    public int Speed;
    public float DamageResistance = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitVariables();
    }

    public void Tick()
    {
        for (int i = 0; i < ActiveEffects.Count; i++)
        {
            if (ActiveEffects[i].ExpireTime > 0f)
            {
                if (ActiveEffects[i].DamageDelay > 0f)
                {
                    ActiveEffects[i].DamageDelay -= Time.deltaTime;
                }
                else
                {
                    GameLoopManager.EnqueueDamageData(new EnemyDamageData(this, ActiveEffects[i].Damage, 1f));
                    ActiveEffects[i].DamageDelay = 1f / ActiveEffects[i].DamageRate;
                }

                ActiveEffects[i].ExpireTime -= Time.deltaTime;
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

        isDead = false;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;

    }

    public void StripTag(EnemyTags tag)
    {
        tags.Remove(tag);
    }
}
