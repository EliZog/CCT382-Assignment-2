using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using TMPro;
using UnityEngine.UIElements;

public class GameLoopManager : MonoBehaviour
{
    public static List<TowerBehaviour> TowersInGame;
    public static Vector3[] NodePositions;
    public static float[] NodeDistances;

    private static Queue<ApplyEffectData> EffectsQueue;
    private static Queue<EnemyDamageData> DamageData;
    private static Queue<EnemyStats> EnemiesToRemove;
    private static Queue<int> EnemyIDsToSummon;

    private PlayerStats PlayerStatistics;
    [SerializeField] private TextMeshProUGUI WavesText;
    [SerializeField] private UnityEngine.UI.Button NextWaveButton;
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private GameObject VictoryScreen;

    public Transform NodeParent;
    public bool LoopShouldEnd;

    [System.Serializable]
    public class WaveEnemy
    {
        public int enemyID = 1;        // which enemy prefab ID (same as your test IDs)
        public int count = 5;          // how many of this enemy in the wave
        public float spawnInterval = 0.5f; // time between each spawn of this enemy type
    }

    [System.Serializable]
    public class Wave
    {
        public List<WaveEnemy> enemies = new List<WaveEnemy>();
        public float delayAfterWave = 3f; // time between this wave and the next one
    }

    public List<Wave> Waves = new List<Wave>();
    private int _currentWaveIndex = 0;
    private bool _waveIsRunning = false;

    private bool hasWon = false; // guard so we only trigger once
    public int MaxWaves { get; private set; }
    public int CurrentWave { get; private set; }

    private void UpdateWaveText()
    {
        if (WavesText == null) return;

        WavesText.text = $"{CurrentWave}/{MaxWaves}";
    }

    private void UpdateWaveButtonState()
    {
        if (NextWaveButton == null) return;

        NextWaveButton.interactable = !_waveIsRunning;
    }

    private void Victory()
    {
        if (hasWon) return;
        hasWon = true;

        LoopShouldEnd = true;
        _waveIsRunning = false;
        UpdateWaveButtonState(); // if you have this from before

        if (VictoryScreen != null)
        {
            VictoryScreen.SetActive(true);
        }
    }

    private void GameOver()
    {
        LoopShouldEnd = true;

        // stop wave logic
        _waveIsRunning = false;
        UpdateWaveButtonState();  // if you have this from earlier

        if (GameOverScreen != null)
        {
            GameOverScreen.SetActive(true);
        }
    }

    private void Start()
    {
        if (GameOverScreen != null)
        {
            GameOverScreen.SetActive(false);
        }
        if (VictoryScreen != null)
        {
            VictoryScreen.SetActive(false);
        }

        MaxWaves = (Waves != null) ? Waves.Count : 0;
        CurrentWave = 0;

        UpdateWaveText();  // Wave 0 / X

        PlayerStatistics = FindObjectOfType<PlayerStats>();
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

        NodeDistances = new float[NodePositions.Length - 1];
        for (int i = 0; i < NodeDistances.Length; i++)
        {
            NodeDistances[i] = Vector3.Distance(NodePositions[i], NodePositions[i + 1]);
        }

        // Initialize wave counters based on Inspector list
        MaxWaves = (Waves != null) ? Waves.Count : 0;
        CurrentWave = 0; // no wave started yet
        UpdateWaveText();
        UpdateWaveButtonState();

        StartCoroutine(GameLoop());
    }

    void SummonTest()
    {
        EnqueueEnemyIDToSummon(1);
    }

    void SummonTestStrong()
    {
        EnqueueEnemyIDToSummon(3);
    }

    IEnumerator WaveLoop()
    {

        if (_waveIsRunning || _currentWaveIndex >= Waves.Count)
            yield break;

        _waveIsRunning = true;
        UpdateWaveButtonState(); // enable button

        CurrentWave = _currentWaveIndex + 1;
        UpdateWaveText();


        Wave currentWave = Waves[_currentWaveIndex];

        // 1-based for UI: Wave 1, Wave 2, ...
        CurrentWave = _currentWaveIndex + 1;

        foreach (WaveEnemy waveEnemy in currentWave.enemies)
        {
            for (int i = 0; i < waveEnemy.count; i++)
            {
                EnqueueEnemyIDToSummon(waveEnemy.enemyID);
                yield return new WaitForSeconds(waveEnemy.spawnInterval);
            }
        }

        // Wait until this wave is cleared before allowing next
        while (EntitySummoner.EnemiesInGame.Count > 0 && !LoopShouldEnd)
        {
            yield return null;
        }

        // reset all tower dialogue buttons
        foreach (TowerBehaviour Tower in TowersInGame)
        {
            if (Tower.level < 3)
                Tower.ResetDialogue();
        }

        _currentWaveIndex++;
        _waveIsRunning = false;
        UpdateWaveButtonState(); // re-enable button

        // Optionally: when all waves are done, CurrentWave can stay as last wave
        // or you can set it to 0 or some "completed" sentinel if you want.
    }


    public void StartNextWave()
    {
        if (!_waveIsRunning && _currentWaveIndex < Waves.Count && !LoopShouldEnd)
        {
            StartCoroutine(WaveLoop());
        }
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
                EnemyStats enemy = EntitySummoner.EnemiesInGame[i];
                enemy.NodeIndex = NodeIndices[i];

                if (enemy.NodeIndex == NodePositions.Length)
                {
                    // Enemy reached the end: damage player
                    PlayerStatistics.RemoveHealth(enemy.health);

                    // Optional: clamp to zero and maybe end the game
                    if (PlayerStatistics.GetHealth() <= 0f)
                    {
                        PlayerStatistics.SetHealthToZero();
                        GameOver(); // end game
                        
                    }

                    // Remove the enemy from the game
                    EnqueueEnemyToRemove(enemy);
                }
            }


            EnemySpeeds.Dispose();
            NodeIndices.Dispose();
            EnemyAccess.Dispose();
            NodesToUse.Dispose();

            // Tick Towers

            foreach (TowerBehaviour tower in TowersInGame)
            {
                tower.Target = TowerTargeting.GetTarget(tower, tower.TargetType, tower.Tags);
                tower.Tick();
            }
            
            // Apply Effects
            if (EffectsQueue.Count > 0)
            {
                for (int i = 0; i < EffectsQueue.Count; i++)
                {
                    ApplyEffectData CurrentDamageData = EffectsQueue.Dequeue();
                    Effect EffectDuplicate = CurrentDamageData.EnemyToAffect.ActiveEffects.Find(x => x.EffectName == CurrentDamageData.EffectToApply.EffectName);
                    
                    if (EffectDuplicate == null) // || EffectDuplicate.EffectName == "Slow")
                    {
                        //Debug.Log("Slow affect should be applied");
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
                    PlayerStatistics.AddMoney((int)CurrentDamageData.TotalDamage); // money is added by damage done

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


            // Check if game has ended

            if (_currentWaveIndex >= MaxWaves &&
                EntitySummoner.EnemiesInGame.Count == 0 &&
                !LoopShouldEnd)
            {
                // All waves spawned and cleared
                // You can trigger win screen here
                Victory();
                LoopShouldEnd = true;
            }

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
    public Effect(string effectName, float damageRate, float damage, float expireTime, float speedDebuff)
    {
        ExpireTime = expireTime;
        EffectName = effectName;
        DamageRate = damageRate;
        Damage = damage;
        SpeedDebuff = speedDebuff;
    }

    public string EffectName;

    public float DamageDelay;
    public float DamageRate;
    public float Damage;

    public float ExpireTime;
    public float SpeedDebuff;


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
    public EnemyDamageData(EnemyStats target, float damage, float resistance, float speedDebuff)
    {
        TargetEnemy = target;
        TotalDamage = damage;
        Resistance = resistance;
        SpeedDebuff = speedDebuff;
    }

    public EnemyStats TargetEnemy;
    public float TotalDamage;
    public float Resistance;
    public float SpeedDebuff;
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