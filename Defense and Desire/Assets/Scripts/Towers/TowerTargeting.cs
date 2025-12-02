using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TowerTargeting
{
    public enum TargetType
    {
        First,
        Last,
        Close,
        Strong,
        Weak
    }


    public static EnemyStats GetTarget(TowerBehaviour currentTower, TargetType targetMethod, List<TowerTags.Tags> tags)
    {
        if (currentTower == null) return null;

        // Find enemies in range
        Collider[] enemiesInRange = Physics.OverlapSphere(
            currentTower.transform.position,
            currentTower.Range,
            currentTower.EnemiesLayer
        );

        if (enemiesInRange.Length == 0)
            return null;

        // Native arrays
        NativeArray<EnemyData> enemiesToCalculate =
            new NativeArray<EnemyData>(enemiesInRange.Length, Allocator.TempJob);
        NativeArray<Vector3> nodePositions =
            new NativeArray<Vector3>(GameLoopManager.NodePositions, Allocator.TempJob);
        NativeArray<float> nodeDistances =
            new NativeArray<float>(GameLoopManager.NodeDistances, Allocator.TempJob);
        NativeArray<int> bestEnemyIndex =
            new NativeArray<int>(1, Allocator.TempJob);   // output

        bestEnemyIndex[0] = -1;

        // Fill data
        for (int i = 0; i < enemiesToCalculate.Length; i++)
        {
            EnemyStats currentEnemy =
                enemiesInRange[i].transform.parent.GetComponent<EnemyStats>();

            if (currentEnemy == null)
                continue;

            // DEBUG WITH ELIAS // 

            Debug.Log("Enemy ID: " + currentEnemy.ID);

            bool AP = currentEnemy.ID == 3 && !tags.Contains(TowerTags.Tags.ArmourPiercing);

            Debug.Log("AP tag: " + AP);

            if (AP)
                continue;
            
            Debug.Log("AP enemy is being incorrectly targeted");

            int enemyIndexInList =
                EntitySummoner.EnemiesInGame.FindIndex(x => x == currentEnemy);

            enemiesToCalculate[i] = new EnemyData(
                currentEnemy.transform.position,
                currentEnemy.NodeIndex,
                currentEnemy.health,
                currentEnemy.maxHealth,
                enemyIndexInList
            );
        }

        // Initial compare value matches the type (min vs max)
        float initialCompare;

        switch (targetMethod)
        {
            case TargetType.Last:
            case TargetType.Strong:
                initialCompare = Mathf.NegativeInfinity;   // looking for largest value
                break;

            case TargetType.Weak:
            case TargetType.First:
            case TargetType.Close:
            default:
                initialCompare = Mathf.Infinity;           // looking for smallest value
                break;
        }


        // Setup job
        SearchForEnemy enemySearchJob = new SearchForEnemy
        {
            Enemies = enemiesToCalculate,
            NodePositions = nodePositions,
            NodeDistances = nodeDistances,
            BestEnemyIndex = bestEnemyIndex,
            TowerPosition = currentTower.transform.position,
            InitialCompareValue = initialCompare,
            TargetingType = (int)targetMethod
        };

        // Schedule & complete (single-threaded job)
        JobHandle handle = enemySearchJob.Schedule();
        handle.Complete();

        int selectedIndex = bestEnemyIndex[0];

        if (selectedIndex == -1)
        {
            // No valid target found, clean up and bail
            enemiesToCalculate.Dispose();
            nodePositions.Dispose();
            nodeDistances.Dispose();
            bestEnemyIndex.Dispose();
            return null;
        }

        // Read from the NativeArray *before* disposing it
        int enemyIndexToReturn = enemiesToCalculate[selectedIndex].EnemyIndex;

        // Now it's safe to dispose everything
        enemiesToCalculate.Dispose();
        nodePositions.Dispose();
        nodeDistances.Dispose();
        bestEnemyIndex.Dispose();

        return EntitySummoner.EnemiesInGame[enemyIndexToReturn];
    }

    struct EnemyData
    {
        public EnemyData(Vector3 position, int nodeIndex, float hp, float maxHP, int enemyIndex)
        {
            EnemyPosition = position;
            NodeIndex = nodeIndex;
            EnemyIndex = enemyIndex;
            Health = hp;
            MaxHealth = maxHP;
        }

        public Vector3 EnemyPosition;
        public int EnemyIndex;
        public int NodeIndex;
        public float Health;
        public float MaxHealth;


    }

    struct SearchForEnemy : IJob
    {
        [ReadOnly] public NativeArray<EnemyData> Enemies;
        [ReadOnly] public NativeArray<Vector3> NodePositions;
        [ReadOnly] public NativeArray<float> NodeDistances;

        public NativeArray<int> BestEnemyIndex; // length 1

        public Vector3 TowerPosition;
        public float InitialCompareValue;
        public int TargetingType; // 0 = First, 1 = Last, 2 = Close, 3 = Strong

        public void Execute()
        {
            int bestIndex = -1;
            float compare = InitialCompareValue;

            for (int index = 0; index < Enemies.Length; index++)
            {
                EnemyData enemy = Enemies[index];

                // if enemyIndex is -1, means it wasn't found in EnemiesInGame
                if (enemy.EnemyIndex == -1)
                    continue;

                switch (TargetingType)
                {
                    case 0: // First � smallest distance to end
                        {
                            float distToEnd = GetDistanceToEnd(enemy);
                            if (distToEnd < compare)
                            {
                                compare = distToEnd;
                                bestIndex = index;
                            }
                            break;
                        }

                    case 1: // Last � largest distance to end
                        {
                            float distToEnd = GetDistanceToEnd(enemy);
                            if (distToEnd > compare)
                            {
                                compare = distToEnd;
                                bestIndex = index;
                            }
                            break;
                        }

                    case 2: // Close � smallest distance to tower
                        {
                            float distToTower = Vector3.Distance(
                                TowerPosition,
                                enemy.EnemyPosition
                            );
                            if (distToTower < compare)
                            {
                                compare = distToTower;
                                bestIndex = index;
                            }
                            break;
                        }

                    case 3: // Strong
                        {
                            if (enemy.MaxHealth > compare)
                            {
                                compare = enemy.MaxHealth;
                                bestIndex = index;
                            }
                            break;
                        }

                    case 4: // Weak
                        {
                            if (enemy.MaxHealth < compare)
                            {
                                compare = enemy.MaxHealth;
                                bestIndex = index;
                            }
                            break;
                        }
                }
            }

            BestEnemyIndex[0] = bestIndex;
        }

        private float GetDistanceToEnd(EnemyData enemy)
        {
            // Safety: if NodeIndex is at or beyond the end, distance is 0
            if (enemy.NodeIndex >= NodePositions.Length)
                return 0f;

            float finalDistance = Vector3.Distance(
                enemy.EnemyPosition,
                NodePositions[enemy.NodeIndex]
            );

            // Sum remaining segment distances
            for (int i = enemy.NodeIndex; i < NodeDistances.Length; i++)
            {
                finalDistance += NodeDistances[i];
            }

            return finalDistance;
        }
    }
}
