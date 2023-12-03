using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using static ObstacleManager;

public class DifficultyController : MonoBehaviour
{
    public SceneMovement cameraScript;
    public ObstacleManager obstacleManager;
    public GameStateManager gameStateManager;

    public GameObject phaseTextObject,
        phaseDescriptionObject;

    private enum PhaseTypes
    {
        SPEED_UP,
        FREQUENCY_UP,
        BOTH_UP
    }

    private Dictionary<PhaseTypes, float> phaseChances = new Dictionary<PhaseTypes, float>()
    {
        { PhaseTypes.SPEED_UP, 45f },
        { PhaseTypes.FREQUENCY_UP, 45f },
        { PhaseTypes.BOTH_UP, 10f }
    };

    private float elapsedTime = 0f,
        phaseTime,
        phaseTextDisplayTime = 2f;

    private int currentPhase = 0;
    private PhaseTypes phaseType = PhaseTypes.SPEED_UP;

    private Dictionary<PhaseTypes, string> phaseDescriptions = new Dictionary<PhaseTypes, string>()
    {
        { PhaseTypes.SPEED_UP, "+Speed\n-Obstacles" },
        { PhaseTypes.FREQUENCY_UP, "-Speed\n+Obstacles" },
        { PhaseTypes.BOTH_UP, "+Speed\n+Obstacles" },
    };

    void Start()
    {
        elapsedTime = phaseTime = 0f;
        currentPhase = 1;
    }

    void Update()
    {
        if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime - phaseTime >= 30f)
        {
            phaseTime = elapsedTime;
            phaseType = DeterminePhaseType();

            UpdatePhaseParameters();

            DisplayPhase(
                string.Format("Phase {0}", currentPhase),
                phaseDescriptions[phaseType]
            );
        }
    }

    private void DisplayPhase(string phaseText, string phaseDescription)
    {
        phaseTextObject.GetComponent<Text>().text = phaseText;
        phaseTextObject.SetActive(true);

        phaseDescriptionObject.GetComponent<Text>().text = phaseDescription;
        phaseDescriptionObject.SetActive(true);

        StartCoroutine(HideTextAfterDelay());
    }

    private IEnumerator HideTextAfterDelay()
    {
        yield return new WaitForSeconds(phaseTextDisplayTime);

        phaseTextObject.SetActive(false);
        phaseDescriptionObject.SetActive(false);
    }

    private PhaseTypes DeterminePhaseType()
    {
        float _randomValue = Random.Range(0f, 100f);
        foreach (var __phase in phaseChances.Keys)
        {
            _randomValue -= phaseChances[__phase];
            if (_randomValue <= 0)
            {
                return __phase;
            }
            else continue;
        }

        return PhaseTypes.SPEED_UP;
    }

    private void UpdatePhaseParameters()
    {
        switch (phaseType)
        {
            case PhaseTypes.SPEED_UP:
                {
                    cameraScript.SetSpeedMultiplier(
                        Mathf.Pow(1.0125f, ++currentPhase)
                    );

                    obstacleManager.SetSpawnRate(0.98f);
                }
                break;
            case PhaseTypes.FREQUENCY_UP:
                {
                    cameraScript.SetSpeedMultiplier(
                        Mathf.Pow(0.98f, ++currentPhase)
                    );

                    obstacleManager.SetSpawnRate(1.1f);
                }
                break;
            case PhaseTypes.BOTH_UP:
                {
                    cameraScript.SetSpeedMultiplier(
                        Mathf.Pow(1.0125f, ++currentPhase)
                    );

                    obstacleManager.SetSpawnRate(1.025f);
                }
                break;
        }
    }
}
