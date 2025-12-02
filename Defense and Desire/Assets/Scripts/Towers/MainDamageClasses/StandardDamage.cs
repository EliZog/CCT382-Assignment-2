using UnityEngine;

public interface IDamageMethod
{
    public void Init(float Damage, float Firerate);
    public void DamageTick(EnemyStats Target);

}
public class StandardDamage : MonoBehaviour, IDamageMethod
{
    private float Damage;
    private float Firerate;
    private float Delay;
    private TowerBehaviour Tower;

    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
        Delay = 1f / Firerate;
        Tower = GetComponent<TowerBehaviour>();
    }
    public void DamageTick(EnemyStats Target)
    {
        if (Target)
        {
            if (Delay > 0f)
            {
                Delay -= Time.deltaTime;
                return;
            }

            bool weakened = Target.health / Target.maxHealth <= 0.5;

            GameLoopManager.EnqueueDamageData(new EnemyDamageData(Target, Tower.Upgrade3 && weakened ? Damage * 2 : Damage, Target.DamageResistance, 0));
            Delay = 1f / Firerate;
        }
        
    }

    
}
