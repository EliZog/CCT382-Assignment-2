using UnityEngine;
public class MissileDamage : MonoBehaviour, IDamageMethod 
{ 
    public LayerMask EnemiesLayer;
    [SerializeField] private ParticleSystem MissileSystem;
    [SerializeField] private Transform TowerHead;

    private ParticleSystem.MainModule MissileSystemMain; 
    public float Damage;
    private float Firerate;
    private float Delay;
    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
        Delay = 1f / Firerate;
    } 
    public void DamageTick(EnemyStats Target)
    {
        if (Target)
        {
            if (Delay > 0f)
            {
                Delay -= Time.deltaTime; return;
            }

            Vector3 dir = (Target.transform.position - TowerHead.position).normalized;

            // spawn slightly in front of the tower muzzle
            float muzzleOffset = 3f;
            MissileSystem.transform.position = TowerHead.position + dir * muzzleOffset;
            MissileSystem.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            MissileSystem.Play();

            Delay = 1f / Firerate;
        } 

    } 
}