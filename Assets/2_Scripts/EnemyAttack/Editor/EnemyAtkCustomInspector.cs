using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(EnemyAttack), true)]
public class EnemyAtkCustomInspector : Editor
{
    private Animator _targetAnimator;
    private List<AnimatorState> _states = new();
    private int _selectedStateIndex;

    private SerializedProperty _hasAnim;
    private SerializedProperty _atkStateName;
    private SerializedProperty _transitionTime;
    private SerializedProperty _preHeatProperty;
    private SerializedProperty _recoveryProperty;
    
    private bool _hasController;
    private bool _hasTargetAnimator;
    private bool _hasFoundAtkStateName;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        if (!_hasAnim.boolValue || !CheckEntry())
        {
            if (_hasController)
            {
                EditorGUILayout.PropertyField(_preHeatProperty);
                EditorGUILayout.PropertyField(_recoveryProperty);
                
                serializedObject.ApplyModifiedProperties();
            }
            else 
                ShowWarning("직접 경로(즉발): 애니·타이밍 모두 사용되지 않습니다");
            
            return;
        }

        if (_states.Count == 0)
        {
            ShowWarning("클립 상태를 찾지 못했습니다 (Override Controller?)");
            return;
        }
        
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Attack Animation Config", EditorStyles.boldLabel);
        
        var statesNames = _states.Select(state => state.name).ToArray();
        
        // ====== 애니메이션 상태 이름 드랍다운 =======
        EditorGUI.BeginChangeCheck();
        _selectedStateIndex = EditorGUILayout.Popup("State Name", _selectedStateIndex, statesNames);

        if (EditorGUI.EndChangeCheck() || (!_hasFoundAtkStateName && string.IsNullOrEmpty(_atkStateName.stringValue)))
        {
            _atkStateName.stringValue = statesNames[_selectedStateIndex];
            _hasFoundAtkStateName = true;
        }

        // ======================================
        
        // ====== 트랜지션 프로퍼티 =======
        EditorGUILayout.PropertyField(_transitionTime);
        // =============================
        
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
        {
            serializedObject.ApplyModifiedProperties();
            return;
        }


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

    /// <summary>
    /// 조율자, 애니메이터, 상태 이름이 존재하는지 확인
    /// </summary>
    private bool CheckEntry()
    {
        if (!_hasController)
        {
            return false;
        }
        
        if (!_hasTargetAnimator)
        {
            ShowWarning("Animator가 없습니다.");
            return false;
        }
        
        if (!_hasFoundAtkStateName)
        {
            ShowWarning($"'{_atkStateName.stringValue}' State를 찾지 못했습니다.");
        }

        return true;
    }
    
    private void OnEnable()
    {
        FindSerializedObjectProperty();
        
        var enemyAtk = (EnemyAttack)target;
        _targetAnimator = enemyAtk.gameObject.GetComponentInChildren<Animator>();
        _hasTargetAnimator = _targetAnimator != null;
        _hasController = enemyAtk.gameObject.TryGetComponent(out EnemyAttackController _);
        
        if (!_hasTargetAnimator)
        {
            return;
        }

        _states = AnimatorUtility.GetState(_targetAnimator);
        FindSavedStateName();
    }

    private void FindSavedStateName()
    {
        _selectedStateIndex = _states.FindIndex(state => state.name == _atkStateName.stringValue);
        _hasFoundAtkStateName = _selectedStateIndex != -1;
        
        if(!_hasFoundAtkStateName)
            _selectedStateIndex = 0;
    }

    private void FindSerializedObjectProperty()
    {
        _hasAnim = serializedObject.FindProperty("hasAttackAnimation");
        _atkStateName = serializedObject.FindProperty("atkStateName");
        _transitionTime = serializedObject.FindProperty("transitionTime");
        _preHeatProperty = serializedObject.FindProperty("preheatTime");
        _recoveryProperty = serializedObject.FindProperty("recoveryTime");
    }
    
    private void ShowWarning(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.Warning);
    }
}
