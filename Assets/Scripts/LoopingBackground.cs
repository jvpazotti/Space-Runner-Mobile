using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingBackground : MonoBehaviour
{
    public float backgroundSpeed;
    public Renderer backgroundRenderer;

    public GameStateManager gameStateManager;

    void Update()
    {
        if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
            return;

        backgroundRenderer.material.mainTextureOffset += new Vector2(0f, backgroundSpeed * Time.deltaTime);
    }

    public void SetBackgroundSpeed(float speed)
    {
        backgroundSpeed = speed;
    }

    public float GetBackgroundSpeed()
    {
        return backgroundSpeed;
    }
}
