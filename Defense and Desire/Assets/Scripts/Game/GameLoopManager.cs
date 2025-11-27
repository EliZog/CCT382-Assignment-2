using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    public static Vector3[] NodePositions;
    private static Queue<EnemyStats> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    // Start
    private void Start()
    {
        EnemyIDsToSummon = new Queue<int>();
        EntitySumoner.Init();

        NodePositions = new Vector3[NodeParent.childCount];
        for (int i = 0; i < NodePositions.Length; i++) 
        {
            NodePositions[i] = NodeParent.GetChild(i).position;
        }

        StartCoroutine(GameLoop());
        InvokeRepeating("SummonTest", 0f, 1f);
        InvokeRepeating("RemoveTest", 0f, 1f);
    }

    void RemoveTest()
    {
        if (EntitySumoner.EnemiesInGame.Count > 0)
        {
            EntitySumoner.RemoveEnemy(
                EntitySumoner.EnemiesInGame[Random.Range(0, EntitySumoner.EnemiesInGame.Count)]
                );
        }
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
                    EntitySumoner.SummonEnemy(EnemyIDsToSummon.Dequeue());
                }
            }

            // Spawn Towers

            // Move Enemies

            // Tick Towers

            // Apply Effects

            // Damage Enemies

            // Remove Enemies

            if (EnemiesToRemove.Count > 0) {
                for (int i = 0; i < EnemiesToRemove.Count; i++)
                {
                    EntitySumoner.RemoveEnemy(EnemiesToRemove.Dequeue());
                }
            }

            // Remove Towers


            yield return null;
        }
        
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
