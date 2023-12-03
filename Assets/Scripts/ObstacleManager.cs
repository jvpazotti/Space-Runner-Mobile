using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstacleManager : MonoBehaviour
{
    public GameStateManager gameStateManager;

    public GameObject baseObstacle,
        baseOxygenCanister,
        baseHealthPack;

    public Player player;

    public float minX, maxX,
        minY, maxY,
        timeBetweenSpawn,
        spawnTime,
        oxygenCanisterProbabilty = 0.2f;  // 20% de chance de gerar um cilindro de oxigênio em vez de um obstáculo.;

    public enum ObstacleTypes
    {
        NONE,
        STATIC,
        MOVING,
        MOVING_RANDOM,
        FOLLOWER
    };

    public enum ObjectTypes
    {
        OBSTACLE,
        OXYGEN_CANISTER,
        HEALTH_PACK
    }

    public Dictionary<ObjectTypes, float> objectChances = new Dictionary<ObjectTypes, float>()
    {
        { ObjectTypes.OBSTACLE, 90f },
        { ObjectTypes.OXYGEN_CANISTER, 7.5f },
        { ObjectTypes.HEALTH_PACK, 2.5f }
    };

    public Dictionary<ObstacleTypes, float> obstacleChances = new Dictionary<ObstacleTypes, float>()
    {
        { ObstacleTypes.STATIC, 60f },
        { ObstacleTypes.MOVING, 25f },
        { ObstacleTypes.FOLLOWER, 15f }
    };

    void Update()
    {
        if (!gameStateManager.state.Equals(GameStateManager.GameStates.PLAYING))
        {
            spawnTime = Time.time + timeBetweenSpawn;
            return;
        }

        if (Time.time > spawnTime)
        {
            Spawn();
            spawnTime = Time.time + timeBetweenSpawn;
        }
    }

    void Spawn()
    {
        Vector3 _randomCoordinates = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f
        );

        ObjectTypes _objectType = DetermineObjectType();
        switch (_objectType)
        {
            case ObjectTypes.OBSTACLE:
                {
                    Obstacle _obstacle = Instantiate(baseObstacle,
                            transform.position + _randomCoordinates,
                            transform.rotation).GetComponent<Obstacle>();

                    DetermineObstacleType(_obstacle);
                    _obstacle.UpdateData();
                }
                break;
            case ObjectTypes.OXYGEN_CANISTER:
                {
                    GameObject _object = Instantiate(baseOxygenCanister,
                            transform.position + _randomCoordinates,
                            transform.rotation);
                }
                break;
            case ObjectTypes.HEALTH_PACK:
                {
                    GameObject _object = Instantiate(baseHealthPack,
                            transform.position + _randomCoordinates,
                            transform.rotation);
                }
                break;
        }
    }

    public void SetSpawnRate(float multiplier)
    {
        timeBetweenSpawn /= multiplier; // Dividimos porque quanto menor o tempo entre spawns, mais rápido os obstáculos aparecerão

        timeBetweenSpawn = math.clamp(timeBetweenSpawn, 0.25f, 2f);
    }

    private ObjectTypes DetermineObjectType()
    {
        float _randomValue = Random.Range(0f, 100f);
        foreach (var __object in objectChances.Keys)
        {
            _randomValue -= objectChances[__object];
            if (_randomValue <= 0)
            {
                if (__object.Equals(ObjectTypes.HEALTH_PACK) &&
                    player.lives >= player.maxLives)
                    return ObjectTypes.OXYGEN_CANISTER;

                return __object;
            }
            else continue;
        }

        return ObjectTypes.OBSTACLE;
    }

    private void DetermineObstacleType(Obstacle targetObstacle)
    {
        float _randomValue = Random.Range(0f, 100f);
        foreach (var __obstacle in obstacleChances.Keys)
        {
            _randomValue -= obstacleChances[__obstacle];
            if (_randomValue <= 0)
            {
                targetObstacle.obstacleType = __obstacle;
                break;
            }
            else continue;
        }
    }

}
