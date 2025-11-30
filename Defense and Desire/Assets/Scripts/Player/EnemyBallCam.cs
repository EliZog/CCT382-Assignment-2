using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // NEW input system

public class EnemyBallCam : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Space;   // press Space to toggle follow mode

    [Header("Follow settings")]
    public float followDistance = 8f;
    public float followHeight = 4f;
    public float followPositionSmoothTime = 0.2f;
    public float followRotationSmoothSpeed = 5f;

    [Header("Free cam movement")]
    public float moveSpeed = 12f;
    public float fastSpeedMultiplier = 3f;
    public float lookSensitivity = 120f; // degrees per second

    private bool followMode = false;
    private EnemyStats currentTarget;
    private Vector3 followPosVelocity;

    // for free-cam mouse look
    private float yaw;
    private float pitch;

    private void Start()
    {
        // Initialize yaw/pitch from current rotation
        Vector3 e = transform.rotation.eulerAngles;
        pitch = e.x;
        yaw = e.y;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // Use SPACE directly
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            followMode = !followMode;

            if (!followMode)
            {
                // Leaving follow mode – keep current rotation for free-cam
                currentTarget = null;
                Vector3 e = transform.rotation.eulerAngles;
                pitch = e.x;
                yaw = e.y;
            }
        }


        if (followMode)
        {
            UpdateFollowMode();
        }
        else
        {
            UpdateFreeCamMode();
        }
    }

    // ================= FOLLOW MODE =================

    private void UpdateFollowMode()
    {
        EnemyStats leader = GetLeadingEnemy();
        if (leader == null)
            return;

        if (leader != currentTarget)
        {
            currentTarget = leader;
        }

        Transform t = currentTarget.transform;

        // Position: fixed distance behind + height above enemy
        Vector3 behind = -t.forward * followDistance;
        Vector3 up = Vector3.up * followHeight;
        Vector3 desiredPos = t.position + behind + up;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref followPosVelocity,
            followPositionSmoothTime
        );

        // Rotation: look at enemy
        Vector3 dir = t.position - transform.position;
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                followRotationSmoothSpeed * Time.deltaTime
            );
        }
    }

    private EnemyStats GetLeadingEnemy()
    {
        List<EnemyStats> enemies = EntitySummoner.EnemiesInGame;

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

            if (nodeIndex < GameLoopManager.NodePositions.Length)
            {
                Vector3 nextNode = GameLoopManager.NodePositions[nodeIndex];
                float dist = Vector3.Distance(e.transform.position, nextNode);

                // closer to next node => more progress
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

    // ================= FREE CAM MODE =================

    private void UpdateFreeCamMode()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null)
            return;

        // Movement (WASDQE)
        Vector3 move = Vector3.zero;
        if (keyboard.wKey.isPressed) move += transform.forward;
        if (keyboard.sKey.isPressed) move -= transform.forward;
        if (keyboard.dKey.isPressed) move += transform.right;
        if (keyboard.aKey.isPressed) move -= transform.right;

        if (move.sqrMagnitude > 1e-6f)
        {
            move.Normalize();
            float speed = moveSpeed;
            if (keyboard.leftShiftKey.isPressed)
                speed *= fastSpeedMultiplier;

            transform.position += move * speed * Time.deltaTime;
        }

        // Mouse look while holding right mouse button
        if (mouse != null && mouse.rightButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            // delta is pixels this frame; scale by sensitivity & deltaTime
            yaw += delta.x * lookSensitivity * Time.deltaTime;
            pitch -= delta.y * lookSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
