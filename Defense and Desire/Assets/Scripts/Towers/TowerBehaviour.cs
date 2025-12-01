using System.Collections.Generic;
using UnityEngine;

public class TowerTags
{
    public enum Tags {
        ArmourPiercing, 
        AntiAir, 
        ColdHearted
    }
}

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask EnemiesLayer;
    
    public EnemyStats Target;
    public TowerTargeting.TargetType TargetType;
    public Transform TowerPivot;

    public int SummonCost = 100;
    public float Damage;
    public float Firerate;
    public float Range;

    public List<TowerTags.Tags> Tags;

    public bool Upgrade1;
    public bool Upgrade2;
    public bool Upgrade3;

    private float Delay; // time before apply damage

    private IDamageMethod CurrentDamageMethodClass;

    /* 
    *   *** THIS IS FOR TESTING; REMOVE WHEN NEEDED *** 
    */
    bool first1 = true;
    bool first2 = true;
    bool first3 = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentDamageMethodClass = GetComponent<IDamageMethod>();
        Tags = new List<TowerTags.Tags>();

        Upgrade1 = false;
        Upgrade2 = false;
        Upgrade3 = false;

        if (CurrentDamageMethodClass == null)
        {
            Debug.LogError("TOWERS: No damage class attached to given tower!");
        }
        else 
        {
            CurrentDamageMethodClass.Init(Damage, Firerate);
        }
        Delay = 1 / Firerate;
    }

    // Update is called once per frame
    public void Tick()
    {

        CurrentDamageMethodClass.DamageTick(Target);

        if (Target != null)
        {
            TowerPivot.transform.rotation = Quaternion.LookRotation(Target.transform.position - transform.position);
        }
    }

    /* 
    *   *** THIS IS FOR TESTING; REMOVE WHEN NEEDED *** 
    */
    public void Update()
    {
        if (Upgrade1 && first1)
        {
            first1 = false;
            GunnerUpgrade1();
        }
        if (Upgrade2 && first2) 
        {
            first2 = false;
            GunnerUpgrade2();
        }
        if (Upgrade3 && first3) 
        {
            first3 = false;
            GunnerUpgrade3();
        }
    }

    public void GunnerUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.AntiAir);
        Debug.Log("Gunner Upgraded to level 1!");
    }

    public void GunnerUpgrade2()
    {
        Upgrade2 = true;
        Range = 8;
        Firerate = 2;
        Debug.Log("Gunner Upgraded to level 2!");
    }

    public void GunnerUpgrade3()
    {
        Upgrade3 = true;
        Debug.Log("Gunner Upgraded to level 3!");
    }
}
