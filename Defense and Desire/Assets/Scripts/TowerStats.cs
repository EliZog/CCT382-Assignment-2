using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    [SerializeField] public int cost;
    [SerializeField] public int current_value;

    [SerializeField] public int radius;
    [SerializeField] public int damage;
    [SerializeField] public float attackSpeed;
    [SerializeField] public List<string> tags;

    [SerializeField] public int R_level;
    [SerializeField] public int R_bar;
    [SerializeField] public int R_exp;

    void UpgradeRange(int value)
    {
        radius += value;
    }

    void UpgradeDamage(int value)
    {
        damage += value;
    }

    void UpgradeSpeed(int value)
    {
        attackSpeed -= value;
    }

    void AddTag(string tag)
    {
        tags.Add(tag);
    }

    void IncreaseValue(int value)
    {
        current_value += value;
    }

    void StrengthenRelationship()
    {
        R_exp += 1;

        if (R_exp >= R_bar)
        {
            R_exp = 0;
            R_bar += 1;
            R_level += 1;
        }
    }

    void Start()
    {
        R_level = 0;
        R_bar = 2;
        R_exp = 0;
    }
}
