using System.Collections.Generic;
using UnityEngine;

public class FlowField : MonoBehaviour
{
    [SerializeField] private bool drawGizmos = true;
    
    [Header("Components")]
    [SerializeField] private CityGenerator cityGenerator;
    [SerializeField] private Transform playerTf;
    [SerializeField] private Transform blockingObstacleParent;

    [Header("Setting")] 
    [SerializeField, Min(1)] private int sub = 1;

    [Header("LOS")] 
    [SerializeField] private bool losEnable = true;
    [SerializeField, Min(0)] private float losRadius = 15f;

    public static FlowField Instance => _instance;
    private static FlowField _instance;
    
    private CityLayout _layout;
    private int _width = 0;
    private int _height = 0;
    private float _cellSize = 0f;
    private int _losRadiusCell = 1;
    private Node[,] _flowField;

    private Vector2Int _curPlayerCell;
    private Vector3 _originCellPos;
    
    private HashSet<Vector2Int> _obstacleIndex;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("FlowField가 씬에 둘 이상 존재.");
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        if (cityGenerator.CityLayout == null)
        {
            Debug.LogError("CityLayout is NULL");
            _instance = null;
            enabled = false;
            
            return;
        }
        
        _layout = cityGenerator.CityLayout;
        _width = _layout.Width * sub;
        _height = _layout.Height * sub;
        _cellSize = _layout.CellSize / (float)sub;
        _losRadiusCell = Mathf.CeilToInt(losRadius / _cellSize);

        _flowField = new Node[_width, _height];

        for (var x = 0; x < _width; ++x)
        {
            for (var y = 0; y < _height; ++y)
            {
                _flowField[x, y] = new Node(int.MaxValue, Vector2.zero);
            }
        }

        var half = _layout.CellSize * 0.5f;
        // 셀의 피봇을 중심이 아닌 좌하단으로 잡음.
        _originCellPos = _layout.ConvertCellPosToWorld(0,0) - new Vector3(half, 0, half);
        
        GetObstaclePositions();
        SetupFlowField();
    }

    public Vector3 GetSmoothedDirection(Vector3 pos)
    {
        if (_flowField == null) return Vector3.zero;
        
        var cx = (pos.x - _originCellPos.x) / _cellSize - 0.5f;
        var cz = (pos.z - _originCellPos.z) / _cellSize - 0.5f;
        
        // 중심 격자 단위로 표현된 현재 위치
        var gridPos = new Vector2(cx, cz);
        
        // 어느 두 표본인가?
        var x0 = Mathf.FloorToInt(gridPos.x); 
        var y0 = Mathf.FloorToInt(gridPos.y);
        
        // 그 표본 사이에 어디에 위치해있는가?
        var tx = gridPos.x - x0;
        var ty = gridPos.y - y0;

        var sum = Vector3.zero;

        for (var dx = 0; dx <= 1; ++dx)
        {
            for (var dy = 0; dy <= 1; ++dy)
            {
                var x = x0 + dx;
                var y = y0 + dy;
                
                if(!InBounds(x, y)) continue;
                if(!IsPassable(x, y)) continue;
                if(_flowField[x,y].Direction == Vector2.zero) continue;
                
                // 가중치 구하기
                // dx == 0이면 왼쪽 표본. 내가 오른쪽으로 tx 만큼 갔다면, 왼쪽에서 그만큼 멀어진거니 지분은 1-tx.
                var wx = (dx == 0) ? 1f - tx : tx;
                var wy = (dy == 0) ? 1f - ty : ty;
                
                // 최종 가중치
                var w = wx * wy;

                var d = _flowField[x, y].Direction;
                sum += new Vector3(d.x, 0f, d.y).normalized * w;
            }
        }

        if (sum.sqrMagnitude < 1e-6f)
        {
            var v2D = GetCurrentCellDirection(pos);
            return new Vector3(v2D.x, 0f, v2D.y).normalized;
        }

        return sum.normalized;
    }
    
    public Vector2 GetCurrentCellDirection(Vector3 pos)
    {
        if(_flowField == null)  return Vector2.zero;

        var index = WorldToCell(pos);
        
        return (index.x < 0 || index.x >= _width || index.y < 0 || index.y >= _height)
            ? Vector2.zero
            : _flowField[index.x, index.y].Direction;
    }

    public bool IsBlocked(Vector3 worldPos)
    {
        var c = WorldToCell(worldPos);

        if (c.x < 0 || c.x >= _width || c.y < 0 || c.y >= _height) return true;
        return !IsPassable(c.x, c.y);
    }
    
    private bool InBounds(int x, int y) =>  x >= 0 && x < _width && y >= 0 && y < _height;
    
    private void Update()
    {
        var playerIndex = WorldToCell(playerTf.position);

        if (playerIndex != _curPlayerCell)
        {
            _curPlayerCell = playerIndex;
            UpdateFlowField();
        }
    }

    private void SetupFlowField()
    {
        for (var x = 0; x < _width; ++x)
        {
            for (var y = 0; y < _height; ++y)
            {
                if (IsPassable(x, y)) continue;

                for (var i = 0; i < 4; ++i)
                {
                    var dx = _searchDir[i].x;
                    var dy = _searchDir[i].y;

                    var nx = dx + x;
                    var ny = dy + y;

                    if (nx < 0 || nx >= _width || ny < 0 || ny >= _height) continue;
                    if (IsPassable(nx, ny))
                    {
                        _flowField[x, y].Direction = _searchDir[i];
                        break;
                    }
                }
            }
        }
    }

    private void UpdateFlowField()
    {
        ResetNode();
        
        var queue = new Queue<Vector2Int>();
        var passable = new List<Vector2Int>();
        var visited = new bool[_width * _height];
        
        queue.Enqueue(_curPlayerCell);
        visited[_curPlayerCell.x * _height + _curPlayerCell.y] = true;
        _flowField[_curPlayerCell.x, _curPlayerCell.y] = new Node(0, Vector2.zero);
        
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            var curCost = _flowField[node.x, node.y].Cost;
            
            // 4방향 탐색 먼저
            for (var i = 0; i < 4; ++i)
            {
                var sx = node.x + _searchDir[i].x;
                var sy = node.y + _searchDir[i].y;
        
                if (sx < 0 || sx >= _width || sy < 0 || sy >= _height) continue;
                if (visited[sx * _height + sy] || IsPassable(sx, sy) == false) continue;
        
                _flowField[sx, sy].Cost = curCost + 1;
                
                queue.Enqueue(new Vector2Int(sx, sy));
                passable.Add(new Vector2Int(sx, sy));
                visited[sx * _height + sy] = true;
            }
        }
        
        foreach (var pNode in passable)
        {
            var min = _flowField[pNode.x, pNode.y].Cost;
            _flowField[pNode.x, pNode.y].IsInBoundLos = false;
        
            foreach (var s in _searchDir)
            {
                var dx = pNode.x + s.x;
                var dy = pNode.y + s.y;
                
                if(dx < 0 || dx >= _width || dy < 0 || dy >= _height)
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
        
        if(losEnable)
            OverwriteLosDirection(_curPlayerCell);
    }

    private void OverwriteLosDirection(Vector2Int p)
    {
        if (!IsPassable(p.x, p.y)) return;

        var r = _losRadiusCell;
        var powR = r * r;
        var check = Vector2Int.zero;

        for (var dx = -r; dx <= r; dx++)
        {
            for (var dy = -r; dy <= r; dy++)
            {
                if(dx * dx + dy * dy > powR) continue;

                check.x = p.x + dx;
                check.y = p.y + dy;

                if (!InBounds(check.x, check.y)) continue;
                if (!IsPassable(check.x, check.y)) continue;
                if (!HasLineOfSight(p, check)) continue;
                
                _flowField[check.x, check.y].Direction = p - check;
                _flowField[check.x, check.y].IsInBoundLos = true;
            }
        }
    }

    private bool HasLineOfSight(Vector2Int start, Vector2Int end)
    {
        var dx = end.x - start.x;
        var dy = end.y - start.y;
        
        // 전체 이동 칸 수
        var nx = Mathf.Abs(dx);
        var ny =  Mathf.Abs(dy);
        
        // 방향
        var sx = System.Math.Sign(dx);
        var sy = System.Math.Sign(dy);
        
        // 이동한 칸 수
        var ix = 0;
        var iy = 0;

        var cur = start;

        while (ix < nx || iy < ny)
        {
            var tx = (1 + 2 * ix) * ny;
            var ty = (1 + 2 * iy) * nx;

            if (tx < ty)    // 세로벽(우측or좌측)에 도달
            {
                cur.x += sx;
                ix++;
            }
            else if (tx == ty)  // 꼭짓점 이동
            {
                // 꼭짓점 통과: 양 옆 칸 검사 (방향장 대각 규칙과 동일)
                if (!IsPassable(cur.x + sx, cur.y) || !IsPassable(cur.x, cur.y + sy))
                {
                    return false;
                }
                
                cur.x += sx;
                cur.y += sy;

                ix++;
                iy++;
            }
            else                // 가로벽(위or아래)에 도달
            {
                cur.y += sy;
                iy++;
            }

            if (!IsPassable(cur.x, cur.y))
                return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        if (drawGizmos == false || _flowField == null) return;

        for (var x = 0; x < _width; ++x)
        {
            for (var y = 0; y < _height; ++y)
            {
                if (_flowField[x, y].Direction == Vector2.zero) continue;

                // 중앙을 좌하단으로 맞췄으니까, 다시 중앙으로 맞춤.
                var center = _originCellPos + new Vector3((x + 0.5f) * _cellSize, 0f, (y + 0.5f) * _cellSize);
                
                var dir = _flowField[x, y].Direction;
                var worldDir = new Vector3(dir.x, 0f, dir.y).normalized;
                var tip = center + worldDir * (_cellSize * 0.4f);
                
                Gizmos.color = (_flowField[x,y].IsInBoundLos) ? Color.red : Color.cyan;
                Gizmos.DrawLine(center, tip);
                
                var back1 = Quaternion.Euler(0,  150f, 0) * worldDir;
                var back2 = Quaternion.Euler(0, -150f, 0) * worldDir;
                Gizmos.DrawLine(tip, tip + back1 * (_cellSize * 0.15f));
                Gizmos.DrawLine(tip, tip + back2 * (_cellSize * 0.15f));
            }
        }
    }
    
    private Vector2Int WorldToCell(Vector3 worldPos)
    {
        var col = Mathf.FloorToInt((worldPos.x - _originCellPos.x) / _cellSize);
        var row = Mathf.FloorToInt((worldPos.z - _originCellPos.z) / _cellSize);
        
        return new Vector2Int(col, row);
    }

    private bool IsPassable(int x, int y)
    {
        return _layout.Cells[x / sub, y / sub] is ECellType.CatWalk or ECellType.Road
               && !_obstacleIndex.Contains(new Vector2Int(x, y));
    }

    private void ResetNode()
    {
        for (var x = 0; x < _width; ++x)
        {
            for (var y = 0; y < _height; ++y)
            {
                if (!IsPassable(x,y)) continue;
                
                _flowField[x, y].Cost = int.MaxValue;
                _flowField[x, y].Direction = Vector2.zero;
            }
        }
    }

    private void GetObstaclePositions()
    {
        _obstacleIndex = new HashSet<Vector2Int>();

        foreach (Transform child in blockingObstacleParent)
        {
            var b = child.GetComponent<BoxCollider>().bounds;
            var minCell = WorldToCell(b.min);
            var maxCell = WorldToCell(b.max);

            for (var x = minCell.x; x <= maxCell.x; x++)
            {
                for (var y = minCell.y; y <= maxCell.y; y++)
                {
                    _obstacleIndex.Add(new Vector2Int(x, y));
                }
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

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}

public struct Node
{
    public int Cost;
    public Vector2 Direction
    {
        get { return _direction;}
        set
        {
            _direction = value.normalized;
        }
    }
    private Vector2 _direction;
    
    // 테스트 코드 끝나면 삭제
    public bool IsInBoundLos;
    
    public Node(int cost, Vector2 direction)
    {
        Cost = cost;
        _direction = direction.normalized;
        IsInBoundLos = false;
    }
}

