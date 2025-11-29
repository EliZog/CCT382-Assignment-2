using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask EnemiesLayer;
    
    public EnemyStats Target;
    public Transform TowerPivot;

    public float Damage;
    public float Firerate;
    public float Range;

    private float Delay; // time before apply damage

    private IDamageMethod CurrentDamageMethodClass;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentDamageMethodClass = GetComponent<IDamageMethod>();

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
}
