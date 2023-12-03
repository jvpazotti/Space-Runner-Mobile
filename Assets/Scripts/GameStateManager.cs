using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static System.Net.Mime.MediaTypeNames;
using Button = UnityEngine.UI.Button;

public class GameStateManager : MonoBehaviour
{
    public enum GameStates
    {
        MAIN_MENU,
        PLAYING,
        GAME_OVER,

        GAME_OVER_ADS,
        GAME_OVER_ADS_COMPLETE
    }

    public GameStates state = GameStates.MAIN_MENU;

    public Player player;
    public GameObject gameOverPanel,
        controlsOverlayPanel,
        continueButton,
        restartButton;

    public UnityEngine.UI.Text survivalTimeText,
        gameOverReasonText,
        highScoreText;

    public RewardedAds rewardedAds;
    public MusicController musicController;

    public bool hideTutorial = false;
    private int deathCounter = 0;

    private void Start()
    {
        
    }

    private void Update()
    {
        switch (state)
        {
            case GameStates.PLAYING:
                {
                    bool _movementInput = Input.touchCount > 0 ||
                        Input.GetAxisRaw("Horizontal") != 0f ||
                        Input.GetAxisRaw("Vertical") != 0f;

                    if (_movementInput &&
                        !hideTutorial)
                    {
                        Time.timeScale = 1f;

                        StartCoroutine(FadeOutPanel(0.5f, "Left Controls Panel"));
                        StartCoroutine(FadeOutPanel(0.5f, "Right Controls Panel"));

                        if (deathCounter < 1)
                            rewardedAds.LoadAd();

                        hideTutorial = true;
                    }
                    else if (!hideTutorial && !_movementInput)
                        Time.timeScale = 0f;

                }
                break;
            case GameStates.GAME_OVER:
                {
                    GameOver();


                    state = GameStates.GAME_OVER_ADS;
                }
                break;
            case GameStates.GAME_OVER_ADS_COMPLETE:
                {
                    StartGame(true);
                }
                break;
            default:
                break;
        }
    }

    public void StartGame(bool resume = false)
    {
        if (!resume)
        {
            SceneManager.LoadScene("GameScene");
            deathCounter = 0;
        }

        state = GameStates.PLAYING;
        Time.timeScale = 1f;

        controlsOverlayPanel.SetActive(true);
        gameOverPanel.SetActive(
            hideTutorial = !(player.spriteRenderer.enabled = true)
        );

        player.Respawn();

        if (!musicController.IsMusicPlaying())
            musicController.ResumeAudio();
    }

    private void GameOver()
    {
        highScoreText.color = survivalTimeText.color = gameOverReasonText.color = new Color(1f, 1f, 1f, 0f);

        float _survivalTime = Time.time - GameTimer.startTime;
        survivalTimeText.text = string.Format("Survival Time: {0}:{1}",
            ((int)_survivalTime / 60).ToString(),
            (_survivalTime % 60).ToString("f0").PadLeft(2, '0')
        );

        float _currentHighScore = PlayerPrefs.GetFloat("high score", 0f);
        highScoreText.text = string.Format("Current Highscore\n{0}:{1}",
            ((int)_currentHighScore / 60).ToString(),
            (_currentHighScore % 60).ToString("f0").PadLeft(2, '0')
        );

        if (_survivalTime > _currentHighScore)
        {
            PlayerPrefs.SetFloat("high score", _survivalTime);
            highScoreText.text = string.Format("New Highscore!\n{0}:{1}",
                ((int)_survivalTime / 60).ToString(),
                (_survivalTime % 60).ToString("f0").PadLeft(2, '0')
            );
        }

        gameOverReasonText.text = player.loseCondition == Player.loseConditions.DROWN ?
            "You ran out of oxygen!" :
            "Your ship was destroyed!";

        continueButton.SetActive(++deathCounter <= 1 &&
            continueButton.GetComponent<Button>().interactable
        );
        if (deathCounter <= 1 &&
            continueButton.GetComponent<Button>().interactable)
        {
            continueButton.transform.localPosition = new Vector3(100f, continueButton.transform.localPosition.y);
            restartButton.transform.localPosition = new Vector3(-100f, restartButton.transform.localPosition.y);
        }
        else
        {
            restartButton.transform.localPosition = new Vector3(0f, restartButton.transform.localPosition.y);
        }

        controlsOverlayPanel.SetActive(false);
        gameOverPanel.SetActive(!(player.spriteRenderer.enabled = false));
        player.SetHUD(false);

        musicController.KillAudio();

        StartCoroutine(FadeInText(0.75f, 0.25f, new UnityEngine.UI.Text[] { gameOverReasonText, survivalTimeText, highScoreText }));
    }

    private IEnumerator FadeInText(float duration, float delay, UnityEngine.UI.Text[] text_array)
    {
        foreach (UnityEngine.UI.Text text in text_array)
        {
            while (text.color.a < 1f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / duration));
                yield return null;
            }

            yield return new WaitForSeconds(delay);
        }
    }

    public IEnumerator FadeOutPanel(float duration, string panelName)
    {
        UnityEngine.UI.Image _panel = GameObject.Find(panelName).GetComponent<UnityEngine.UI.Image>();

        foreach (UnityEngine.UI.RawImage rawImage in _panel.GetComponentsInChildren<RawImage>())
        {
            StartCoroutine(FadeOutRawImage(duration, rawImage));
        }

        while (_panel.color.a > 0f)
        {
            _panel.color = new Color(_panel.color.r, _panel.color.g, _panel.color.b, _panel.color.a - (Time.deltaTime / duration));
            yield return null;
        }
    }
    public IEnumerator FadeOutRawImage(float duration, UnityEngine.UI.RawImage rawImage)
    {
        while (rawImage.color.a > 0f)
        {
            rawImage.color = new Color(rawImage.color.r, rawImage.color.g, rawImage.color.b, rawImage.color.a - (Time.deltaTime / duration));
            yield return null;
        }
    }
}