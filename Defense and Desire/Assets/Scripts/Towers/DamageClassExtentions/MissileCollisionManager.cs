using System.Collections.Generic;
using UnityEngine;

public class MissileCollisionManager : MonoBehaviour
{
    [SerializeField] private MissileDamage BaseClass;
    [SerializeField] private ParticleSystem ExplosionSystem;
    [SerializeField] private ParticleSystem MissileSystem;
    [SerializeField] private float ExplosionRadius;
    private List<ParticleCollisionEvent> MissileCollisions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MissileCollisions = new List<ParticleCollisionEvent>();
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
                EnemyStats EnemyToDamage = EntitySumoner.EnemyTransformPairs[EnemiesInRadius[i].transform.parent];
                EnemyDamageData DamageToApply = new EnemyDamageData(EnemyToDamage, BaseClass.Damage, EnemyToDamage.DamageResistance);
                GameLoopManager.EnqueueDamageData(DamageToApply);
            }
        }
    }
}
