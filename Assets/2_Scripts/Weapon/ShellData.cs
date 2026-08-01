using UnityEngine;

[CreateAssetMenu(fileName = "ShellData", menuName = "Scriptable Objects/ShellData")]
public class ShellData : ScriptableObject
{
    [SerializeField] private Shell shellPrefab;
    [SerializeField] private string shellName;
    [SerializeField] private float blastRadius;
    [SerializeField] private float velocity;
    [SerializeField] private float minFireDistance;

    public Shell ShellPrefab => shellPrefab;
    public string ShellName => shellName;
    public float BlastRadius => blastRadius;
    public float Velocity => velocity;
    public float MinFireDistance => minFireDistance;
}
