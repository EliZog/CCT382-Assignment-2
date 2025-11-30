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

    [Header("Return to start settings")]
    public float returnDuration = 1f; // how long to tween back to initial camera

    private bool followMode = false;
    private EnemyStats currentTarget;
    private Vector3 followPosVelocity;

    // for free-cam mouse look
    private float yaw;
    private float pitch;

    // store initial camera transform
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // tween back to initial
    private bool isReturningToInitial = false;
    private float returnStartTime;
    private Vector3 returnStartPosition;
    private Quaternion returnStartRotation;
    private Vector3 returnVelocity;  // NEW


    private void Start()
    {
        // Store the initial camera transform
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Initialize yaw/pitch from current rotation
        Vector3 e = transform.rotation.eulerAngles;
        pitch = e.x;
        yaw = e.y;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null)
            return;

        // Handle space toggle (only if we're not currently tweening back)
        if (keyboard.spaceKey.wasPressedThisFrame && !isReturningToInitial)
        {
            if (!followMode)
            {
                // Trying to enter follow mode
                EnemyStats leader = GetLeadingEnemy();
                if (leader == null)
                {
                    // No enemies: tween back to initial camera position instead
                    StartReturnToInitial();
                }
                else
                {
                    followMode = true;
                    currentTarget = leader;
                }
            }
            else
            {
                // Leaving follow mode – stay where we are, go back to free cam
                followMode = false;
                currentTarget = null;
                Vector3 e = transform.rotation.eulerAngles;
                pitch = e.x;
                yaw = e.y;
            }
        }

        // If we're currently tweening back to the initial view, do that and skip other modes
        if (isReturningToInitial)
        {
            UpdateReturnToInitial();
            return;
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

        // NEW: if no enemies left while following, go back to initial camera
        if (leader == null)
        {
            followMode = false;
            currentTarget = null;
            StartReturnToInitial();
            return;
        }

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

    // ================ RETURN TO INITIAL VIEW =================

    private void StartReturnToInitial()
    {
        isReturningToInitial = true;
        returnStartTime = Time.time;
        returnStartPosition = transform.position;
        returnStartRotation = transform.rotation;
    }

    private void UpdateReturnToInitial()
    {
        // SmoothDamp the position (same feel as follow mode)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            initialPosition,
            ref returnVelocity,
            followPositionSmoothTime // same smoothing as follow mode
        );

        // Smoothly rotate toward the initial rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            initialRotation,
            followRotationSmoothSpeed * Time.deltaTime
        );

        // Stop returning when we're close enough
        if (Vector3.Distance(transform.position, initialPosition) < 0.05f &&
            Quaternion.Angle(transform.rotation, initialRotation) < 1f)
        {
            isReturningToInitial = false;

            // Sync free cam yaw/pitch to new rotation
            Vector3 e = transform.rotation.eulerAngles;
            pitch = e.x;
            yaw = e.y;
        }
    }

}
