using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameTimer : MonoBehaviour
{
    public GameStateManager gameStateManager;
    public Text timerText; // Referência ao componente Text para exibir o tempo.
    public static float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    void Update()
    {
        float t = Time.time - startTime;

        if(!gameStateManager.hideTutorial)
            t = PlayerPrefs.GetFloat("high score", 0f);

        string minutes = ((int)t / 60).ToString();
        string seconds = (t % 60).ToString("f0"); // "f2" limita a exibição a duas casas decimais.

        timerText.text = string.Format("{0}:{1}",
            minutes, seconds.PadLeft(2, '0')
        );

    }
}
