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
    [SerializeField] private float radiusScale = 0.5f;
    [SerializeField, Min(1)] private float limitRatio = 1.5f;
    
    private IEnumerator _spawnRoutine;
    private const float GoldenAngleRad = 2.39996f;
    
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
        // 지금은 Wave를 Start()에서 호출. 후에는 특정 조건을 만족해야 스폰시키므로(더 이상 Start에서 호출하지 않으므로)
        // FlowField()의 start가 호출된 뒤에 호출할 수 있도록 임시 조치.
        yield return null;
        
        var spawnerPos = transform.position;
        
        foreach (var waveData in spawnWave)
        {
            var n = 0;
            var totalSpawnCount = waveData.GetAllSpawnCount();
            
            foreach (var data in waveData.Wave)
            {
                var c = 0;
                var spawnCount = data.Count;

                while (c < spawnCount && n < totalSpawnCount * limitRatio)
                {
                    var angle = n * GoldenAngleRad;
                    var radius = Mathf.Sqrt(n) * radiusScale;
                    
                    var x = Mathf.Cos(angle) * radius;
                    var z = Mathf.Sin(angle) * radius;
                    
                    var spawnPos = new Vector3(x + spawnerPos.x, 0, z + spawnerPos.z);

                    if (FlowField.Instance.IsBlocked(spawnPos))
                    {
                        n++;
                        continue;
                    }
                    
                    var e = Instantiate(data.Enemy, transform);
                    
                    e.transform.position = spawnPos;
                    e.gameObject.SetActive(true);

                    n++;
                    c++;
                }

                if (c < spawnCount)
                {
                    Debug.LogWarning("정상적인 스폰 수보다 적게 스폰되었습니다.");
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

    public int GetAllSpawnCount()
    {
        var result = 0;

        foreach (var data in Wave)
        {
            result += data.Count;
        }

        return result;
    }
}

[Serializable]
public struct EnemySpawnData
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private int count;

    public Enemy Enemy => enemy;
    public int Count => count;
}
