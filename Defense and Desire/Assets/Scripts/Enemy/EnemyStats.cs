using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyTags {Armoured, Flying, Charming, Vehicle, Bleeding}

    [SerializeField] public int health;
    [SerializeField] public int maxHealth;
    [SerializeField] public bool isDead;
    [SerializeField] public List<EnemyTags> tags;

    public int ID;
    public int NodeIndex;
    public int Speed;
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

    public int HealthLeft()
    {
        return health;
    }

    public virtual void Die()
    {
        isDead = true;
    }

    public void SetHealthTo(int healthToSetTo)
    {
        health = healthToSetTo;
        CheckHealth();
    }

    public void TakeDamage(int damage)
    {
        int healthAfterDamage = health - damage;
        SetHealthTo(healthAfterDamage);
    }

    public void Heal(int heal)
    {
        int healthAfterHeal = health + heal;
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
