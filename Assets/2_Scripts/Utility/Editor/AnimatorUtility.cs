using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public static class AnimatorUtility
{
    public static List<AnimatorState> GetState(Animator animator)
    {
        var controller = animator.runtimeAnimatorController as AnimatorController;
        var states = new List<AnimatorState>();

        if (controller == null)
            return null;
        
        foreach (var layer in controller.layers)
        {
            CollectStates(layer.stateMachine, states);
        }

        return states;
    }

    private static void CollectStates(AnimatorStateMachine stateMachine, List<AnimatorState> states)
    {
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            
            if(state.motion is AnimationClip)
                states.Add(childState.state);
        }

        foreach (var chileMachine in stateMachine.stateMachines)
        {
            CollectStates(chileMachine.stateMachine, states);
        }
    }
}
