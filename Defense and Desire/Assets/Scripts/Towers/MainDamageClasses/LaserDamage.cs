using UnityEngine;

public class LaserDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] private Transform LaserPivot;
    [SerializeField] private LineRenderer LaserRenderer;

    private float Damage;
    private float Firerate;
    private float Delay;
    private float BaseDamage;

    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
        Delay = 1f / Firerate;
        BaseDamage = Damage;

    }
    public void DamageTick(EnemyStats Target)
    {
        if (Target)
        {
            LaserRenderer.enabled = true;
            LaserRenderer.SetPosition(0, LaserPivot.position);
            LaserRenderer.SetPosition(1, Target.RootPart.position);


            if (Delay > 0f)
            {
                Delay -= Time.deltaTime;
                return;
            }

            // apply laser slow effect
            GameLoopManager.EnqueueDamageData(new EnemyDamageData(Target, Damage, Target.DamageResistance, 0));
            Delay = 1f / Firerate;
            Damage *= 2;
            return;
        }
        else
        {
            Damage = BaseDamage;
        }
        LaserRenderer.enabled = false;
    }

    
}
