using UnityEngine;

[CreateAssetMenu(fileName = "GroupConfig", menuName = "Scriptable Objects/GroupConfig")]
public class GroupConfig : ScriptableObject
{
    [SerializeField, Min(1)] private float idleDurationMinTime;
    [SerializeField, Min(1.1f)] private float idleDurationMaxTime;
    [SerializeField, Min(1)] private float moveDurationMinTime;
    [SerializeField, Min(1.1f)] private float moveDurationMaxTime;
    [SerializeField] private float minDelayTime;
    [SerializeField] private float maxDelayTime;
    [SerializeField, Range(0.01f, 1f)] private float moveSpeedScale = 0.4f;

    [Header("Cluster movement settings")] 
    [SerializeField, Min(0.1f)] private float biasStartDistance = 10f;
    [SerializeField, Min(0.1f)] private float biasFullDistance = 30f;

    public float IdleDurationMinTime => idleDurationMinTime;
    public float IdleDurationMaxTime => idleDurationMaxTime;
    public float MoveDurationMinTime => moveDurationMinTime;
    public float MoveDurationMaxTime => moveDurationMaxTime;
    public float MinDelayTime => minDelayTime;
    public float MaxDelayTime => maxDelayTime;
    public float MoveSpeedScale => moveSpeedScale;
    public float BiasStartDistance => biasStartDistance;
    public float BiasFullDistance => biasFullDistance;
}
