using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Obstacle : MonoBehaviour
{
    public GameObject explosionPrefab;
    public AudioClip explosionSound;

    public ObstacleManager.ObstacleTypes obstacleType = ObstacleManager.ObstacleTypes.NONE;

    private AudioSource audioSource;
    private Player player;

    private float transformOffset = 0f,
        offsetRate = 0.01f;

    private float offsetThreshold = 0.5f,
        offsetThresholdMin = 0.1f,
        offsetThresholdMax = 1f;

    private float offsetScale = 0.03f;

    private float timeToReach = 0.5f,
        timeToReachMin = 0.4f,
        timeToReachMax = 0.8f;

    private int offsetSwitch = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Border")
        {
            Destroy(gameObject);
        }
        else if (collision.tag == "Player")
        {
            Player playerScript = collision.gameObject.GetComponent<Player>();

            if (!playerScript.OnObstacleCollision())
                return;

            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Destroy(explosion, 2f);
            Destroy(gameObject);
        }
    }

    void Update()
    {
        switch (obstacleType)
        {
            case ObstacleManager.ObstacleTypes.STATIC:
                {
                    gameObject.transform.eulerAngles = new Vector3(0f, 0f, 180f);
                }
                break;
            case ObstacleManager.ObstacleTypes.MOVING:
            case ObstacleManager.ObstacleTypes.MOVING_RANDOM:
                {
                    transformOffset += offsetRate * offsetSwitch;

                    if (Mathf.Abs(transformOffset) >= offsetThreshold)
                    {
                        offsetSwitch *= -1;
                        gameObject.transform.eulerAngles = new Vector3(0f, 0f,
                            180 + (45f * offsetSwitch)
                        );
                    }

                    gameObject.transform.position += new Vector3(
                        transformOffset * offsetScale,
                        0f, 0f
                    );
                }
                break;
            case ObstacleManager.ObstacleTypes.FOLLOWER:
                {
                    if (!player)
                        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
                    else
                    {
                        gameObject.transform.position += new Vector3(
                            (player.transform.position.x - gameObject.transform.position.x) * (Time.deltaTime / timeToReach),
                            0f, 0f
                        );

                        float _rotation = 45 / 2 * (player.transform.position.x - gameObject.transform.position.x);

                        gameObject.transform.eulerAngles = new Vector3(0f, 0f, 180f + _rotation);
                    }

                }
                break;

            default:
                break;
        }
    }

    public void UpdateData()
    {
        switch (obstacleType)
        {
            case ObstacleManager.ObstacleTypes.MOVING_RANDOM:
                {
                    float _cameraDistance = (gameObject.transform.position - Camera.main.transform.position).z;

                    float _leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, _cameraDistance)).x;
                    float _rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, _cameraDistance)).x;

                    offsetThresholdMax = math.min(
                                 math.abs(gameObject.transform.position.x - _leftEdge - 1) / offsetScale,
                                 math.abs(gameObject.transform.position.x - _rightEdge - 1) / offsetScale
                    );

                    offsetThreshold = Random.Range(offsetThresholdMin, offsetThresholdMax);
                }
                break;
            case ObstacleManager.ObstacleTypes.FOLLOWER:
                {
                    timeToReach = Random.Range(timeToReachMin, timeToReachMax);
                }
                break;

            default:
                break;
        }
    }
}
