using UnityEngine;

public class FireTriggerManager : MonoBehaviour
{

    [SerializeField] private FlamethrowerDamage BaseClass;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Should apply fire effect");
            Effect FlameEffect = new Effect("Fire", BaseClass.Firerate, BaseClass.Damage, 5f);
            ApplyEffectData EffectData = new ApplyEffectData(EntitySummoner.EnemyTransformPairs[other.transform.parent], FlameEffect);
            GameLoopManager.EnqueueEffectToApply(EffectData);
        }
        Debug.Log(other.gameObject.tag);
    }
}
