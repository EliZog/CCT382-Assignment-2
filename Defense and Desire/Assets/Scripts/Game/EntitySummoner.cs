using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySumoner : MonoBehaviour
{
    public static List<EnemyStats> EnemiesInGame;
    public static Dictionary<int, GameObject> EnemyPrefabs;
    public static Dictionary<int, Queue<EnemyStats>> EnemyObjectPools;
    void Start() {
        EnemyPrefabs = new Dictionary<int, GameObject>();
        EnemyObjectPools = new Dictionary<int, Queue<EnemyStats>>();
        EnemiesInGame = new List<EnemyStats>();
    }
}
