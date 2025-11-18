using System.Collections.Generic;
using UnityEngine;

public class TowerStats : MonoBehaviour
{
    
    [SerializeField] public int radius;
    [SerializeField] public int damage;
    [SerializeField] public float attackSpeed;

    [SerializeField] public int R_level;
    [SerializeField] public int R_bar;
    [SerializeField] public int R_exp;
    [SerializeField] public List<string> upgrades;


    void Start()
    {
        
    }

    
}
