using UnityEngine;

public class FlamethrowerDamage : MonoBehaviour, IDamageMethod
{

    [SerializeField] private Collider FireTrigger;
    [SerializeField] private ParticleSystem FireEffect;

    public float Damage;
    public float Firerate;
    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
    }
    public void DamageTick(EnemyStats Target)
    {
        FireTrigger.enabled = Target != null;

        if (Target)
        {
            if (!FireEffect.isPlaying) FireEffect.Play();
            return;
        }

        FireEffect.Stop();
    }
}
