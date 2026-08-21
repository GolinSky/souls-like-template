// AnimComposerEditorUI.cs
//
// Author: Jorge Dinares
// Copyright © 2025 Jorge Dinares. All rights reserved.
//
// Description:
// A custom editor for the ScriptableObject_AnimComposer. This script creates an interactive and
// real-time animation preview window. It provides tools for
// animation playback, timeline scrubbing, track management, and debugging.
//

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using System;
using System.Reflection;

namespace Jorjouto.AnimComposerSystem.ACSEditor
{
    /// <summary>
    /// A custom editor for the <see cref="ScriptableObject_AnimComposer"/> class.
    /// This editor creates a real-time animation preview in the Inspector, allowing users to
    /// play, pause, and scrub animations, manage tracks and action blocks, and configure
    /// preview settings.
    /// </summary>
    public class AnimComposerEditorUI
    {
        #region Variables

        #region Defaults

        /// <summary>
        /// The GUID of the UXML layout asset used to build the editor interface.
        /// </summary>
        #if UNITY_2023_2_OR_NEWER
        private const string guid = "3cf3cb3af2849894db27dab966b6cbf2";
        #else
        private const string guid = "3a28e6bec5f85304f8166f19ad1003d9";
        #endif

        /// <summary>
        /// Cached path to the UXML layout asset resolved from the GUID.
        /// </summary>
        private readonly string uxmlPath = AssetDatabase.GUIDToAssetPath(guid);

        /// <summary>
        /// The UXML file used to define the editor's layout.
        /// </summary>
        private readonly VisualTreeAsset m_UXML = null;

        #endregion

        #region Preview

        /// <summary>
        /// Serialized representation of the target object used for property binding.
        /// </summary>
        private readonly SerializedObject serializedObject = null;

        /// <summary>
        /// The GameObject instantiated for animation preview inside the editor.
        /// </summary>
        private GameObject previewObject;

        private PreviewItemsViewElement previewItemsViewElement = null;

        /// <summary>
        /// The animator component attached to the preview object used to evaluate animation clips.
        /// </summary>
        private Animator previewAnimator;

        /// <summary>
        /// Cached array containing all ActionBlock types discovered via reflection.
        /// </summary>
        private Type[] actionBlockTypes;

        /// <summary>
        /// User-friendly names corresponding to the discovered ActionBlock types.
        /// </summary>
        private string[] actionBlockTypeNames;

        /// <summary>
        /// Last registered mouse position when moving inside track. Used for copy paste with key.
        /// </summary>
        private Vector2 lastMousePositionInTrack;

        #endregion

        #region Sound Playback

        /// <summary>
        /// Audio source used to simulate sound playback for debugging ActionBlocks.
        /// </summary>        
        private AudioSource debugAudioSource = null;

        /// <summary>
        /// Hidden GameObject that hosts the debug audio source.
        /// </summary>
        private GameObject debugAudioGO = null;

        #endregion

        #region Animation Playback

        /// <summary>
        /// Playable graph responsible for evaluating the animation preview.
        /// </summary>
        private PlayableGraph playableGraph;

        /// <summary>
        /// Playable instance representing the currently loaded animation clip.
        /// </summary>
        private AnimationClipPlayable currentAnimation;

        /// <summary>
        /// Output node connecting the playable graph to the preview animator.
        /// </summary>
        private AnimationPlayableOutput playableOutput;

        /// <summary>
        /// Indicates whether the preview animation is currently playing.
        /// </summary>
        private bool isPlaying = false;

        /// <summary>
        /// Current playback time of the animation preview in seconds.
        /// </summary>
        private float animationTime = 0f;

        /// <summary>
        /// Playback time recorded during the previous update.
        /// </summary>
        private float lastAnimationTime = 0f;

        /// <summary>
        /// Current animation frame derived from the animation time and frame rate.
        /// </summary>
        private int currentFrame = 0;

        /// <summary>
        /// Frame index recorded during the previous update.
        /// </summary>
        private int previousFrame = 0;

        /// <summary>
        /// Timestamp of the last editor update tick.
        /// </summary>
        private double lastEditorTime = 0f;

        /// <summary>
        /// Time difference between the current and previous editor update.
        /// </summary>
        private float editorDeltaTime = 0f;

        /// <summary>
        /// Delta time applied to animation evaluation, scaled by playback speed.
        /// </summary>
        private float animationDeltaTime = 0f;

        /// <summary>
        /// Last animation time that was evaluated by the playable graph.
        /// Used to prevent redundant evaluations.
        /// </summary>
        private float lastEvaluatedAnimationTime = -1f;

        /// <summary>
        /// Animation clip length in seconds
        /// </summary>
        private float clipLength = 0f;

        /// <summary>
        /// Total frame count calculated from animation length and frame rate.
        /// </summary>
        private int clipFrames = 0;

        /// <summary>
        /// Animation frame rate
        /// </summary>
        private float clipFrameRate = 0f;


        /// <summary>
        /// Reference to the target ScriptableObject containing animation composition data.
        /// </summary>
        private ScriptableObject_AnimComposer animComposer = null;

        #endregion

        #region Visual Elements

        private LayoutButtonsContainerVisualElement layoutButtonsContainer = null;
        private VisualElement configurationParentElement = null;
        private ScrollView configurationPanelScrollView = null;
        private ObjectField animationSelector = null;
        private ObjectField previewModelSelector = null;
        private Toggle previewRootMotionSelector = null;
        private Toggle previewFootIKSelector = null;
        private VisualElement rootMotionFoldout = null;
        public VisualElement Root { get; private set; }= null;
        private Button extractRootMotionCurvesButton = null;
        private PreviewWindowElement previewWindow = null;
        private Button playPauseButton = null;
        private Button stopButton = null;
        private Button removeTrackButton = null;  
        private Button addTrackButton = null;
        private Slider timelineSlider = null;
        private TimelineBar timelineBar = null;
        private VisualElement actionBlockDataContainer = null;
        private VisualElement tracksVisual = null;
        private Label timeValue = null;
        private Label normalizedTimeValue = null;
        private Label frameValue = null;
        private CursorElement customCursor = null;
        private ActionBlockElement selectedActionBlock = null;
        private ActionTrackElement selectedActionTrack = null;
        private readonly HashSet<ActionBlockData> activeActionBlocks = new();
        private readonly HashSet<ActionBlockData> presentActionBlocks = new();
        private readonly List<ActionBlockData> activeActionBlocksToRemove = new();
        private ActionBlockData copiedActionBlock = null;
        private AnimationTrack copiedActionTrack = null;
        private readonly List<ActionTrackElement> actionTrackContainers = new();

        #endregion

        #endregion

        #region Methods

        #region Standard Editor Methods

        /// <summary>
        /// Called when the Inspector GUI needs to be created.
        /// This is the main entry point for building the custom editor layout.
        /// </summary>
        /// <returns>The root <see cref="VisualElement"/> of the Inspector GUI.</returns>
        public AnimComposerEditorUI(ScriptableObject_AnimComposer target, SerializedObject serializedObject)
        {
            m_UXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            this.serializedObject = serializedObject;
            animComposer = target;

            InitializeRoot();
            CollectAllActionBlockTypes();
            CreateAndStoreVisualElements();
            InitializePreview(true);
            BuildActionTracks();
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            Root.schedule.Execute(()=> PerformBindings()).StartingIn(50);
        }

        #endregion

        #region Root Initialization

        /// <summary>
        /// Initializes the root visual element and loads the UXML tree.
        /// </summary>
        private void InitializeRoot()
        {
            Root = new VisualElement();

            m_UXML.CloneTree(Root);
            ScheduleRootTick();
        }

        /// <summary>
        /// Schedules a per-frame update tick for the editor.
        /// This is used to update the animation, UI, and repaint the preview window.
        /// </summary>
        private void ScheduleRootTick()
        {
            Root.schedule.Execute(() =>
            {
                if(animComposer.AnimationClip == null || animComposer.PreviewModel == null)
                {
                    return;
                }

                previewWindow.UpdateManualCamera();

                if (timeValue != null && normalizedTimeValue != null)
                {
                    float normalizedTime = clipLength > 0f ? animationTime / clipLength : 0f;
                    string timeText = $"{animationTime:F2} / {clipLength:F2} sec";
                    string normalizedTimeText = $"{normalizedTime:F2} / 1.00";
                    
                    if (timeValue.text != timeText)
                    {
                        timeValue.text = timeText;
                    }
                    if (normalizedTimeValue.text != normalizedTimeText)
                    {
                        normalizedTimeValue.text = normalizedTimeText;
                    }
                }

                if (frameValue != null)
                {
                    string frameText = $"{currentFrame} ";
                    if (frameValue.text != frameText)
                    {
                        frameValue.text = frameText;
                    }
                }

                if (!timelineBar.IsDragging)
                {
                    timelineBar.SetTime(animationTime);
                }

                if (clipFrames > 2 && isPlaying)
                {
                    UpdatePreview(clipLength, clipFrameRate);
                    UpdateTimelineSlider();
                    animationDeltaTime = Mathf.Max(0.0f, animationTime - lastAnimationTime);

                    if (lastAnimationTime != animationTime)
                    {
                        ExecuteActionBlocksDebugLogic(false);
                    }
                }
                else
                {
                    lastEditorTime = EditorApplication.timeSinceStartup;

                    //If animation is 2 frames or less. Just render last frame as pose.
                    if (clipFrames <= 2)
                    {
                        // Single-frame animation — just force last frame
                        animationTime = clipLength;
                        currentFrame = Mathf.FloorToInt(animationTime * clipFrameRate);

                        if (currentAnimation.IsValid() && playableGraph.IsValid())
                        {
                            currentAnimation.SetTime(animationTime);
                            playableGraph.Evaluate();
                            lastEvaluatedAnimationTime = animationTime;
                        }
                    }
                }

                previewWindow.UpdateAnimationToPreviewTexture(previewObject, animationTime);
                lastAnimationTime = animationTime;

            }).Every(8);
        }

        #endregion

        #region Action Blocks

        /// <summary>
        /// Collects all valid <see cref="ActionBlock_Base"/> types in the project.
        /// </summary>
        private void CollectAllActionBlockTypes()
        {
            
            actionBlockTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ActionBlock_Base)) && !t.IsAbstract)
                .ToArray();

            actionBlockTypeNames = actionBlockTypes
            .Select(t =>
            {
                var attr = t.GetCustomAttribute<ActionSubGroupAttribute>();
                string group = !string.IsNullOrEmpty(attr?.Group) ? attr.Group + "/" : "";
                string name = t.Name.StartsWith("ActionBlock_") ? t.Name["ActionBlock_".Length..] : t.Name;
                return $"{group}{name}";
            })
            .ToArray();
        }

        #endregion

        #region Visual Elements Creation

        /// <summary>
        /// Finds and stores references to all relevant UI elements from the UXML.
        /// </summary>
        private void CreateAndStoreVisualElements()
        {
            configurationParentElement = Root.Q<VisualElement>("ParentVisualElement");
            configurationPanelScrollView = Root.Q<ScrollView>("ConfigurationPanelScrollView");
            layoutButtonsContainer = Root.Q<LayoutButtonsContainerVisualElement>();
            layoutButtonsContainer.Initialize(configurationParentElement, configurationPanelScrollView);

            animationSelector = Root.Q<ObjectField>("AnimationSelector");
            previewModelSelector = Root.Q<ObjectField>("PreviewModelSelector");
            previewItemsViewElement = Root.Q<PreviewItemsViewElement>();
            previewRootMotionSelector = Root.Q<Toggle>("PreviewRootMotionSelector");
            previewFootIKSelector = Root.Q<Toggle>("PreviewFootIKSelector");

            rootMotionFoldout = Root.Q<VisualElement>("RootMotionFoldout");

            rootMotionFoldout.Insert(0, new HelpBox(
                                            "- If CharacterController component present, calls CharacterController.Move\n" +
                                            "- If RigidBody component present, calls RigidBody.MovePosition\n" +
                                            "- Otherwise, calls transform.positon.\n\n" +
                                            "Make root motion scripted by adding an empty OnAnimatorMove on a\n" +
                                            "component of the character mesh. Check the template character for guidance.\n",
                                            HelpBoxMessageType.Info));

            previewWindow = Root.Q<PreviewWindowElement>("PreviewWindowElement");
            previewWindow.Initialize(animComposer);

            playPauseButton = Root.Q<Button>("PlayPauseButton");
            extractRootMotionCurvesButton = Root.Q<Button>("ExtractRootMotionCurvesButton");
            stopButton = Root.Q<Button>("StopButton");
            timelineBar = Root.Q<TimelineBar>("TimelineBar");
            timelineSlider = Root.Q<Slider>("TimelineSlider");
            tracksVisual = Root.Q<VisualElement>("TracksVisual");
            timeValue = Root.Q<Label>("TimeValueVisual");
            normalizedTimeValue = Root.Q<Label>("NormalizedTimeValueVisual");
            frameValue = Root.Q<Label>("FrameValueVisual");
            actionBlockDataContainer = Root.Q<VisualElement>("ActionBlockDataContainer");
            addTrackButton = Root.Q<Button>("AddTrackButton");
            removeTrackButton = Root.Q<Button>("RemoveTrackButton");
            customCursor = Root.Q<CursorElement>("CustomCursor");
        }

        private TreeViewItemData<string> BuildTree(Transform t, ref int nextId)
        {
            int myId = nextId++;
            var children = new List<TreeViewItemData<string>>();
            foreach (Transform child in t)
            {
                children.Add(BuildTree(child, ref nextId));
            }
            return new TreeViewItemData<string>(myId, t.name, children);
        }

        /// <summary>
        /// Clears and rebuilds all action track UI elements based on the data in the <see cref="ScriptableObject_AnimComposer"/>.
        /// </summary>
        private void BuildActionTracks()
        {
            CleanupTrackContainers();
            tracksVisual.Unbind();
            tracksVisual.Clear();

            if (animComposer.Tracks.Count == 0)
            {
                return;
            }

            for (int trackIndex = 0; trackIndex < animComposer.Tracks.Count; trackIndex++)
            {
                var trackData = animComposer.Tracks[trackIndex];

                var trackElement = new ActionTrackElement(
                    serializedObject,
                    actionBlockTypeNames,
                    customCursor,
                    actionBlockDataContainer,
                    trackData,
                    trackIndex
                );

                trackElement.OnActionBlockCopied += CopyActionBlock;
                trackElement.OnActionBlockDeleted += DeleteActionBlock;
                trackElement.OnActionBlockEnabled += EnableActionBlock;
                trackElement.OnActionBlockDisabled += DisableActionBlock;
                trackElement.OnTrackMenuClickDelete += DeleteTrack;
                trackElement.OnTrackMenuClickInsert += InsertNewTrack;
                trackElement.OnTrackMenuClickMoveUp += MoveUpTrack;
                trackElement.OnTrackMenuClickMoveDown += MoveDownTrack;
                trackElement.OnTrackMenuClickCopy += CopyTrack;
                trackElement.OnTrackMenuClickPasteTrack += PasteTrack;

                CreateActionTrackSelectBindings(trackElement);
                CreateActionBlockSelectBinding(trackElement);
                CreateTrackMenuClickPasteBlockBinding(trackElement);
                CreateTrackMenuClickAddBlockBinding(trackElement);

                if (animComposer.AnimationClip != null)
                {
                    trackElement.RebuildActionBlocks(clipLength, clipFrameRate);
                }

                trackElement.RegisterCallback<MouseMoveEvent>(OnMouseMoveInsideTrack);

                actionTrackContainers.Add(trackElement);
                tracksVisual.Add(trackElement);
            }

            EditorUtility.SetDirty(animComposer);
        }

        private void OnMouseMoveInsideTrack(MouseMoveEvent mouseMoveEvent)
        {
            lastMousePositionInTrack = mouseMoveEvent.localMousePosition;
        }

        private void BuildPreviewItemsList()
        {
            previewItemsViewElement.InitializeItemList(previewObject, serializedObject.FindProperty("PreviewItems"));
        }

        /// <summary>
        /// Rebuilds the action blocks on all existing track elements.
        /// </summary>
        private void RebuildAllActionBlocks()
        {
            foreach (ActionTrackElement track in actionTrackContainers)
            {
                track.RebuildActionBlocks(clipLength, clipFrameRate);
            }
        }

        #endregion

        #region Bindings

        /// <summary>
        /// Sets up all event listeners and bindings for the UI elements.
        /// </summary>
        private void PerformBindings()
        {
            CreateGeneralInputBindings();
            CreateTimelineBindings();
            CreateAnimationSelectionBindings();
            CreateModelSelectionBindings();
            CreatePreviewRootMotionToggleBindings();
            CreatePreviewFootIKToggleBindings();
            CreateRootMotionCurvesButtonBindings();
            CreateTimelineSliderBindings();
            CreatePlayPauseButtonBindings();
            CreateStopButtonBindings();
            CreateAddTrackButtonBindings();
            CreateRemoveTrackButtonBindings();
            CreatePreviewItemsListBindings();
            CreatePressedKeyBindingsOnTracksVisual();
        }

        /// <summary>
        /// Sets up bindings for general mouse events on the UI panel.
        /// </summary>
        private void CreateGeneralInputBindings()
        {
            Root.RegisterCallback<MouseDownEvent>(OnUIPanelMouseDown);
            Root.RegisterCallback<MouseMoveEvent>(OnUIPanelMouseMove);
            Root.RegisterCallback<MouseUpEvent>(OnUIPanelMouseUp);
            Root.RegisterCallback<MouseLeaveEvent>(OnUIPanelMouseLeave);
        }

        /// <summary>
        /// Handles mouse down events occurring anywhere on the editor UI panel.
        /// If the left mouse button is pressed and a track is currently selected,
        /// the selection is cleared. This allows users to deselect tracks by
        /// clicking on empty areas of the panel.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="MouseDownEvent"/> containing information about the mouse
        /// button pressed and the pointer position.
        /// </param>
        private void OnUIPanelMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                if (selectedActionTrack != null)
                {
                    selectedActionTrack?.DeselectTrack();
                    selectedActionTrack = null;
                }
            }
        }

        /// <summary>
        /// Handles mouse movement events across the editor UI panel.
        /// This method forwards horizontal mouse movement to the currently selected
        /// action block for dragging or resizing operations and updates the position
        /// of the custom cursor used within the timeline editor.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="MouseMoveEvent"/> containing mouse movement delta
        /// and the current cursor position relative to the UI panel.
        /// </param>
        private void OnUIPanelMouseMove(MouseMoveEvent evt)
        {
            selectedActionBlock?.OnMouseMove(evt.mouseDelta.x);
            customCursor.style.left = evt.localMousePosition.x;
            customCursor.style.top = evt.localMousePosition.y;

            evt.StopPropagation();
        }

        /// <summary>
        /// Handles mouse button release events on the editor UI panel.
        /// When the left mouse button is released and an action block is being
        /// manipulated, the block is notified so it can finalize its drag or
        /// resize operation.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="MouseUpEvent"/> containing the released mouse button
        /// and pointer position.
        /// </param>
        private void OnUIPanelMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0 && selectedActionBlock != null)
            {
                selectedActionBlock.OnMouseUp();
            }

            evt.StopPropagation();
        }

        /// <summary>
        /// Handles the event triggered when the mouse cursor leaves the UI panel.
        /// If an action block is currently being manipulated, the interaction is
        /// safely cancelled to prevent blocks remaining in a dragged state when
        /// the cursor exits the editor area.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="MouseLeaveEvent"/> triggered when the cursor exits
        /// the bounds of the panel.
        /// </param>
        private void OnUIPanelMouseLeave(MouseLeaveEvent evt)
        {
            selectedActionBlock?.ReleaseActionBlock();
            evt.StopPropagation();
        }

        /// <summary>
        /// Sets up bindings for the timeline bar.
        /// </summary>
        private void CreateTimelineBindings() => timelineBar.OnTimeChanged += OnTimelineBarModified;

        /// <summary>
        /// Sets up bindings for the animation clip selector.
        /// </summary>
        private void CreateAnimationSelectionBindings()
        {
            animationSelector.RegisterValueChangedCallback(evt =>
            {
                if(evt.previousValue == null && evt.newValue != null)
                {
                    InitializePreview(true);
                }
                else if(evt.newValue == null || previewObject == null)
                {
                    CleanupAnimation();
                    return;
                }

                UpdateAnimation();
                
                if (previewObject != null)
                {
                    previewObject.transform.position = Vector3.zero;
                }
            });
        }

        /// <summary>
        /// Sets up bindings for the preview model selector.
        /// </summary>
        private void CreateModelSelectionBindings()
        {
            previewModelSelector.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue == null)
                {
                    CleanupAnimation();
                }
                else
                {
                    InitializePreview(true);
                }
            });
        }

        /// <summary>
        /// Sets up bindings for the root motion toggle.
        /// </summary>
        private void CreatePreviewRootMotionToggleBindings()
        {
            previewRootMotionSelector.RegisterValueChangedCallback(evt =>
            {
                if (previewAnimator != null)
                {
                    previewAnimator.applyRootMotion = evt.newValue;
                }
            });
        }

        /// <summary>
        /// Sets up bindings for the preview foot IK toggle.
        /// </summary>
        private void CreatePreviewFootIKToggleBindings()
        {
            previewFootIKSelector.RegisterValueChangedCallback(evt =>
            {
                if (currentAnimation.IsValid())
                {
                    currentAnimation.SetApplyFootIK(evt.newValue);
                }
            });
        }

        /// <summary>
        /// Sets up bindings for the play/pause button.
        /// </summary>
        private void CreatePlayPauseButtonBindings()
        {
            playPauseButton.RegisterCallback<PointerUpEvent>(evt =>
            {
                isPlaying = !isPlaying;

                if (isPlaying && currentAnimation.IsValid() && previewObject != null)
                {
                    lastEditorTime = EditorApplication.timeSinceStartup;
                    playPauseButton.RemoveFromClassList("PlayButton");
                }
                else
                {
                    playPauseButton.AddToClassList("PlayButton");
                }
            });
        }

        /// <summary>
        /// Sets up bindings for the stop button.
        /// </summary>
        private void CreateStopButtonBindings()
        {
            stopButton.clicked += () =>
            {
                playPauseButton.AddToClassList("PlayButton");
                ResetAnimation();
            };
        }

        /// <summary>
        /// Sets up bindings for the "add track" button.
        /// </summary>
        private void CreateAddTrackButtonBindings()
        {
            addTrackButton.clicked += () =>
            {
                if (selectedActionTrack != null && selectedActionTrack.TrackIndex < (animComposer.Tracks.Count - 1))
                {
                    animComposer.Tracks.Insert(selectedActionTrack.TrackIndex + 1, new AnimationTrack
                    {
                        ActionBlocks = new List<ActionBlockData>()
                    });
                }
                else
                {
                    animComposer.Tracks.Add(new AnimationTrack
                    {
                        ActionBlocks = new List<ActionBlockData>()
                    });
                }

                BuildActionTracks();
            };
        }

        /// <summary>
        /// Sets up bindings for the "remove track" button.
        /// </summary>
        private void CreateRemoveTrackButtonBindings()
        {
            removeTrackButton.clicked += () =>
            {
                if (animComposer.Tracks.Count == 0)
                {
                    return;
                }

                int indexToRemove = selectedActionTrack != null ? selectedActionTrack.TrackIndex : animComposer.Tracks.Count - 1;
                CleanupSelectionAndPropertyPanel();
                animComposer.Tracks.RemoveAt(indexToRemove);
                BuildActionTracks();
            };
        }

        /// <summary>
        /// Sets up bindings for the preview items list view.
        /// </summary>
        private void CreatePreviewItemsListBindings()
        {
            previewItemsViewElement.OnPreviewItemChanged += (index, item) => previewWindow.LoadPreviewItems(previewObject);
            previewItemsViewElement.OnAttachSocketAssignedToPreviewItem += previewWindow.OnAttachSocketAssignedToPreviewItem;
            previewItemsViewElement.OnPreviewItemOffsetPositionChanged += previewWindow.UpdatePreviewItemPosition;
            previewItemsViewElement.OnPreviewItemOffsetRotationChanged += previewWindow.UpdatePreviewItemRotation;
            previewItemsViewElement.OnPreviewItemScaleChanged += previewWindow.UpdatePreviewItemScale;
            previewItemsViewElement.OnPreviewItemVisibilityChanged += previewWindow.UpdatePreviewItemVisibility;
        }

        /// <summary>
        /// Sets up bindings for the timeline slider.
        /// </summary>
        private void CreateTimelineSliderBindings()
        {
            timelineSlider.RegisterValueChangedCallback(evt =>
            {
                if (animComposer.AnimationClip != null && animComposer.PreviewModel != null)
                {
                    animationTime = Mathf.Clamp(evt.newValue * clipLength, 0.0f, clipLength);
                    currentFrame = Mathf.FloorToInt(animationTime * clipFrameRate);
                    UpdateAnimationTimeManually();
                }
            });
        }

        /// <summary>
        /// Event handler for when the timeline bar is manually modified by the user.
        /// </summary>
        /// <param name="newTime">The new time in seconds.</param>
        private void OnTimelineBarModified(float newTime)
        {
            if (animComposer == null || animComposer.AnimationClip == null)
            {
                return;
            }

            animationTime = Mathf.Clamp(newTime, 0.0f, clipLength);
            currentFrame = Mathf.FloorToInt(animationTime * clipFrameRate);
            UpdateTimelineSlider();
            UpdateAnimationTimeManually();
        }

        private void UpdateAnimationTimeManually()
        {
            if(currentAnimation.IsValid())
            {
                currentAnimation.SetTime(animationTime);
                playableGraph.Evaluate();
            }

            if (timeValue != null && normalizedTimeValue != null)
            {
                float normalizedTime = clipLength > 0f ? animationTime / clipLength : 0f;
                string timeText = $"{animationTime:F2} / {clipLength:F2} sec";
                string normalizedTimeText = $"{normalizedTime:F2} / 1.00";

                if (timeValue.text != timeText)
                    timeValue.text = timeText;

                if (normalizedTimeValue.text != normalizedTimeText)
                    normalizedTimeValue.text = normalizedTimeText;
            }

            if (frameValue != null)
            {
                string frameText = $"{currentFrame} ";

                if (frameValue.text != frameText)
                    frameValue.text = frameText;
            }

            if (isPlaying == true)
            {
                playPauseButton.AddToClassList("PlayButton");
                isPlaying = false;
                previewWindow.ResetLasRenderedAnimationTime();
            }
            
            animationDeltaTime = Mathf.Max(0.0f, animationTime - lastAnimationTime);

            ExecuteActionBlocksDebugLogic(true);
            lastAnimationTime = animationTime;
        }

        /// <summary>
        /// Sets up bindings for the "Extract Root Motion Curves" button.
        /// </summary>
        private void CreateRootMotionCurvesButtonBindings()
        {
            extractRootMotionCurvesButton.clicked += () =>
            {
                if (animComposer.AnimationClip == null)
                    return;

                // support undo
                Undo.RecordObject(animComposer, "Extract Root Motion Curves");

                animComposer.RootMotionCurves.Clear();
                animComposer.NormalizedRootMotionCurves.Clear();

                var bindings = AnimationUtility.GetCurveBindings(animComposer.AnimationClip);

                float minTime = animComposer.CurvesFrameRange.x / clipFrameRate;
                float maxTime = animComposer.CurvesFrameRange.y / clipFrameRate;
                float timeDelta = maxTime - minTime;

                foreach (var binding in bindings)
                {
                    if (binding.propertyName == "RootT.x" ||
                        binding.propertyName == "RootT.y" ||
                        binding.propertyName == "RootT.z")
                    {
                        var curve = AnimationUtility.GetEditorCurve(animComposer.AnimationClip, binding);
                        var newCurve = new AnimationCurve();
                        var normalizedCurve = new AnimationCurve();

                        float largestKeyValue = 0f;
                        float smallestKeyValue = Mathf.Infinity;
                        float newValue;
                        float newTime;

                        if (curve != null && curve.length > 0)
                        {
                            int curveFactor = 1;

                            if( binding.propertyName == "RootT.x" && animComposer.FlipXCurve ||
                                binding.propertyName == "RootT.y" && animComposer.FlipYCurve ||
                                binding.propertyName == "RootT.z" && animComposer.FlipZCurve)
                            {
                                curveFactor = -1;
                            }

                            float timeStep = maxTime / animComposer.NormalizationSamples;
                            
                            float valueOffset = curve.Evaluate(minTime) * curveFactor;

                            for(float i = minTime; i <= maxTime; i += timeStep)
                            {
                                newValue = (curve.Evaluate(i) * curveFactor) - valueOffset;
                                newTime = i - minTime;
                                newCurve.AddKey(newTime, newValue);
                                largestKeyValue = Mathf.Max(largestKeyValue, newValue);
                                smallestKeyValue = Mathf.Min(smallestKeyValue, newValue);
                            }

                            animComposer.RootMotionCurves.Add(newCurve);


                            if(largestKeyValue > 0f)
                            {
                                for(int i = 0; i < newCurve.length ; i++)
                                {
                                    newValue = newCurve[i].value / largestKeyValue;
                                    newTime = newCurve[i].time / timeDelta;
                                    normalizedCurve.AddKey(newTime, newValue);
                                }
                            }

                            animComposer.NormalizedRootMotionCurves.Add(normalizedCurve);
                        }
                    }
                }

                // ensure the asset/inspector know it changed
                EditorUtility.SetDirty(animComposer);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            };
        }

        #region Action Track and Block Bindings

        /// <summary>
        /// Binds track selected event to functionality that stores currently selected track.
        /// </summary>
        private void CreateActionTrackSelectBindings(ActionTrackElement actionTrack)
        {
            actionTrack.OnActionTrackSelected += SelectActionTrack;
        }

        /// <summary>
        /// Binds block selected event to functionality that stores currently selected block.
        /// </summary>
        private void CreateActionBlockSelectBinding(ActionTrackElement actionTrack)
        {
            actionTrack.OnActionBlockSelected += SelectActionBlock;
        }

        private void SelectActionTrack(ActionTrackElement actionTrack)
        {
            selectedActionBlock?.DeselectBlock();
            selectedActionBlock = null;

            if (selectedActionTrack != null && selectedActionTrack != actionTrack)
            {
                selectedActionTrack.DeselectTrack();
            }

            selectedActionTrack = actionTrack;
        }

        private void SelectActionBlock(ActionBlockElement actionBlock)
        {
            if (selectedActionBlock != null && selectedActionBlock != actionBlock)
            {
                selectedActionBlock.DeselectBlock();
            }

            selectedActionBlock = actionBlock;

            selectedActionTrack?.DeselectTrack();
            selectedActionTrack = null;

            if (actionBlockDataContainer != null)
            {
                actionBlockDataContainer.Unbind();
                actionBlockDataContainer.Clear();
            }

            actionBlock.SelectActionBlock();
        }

        private void CopyActionBlock(ActionBlockElement block)
        {
            ActionBlockData blockToCopy = animComposer.Tracks[block.TrackIndex].ActionBlocks[block.ActionBlockIndex];
            copiedActionBlock = blockToCopy.Clone();
            customCursor.SetHasBlockCopied(true);
        }

        private void DeleteActionBlock(ActionBlockElement actionBlockToDelete)
        {
            int trackIndex = actionBlockToDelete.TrackIndex;
            int blockIndex = actionBlockToDelete.ActionBlockIndex;

            CleanupSelectionAndPropertyPanel();
            animComposer.Tracks[trackIndex].ActionBlocks.RemoveAt(blockIndex);
            BuildActionTracks();
        }

        private void EnableActionBlock(ActionBlockElement actionBlockToEnable)
        {
            actionBlockToEnable.ActionBlockData.IsDisabled = false;
            actionBlockToEnable.style.opacity = 1f; // visually indicate enabled state
        }

        private void DisableActionBlock(ActionBlockElement actionBlockToDisable)
        {
            actionBlockToDisable.ActionBlockData.IsDisabled = true;
            actionBlockToDisable.style.opacity = 0.3f; // visually indicate disabled state
        }

        private void DeleteTrack(ActionTrackElement trackToDelete)
        {
            int trackIndex = trackToDelete.TrackIndex;

            CleanupSelectionAndPropertyPanel();
            animComposer.Tracks.RemoveAt(trackIndex);
            BuildActionTracks();
        }

        private void InsertNewTrack(ActionTrackElement selectedTrack)
        {
            int trackIndex = selectedTrack.TrackIndex;
            AnimationTrack trackToAdd = new()
            {
                ActionBlocks = new List<ActionBlockData>()
            };

            if (trackIndex >= (animComposer.Tracks.Count - 1))
            {
                animComposer.Tracks.Add(trackToAdd);
            }
            else
            {
                animComposer.Tracks.Insert(selectedTrack.TrackIndex + 1, trackToAdd);
            }

            BuildActionTracks();
        }

        private void MoveUpTrack(ActionTrackElement track)
        {
            int trackIndex = track.TrackIndex;

            if (trackIndex == 0)
            {
                return;
            }

            AnimationTrack trackToAdd = new()
            {
                ActionBlocks = animComposer.Tracks[trackIndex]
                .ActionBlocks
                .Select(block => block.Clone())
                .ToList()
            };

            animComposer.Tracks.Insert(trackIndex - 1, trackToAdd);

            CleanupSelectionAndPropertyPanel();
            animComposer.Tracks.RemoveAt(trackIndex + 1);
            BuildActionTracks();
            SelectActionTrack(actionTrackContainers[trackIndex - 1]);
        }

        private void MoveUpBlock(ActionBlockElement block)
        {
            int trackIndex = block.TrackIndex;
            int blockIndex = block.ActionBlockIndex;

            if (trackIndex == 0)
            {
                return;
            }

            ActionBlockData blockCopy = animComposer.Tracks[trackIndex].ActionBlocks[blockIndex].Clone();

            CleanupSelectionAndPropertyPanel();

            animComposer.Tracks[trackIndex - 1].ActionBlocks.Add(blockCopy);
            animComposer.Tracks[trackIndex].ActionBlocks.RemoveAt(blockIndex);

            BuildActionTracks();

            SelectActionBlock(actionTrackContainers[trackIndex - 1].Children().Last() as ActionBlockElement);
        }

        private void MoveDownTrack(ActionTrackElement track)
        {
            int trackIndex = track.TrackIndex;

            if (trackIndex >= (animComposer.Tracks.Count - 1))
            {
                return;
            }

            AnimationTrack trackToAdd = new()
            {
                ActionBlocks = animComposer.Tracks[trackIndex]
                .ActionBlocks
                .Select(block => block.Clone())
                .ToList()
            };

            if (trackIndex >= (animComposer.Tracks.Count - 2))
            {
                animComposer.Tracks.Add(trackToAdd);
            }
            else
            {
                animComposer.Tracks.Insert(trackIndex + 2, trackToAdd);
            }

            CleanupSelectionAndPropertyPanel();
            animComposer.Tracks.RemoveAt(trackIndex);
            BuildActionTracks();
            SelectActionTrack(actionTrackContainers[trackIndex + 1] as ActionTrackElement);
        }

        private void MoveDownBlock(ActionBlockElement block)
        {
            int trackIndex = block.TrackIndex;
            int blockIndex = block.ActionBlockIndex;

            if (trackIndex >= (animComposer.Tracks.Count - 1))
            {
                return;
            }

            ActionBlockData blockCopy = animComposer.Tracks[trackIndex].ActionBlocks[blockIndex].Clone();

            animComposer.Tracks[trackIndex + 1].ActionBlocks.Add(blockCopy);

            CleanupSelectionAndPropertyPanel();
            animComposer.Tracks[trackIndex].ActionBlocks.RemoveAt(blockIndex);

            BuildActionTracks();
            SelectActionBlock(actionTrackContainers[trackIndex + 1].Children().Last() as ActionBlockElement);
        }

        private void CopyTrack(ActionTrackElement trackToCopy)
        {
            copiedActionTrack = new AnimationTrack
            {
                ActionBlocks = animComposer.Tracks[trackToCopy.TrackIndex]
                .ActionBlocks
                .Select(block => block.Clone())
                .ToList()
            };

            customCursor.SetHasTrackCopied(true);
        }

        private void PasteTrack(ActionTrackElement trackToOverride)
        {
            if (copiedActionTrack == null)
            {
                return;
            }

            AnimationTrack trackToAdd = new()
            {
                ActionBlocks = copiedActionTrack
                .ActionBlocks
                .Select(block => block.Clone())
                .ToList()
            };

            animComposer.Tracks[trackToOverride.TrackIndex] = trackToAdd;
            BuildActionTracks();
        }

        private void PasteActionBlock(ActionTrackElement track, Vector2 position)
        {
            int trackIndex = track.TrackIndex;
            Rect timelineRect = timelineBar.contentRect;
            float xPosition = Mathf.Clamp(position.x, 0, timelineRect.width);
            int nearestFrame = Mathf.FloorToInt(xPosition / timelineBar.GetFrameWidth());
            nearestFrame = Mathf.Clamp(nearestFrame, 0, clipFrames - 1);
            int frameLength = copiedActionBlock.EndFrame - copiedActionBlock.StartFrame;
            int end = Mathf.Min(nearestFrame + frameLength, clipFrames);
            if (end <= nearestFrame)
                end = nearestFrame + 1;

            var newBlock = new ActionBlockData
            {
                StartFrame = nearestFrame,
                StartTime = nearestFrame / clipFrameRate,
                EndFrame = end,
                EndTime = Mathf.Min(clipLength, end / clipFrameRate),
                Action = copiedActionBlock.Action.Clone()
            };

            animComposer.Tracks[trackIndex].ActionBlocks.Add(newBlock);
            BuildActionTracks();
        }

        private void CreateTrackMenuClickPasteBlockBinding(ActionTrackElement actionTrack)
        {
            actionTrack.OnTrackMenuClickPasteBlock += PasteActionBlock;
        }

        private void CreateTrackMenuClickAddBlockBinding(ActionTrackElement actionTrack)
        {
            actionTrack.OnTrackMenuClickAddBlock += (track, position, index) =>
            {

                Rect timelineRect = timelineBar.contentRect;
                float xPosition = Mathf.Clamp(position.x, 0, timelineRect.width);
                int nearestFrame = Mathf.FloorToInt(xPosition / timelineBar.GetFrameWidth());
                nearestFrame = Mathf.Clamp(nearestFrame, 0, clipFrames - 1);
                int blockLength = 5;
                int end = Mathf.Min(nearestFrame + blockLength, clipFrames);
                if (end <= nearestFrame)
                    end = nearestFrame + 1;

                var newBlock = new ActionBlockData
                {
                    StartFrame = nearestFrame,
                    StartTime = nearestFrame / clipFrameRate,
                    EndFrame = end,
                    EndTime = Mathf.Min(clipLength, end / clipFrameRate),
                    Action = (ActionBlock_Base)Activator.CreateInstance(actionBlockTypes[index])
                };

                animComposer.Tracks[track.TrackIndex].ActionBlocks.Add(newBlock);
                BuildActionTracks();
            };
        }

        private void CreatePressedKeyBindingsOnTracksVisual()
        {
            tracksVisual.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.C)
                {
                    if (selectedActionTrack != null)
                    {
                        CopyTrack(selectedActionTrack);
                    }
                    else if (selectedActionBlock != null)
                    {
                        CopyActionBlock(selectedActionBlock);
                    }
                }
                else if (evt.keyCode == KeyCode.E)
                {
                    if (selectedActionBlock != null)
                    {
                        if(selectedActionBlock.ActionBlockData.IsDisabled)
                        {
                            EnableActionBlock(selectedActionBlock);
                        }
                        else
                        {
                            DisableActionBlock(selectedActionBlock);
                        }
                    }
                }
                else if (evt.keyCode == KeyCode.Z)
                {
                    selectedActionBlock?.EditScript();
                }
                else if (evt.keyCode == KeyCode.V &&
                        selectedActionTrack != null &&
                        copiedActionBlock != null)
                {
                    PasteActionBlock(selectedActionTrack, lastMousePositionInTrack);
                }
                else if (evt.keyCode == KeyCode.D && selectedActionTrack != null)
                {
                    PasteTrack(selectedActionTrack);
                }
                else if ((evt.keyCode == KeyCode.I ||
                        evt.keyCode == KeyCode.Plus ||
                        evt.keyCode == KeyCode.KeypadPlus) &&
                        selectedActionTrack != null)
                {
                    InsertNewTrack(selectedActionTrack);
                }
                else if (evt.keyCode == KeyCode.Delete ||
                        evt.keyCode == KeyCode.KeypadPeriod ||
                        evt.keyCode == KeyCode.Minus ||
                        evt.keyCode == KeyCode.Backspace ||
                        evt.keyCode == KeyCode.KeypadMinus)
                {
                    if (selectedActionTrack != null)
                    {
                        DeleteTrack(selectedActionTrack);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                    else if (selectedActionBlock != null)
                    {
                        DeleteActionBlock(selectedActionBlock);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                }
                else if (evt.keyCode == KeyCode.W ||
                        evt.keyCode == KeyCode.UpArrow ||
                        evt.keyCode == KeyCode.Keypad8)
                {
                    if (selectedActionTrack != null)
                    {
                        MoveUpTrack(selectedActionTrack);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                    else if (selectedActionBlock != null)
                    {
                        MoveUpBlock(selectedActionBlock);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                }
                else if (evt.keyCode == KeyCode.S ||
                        evt.keyCode == KeyCode.DownArrow ||
                        evt.keyCode == KeyCode.Keypad2)
                {
                    if (selectedActionTrack != null)
                    {
                        MoveDownTrack(selectedActionTrack);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                    else if (selectedActionBlock != null)
                    {
                        MoveDownBlock(selectedActionBlock);
                        customCursor.TryChangeMouseCursor(null, false);
                    }
                }
                evt.StopPropagation();
                tracksVisual.Focus();
            });
        }

        #endregion

        #endregion

        #region Reset Methods

        /// <summary>
        /// Resets the b bar values based on the current animation clip.
        /// </summary>
        private void ResetTimelineValues()
        {
            timelineBar.AnimationLength = clipLength;
            timelineBar.FrameRate = clipFrameRate;
        }

        /// <summary>
        /// Resets the animation playback to the beginning.
        /// </summary>
        private void ResetAnimation()
        {
            animationTime = 0f;
            lastAnimationTime = 0f;
            currentFrame = 0;
            previousFrame = 0;
            animationDeltaTime = 0f;
            isPlaying = false;
            previewWindow.ResetLasRenderedAnimationTime();
            lastEditorTime = EditorApplication.timeSinceStartup;

            if (animComposer.AnimationClip != null)
            {
                currentAnimation.SetTime(animationTime);
                playableGraph.Evaluate();
                lastEvaluatedAnimationTime = animationTime;
                UpdateTimelineSlider();
            }

            ExitAnyActiveActionBlock();
        }

        #endregion

        #region Preview Initialization

        /// <summary>
        /// Ensures the preview object contains a valid <see cref="Animator"/> component
        /// and configures it for use in the editor animation preview.
        /// </summary>
        /// <remarks>
        /// If the preview object does not already contain an animator, one is added
        /// dynamically. The animator is configured to always animate regardless of
        /// visibility and optionally apply root motion depending on the preview
        /// settings defined in the <see cref="ScriptableObject_AnimComposer"/>.
        /// </remarks>
        private void InitializeAnimator()
        {
            previewAnimator = previewObject.GetComponent<Animator>();

            if (previewAnimator == null)
            {
                previewAnimator = previewObject.AddComponent<Animator>();
            }

            previewAnimator.applyRootMotion = animComposer.PreviewRootMotion;
            previewAnimator.stabilizeFeet = false;
            previewAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        /// <summary>
        /// Creates and configures the <see cref="PlayableGraph"/> used to evaluate
        /// animation clips in the preview system.
        /// </summary>
        /// <remarks>
        /// Any existing playable graph is destroyed before creating a new one to
        /// prevent graph leaks in the editor. The graph operates in
        /// <see cref="DirectorUpdateMode.Manual"/> mode so the editor tool can
        /// control animation playback time explicitly.
        /// </remarks>
        private void InitializePlayableGraph()
        {
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
                playableGraph = default;
            }

            playableGraph = PlayableGraph.Create("PreviewGraph");
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", previewAnimator);
        }

        /// <summary>
        /// Creates the temporary audio source used to play sound effects during
        /// action block debugging in the animation preview.
        /// </summary>
        /// <remarks>
        /// The audio source is attached to a hidden GameObject that exists only
        /// within the editor preview environment. If a previous debug audio object
        /// exists, it is destroyed to ensure only one preview audio source is active.
        /// </remarks>
        private void InitializeDebugAudioSource()
        {
            if (debugAudioGO != null)
            {
                Object.DestroyImmediate(debugAudioGO);
            }

            debugAudioGO = new GameObject("PreviewAudioSource") { hideFlags = HideFlags.HideAndDontSave };
            debugAudioSource = debugAudioGO.AddComponent<AudioSource>();
            debugAudioSource.spatialBlend = 0f;
            debugAudioSource.playOnAwake = false;
        }

        /// <summary>
        /// Initializes the animation preview environment for the editor tool.
        /// </summary>
        /// <param name="bResetCamera">
        /// Determines whether the preview camera should be reset to its default
        /// position when the preview model is initialized.
        /// </param>
        /// <remarks>
        /// This method prepares all runtime components required for previewing
        /// animations in the editor, including:
        /// <list type="bullet">
        /// <item>Creating or updating the preview render utility.</item>
        /// <item>Initializing the preview animator.</item>
        /// <item>Building the preview items list.</item>
        /// <item>Creating the animation playable graph.</item>
        /// <item>Setting up the debug audio system.</item>
        /// <item>Starting animation playback.</item>
        /// </list>
        /// The preview is only initialized if both a preview model and an animation
        /// clip are assigned in the <see cref="ScriptableObject_AnimComposer"/>.
        /// </remarks>
        private void InitializePreview(bool bResetCamera)
        {
            CleanupPreview();

            if (animComposer.PreviewModel != null && animComposer.AnimationClip != null)
            {
                previewWindow.UpdatePreviewRenderUtility(ref previewObject, bResetCamera);
                InitializeAnimator();
                BuildPreviewItemsList();       
                InitializePlayableGraph();
                InitializeDebugAudioSource();
                UpdateAnimation();
            }
        }

        #endregion

        #region Preview Update Methods

        /// <summary>
        /// Updates the animation clip used in the playable graph.
        /// </summary>
        private void UpdateAnimation()
        {
            if (!playableGraph.IsValid())
            {
                InitializePreview(false);
                return;
            }

            currentAnimation = AnimationClipPlayable.Create(playableGraph, animComposer.AnimationClip);
            currentAnimation.SetApplyFootIK(animComposer.PreviewFootIK);
            playableOutput.SetSourcePlayable(currentAnimation);

            animationTime = 0f;
            lastAnimationTime = 0f;
            animationDeltaTime = 0f;
            previousFrame = 0;
            currentFrame = 0;
            clipFrameRate = animComposer.AnimationClip.frameRate;
            clipLength = animComposer.AnimationClip.length;
            clipFrames = Mathf.CeilToInt(clipLength * clipFrameRate);
            lastEvaluatedAnimationTime = 0f;
            previewWindow.ResetLasRenderedAnimationTime();

            currentAnimation.SetTime(animationTime);
            playableGraph.Evaluate();
            
            isPlaying = true;
            
            playPauseButton.RemoveFromClassList("PlayButton");

            lastEditorTime = EditorApplication.timeSinceStartup;
            ResetTimelineValues();
            RebuildAllActionBlocks();
            ExitAnyActiveActionBlock();
        }

        /// <summary>
        /// Updates the animation playback time and frame based on editor delta time.
        /// </summary>
        private void UpdatePreview(float clipLength, float clipFrameRate)
        {
            if (previewAnimator == null || !currentAnimation.IsValid() || animComposer.AnimationClip == null || clipFrames < 2)
            {
                return;
            }

            double currentEditorTime = EditorApplication.timeSinceStartup;

            if(isPlaying)
            {
                editorDeltaTime = (float)(currentEditorTime - lastEditorTime);
                animationTime += editorDeltaTime * animComposer.PlayRate;
                animationTime %= clipLength;

                currentFrame = Mathf.FloorToInt(animationTime * clipFrameRate);
            }

            if (previousFrame > currentFrame)
            {
                ExitAnyActiveActionBlock();
            }

            if (!Mathf.Approximately(animationTime, lastEvaluatedAnimationTime))
            {
                currentAnimation.SetTime(animationTime);
                playableGraph.Evaluate();
                lastEvaluatedAnimationTime = animationTime;
            }

            lastEditorTime = currentEditorTime;
        }

        /// <summary>
        /// Updates the timeline slider's value without triggering its change event.
        /// </summary>
        private void UpdateTimelineSlider() => timelineSlider.SetValueWithoutNotify(animationTime / clipLength);

        #endregion

        #region Miscellaneous

        /// <summary>
        /// Returns true if the asset is part of a Unity package (immutable).
        /// </summary>
        public static bool IsInImmutablePackage(Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);

            if (path.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase))
                return true;

            if (path.Contains("PackageCache", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        #endregion

        #region Cleanup

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Dispose();
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        /// <summary>
        /// Called when the editor window is disabled or updated.
        /// Cleans up all preview objects, event handlers, and UI elements to prevent memory leaks.
        /// </summary>
        public void Dispose()
        {
            CleanupTrackContainers();
            CleanupAnimation();

            Root?.Unbind();
            Root?.Clear();
        }

        /// <summary>
        /// Cleans up all settings linked to an animation
        /// </summary>
        private void CleanupAnimation()
        {    
            timelineBar?.SetTime(0.0f, true);

            CleanupPreview();
            CleanupSelectionAndPropertyPanel();
            ExitAnyActiveActionBlock();

            if (playableGraph.IsValid())
            {
                if (playableGraph.IsPlaying())
                {
                    playableGraph.Stop();
                }

                playableGraph.Destroy();
                playableGraph = default;
            }

            if (debugAudioGO != null)
            {
                Object.DestroyImmediate(debugAudioGO);
                debugAudioGO = null;
            }
        }

        /// <summary>
        /// Cleans up all resources related to the preview.
        /// </summary>
        private void CleanupPreview()
        {
            isPlaying = false;
            previewAnimator = null;

            previewWindow?.CleanupPreviewWindow();
            previewObject = null;

            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
                playableGraph = default;
            }

            if (debugAudioGO != null)
            {
                Object.DestroyImmediate(debugAudioGO);
                debugAudioGO = null;
            }

            currentAnimation = default;
        }

        /// <summary>
        /// Cleans up the visual elements for all track containers.
        /// </summary>
        private void CleanupTrackContainers()
        {
            foreach (ActionTrackElement actionTrack in actionTrackContainers)
            {
                actionTrack.CleanupDelegates();
                actionTrack.UnregisterCallback<MouseMoveEvent>(OnMouseMoveInsideTrack);

                #if UNITY_2023_2_OR_NEWER
                actionTrack.ClearBindings();
                #endif
                
                actionTrack.Clear();
            }

            actionTrackContainers.Clear();
        }

        /// <summary>
        /// Deselects all UI elements and clears the property panel.
        /// </summary>
        private void CleanupSelectionAndPropertyPanel()
        {
            selectedActionBlock?.DeselectBlock();
            selectedActionTrack?.DeselectTrack();

            selectedActionBlock = null;
            selectedActionTrack = null;

            if (actionBlockDataContainer != null)
            {
                actionBlockDataContainer.Unbind();
                actionBlockDataContainer.Clear();
                actionBlockDataContainer.style.display = DisplayStyle.None;
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Executes the debug logic for action blocks, triggering events based on the animation's current frame.
        /// </summary>
        private void ExecuteActionBlocksDebugLogic(bool bWasTimeModifiedManually)
        {
            presentActionBlocks.Clear();

            foreach (var track in animComposer.Tracks)
            {
                foreach (var block in track.ActionBlocks)
                {
                    presentActionBlocks.Add(block);

                    if (block.Action == null)
                    {
                        continue;
                    }
                    else if(!block.Action.DebugEnabled || block.IsDisabled)
                    {
                        if(activeActionBlocks.Contains(block))
                        {
                            block.Action.OnDebugExit();
                            activeActionBlocks.Remove(block);
                        }
                        continue;
                    }

                    bool shouldStart = false;

                    if(!activeActionBlocks.Contains(block))
                    {
                        shouldStart =  (previousFrame == 0 && block.StartFrame == 0) ||
                                        (previousFrame < block.StartFrame && currentFrame >= block.StartFrame) ||
                                        (previousFrame > currentFrame && currentFrame >= block.StartFrame && currentFrame <= block.EndFrame);
                    }

                    if (shouldStart)
                    {
                        block.Action.OnDebugStart(
                            previewWindow.PreviewAttachedItems,
                            previewWindow.PreviewRenderUtility,
                            previewObject,
                            debugAudioSource,
                            block.StartTime,
                            block.EndTime,
                            animComposer.PlayRate
                        );

                        block.Action.OnDebugUpdate(animationDeltaTime);
                        activeActionBlocks.Add(block);
                    }
                    else if (currentFrame >= block.StartFrame &&
                            (currentFrame <= block.EndFrame || 
                            (!bWasTimeModifiedManually && block.Action.DebugExitOnAnimationEnd)))
                    {
                        block.Action.OnDebugUpdate(animationDeltaTime);
                    }
                    else if ((currentFrame > block.EndFrame || currentFrame < block.StartFrame) &&
                            activeActionBlocks.Contains(block))
                    {
                        block.Action.OnDebugExit();
                        activeActionBlocks.Remove(block);
                    }
                }
            }

            // Remove any lingering active blocks that no longer exist or are disabled
            activeActionBlocksToRemove.Clear();
            foreach (var block in activeActionBlocks)
            {
                if (block == null || !block.Action.DebugEnabled || !presentActionBlocks.Contains(block))
                {
                    activeActionBlocksToRemove.Add(block);
                }
            }

            foreach (var block in activeActionBlocksToRemove)
            {
                block?.Action.OnDebugExit();
                activeActionBlocks.Remove(block);
            }

            previousFrame = currentFrame;
        }

        /// <summary>
        /// Forces all currently active action blocks to exit their debug state.
        /// </summary>
        /// <remarks>
        /// This method iterates through all action blocks that are currently
        /// registered as active during animation preview playback and invokes
        /// their <c>OnDebugExit</c> method. This ensures any temporary state,
        /// preview effects, or debug logic started by the block is properly
        /// terminated.
        /// <para>
        /// Blocks that are null or have debugging disabled are ignored.
        /// </para>
        /// <para>
        /// After processing, the active action block collection is cleared to
        /// ensure no stale references remain in the preview system.
        /// </para>
        /// </remarks>
        private void ExitAnyActiveActionBlock()
        {
            foreach (var block in activeActionBlocks)
            {
                if (block == null)
                {
                    continue;
                }

                block.Action.OnDebugExit();
            }

            activeActionBlocks.Clear();
        }

        #endregion

        #endregion
    }
}