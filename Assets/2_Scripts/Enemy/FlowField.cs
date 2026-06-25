using System;
using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CityGenerator cityGenerator;
    [SerializeField] private Transform playerTf;

    private CityLayout _layout;
    private Node[,] _flowField;

    private Vector2Int _curPlayerCell;
    private Vector3 _originCellPos;

    private void Start()
    {
        if (cityGenerator.CityLayout == null)
        {
            cityGenerator.GenerateCity();
        }
        
        _layout = cityGenerator.CityLayout;
        _flowField = new Node[_layout.Width, _layout.Height];

        for (var x = 0; x < _layout.Width; ++x)
        {
            for (var y = 0; y < _layout.Height; ++y)
            {
                _flowField[x, y] = new Node(int.MaxValue, Vector2Int.zero);
            }
        }
        
        _originCellPos = _layout.ConvertCellPosToWorld(0,0);
    }

    private void Update()
    {
        var playerIndex = FindPlayerIndex();

        if (playerIndex != _curPlayerCell)
        {
            UpdateFlowField();
            _curPlayerCell = playerIndex;
        }
    }

    private void UpdateFlowField()
    {
        ResetNode();
        
        var queue = new Queue<Vector2Int>();
        var passable = new List<Vector2Int>();
        var visited = new bool[_layout.Width, _layout.Height];
        
        queue.Enqueue(_curPlayerCell);
        visited[_curPlayerCell.x, _curPlayerCell.y] = true;
        _flowField[_curPlayerCell.x, _curPlayerCell.y] = new Node(0, Vector2Int.zero);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var curCost = _flowField[node.x, node.y].Cost;
            
            // 4방향 탐색 먼저
            for (var i = 0; i < 4; ++i)
            {
                var sx = node.x + _searchDir[i].x;
                var sy = node.y + _searchDir[i].y;

                if (sx < 0 || sx >= _layout.Width || sy < 0 || sy >= _layout.Height) continue;
                if (visited[sx, sy] || IsPassable(sx, sy) == false) continue;

                _flowField[sx, sy].Cost = curCost + 1;
                
                queue.Enqueue(new Vector2Int(sx, sy));
                passable.Add(new Vector2Int(sx, sy));
                visited[sx, sy] = true;
            }
        }
        
        foreach (var pNode in passable)
        {
            var min = _flowField[pNode.x, pNode.y].Cost;

            foreach (var s in _searchDir)
            {
                var dx = pNode.x + s.x;
                var dy = pNode.y + s.y;
                
                if(dx < 0 || dx >= _layout.Width || dy < 0 || dy >= _layout.Height)
                    continue;

                if (s.x != 0 && s.y != 0)
                {
                    if(!IsPassable(dx, pNode.y) || !IsPassable(pNode.x, dy))
                        continue;
                }

                if (min > _flowField[dx, dy].Cost)
                {
                    min = _flowField[dx, dy].Cost;
                    _flowField[pNode.x, pNode.y].Direction = s;
                }
            }
        }
    }
    
    private Vector2Int FindPlayerIndex()
    {
        var playerPos = playerTf.position;
        
        var col = Mathf.FloorToInt((playerPos.x - _originCellPos.x) / _layout.CellSize);
        var row = Mathf.FloorToInt((playerPos.z - _originCellPos.z) / _layout.CellSize);
        
        return new Vector2Int(col, row);
    }

    private bool IsPassable(int x, int y)
    {
        return _layout.Cells[x, y] is ECellType.CatWalk or ECellType.Road;
    }

    private void ResetNode()
    {
        for (var x = 0; x < _layout.Width; ++x)
        {
            for (var y = 0; y < _layout.Height; ++y)
            {
                _flowField[x, y].Cost = int.MaxValue;
                _flowField[x, y].Direction = Vector2Int.zero;
            }
        }
    }

    private readonly Vector2Int[] _searchDir = new[]
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1), // 상, 하
        new Vector2Int(-1, 0), new Vector2Int(1, 0), // 좌, 우
        new Vector2Int(-1, 1), new Vector2Int(1, 1), // 좌상, 우상
        new Vector2Int(-1, -1), new Vector2Int(1, -1)  // 좌하, 우하
    };
}

public struct Node
{
    public int Cost;
    public Vector2Int Direction;
    
    public Node(int cost, Vector2Int direction)
    {
        Cost = cost;
        Direction = direction;
    }
}

