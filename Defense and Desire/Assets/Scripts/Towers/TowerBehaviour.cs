using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    public int ExpNeeded;
    public int Exp;
    public int level;

    public bool Upgrade1;
    public bool Upgrade2;
    public bool Upgrade3;

    public Button FlirtButton;
    public Button UpgradeButton1;
    public Button UpgradeButton2;
    public Button UpgradeButton3;

    public GameObject Heart1;
    public GameObject Heart2;
    public GameObject Heart3;

    public GameObject lock1;
    public GameObject lock2;
    public GameObject lock3;

    private float Delay; // time before apply damage

    private IDamageMethod CurrentDamageMethodClass;

    /* 
    *   *** THIS IS FOR TESTING; REMOVE WHEN NEEDED *** 
    */
    // bool first1 = true;
    // bool first2 = true;
    // bool first3 = true;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentDamageMethodClass = GetComponent<IDamageMethod>();
        Tags = new List<TowerTags.Tags>();

        Menu = false;
        UpgradeMenu.enabled = false;
        UpgradeMenu.gameObject.SetActive(false);

        ExpNeeded = 2;
        Exp = 0;
        level = 0;

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
        // if (Upgrade1 && first1)
        // {
        //     first1 = false;
        //     FlamethrowerUpgrade1();
        // }
        // if (Upgrade2 && first2)
        // {
        //     first2 = false;
        //     FlamethrowerUpgrade2();
        // }
        // if (Upgrade3 && first3)
        // {
        //     first3 = false;
        //     FlamethrowerUpgrade3();
        // }

        if (level >= 1)
        {
            lock1.SetActive(false);
        }

        if (Upgrade1)
        {
            UpgradeButton1.interactable = false;

            if (level >= 2)
                lock2.SetActive(false);
        
        }

        if (Upgrade2)
        {
            UpgradeButton2.interactable = false;

            if (level >= 3)
                lock3.SetActive(false);
        }
        
        if (Upgrade3)
        {
            UpgradeButton3.interactable = false;
        }

    }

    public void ToggleMenu()
    {
        Menu = !Menu;
        UpgradeMenu.enabled = Menu;
        UpgradeMenu.gameObject.SetActive(Menu);
    }

    public void IncreaseRelationship()
    {
        Exp += 1;

        if (Exp >= ExpNeeded)
        {
            Exp = 0;
            ExpNeeded += 1;
            level += 1;
        }

        if (level == 1)
        {
            Image Heart = Heart1.GetComponent<Image>();
            Heart.color = Color.red;
        }

        if (level == 2)
        {
            Image Heart = Heart2.GetComponent<Image>();
            Heart.color = Color.red;
        }

        if (level == 3)
        {
            Image Heart = Heart3.GetComponent<Image>();
            Heart.color = Color.red;
            FlirtButton.interactable = false;
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

        temp.extraFireTriggers.SetActive(true);
        temp.FireEffect.startSpeed = 10;
        Debug.Log("Flamethrower Upgraded to level 3!");
    }

    public void LaserUpgrade1()
    {
        Upgrade1 = true;
        Tags.Add(TowerTags.Tags.ColdHearted);
        Debug.Log("Laser Upgraded to level 1!");
    }

    public void LaserUpgrade2()
    {
        Upgrade2 = true;
        Firerate = 10;
        Debug.Log("Laser Upgraded to level 2!");
    }

    public void LaserUpgrade3()
    {
        Upgrade3 = true;
        LaserDamage temp = GetComponent<LaserDamage>();

        Debug.Log("Laser Upgraded to level 3!");
    }
}
