using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MeleeAttack))]
public class MeleeAtkCustomInspecter : Editor
{
    private MeleeAttack _meleeAttack;
    private Animator _targetAnimator;
    private List<AnimatorState> _states = new();
    private int _selectedStateIndex;
    
    private SerializedProperty _preHeatProperty;
    private SerializedProperty _recoveryProperty;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.LabelField("Attack Animation Config", EditorStyles.boldLabel);

        if (_states.Count == 0)
            return;
        
        var statesNames = _states.Select(state => state.name).ToArray();
        
        // 애니메이션 상태 이름 드랍다운
        _selectedStateIndex = EditorGUILayout.Popup("State Name", _selectedStateIndex, statesNames);
        
        var selectedState = _states[_selectedStateIndex];
        var found = TryFindTransitionExitTime(selectedState.transitions, out var exitTime);

        if (!found)
        {
            EditorGUILayout.HelpBox(
                $"'{selectedState.name}'에 Exit 타임 전환이 없습니다. 애니가 해당 상태에서 멈춥니다. 클립 끝 지점으로 계산합니다.",
                MessageType.Warning);
        }
        var clip = selectedState.motion as AnimationClip;

        if (clip == null)
            return;

        var exitDuration = exitTime * clip.length;
        if (selectedState.speed != 0)
            exitDuration /= selectedState.speed;
        
        CreatePreheatSlider(exitDuration);
        
        var rect = EditorGUILayout.GetControlRect();

        EditorGUI.ProgressBar(
            rect,
            exitDuration > 0f ? _recoveryProperty.floatValue / exitDuration : 0f,
            $"Recovery: {_recoveryProperty.floatValue:F2}s"
        );

        serializedObject.ApplyModifiedProperties();
    }

    private bool TryFindTransitionExitTime(AnimatorStateTransition[] transitions, out float exitTime) 
    {
        foreach (var t in transitions)
        {
            if (t.hasExitTime)
            {
                exitTime = t.exitTime;
                return true;
            }
        }
        
        exitTime = 1f;
        return false;
    }

    private void CreatePreheatSlider(float maxValue)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        
        var v = _preHeatProperty.floatValue;
        
        EditorGUILayout.PrefixLabel("Preheat Time");
        // 슬라이더바 - 즉시반영
        v = GUILayout.HorizontalSlider(v, 0f, maxValue, GUILayout.ExpandWidth(true));
        // 필드 - 지연반영
        v = EditorGUILayout.DelayedFloatField(v, GUILayout.Width(50));

        if (EditorGUI.EndChangeCheck())
        {
            v = Mathf.Clamp(v, 0f, maxValue);
            
            _preHeatProperty.floatValue = v;
            _recoveryProperty.floatValue = maxValue - v;
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void OnEnable()
    {
        _meleeAttack = (MeleeAttack)target;
        _targetAnimator = _meleeAttack.gameObject.GetComponentInChildren<Animator>();

        if (_targetAnimator == null)
        {
            Debug.LogWarning("No Animator component found on " + target.name);
            return;
        }

        _states = AnimatorUtility.GetState(_targetAnimator);

        _preHeatProperty = serializedObject.FindProperty("preheatTime");
        _recoveryProperty = serializedObject.FindProperty("recoveryTime");
    }
}
