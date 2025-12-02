using System.Collections.Generic;
using UnityEngine;

public class MissileCollisionManager : MonoBehaviour
{
    [SerializeField] private MissileDamage BaseClass;
    [SerializeField] private ParticleSystem ExplosionSystem;
    [SerializeField] private ParticleSystem MissileSystem;
    public float ExplosionRadius;
    public bool ShellShock;
    private List<ParticleCollisionEvent> MissileCollisions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MissileCollisions = new List<ParticleCollisionEvent>();
        ShellShock = false;
    }

    // Update is called once per frame
    public void OnParticleCollision(GameObject other)
    {
        MissileSystem.GetCollisionEvents(other, MissileCollisions);

        for (int collisionevent = 0; collisionevent < MissileCollisions.Count; collisionevent++)
        {

            ExplosionSystem.transform.position = MissileCollisions[collisionevent].intersection;
            ExplosionSystem.Play();
            Collider[] EnemiesInRadius = Physics.OverlapSphere(
                MissileCollisions[collisionevent].intersection, 
                ExplosionRadius, 
                BaseClass.EnemiesLayer
            );

            for (int i = 0; i < EnemiesInRadius.Length; i++)
            {
                EnemyStats EnemyToDamage = EntitySummoner.EnemyTransformPairs[EnemiesInRadius[i].transform.parent];
                EnemyDamageData DamageToApply = new EnemyDamageData(EnemyToDamage, BaseClass.Damage, EnemyToDamage.DamageResistance, 0);
                GameLoopManager.EnqueueDamageData(DamageToApply);

                if (ShellShock)
                {
                    Debug.Log("Shell Shock Ability has been activated");
                    Effect ShellShockEffect = new Effect("ShellShock", 0, 0, 1f, 0);
                    ApplyEffectData EffectData = new ApplyEffectData(EnemyToDamage, ShellShockEffect);
                    GameLoopManager.EnqueueEffectToApply(EffectData);
                }
            }
        }
    }
}
