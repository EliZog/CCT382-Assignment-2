using UnityEngine;

public class LaserDamage : MonoBehaviour, IDamageMethod
{
    public Transform LaserPivot;
    public LineRenderer LaserRenderer;

    public float Damage;
    public float Firerate;
    public float Delay;
    public float BaseDamage;
    public EnemyStats PrevTarget;

    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
        Delay = 1f / Firerate;
        BaseDamage = Damage;
        PrevTarget = null;

    }
    public void DamageTick(EnemyStats Target)
    {
        if (Target)
        {
            if (Target != PrevTarget)
            {
                Damage = BaseDamage;
                PrevTarget = Target;
            }

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
            Damage *= 1.2f;

            Effect SlowEffect = new Effect("Slow", 0, 0, 0.2f, 0);
            ApplyEffectData EffectData = new ApplyEffectData(Target, SlowEffect);
            GameLoopManager.EnqueueEffectToApply(EffectData);

            return;
        }
        LaserRenderer.enabled = false;
    }

    
}
