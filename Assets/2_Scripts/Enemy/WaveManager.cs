using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private FlowField flowField;
    [SerializeField] private SwarmEnemy enemy;

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
            var e = Instantiate(enemy, transform);
            
            e.transform.position = transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            e.FlowField = flowField;
            e.gameObject.SetActive(true);
        }
    }
}
