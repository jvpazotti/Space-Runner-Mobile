using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SceneMovement : MonoBehaviour
{
    public float cameraSpeed;

    public GameStateManager gameStateManager;

    void Update()
    {
        if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
            return;

        transform.position += new Vector3(0, cameraSpeed * Time.deltaTime, 0);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        cameraSpeed *= multiplier;

        cameraSpeed = math.clamp(cameraSpeed, 5f, 14f);
    }
}