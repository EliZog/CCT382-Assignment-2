using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TowerTargeting
{
    public enum TargetType
    {
        First,
        Last,
        Close
    }

    public static EnemyStats GetTarget(TowerBehaviour currentTower, TargetType targetMethod)
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

            int enemyIndexInList =
                EntitySumoner.EnemiesInGame.FindIndex(x => x == currentEnemy);

            enemiesToCalculate[i] = new EnemyData(
                currentEnemy.transform.position,
                currentEnemy.NodeIndex,
                currentEnemy.health,
                enemyIndexInList
            );
        }

        // Initial compare value
        float initialCompare =
            (targetMethod == TargetType.Last) ? Mathf.NegativeInfinity : Mathf.Infinity;

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

        return EntitySumoner.EnemiesInGame[enemyIndexToReturn];

    }

    struct EnemyData
    {
        public EnemyData(Vector3 position, int nodeIndex, float hp, int enemyIndex)
        {
            EnemyPosition = position;
            NodeIndex = nodeIndex;
            EnemyIndex = enemyIndex;
            Health = hp;
        }

        public Vector3 EnemyPosition;
        public int EnemyIndex;
        public int NodeIndex;
        public float Health;
    }

    struct SearchForEnemy : IJob
    {
        [ReadOnly] public NativeArray<EnemyData> Enemies;
        [ReadOnly] public NativeArray<Vector3> NodePositions;
        [ReadOnly] public NativeArray<float> NodeDistances;

        public NativeArray<int> BestEnemyIndex; // length 1

        public Vector3 TowerPosition;
        public float InitialCompareValue;
        public int TargetingType; // 0 = First, 1 = Last, 2 = Close

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

                float value = 0f;

                switch (TargetingType)
                {
                    case 0: // First
                    case 1: // Last
                        value = GetDistanceToEnd(enemy);
                        break;

                    case 2: // Close
                        value = Vector3.Distance(
                            TowerPosition,
                            enemy.EnemyPosition
                        );
                        break;
                }

                bool isBetter =
                    (TargetingType == 1) ? value > compare  // Last -> max
                                         : value < compare; // First / Close -> min

                if (isBetter)
                {
                    compare = value;
                    bestIndex = index;
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
