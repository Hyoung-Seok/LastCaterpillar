using System;
using System.Collections.Generic;
using UnityEngine;

public class CityAssetLoader
{
    private const string PATH = "Building";
    private Dictionary<(ECellType, Vector2Int), List<GameObject>> _catalog;

    public GameObject GetRandom(ECellType type, Vector2Int size, System.Random rng)
    {
        _catalog ??= LoadAllPrefabs();
        
        if(_catalog.TryGetValue((type, size), out var result) && result.Count > 0)
            return result[rng.Next(result.Count)];

        return null;
    }

    public List<Vector2Int> GetPossibleSize(ECellType type)
    {
        _catalog ??= LoadAllPrefabs();
        var result = new List<Vector2Int>();

        foreach (var keyTuple in _catalog.Keys)
        {
            if (keyTuple.Item1 == type)
            {
                result.Add(keyTuple.Item2);
            }
        }

        return result;
    }
    
    private Dictionary<(ECellType, Vector2Int), List<GameObject>> LoadAllPrefabs()
    {
        var result = new Dictionary<(ECellType, Vector2Int), List<GameObject>>();
        var prefabs = Resources.LoadAll<GameObject>(PATH);

        foreach (var p in prefabs)
        {
            var parts = p.name.Split('_');
            
            if(parts.Length < 2) continue;
            
            if(Enum.TryParse(parts[0], out ECellType cellType) == false) continue;
            if(TryParserSize(parts[1], out var size) == false) continue;
            
            if(result.ContainsKey((cellType, size)) == false)
                result.Add((cellType, size), new List<GameObject>());
            
            result[(cellType, size)].Add(p);
        }
        
        return result;
    }

    private bool TryParserSize(string str, out Vector2Int size)
    {
        size = default;
        var parts = str.ToLowerInvariant().Split('x');

        if (parts.Length != 2) return false;

        if (int.TryParse(parts[0], out var x) == false)
            return false;

        if (int.TryParse(parts[1], out var y) == false)
            return false;
        
        size = new Vector2Int(x, y);
        return true;
    }
}
