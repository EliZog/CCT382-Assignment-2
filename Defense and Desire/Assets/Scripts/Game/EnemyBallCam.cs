using System.Collections.Generic;
using UnityEngine;

public class EnemyBallCam : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Space;   // press Space to toggle

    [Header("Follow settings")]
    public float followDistance = 8f;           // how far behind the enemy
    public float followHeight = 4f;             // how high above the enemy
    public float positionSmoothTime = 0.2f;     // camera movement smoothing
    public float rotationSmoothSpeed = 5f;      // camera rotation smoothing

    private bool followMode = false;
    private EnemyStats currentTarget;
    private Vector3 positionVelocity;

    void Update()
    {
        // Toggle ball-cam
        if (Input.GetKeyDown(toggleKey))
        {
            followMode = !followMode;

            if (!followMode)
            {
                currentTarget = null; // go back to normal camera behaviour
            }
        }

        if (!followMode)
            return;

        // Pick the topmost / leading enemy
        EnemyStats leader = GetLeadingEnemy();
        if (leader == null)
            return;

        // If target changed (previous died or another moved ahead), switch
        if (leader != currentTarget)
        {
            currentTarget = leader;
        }

        // Calculate desired camera position at fixed distance & height
        Transform t = currentTarget.transform;

        // “Behind” the enemy, based on its forward direction
        Vector3 behind = -t.forward * followDistance;
        Vector3 up = Vector3.up * followHeight;
        Vector3 desiredPos = t.position + behind + up;

        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref positionVelocity,
            positionSmoothTime
        );

        // Smoothly rotate camera to look at enemy
        Vector3 dir = t.position - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot =
                Quaternion.LookRotation(dir.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Returns the enemy that is furthest along the path.
    /// Uses NodeIndex plus closeness to next node as a tiebreaker.
    /// </summary>
    private EnemyStats GetLeadingEnemy()
    {
        List<EnemyStats> enemies = EntitySumoner.EnemiesInGame;

        if (enemies == null || enemies.Count == 0)
            return null;

        EnemyStats best = null;
        float bestProgress = float.NegativeInfinity;

        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyStats e = enemies[i];
            if (e == null || e.isDead)
                continue;

            int nodeIndex = e.NodeIndex;
            float progress = nodeIndex;

            // If not at end of path, use distance to next node as tiebreaker
            if (nodeIndex < GameLoopManager.NodePositions.Length)
            {
                Vector3 nextNode = GameLoopManager.NodePositions[nodeIndex];
                float dist = Vector3.Distance(e.transform.position, nextNode);

                // Closer to next node = slightly more progress
                progress += (1f - Mathf.Clamp01(dist));
            }

            if (progress > bestProgress)
            {
                bestProgress = progress;
                best = e;
            }
        }

        return best;
    }
}
