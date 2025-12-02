using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySummoner : MonoBehaviour
{
    public static List<EnemyStats> EnemiesInGame;
    public static List<Transform> EnemiesInGameTransform;

    public static Dictionary<Transform, EnemyStats> EnemyTransformPairs;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<EnemyStats>> EnemyObjectPools;

    private static bool isInitialized;

    public static void Init() {
        if (!isInitialized)
        {
            EnemyTransformPairs = new Dictionary<Transform, EnemyStats>();
            EnemyPrefabs = new Dictionary<int, GameObject>();
            EnemyObjectPools = new Dictionary<int, Queue<EnemyStats>>();
            EnemiesInGameTransform = new List<Transform>();
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

                SummonedEnemy.gameObject.SetActive(true);
            }
            else
            {
                // Instantiate new instance of enemy and initialize
                GameObject NewEnemy = Instantiate(EnemyPrefabs[EnemyID], GameLoopManager.NodePositions[0], Quaternion.identity);
                SummonedEnemy = NewEnemy.GetComponent<EnemyStats>();
                SummonedEnemy.InitVariables();
            }
        }
        else
        {
            Debug.Log($"ENTITYSUMMONER: ENEMY WITH ID OF {EnemyID} DOES NOT EXIST!");
            return null;
        }
        if (!EnemiesInGame.Contains(SummonedEnemy)) EnemiesInGame.Add(SummonedEnemy);
        if (!EnemiesInGameTransform.Contains(SummonedEnemy.transform)) EnemiesInGameTransform.Add(SummonedEnemy.transform);
        if (!EnemyTransformPairs.ContainsKey(SummonedEnemy.transform)) EnemyTransformPairs.Add(SummonedEnemy.transform, SummonedEnemy);

        SummonedEnemy.ID = EnemyID;
        return SummonedEnemy;
    }

    public static void RemoveEnemy(EnemyStats EnemyToRemove) 
    {
        EnemyObjectPools[EnemyToRemove.ID].Enqueue(EnemyToRemove);
        EnemyToRemove.gameObject.SetActive(false);

        EnemyTransformPairs.Remove(EnemyToRemove.transform);
        EnemiesInGameTransform.Remove(EnemyToRemove.transform);
        EnemiesInGame.Remove(EnemyToRemove);
    }

}
