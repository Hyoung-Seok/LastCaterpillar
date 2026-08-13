using System.Collections.Generic;
using UnityEngine;

public class FieldEnemySpawner : MonoBehaviour
{
    [Header("Spawn Field Enemy")] 
    [SerializeField] private List<FieldEnemy> fieldEnemies;

    [Header("Field Spawn Config")] 
    [SerializeField] private int clusterCount;
    [SerializeField] private float minDistanceClusters;
    [SerializeField, Min(1)] private int minFieldSpawnCount;
    [SerializeField, Min(2)] private int maxFieldSpawnCount;
    [SerializeField] private float radiusScale = 0.4f;
    [SerializeField, Min(1)] private int maxAttemptsCount;
    
    [Header("Enemy Group Transition Config")]
    [SerializeField] private GroupConfig groupConfig;

    private CityLayout _layout;
    private EnemyRegister _enemyRegister;
    private List<Vector2Int> _spawnPoints = new List<Vector2Int>();
    
    private const float GoldenAngleRad = 2.39996f; 
    
    private void Start()
    {
        var cityGenerator = FindAnyObjectByType<CityGenerator>();
        _enemyRegister = FindAnyObjectByType<EnemyRegister>();

        if (cityGenerator == null)
        {
            Debug.LogError($"{nameof(CityGenerator)} could not be found.");
            return;
        }

        if (_enemyRegister == null)
        {
            Debug.LogError($"{nameof(EnemyRegister)} could not be found.");
            return;
        }
        
        _layout = cityGenerator.CityLayout;
        
        if(_layout != null)
            SpawnFieldEnemy();
    }

    private void SpawnFieldEnemy()
    {
        for (var i = 0; i < clusterCount; i++)
        {
            var sp = new Vector2Int();
            var isSuccess = false;

            for (var t = 0; t < maxAttemptsCount; t++)
            {
                if (!TryGetSpawnCellIndex(out sp))
                {
                    Debug.LogWarning("Failed to find spawn point within the specified count");
                    continue;
                }
                
                if (CheckValidSpawnPoint(sp))
                {
                    isSuccess = true;
                    break;
                }
            }

            if (!isSuccess)
            {
                Debug.LogWarning($"클러스터 {i} 배치 실패");
                continue;
            }
            
            _spawnPoints.Add(sp);
            CreateClusters(sp);
        }
    }

    private void CreateClusters(Vector2Int spawnPoint)
    {
        var parent = new GameObject($"Cluster[{spawnPoint.x},{spawnPoint.y}]");
        parent.transform.SetParent(transform);
        
        var pos = CityLayout.ConvertCellPosToWorld(spawnPoint.x, spawnPoint.y, _layout.CellSize);
        var spawnCount = Random.Range(minFieldSpawnCount, maxFieldSpawnCount + 1);
        
        // 여기서 EnemyGroup 만들고 
        var group = new EnemyGroup(groupConfig, Time.time);
        _enemyRegister.RegisterFieldEnemyGroup(group);

        for (var i = 0; i < spawnCount; i++)
        {
            var obj = Instantiate(fieldEnemies[Random.Range(0, fieldEnemies.Count)], parent.transform);
            // 각 FieldEnemy에 EnemyGroup 참조 연결
            obj.Init(group, group.GetRelayDelay(i, spawnCount));

            var angle = i * GoldenAngleRad;
            var radius = Mathf.Sqrt(i) * radiusScale;
            
            var x = Mathf.Cos(angle) * radius;
            var z =  Mathf.Sin(angle) * radius;
            
            obj.transform.position = new Vector3(pos.x + x, 0, pos.z + z);
            obj.gameObject.SetActive(true);
        }
    }

    private bool TryGetSpawnCellIndex(out Vector2Int spawnPoint)
    {
        for (var i = 0; i < maxAttemptsCount; i++)
        {
            var x = Random.Range(0, _layout.Width);
            var y = Random.Range(0, _layout.Height);

            if (_layout.Cells[x, y] == ECellType.Road)
            {
                spawnPoint = new Vector2Int(x, y);
                return true;
            }
        }
        
        spawnPoint = default;
        return false;
    }

    private bool CheckValidSpawnPoint(Vector2Int spawnPoint)
    {
        if (_spawnPoints.Contains(spawnPoint)) return false;

        foreach (var s in _spawnPoints)
        {
            var d = (s - spawnPoint).magnitude;
            
            if (d <= minDistanceClusters)
                return false;
        }
        
        return true;
    }
}