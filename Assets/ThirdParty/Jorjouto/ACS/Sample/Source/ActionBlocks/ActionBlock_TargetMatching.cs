using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Jorjouto.AnimComposerSystem.Sample
{
    public enum EMovementType
    {
        SetLocation,
        CharacterControllerMove,
        Rigidbody
    }
    /// <summary>
    /// Represents an action block that plays an audio clip when executed.
    /// Supports both one-shot and looping playback using Unity's <see cref="AudioSource"/>.
    /// </summary>
    [BlockColor("#4b1e00")]
    [System.Serializable]
    public class ActionBlock_TargetMatching : ActionBlock_Base
    {
        #region Fields
        [SerializeField, Tooltip("The type of movement that will be applied over the owner of the animation")]
        private EMovementType MovementType;
        [SerializeField, Tooltip("Normalized root motion curve in the x plane")]
        private AnimationCurve XPositionCurve = AnimationCurve.EaseInOut(0,0,0,0);
        [SerializeField, Tooltip("Normalized root motion curve in the y plane")]
        private AnimationCurve YPositionCurve = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField, Tooltip("Normalized root motion curve in the z plane")]
        private AnimationCurve ZPositionCurve = AnimationCurve.EaseInOut(0,0,1,1);
        [SerializeField, Tooltip("Rotation interpolation curve (0-1)")]
        private AnimationCurve RotationInterpCurve = AnimationCurve.EaseInOut(0,0,0.15f,1);
        [SerializeField, Tooltip("The offset position between the owner and the warp target")]
        private Vector3 OffsetPosition;
        [SerializeField, Tooltip("The offset rotation between the owner and the warp target")]
        private Vector3 OffsetEulerAngles;
        [SerializeField, Tooltip("If this is set to false, the owner of the animation will face the same direction the target is facing, instead of pointing to it")]
        private bool ShouldOrientToMoveDirection;

        [SerializeField, Tooltip("Whether the owner of this animation should snap to the warp target when the ActionBlock ends")]
        private bool ShouldCompleteMatchOnEnd;

        #region Debug

        #if UNITY_EDITOR

        /// <summary>
        /// The preview item referenced as warp target
        /// </summary>
        [SerializeField, Tooltip("The preview item referenced as warp target")]
        private int DebugWarpTargetPreviewItemIndex;

        #endif

        #endregion


        #endregion

        #region Properties

        private bool bWasRootMotionBlocked;
        private float interpValue;
        private AnimCoordinatorComponent animCoordinator;
        private Transform warpTarget;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 finalPosition;
        private Quaternion finalRotation;
        private Vector3 targetDirection;
        private Vector3 targetHorizontalDirection;
        private Vector3 targetPerpendicularDirection;
        private CharacterController characterController;
        private Rigidbody rigidBody;
        private Quaternion moveDirectionRotation;

        #if UNITY_EDITOR

        private Animator debugAnimator;

        #endif

        #endregion

        #region Standard Functions

        /// <summary>
        /// Called when the action block starts execution.
        /// </summary>
        /// <param name="owner">The GameObject that owns this action block.</param>
        /// <param name="startTime">The start time of the action.</param>
        /// <param name="endTime">The end time of the action.</param>
        /// <param name="rate">The rate of execution.</param>
        public override void OnStart(GameObject owner, float startTime, float endTime, float rate)
        {
            animCoordinator = owner.GetComponent<AnimCoordinatorComponent>();

            base.OnStart(owner, startTime, endTime, rate);

            if(!IsActive)
            {
                return;
            }

            warpTarget = animCoordinator.WarpTarget;
            InitializeWarping(Owner);

            bWasRootMotionBlocked = animCoordinator.GetIsRootMotionBlocked();
            animCoordinator.SetRootMotionBlocked(true);

            if(MovementType == EMovementType.CharacterControllerMove)
            {
                characterController = owner.GetComponent<CharacterController>();
            }
            else if(MovementType == EMovementType.Rigidbody)
            {
                rigidBody = owner.GetComponent<Rigidbody>();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            interpValue = Mathf.Clamp01(interpValue + (deltaTime / duration));

            MoveOwner(GetNextPosition());
            Owner.transform.rotation = GetNextRotation();
        }

        /// <summary>
        /// Called when the action block exits execution.
        /// </summary>
        public override void OnExit()
        {
            base.OnExit();

            animCoordinator.SetRootMotionBlocked(bWasRootMotionBlocked);

            if(ShouldCompleteMatchOnEnd)
            {
                interpValue = 1f;
                MoveOwner(GetNextPosition());
                Owner.transform.rotation = GetNextRotation();
            }
        }


        /// <summary>
        /// Checks if the action block can start execution.
        /// </summary>
        /// <returns>True if sounds are available to play, false otherwise.</returns>
        public override bool CheckCanStartActionBlock()
        {
            // Sound to play should not be null
            return  animCoordinator != null && 
                    animCoordinator.WarpTarget != null;
        }

        #endregion

        #region Movement Functions

        private void InitializeWarping(GameObject owner)
        {
            originalPosition = owner.transform.position;
            originalRotation = owner.transform.rotation;
            targetDirection = (warpTarget.transform.position - owner.transform.position).normalized;
            targetHorizontalDirection = new Vector3(targetDirection.x, 0f, targetDirection.z).normalized;
            targetPerpendicularDirection = Vector3.Cross(targetHorizontalDirection, Vector3.up);

            if(ShouldOrientToMoveDirection)
            {
                finalPosition = warpTarget.transform.position + targetHorizontalDirection * OffsetPosition.z 
                                              + targetPerpendicularDirection * OffsetPosition.x
                                              + Vector3.up * OffsetPosition.y;
                                             
            }
            else
            {
                finalPosition = warpTarget.transform.position + warpTarget.transform.forward * OffsetPosition.z 
                                                              + targetPerpendicularDirection * OffsetPosition.x
                                                              + Vector3.up * OffsetPosition.y;
            }
            
            finalRotation = warpTarget.transform.rotation * Quaternion.Euler(OffsetEulerAngles);
            moveDirectionRotation = Quaternion.LookRotation(targetHorizontalDirection, Vector3.up);
            interpValue = 0f;
        }


        private Vector3 GetNextPosition()
        {
            Vector3 nextPosition = Vector3.Lerp(originalPosition, finalPosition, ZPositionCurve.Evaluate(interpValue));
            nextPosition += targetPerpendicularDirection * XPositionCurve.Evaluate(interpValue);
            nextPosition.y = Mathf.Lerp(originalPosition.y, finalPosition.y, YPositionCurve.Evaluate(interpValue));

            return nextPosition;
        }

        private Quaternion GetNextRotation()
        {
            float rotationInterpValue = Mathf.Clamp01(RotationInterpCurve.Evaluate(interpValue));

            if(ShouldOrientToMoveDirection)
            {
                return Quaternion.Lerp(originalRotation, moveDirectionRotation, rotationInterpValue);
            }
            
            return Quaternion.Lerp(originalRotation, finalRotation, rotationInterpValue);
        }

        private void MoveOwner(Vector3 desiredPosition)
        {
            switch(MovementType)
            {
                case EMovementType.CharacterControllerMove:

                    if(characterController != null)
                    {
                        MoveCharacterController(desiredPosition);
                    }
                    else
                    {
                        MoveActor(Owner, desiredPosition);
                    }

                break;

                case EMovementType.Rigidbody:

                    if (rigidBody != null)
                    {
                        MoveRigidBody(desiredPosition);
                    }
                    else
                    {
                        MoveActor(Owner, desiredPosition);
                    }

                break;

                case EMovementType.SetLocation:

                    MoveActor(Owner, desiredPosition);

                break;
            }
        }

        private void MoveCharacterController(Vector3 desiredPosition)
        {
            characterController.Move(desiredPosition - Owner.transform.position);
        }

        private void MoveRigidBody(Vector3 desiredPosition)
        {
            rigidBody.MovePosition(desiredPosition);
        }

        private void MoveActor(GameObject actor, Vector3 desiredPosition)
        {
            actor.transform.position = desiredPosition;
        }

        #endregion

        #region Debug

        #if UNITY_EDITOR

        /// <summary>
        /// Called when starting the action block in debug mode.
        /// Initializes debug-specific fields and calculates duration.
        /// </summary>
        /// <param name="previewRenderUtility">The preview render utility for debugging.</param>
        /// <param name="previewObject">The preview GameObject used in debugging.</param>
        /// <param name="debugAudioSource">The audio source used for debug playback.</param>
        /// <param name="startTime">The debug start time.</param>
        /// <param name="endTime">The debug end time.</param>
        /// <param name="rate">The debug execution rate.</param>
        public override void OnDebugStart(
                                        List<GameObject> previewAttachedItems,
                                        PreviewRenderUtility previewRenderUtility,
                                        GameObject previewObject,
                                        AudioSource debugAudioSource,
                                        float startTime,
                                        float endTime,
                                        float rate)
        {
            base.OnDebugStart(  previewAttachedItems, 
                                previewRenderUtility, 
                                previewObject, 
                                debugAudioSource, 
                                startTime, 
                                endTime, 
                                rate);

            warpTarget = null;
            debugAnimator = previewObject.GetComponent<Animator>();

            if( previewAttachedItems.Count > DebugWarpTargetPreviewItemIndex && 
                previewAttachedItems[DebugWarpTargetPreviewItemIndex] != null)
            {
                warpTarget = previewAttachedItems[DebugWarpTargetPreviewItemIndex].transform;
            }

            if(warpTarget != null)
            {
                debugAnimator.applyRootMotion = false;
                InitializeWarping(previewObject);
            }
        }

        /// <summary>
        /// Called every frame in debug mode.
        /// </summary>
        /// <param name="deltaTime">The time step of the update.</param>
        public override void OnDebugUpdate(float deltaTime)
        {
            base.OnDebugUpdate(deltaTime);

            if(interpValue == 1f)
            {
                debugAnimator.applyRootMotion = true;
                return;
            }

            interpValue = Mathf.Clamp01(interpValue + (deltaTime / (endTime - startTime)));

            MoveActor(previewObject, GetNextPosition());
            previewObject.transform.rotation = GetNextRotation();
        }

        /// <summary>
        /// Called when exiting debug mode.
        /// </summary>
        public override void OnDebugExit()
        {
            base.OnDebugExit();
            
            if(previewObject != null)
            previewObject.transform.position = Vector3.zero;
            previewObject.transform.rotation = Quaternion.identity;
            debugAnimator.applyRootMotion = true;
        }

        #endif

        #endregion
    }
}
