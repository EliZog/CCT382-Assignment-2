using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyTags {Armoured, Flying, Charming, Vehicle, Bleeding}

    [SerializeField] public float health;
    [SerializeField] public float maxHealth;
    [SerializeField] public bool isDead;
    [SerializeField] public List<EnemyTags> tags;

    public Transform RootPart;
    public int ID;
    public int NodeIndex;
    public int Speed;
    public float DamageResistance = 1f;
    //public GameObject LevelManager;
    //public LevelManager lm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitVariables();
        //lm = LevelManager.GetComponent<LevelManager>();
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
        isDead = false;
        transform.position = GameLoopManager.NodePositions[0];
        NodeIndex = 0;

    }

    public void StripTag(EnemyTags tag)
    {
        tags.Remove(tag);
    }
}
