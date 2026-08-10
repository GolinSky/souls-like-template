using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Animations
{
    public class AnimatorStateMachineReceiver : MonoBehaviour, IAnimatorStateMachineReceiver
    {
        [SerializeField] private Animator animator;
        
        private readonly List<IObserver<AnimatorStateMachineDto>> _observers = new List<IObserver<AnimatorStateMachineDto>>();

        private AnimatorStateMachineDto _animatorStateMachineDto;
        
        public void OnEnable()
        {
            foreach (var behaviour in animator.GetBehaviours<AnimatorStateMachine>())
            {
                behaviour.Initialize(this);
            }
        }
        
        public void OnEnter(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName)
        {
            _animatorStateMachineDto.StateMachineName = stateMachineName;
            _animatorStateMachineDto.StateInfo = stateInfo;
            _animatorStateMachineDto.LayerIndex = layerIndex;
            _animatorStateMachineDto.State = StateMachineState.Enter;
            NotifyObserver();
        }

        public void OnExit(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName)
        {
            _animatorStateMachineDto.StateMachineName = stateMachineName;
            _animatorStateMachineDto.StateInfo = stateInfo;
            _animatorStateMachineDto.LayerIndex = layerIndex;
            _animatorStateMachineDto.State = StateMachineState.Exit;
            NotifyObserver();
        }

        public void OnLoop(int loopIndex, AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName)
        {
            _animatorStateMachineDto.StateMachineName = stateMachineName;
            _animatorStateMachineDto.StateInfo = stateInfo;
            _animatorStateMachineDto.LayerIndex = layerIndex;
            _animatorStateMachineDto.State = StateMachineState.Loop;
            NotifyObserver();
        }

        public void OnQueueCheck(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName)
        {
            _animatorStateMachineDto.StateMachineName = stateMachineName;
            _animatorStateMachineDto.StateInfo = stateInfo;
            _animatorStateMachineDto.LayerIndex = layerIndex;
            _animatorStateMachineDto.State = StateMachineState.QueueCheck;
            NotifyObserver();
        }

        public void OnProgress(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName)
        {
            _animatorStateMachineDto.StateMachineName = stateMachineName;
            _animatorStateMachineDto.StateInfo = stateInfo;
            _animatorStateMachineDto.LayerIndex = layerIndex;
            _animatorStateMachineDto.State = StateMachineState.Progress;
            NotifyObserver();
        }
        
        private void NotifyObserver()
        {
            for (var i = 0; i < _observers.Count; i++)
            {
                _observers[i].UpdateState(_animatorStateMachineDto);
            }
        }

        public void AddObserver(IObserver<AnimatorStateMachineDto> observer)
        {
            if (_observers.Contains(observer))
            {
                Debug.LogError("Trying to add observer, which has already exists in the observer list");
                return;
            }
            _observers.Add(observer);
        }

        public void RemoveObserver(IObserver<AnimatorStateMachineDto> observer)
        {
            if (!_observers.Contains(observer))
            {
                Debug.LogError("Trying to remove observer, which is not exists in the observer list");
                return;
            }
            _observers.Remove(observer);
        }
    }
}
