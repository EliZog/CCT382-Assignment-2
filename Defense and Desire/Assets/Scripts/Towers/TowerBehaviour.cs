using System.Collections.Generic;
using Unity.VisualScripting;
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
    public Canvas UpgradeMenu;
    public bool Menu;

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

        Menu = false;
        UpgradeMenu.enabled = false;
        UpgradeMenu.gameObject.SetActive(false);

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

    public void Update()
    {
        /* --- THIS IS FOR TESTING PURPOSES --- */
        if (Upgrade1 && first1)
        {
            first1 = false;
            MissileUpgrade1();
        }
        if (Upgrade2 && first2)
        {
            first2 = false;
            MissileUpgrade2();
        }
        if (Upgrade3 && first3)
        {
            first3 = false;
            MissileUpgrade3();
        }
    }

    public void ToggleMenu()
    {
        Menu = !Menu;
        UpgradeMenu.enabled = Menu;
        UpgradeMenu.gameObject.SetActive(Menu);
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

    public void MissileUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ArmourPiercing);
        Debug.Log("Missile Upgraded to level 1!");
    }

    public void MissileUpgrade2()
    {
        Upgrade2 = true;
        Damage = 20;
        MissileCollisionManager temp = GetComponentInChildren<MissileCollisionManager>();
        temp.ExplosionRadius = 3;
        Debug.Log("Missile Upgraded to level 2!");
    }

    public void MissileUpgrade3()
    {
        Upgrade3 = true;
        MissileCollisionManager temp = GetComponentInChildren<MissileCollisionManager>();
        temp.ShellShock = true;
        //Debug.Log(temp.ShellShock);
        Debug.Log("Missile Upgraded to level 3!");
    }

    public void FlamethrowerUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ArmourPiercing);
        Debug.Log("Flamethrower Upgraded to level 1!");
    }

    public void FlamethrowerUpgrade2()
    {
        Upgrade2 = true;
        Firerate = 10;
        Debug.Log("Flamethrower Upgraded to level 2!");
    }

    public void FlamethrowerUpgrade3()
    {
        Upgrade3 = true;
        FlamethrowerDamage temp = GetComponent<FlamethrowerDamage>();
        temp.Cerbereus = true;
        temp.FireTrigger.enabled = false;
        temp.FireTrigger2.enabled = true;
        temp.FireTrigger3.enabled = true;
        temp.FireTrigger4.enabled = true;
        temp.FireEffect.startSpeed = 10;
        Debug.Log("Flamethrower Upgraded to level 3!");
    }
}
