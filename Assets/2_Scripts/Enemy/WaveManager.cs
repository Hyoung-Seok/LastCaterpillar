using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private List<Enemy> enemy;

    [Header("Config")] 
    [SerializeField] private int spawnCount;
    
    public void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        for (var i = 0; i < spawnCount; ++i)
        {
            var index = Random.Range(0, enemy.Count);
            var e = Instantiate(enemy[index], transform);
            
            e.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            e.gameObject.SetActive(true);
        }
    }
}
