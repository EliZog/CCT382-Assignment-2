using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GameLoopManager : MonoBehaviour
{

    public static List<TowerBehaviour> TowersInGame;
    public static Vector3[] NodePositions;
    public static float[] NodeDistances;

    private static Queue<ApplyEffectData> EffectsQueue;
    private static Queue<EnemyDamageData> DamageData;
    private static Queue<EnemyStats> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    private void Start()
    {
        EffectsQueue = new Queue<ApplyEffectData>();
        DamageData = new Queue<EnemyDamageData>();
        TowersInGame = new List<TowerBehaviour>();
        EnemyIDsToSummon = new Queue<int>();
        EnemiesToRemove = new Queue<EnemyStats>();
        EntitySummoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];
        for (int i = 0; i < NodePositions.Length; i++) 
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

        NodeDistances = new float[NodePositions.Length-1];
        for (int i = 0; i < NodeDistances.Length; i++)
        {
            NodeDistances[i] = Vector3.Distance(NodePositions[i], NodePositions[i + 1]);
        }
        StartCoroutine(GameLoop());
        InvokeRepeating("SummonTest", 0f, 1f);
    }

    void SummonTest()
    {
        EnqueueEnemyIDToSummon(1);
    }

    IEnumerator GameLoop() 
    {
        while (LoopShouldEnd == false) {

            // Spawn Enemies

            if (EnemyIDsToSummon.Count > 0)
            {
                for (int i = 0; i < EnemyIDsToSummon.Count; i++)
                {
                    EntitySummoner.SummonEnemy(EnemyIDsToSummon.Dequeue());
                }
            }

            // Spawn Towers

            // Move Enemies

            NativeArray<Vector3> NodesToUse = new NativeArray<Vector3>(NodePositions, Allocator.TempJob);
            NativeArray<float> EnemySpeeds = new NativeArray<float>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
            NativeArray<int> NodeIndices = new NativeArray<int>(EntitySummoner.EnemiesInGame.Count, Allocator.TempJob);
            TransformAccessArray EnemyAccess = new TransformAccessArray(EntitySummoner.EnemiesInGameTransform.ToArray(), 2);

            for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++) 
            {
                EnemySpeeds[i] = EntitySummoner.EnemiesInGame[i].Speed;
                NodeIndices[i] = EntitySummoner.EnemiesInGame[i].NodeIndex;
            }

            MoveEnemiesJob MoveJob = new MoveEnemiesJob
            {
                NodePositions = NodesToUse,
                EnemySpeed = EnemySpeeds,
                NodeIndex = NodeIndices,
                deltaTime = Time.deltaTime,
            };

            JobHandle MoveJobHandle = MoveJob.Schedule(EnemyAccess);
            MoveJobHandle.Complete();

            for (int i = 0; i < EntitySummoner.EnemiesInGame.Count; i++) 
            {
                EntitySummoner.EnemiesInGame[i].NodeIndex = NodeIndices[i];

                if (EntitySummoner.EnemiesInGame[i].NodeIndex == NodePositions.Length) 
                {
                    EnqueueEnemyToRemove(EntitySummoner.EnemiesInGame[i]);
                }
            }

            EnemySpeeds.Dispose();
            NodeIndices.Dispose();
            EnemyAccess.Dispose();
            NodesToUse.Dispose();

            // Tick Towers

            foreach (TowerBehaviour tower in TowersInGame)
            {
                tower.Target = TowerTargeting.GetTarget(tower, TowerTargeting.TargetType.First);
                tower.Tick();
            }

            // Apply Effects
            if (EffectsQueue.Count > 0)
            {
                for (int i = 0; i < EffectsQueue.Count; i++)
                {
                    ApplyEffectData CurrentDamageData = EffectsQueue.Dequeue();
                    Effect EffectDuplicate = CurrentDamageData.EnemyToAffect.ActiveEffects.Find(x => x.EffectName == CurrentDamageData.EffectToApply.EffectName);
                    if (EffectDuplicate == null)
                    {
                        CurrentDamageData.EnemyToAffect.ActiveEffects.Add(CurrentDamageData.EffectToApply);
                    }
                    else
                    {
                        EffectDuplicate.ExpireTime = CurrentDamageData.EffectToApply.ExpireTime;
                    }
                    

                }
            }

            //Tick Enemies
            foreach (EnemyStats CurrentEnemy in EntitySummoner.EnemiesInGame)
            {
                CurrentEnemy.Tick();
            }


            // Damage Enemies

            if (DamageData.Count > 0)
            {
                for (int i = 0; i < DamageData.Count; i++)
                {
                    EnemyDamageData CurrentDamageData = DamageData.Dequeue();
                    CurrentDamageData.TargetEnemy.health -= CurrentDamageData.TotalDamage / CurrentDamageData.Resistance;

                    if (CurrentDamageData.TargetEnemy.health <= 0f)
                    {
                        if (!EnemiesToRemove.Contains(CurrentDamageData.TargetEnemy))
                        {
                            EnqueueEnemyToRemove(CurrentDamageData.TargetEnemy);
                        }
                        
                    }
                }
            }

            // Remove Enemies

            if (EnemiesToRemove.Count > 0) {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EntitySummoner.RemoveEnemy(EnemiesToRemove.Dequeue());
                }
            }

            // Remove Towers


            yield return null;
        }
        
    }

    public static void EnqueueEffectToApply(ApplyEffectData effectData)
    {
        EffectsQueue.Enqueue(effectData);
    }

    public static void EnqueueDamageData(EnemyDamageData damageData)
    {
        DamageData.Enqueue(damageData);
    }

    public static void EnqueueEnemyIDToSummon(int ID)
    {
        EnemyIDsToSummon.Enqueue(ID);
    }

    public static void EnqueueEnemyToRemove(EnemyStats EnemyToRemove)
    {
        EnemiesToRemove.Enqueue(EnemyToRemove);
    }
}

public class Effect
{
    public Effect(string effectName, float damageRate, float damage, float expireTime)
    {
        ExpireTime = expireTime;
        EffectName = effectName;
        DamageRate = damageRate;
        Damage = damage;
    }

    public string EffectName;

    public float DamageDelay;
    public float DamageRate;
    public float Damage;

    public float ExpireTime;


}

public struct ApplyEffectData
{
    public ApplyEffectData(EnemyStats enemytoAffect, Effect effectToApply)
    {
        EnemyToAffect = enemytoAffect;
        EffectToApply = effectToApply;
    }

    public EnemyStats EnemyToAffect;
    public Effect EffectToApply;
}
public struct EnemyDamageData
{
    public EnemyDamageData(EnemyStats target, float damage, float resistance)
    {
        TargetEnemy = target;
        TotalDamage = damage;
        Resistance = resistance;
    }

    public EnemyStats TargetEnemy;
    public float TotalDamage;
    public float Resistance;
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> NodePositions;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> EnemySpeed;
    [NativeDisableParallelForRestriction]
    public NativeArray<int> NodeIndex;

    public float deltaTime;
    public void Execute(int index, TransformAccess transform)
    {
        if (NodeIndex[index] < NodePositions.Length)
        {
            Vector3 PostionToMoveTo = NodePositions[NodeIndex[index]];

            transform.position = Vector3.MoveTowards(transform.position, PostionToMoveTo, EnemySpeed[index] * deltaTime);

            if (transform.position == PostionToMoveTo)
            {
                NodeIndex[index]++;
            }
        }
        
    }
}