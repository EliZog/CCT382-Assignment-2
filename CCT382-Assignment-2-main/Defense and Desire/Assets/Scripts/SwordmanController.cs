using UnityEngine;

public class SwordsmanController : TowerStats
{
    public Animator anim = null;
    public TowerStats stats = null;
    [SerializeField] public bool bleed;

    void Start()
    {
        anim = GetComponent<Animator>();
        stats = GetComponent<TowerStats>();
        bleed = false;
    }

    void Update()
    {
        
    }

    void Upgrade1()
    {
        //tags.Add(stats.TowerTags.ArmourPiercing);
    }

    void Upgrade2()
    {
        
    }

    void Upgrade3()
    {
        
    }
}