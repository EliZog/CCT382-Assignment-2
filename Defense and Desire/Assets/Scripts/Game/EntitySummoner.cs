using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySumoner : MonoBehaviour
{
    public static List<EnemyStats> EnemiesInGame;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<EnemyStats>> EnemyObjectPools;

    private static bool isInitialized;

    public static void Init() {
        if (!isInitialized)
        {
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemyObjectPools = new Dictionary<int, Queue<EnemyStats>>();
            EnemiesInGame = new List<EnemyStats>();

            EnemySummonData[] Enemies = Resources.LoadAll<EnemySummonData>("Enemies");
            Debug.Log(Enemies[0].name);


            foreach (EnemySummonData enemy in Enemies)
            {
                EnemyPrefabs.Add(enemy.EnemyID, enemy.EnemyPrefab);
                EnemyObjectPools.Add(enemy.EnemyID, new Queue<EnemyStats>());
            }

            isInitialized = true;

        }
        else {
            Debug.Log("ENTITYSUMMONER: THIS CLASS IS ALREADY INITIALIZED");
        }    
    
    }

    public static EnemyStats SummonEnemy(int EnemyID)
    {
        EnemyStats SummonedEnemy = null;

        if (EnemyPrefabs.ContainsKey(EnemyID))
        {
            Queue<EnemyStats> ReferencedQueue = EnemyObjectPools[EnemyID];

            if (ReferencedQueue.Count > 0)
            {
                // Dequeue enemy and initialize
                SummonedEnemy = ReferencedQueue.Dequeue();
                SummonedEnemy.InitVariables();
            }
            else
            {
                // Instantiate new instance of enemy and initialize
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], Vector3.zero, Quaternion.identity);
                SummonedEnemy = NewEnemy.GetComponent<EnemyStats>();
                SummonedEnemy.InitVariables();
            }
        }
        else
        {
            Debug.Log($"ENTITYSUMMONER: ENEMY WITH ID OF {EnemyID} DOES NOT EXIST!");
            return null;
        }

        return SummonedEnemy;
    }

}
