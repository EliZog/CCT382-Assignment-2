using UnityEngine;

public class FlamethrowerDamage : MonoBehaviour, IDamageMethod
{

    public Collider FireTrigger;
    public GameObject extraFireTriggers;

    public ParticleSystem FireEffect;
    public ParticleSystem FireEffect2;
    public ParticleSystem FireEffect3;
    public AudioSource Audio;

    public float Damage;
    public float Firerate;
    public bool Cerbereus = false;

    public void Init(float Damage, float Firerate)
    {
        this.Damage = Damage;
        this.Firerate = Firerate;
        Audio = GetComponent<AudioSource>();
    }

    public void DamageTick(EnemyStats Target)
    {
        
        if (!Cerbereus)
            FireTrigger.enabled = Target != null;
        else
        {
            extraFireTriggers.SetActive(Target != null);

        }

        if (Target)
        {
            if (!Audio.isPlaying)
                Audio.Play();

            if (!FireEffect.isPlaying) 
                FireEffect.Play();

            if (Cerbereus)
            {
                if (!FireEffect2.isPlaying) 
                    FireEffect2.Play();
                
                if (!FireEffect3.isPlaying) 
                    FireEffect3.Play();
            }
            
            return;
        }

        FireEffect.Stop();
        //Audio.Stop();

        if (Cerbereus)
        {
            FireEffect2.Stop();
            FireEffect3.Stop();   
        }
    }
}
