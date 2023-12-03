using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;

    private Touch touch;
    private Vector2 playerDirection,
        touchStartPosition, touchEndPosition;

    private AudioSource hurtAudio,
        collectHealthAudio,
        collectOxygenAudio;

    public enum loseConditions
    {
        NONE,
        DROWN,
        KILLED
    };

    public loseConditions loseCondition = loseConditions.NONE;

    public float playerSpeed,
        maxOxygen = 100f,  // Valor máximo de oxigênio/combustível.
        currentOxygen,     // Quantidade atual de oxigênio/combustível.
        oxygenConsumptionRate = 2f;  // Taxa de consumo de oxigênio/combustível por segundo.

    public int lives = 3,
        maxLives = 6;

    public bool isInvulnerable = false;

    public SceneMovement cameraScript;
    public GameStateManager gameStateManager;

    public GameObject[] lifeIcons;
    public RectTransform lifeIcons_parentTransform,
        extraLiveIcons_parentTransform;
    public UnityEngine.UI.Slider oxygenSlider;

    public SpriteRenderer spriteRenderer; // Faça isso público para que você possa atribuir no editor do Unity.

    private List<GameObject> playerHudObjects = new List<GameObject>();
    public Material extraLivesMaterial;

    void Start()
    {
        gameStateManager.state = GameStateManager.GameStates.PLAYING;
        currentOxygen = maxOxygen;

        rb = GetComponent<Rigidbody2D>();

        hurtAudio = GameObject.Find("Hurt Audio").GetComponent<AudioSource>();
        collectHealthAudio = GameObject.Find("Collect Health Audio").GetComponent<AudioSource>();
        collectOxygenAudio = GameObject.Find("Collect Oxygen Audio").GetComponent<AudioSource>();

        SetLives(3);
    }

    void Update()
    {
        if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
            return;

        ProcessInput();
        ApplyRotation();
        UpdateConditions();
    }

    private void UpdateConditions()
    {
        loseCondition = loseConditions.NONE;

        oxygenSlider.value = currentOxygen = MathF.Max(currentOxygen - oxygenConsumptionRate * Time.deltaTime, 0f);

        if (lives <= 0) loseCondition = loseConditions.KILLED;
        else if (currentOxygen <= 0f) loseCondition = loseConditions.DROWN;

        if (!loseCondition.Equals(loseConditions.NONE))
            gameStateManager.state = GameStateManager.GameStates.GAME_OVER;
    }

    private void ProcessInput()
    {
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            bool _moveLeft = touch.position.x < Screen.width * 0.5f;
            bool _moveRight = touch.position.x > Screen.width * 0.5f;

            playerDirection = new Vector2(
                _moveLeft ? -1f : _moveRight ? 1f : 0f,
                0f
            );
        }
        else
        {
            // Controles para testes no editor do Unity

            playerDirection = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ).normalized;
        }
    }

    private void ApplyRotation()
    {
        gameObject.transform.eulerAngles = new Vector3(0f, 0f, 90f - 45f * playerDirection.x);
    }

    public void ReplenishOxygen(float amount)
    {
        currentOxygen = MathF.Min(currentOxygen + amount, maxOxygen);
    }

    public bool OnObstacleCollision()
    {
        if (isInvulnerable)
            return false;

        SetLives(lives - 1);

        StartCoroutine(ScreenShake(0.75f, 0.1f));
        StartCoroutine(TimedCollisionEffects());

        hurtAudio.Play();

        return true;
    }

    public bool OnObjectCollision(bool health = false)
    {
        if (isInvulnerable)
            return false;

        collectHealthAudio.time = collectOxygenAudio.time = 0.25f;

        if (health)
            collectHealthAudio.Play();
        else
            collectOxygenAudio.Play();

        return true;
    }

    private IEnumerator TimedCollisionEffects()
    {
        float _cameraSpeed = cameraScript.cameraSpeed;

        isInvulnerable = true;
        cameraScript.SetSpeedMultiplier(0.5f);

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.25f);
        yield return new WaitForSeconds(0.75f);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);

        cameraScript.cameraSpeed = _cameraSpeed;
        isInvulnerable = false;
    }

    public IEnumerator ScreenShake(float duration, float strength)
    {
        Camera _mainCamera = Camera.main;

        float _timeElapsed = 0f;
        while (_timeElapsed < duration)
        {
            if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
                break;

            _mainCamera.transform.localPosition = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                -1f
            );

            _timeElapsed += Time.deltaTime;
            yield return 0;
        }
        _mainCamera.transform.localPosition = new Vector3(0, 0, -1f);
    }

    public void SetLives(int value)
    {
        lives = math.min(value, maxLives);

        updateLives();
    }

    public void Respawn()
    {
        SetHUD(true);

        SetLives(3);
        ReplenishOxygen(maxOxygen);

        transform.localPosition = new Vector3(0f, transform.localPosition.y);
    }

    public void SetHUD(bool state)
    {
        if (playerHudObjects.Count < 1)
            foreach (GameObject __obj in GameObject.FindGameObjectsWithTag("playerHUD"))
                playerHudObjects.Add(__obj);

        foreach (GameObject __obj in playerHudObjects)
        {
            if (__obj.name.Equals("Phase")
                && state)
                continue;

            __obj.SetActive(state);
        }
    }

    private void updateLives()
    {
        int _pendingIcons = math.max(lives - lifeIcons.Length, 0);

        GameObject _firstIcon = lifeIcons[0];
        for (int __index = 0; __index < _pendingIcons; __index++)
        {
            int _iconIndex = lifeIcons.Length;
            bool _isExtraHealth = _iconIndex >= 3;

            GameObject _previousIcon = lifeIcons[lifeIcons.Length - 1];
            GameObject _newIcon = Instantiate(_previousIcon, 
                _isExtraHealth ? extraLiveIcons_parentTransform : lifeIcons_parentTransform
            );

            _newIcon.SetActive(false);
            if (_iconIndex <= 3 && _isExtraHealth)
            {
                _newIcon.transform.localScale = new Vector3(3.3f, 3.3f, 1f);
                _newIcon.transform.localPosition = new Vector3(
                    _firstIcon.transform.localPosition.x,
                    _firstIcon.transform.localPosition.y - 1
                );

                _newIcon.GetComponent<Image>().material = extraLivesMaterial;
                _newIcon.GetComponent<Image>().color = new Color(1f, 0.65f, 0f, 1f);
            }
            else
            {
                _newIcon.transform.localPosition = new Vector3(
                    _previousIcon.transform.localPosition.x + 45f,
                    _previousIcon.transform.localPosition.y
                );
            }

            lifeIcons = lifeIcons.Concat(new[] { _newIcon }).ToArray();
        }

        for (var __lifeIconIndex = 0; __lifeIconIndex < lifeIcons.Length; __lifeIconIndex++)
            lifeIcons[__lifeIconIndex].SetActive(lives > __lifeIconIndex);
    }

    void FixedUpdate()
    {
        float _cameraDistance = (rb.transform.position - Camera.main.transform.position).z;

        float _leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, _cameraDistance)).x;
        float _rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, _cameraDistance)).x;
        float _topEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, _cameraDistance)).y;
        float _bottomEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, _cameraDistance)).y;

        bool _horizontalOutOfBounds = rb.transform.position.x <= _leftEdge && playerDirection.x < 0f ||
            rb.transform.position.x >= _rightEdge && playerDirection.x > 0f;

        bool _verticalOutOfBounds = rb.transform.position.y <= _bottomEdge && playerDirection.y < 0f ||
            rb.transform.position.y >= _topEdge && playerDirection.y > 0f;

        rb.velocity = new Vector2(
            _horizontalOutOfBounds ? 0f : playerDirection.x * playerSpeed,
            _verticalOutOfBounds ? 0f : playerDirection.y * playerSpeed
        );

        rb.transform.position = new Vector3(Mathf.Clamp(rb.transform.position.x, _leftEdge, _rightEdge),
            Mathf.Clamp(rb.transform.position.y, _bottomEdge, _topEdge));
    }
}
