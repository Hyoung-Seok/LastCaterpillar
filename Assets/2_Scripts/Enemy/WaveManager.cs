using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [Header("Config")] 
    [SerializeField] private SpawnWave[] spawnWave;
    [SerializeField] private float spawnInterval;
    
    private IEnumerator _spawnRoutine;
    
    public void Start()
    {
        StartSpawn();
    }

    public void StartSpawn()
    {
        _spawnRoutine = StartSpawnCoroutine();
        StartCoroutine(_spawnRoutine);
    }

    private IEnumerator StartSpawnCoroutine()
    {
        for (var i = 0; i < spawnWave.Length; ++i)
        {
            foreach (var spawnData in spawnWave[i].Wave)
            {
                for (var j = 0; j < spawnData.Count; ++j)
                {
                    var e = Instantiate(spawnData.Enemy, transform);
                    
                    e.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                    e.gameObject.SetActive(true);
                }
            }
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}

[Serializable]
public class SpawnWave
{
    public List<EnemySpawnData> Wave;
}

[Serializable]
public struct EnemySpawnData
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private int count;

    public Enemy Enemy => enemy;
    public int Count => count;
}
