#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoulsLike.Components.Visibility;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace SoulsLike.Editor
{
    public static class EnemyAuthoringValidator
    {
        private const string MENU_PATH = "Tools/SoulsLike/Validate Enemy Authoring";
        private const string BASE_LAYER_NAME = "Base Layer";

        private static readonly string[] REACTION_TRIGGER_NAMES =
        {
            "HitFront",
            "HitBack",
            "HitLeft",
            "HitRight",
            "Blocked",
            "GuardBroken",
            "Parried",
            "PoiseStaggered",
            "StanceBroken"
        };

        private static readonly string[] CRITICAL_VICTIM_STATE_NAMES =
        {
            "CriticalHitOneHand",
            "CriticalHitOneHandDie",
            "CriticalHitTwoHand",
            "CriticalHitTwoHandDie"
        };

        private static readonly string[] CRITICAL_TRIGGER_NAMES =
        {
            "CriticalHitOneHand",
            "CriticalHitOneHandDie",
            "CriticalHitTwoHand",
            "CriticalHitTwoHandDie",
            "GetUp"
        };

        [MenuItem(MENU_PATH)]
        public static void Validate()
        {
            var report = new ValidationReport();
            Dictionary<string, WeaponMovesetDefinition> movesets = FindAssets<WeaponMovesetDefinition>();
            Dictionary<string, EnemyBehaviourProfile> profiles = FindAssets<EnemyBehaviourProfile>();
            Dictionary<string, HealthData> healthData = FindAssets<HealthData>();

            foreach ((string path, WeaponMovesetDefinition moveset) in movesets)
            {
                ValidateMoveset(moveset, path, report);
            }

            foreach ((string path, EnemyBehaviourProfile profile) in profiles)
            {
                ValidateProfile(profile, path, report);
            }

            ValidateEnemyPrefabs(report);
            ValidateEncounterPrefabs(movesets, profiles, healthData, report);
            ValidateEncounterScenes(movesets, profiles, healthData, report);
            ValidateCriticalCompletionCallback(report);
            report.LogSummary(movesets.Count, profiles.Count);
        }

        private static Dictionary<string, TAsset> FindAssets<TAsset>()
            where TAsset : UnityEngine.Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(TAsset).Name}", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToDictionary(
                    path => path,
                    AssetDatabase.LoadAssetAtPath<TAsset>);
        }

        private static void ValidateEnemyPrefabs(ValidationReport report)
        {
            foreach (string path in FindPrefabPaths())
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    EnemyActor actor = prefabContents.GetComponent<EnemyActor>();
                    if (actor == null)
                    {
                        continue;
                    }

                    report.EnemyPrefabCount++;
                    ValidateEnemyPrefab(prefabContents, actor, path, report);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabContents);
                }
            }
        }

        private static void ValidateEnemyPrefab(
            GameObject prefab,
            EnemyActor actor,
            string path,
            ValidationReport report)
        {
            string label = $"Enemy prefab '{path}'";
            RequireComponent<ViewEntity>(prefab, label, report);
            RequireComponent<HealthComponent>(prefab, label, report);
            RequireComponent<CombatDefenseComponent>(prefab, label, report);
            RequireComponent<VisibilityComponent>(prefab, label, report);
            RequireComponent<EnemyHealthUiComponent>(prefab, label, report);
            EnemyNavigationMotor motor = RequireComponent<EnemyNavigationMotor>(
                prefab,
                label,
                report);
            RequireComponentInChildren<TargetLockNode>(prefab, label, report);
            EnemyActionExecutor executor = RequireComponentInChildren<EnemyActionExecutor>(
                prefab,
                label,
                report);
            MeleeHitboxController meleeHitbox = RequireComponentInChildren<MeleeHitboxController>(
                prefab,
                label,
                report);

            SerializedObject actorSerialized = new(actor);
            Animator actorAnimator = GetObjectReference<Animator>(actorSerialized, "animator");
            MeleeHitboxController actorHitbox = GetObjectReference<MeleeHitboxController>(
                actorSerialized,
                "meleeHitbox");
            NavMeshAgent actorAgent = GetObjectReference<NavMeshAgent>(
                actorSerialized,
                "navMeshAgent");
            RequireReference(actorAnimator, "Animator", label, report);
            RequireReference(actorHitbox, "MeleeHitbox", label, report);
            RequireReference(actorAgent, "NavMeshAgent", label, report);

            if (actorHitbox != null && meleeHitbox != actorHitbox)
            {
                report.Error($"{label} actor MeleeHitbox reference does not match its child hitbox.", actor);
            }

            if (actorAgent != null && actorAgent.GetComponent<EnemyActor>() != actor)
            {
                report.Error($"{label} actor NavMeshAgent must belong to the actor GameObject.", actor);
            }

            ValidateMotorReferences(motor, actor, actorAgent, label, report);
            ValidateAnimationReferences(executor, actor, motor, actorAnimator, actorHitbox, label, report);
            ValidateHitboxReferences(meleeHitbox, label, report);
        }

        private static void ValidateMotorReferences(
            EnemyNavigationMotor motor,
            EnemyActor actor,
            NavMeshAgent actorAgent,
            string label,
            ValidationReport report)
        {
            if (motor == null)
            {
                return;
            }

            SerializedObject serialized = new(motor);
            NavMeshAgent agent = GetObjectReference<NavMeshAgent>(serialized, "agent");
            CharacterController controller = GetObjectReference<CharacterController>(
                serialized,
                "controller");
            RequireReference(agent, "EnemyNavigationMotor.agent", label, report);
            RequireReference(controller, "EnemyNavigationMotor.controller", label, report);

            if (agent != null && actorAgent != null && agent != actorAgent)
            {
                report.Error($"{label} motor and actor must reference the same NavMeshAgent.", motor);
            }

            if (controller != null && controller.GetComponent<EnemyActor>() != actor)
            {
                report.Error($"{label} motor CharacterController must belong to the actor GameObject.", motor);
            }
        }

        private static void ValidateAnimationReferences(
            EnemyActionExecutor executor,
            EnemyActor actor,
            EnemyNavigationMotor motor,
            Animator actorAnimator,
            MeleeHitboxController actorHitbox,
            string label,
            ValidationReport report)
        {
            if (executor == null)
            {
                return;
            }

            SerializedObject serialized = new(executor);
            Animator animator = GetObjectReference<Animator>(serialized, "animator");
            EnemyNavigationMotor animationMotor = GetObjectReference<EnemyNavigationMotor>(
                serialized,
                "motor");
            EnemyActor animationActor = GetObjectReference<EnemyActor>(serialized, "actor");
            MeleeHitboxController animationHitbox = GetObjectReference<MeleeHitboxController>(
                serialized,
                "meleeHitbox");
            RequireReference(animator, "EnemyActionExecutor.animator", label, report);
            RequireReference(animationMotor, "EnemyActionExecutor.motor", label, report);
            RequireReference(animationActor, "EnemyActionExecutor.actor", label, report);
            RequireReference(animationHitbox, "EnemyActionExecutor.meleeHitbox", label, report);

            if (animator != null && actorAnimator != null && animator != actorAnimator)
            {
                report.Error($"{label} actor and action executor must reference the same Animator.", executor);
            }

            if (animationMotor != null && motor != null && animationMotor != motor)
            {
                report.Error($"{label} action executor must reference the actor motor.", executor);
            }

            if (animationActor != null && animationActor != actor)
            {
                report.Error($"{label} action executor must reference its owning actor.", executor);
            }

            if (animationHitbox != null && actorHitbox != null && animationHitbox != actorHitbox)
            {
                report.Error($"{label} action executor must reference the actor hitbox.", executor);
            }
        }

        private static void ValidateHitboxReferences(
            MeleeHitboxController hitbox,
            string label,
            ValidationReport report)
        {
            if (hitbox == null)
            {
                return;
            }

            Collider collider = GetObjectReference<Collider>(new SerializedObject(hitbox), "hitbox");
            RequireReference(collider, "MeleeHitboxController.hitbox", label, report);
            if (hitbox.GetComponentInParent<EnemyActor>() == null)
            {
                report.Error($"{label} hitbox must resolve its owner actor through the parent hierarchy.", hitbox);
            }
        }

        private static void ValidateMoveset(
            WeaponMovesetDefinition moveset,
            string path,
            ValidationReport report)
        {
            string label = $"Moveset '{path}'";
            if (moveset.WeaponId.ToString() == "None")
            {
                report.Error($"{label} has no weapon ID.", moveset);
            }

            RuntimeAnimatorController runtimeController = moveset.AnimatorController;
            RequireReference(runtimeController, "AnimatorController", label, report);
            if (runtimeController == null)
            {
                return;
            }

            if (runtimeController is not AnimatorController controller)
            {
                report.Error($"{label} uses '{runtimeController.GetType().Name}', which cannot be inspected as an AnimatorController.", moveset);
                return;
            }

            if (moveset.Moves.Length == 0)
            {
                report.Error($"{label} has no moves. Populate the 'moves' field with authored EnemyMove entries.", moveset);
            }

            foreach (EnemyMove move in moveset.Moves)
            {
                if (move == null)
                {
                    report.Error($"{label} contains a null move entry.", moveset);
                    continue;
                }

                if (move.Action == null)
                {
                    report.Error($"{label} has a move without an action reference.", moveset);
                    continue;
                }

                if (move.Action.ActionId == CharacterActionId.Death
                    || move.Action == moveset.DeathAction)
                {
                    report.Error($"{label} normal moves cannot use the Death action.", moveset);
                    continue;
                }

                ValidateAction(controller, move.Action, label, report);
            }

            if (moveset.DeathAction == null)
            {
                report.Error($"{label} is missing its explicit 'deathAction' reference.", moveset);
            }
            else
            {
                if (moveset.DeathAction.ActionId != CharacterActionId.Death)
                {
                    report.Error($"{label} deathAction must use the Death action ID.", moveset);
                }

                ValidateAction(controller, moveset.DeathAction, label, report);
            }

            foreach (EnemyMove move in moveset.Moves)
            {
                if (move?.Action != null
                    && move.Action.ActionId != CharacterActionId.Death
                    && move.Action != moveset.DeathAction)
                {
                    ValidateFollowUps(move, moveset.Moves, label, report);
                }
            }

            ValidateReactionStates(controller, label, report);
            ValidateCriticalAndDeathStates(controller, moveset.DeathAction, label, report);
        }

        private static void ValidateProfile(
            EnemyBehaviourProfile profile,
            string path,
            ValidationReport report)
        {
            if (!System.Enum.IsDefined(
                    typeof(EnemyActivationMode),
                    profile.ActivationMode))
            {
                report.Error(
                    $"Behaviour profile '{path}' has an invalid activation mode.",
                    profile);
            }

            var serialized = new SerializedObject(profile);
            ValidateNonNegativeProfileFields(
                serialized,
                path,
                profile,
                report,
                "closeAwarenessRange",
                "sightConfirmationSeconds",
                "sightForgetSeconds",
                "soundForgetSeconds",
                "damageForgetSeconds",
                "allyForgetSeconds",
                "softLeashDistance",
                "hardLeashDistance",
                "returnHysteresis",
                "searchPointRadius",
                "searchPauseSeconds",
                "decisionJitterSeconds",
                "firstAttackHesitationMin",
                "firstAttackHesitationMax");

            SerializedProperty verticalFieldOfView = serialized.FindProperty("verticalFieldOfView");
            if (verticalFieldOfView == null
                || verticalFieldOfView.floatValue < 0f
                || verticalFieldOfView.floatValue > 360f)
            {
                report.Error(
                    $"Behaviour profile '{path}' requires verticalFieldOfView between 0 and 360.",
                    profile);
            }

            SerializedProperty searchPointCount = serialized.FindProperty("searchPointCount");
            if (searchPointCount == null
                || searchPointCount.intValue < 0
                || searchPointCount.intValue > 2)
            {
                report.Error(
                    $"Behaviour profile '{path}' requires searchPointCount between 0 and 2.",
                    profile);
            }

            SerializedProperty softLeash = serialized.FindProperty("softLeashDistance");
            SerializedProperty hardLeash = serialized.FindProperty("hardLeashDistance");
            if (softLeash != null && hardLeash != null
                && hardLeash.floatValue < softLeash.floatValue)
            {
                report.Error(
                    $"Behaviour profile '{path}' requires hardLeashDistance to be at least softLeashDistance.",
                    profile);
            }

            SerializedProperty firstAttackHesitationMin = serialized.FindProperty(
                "firstAttackHesitationMin");
            SerializedProperty firstAttackHesitationMax = serialized.FindProperty(
                "firstAttackHesitationMax");
            if (firstAttackHesitationMin != null && firstAttackHesitationMax != null
                && firstAttackHesitationMax.floatValue < firstAttackHesitationMin.floatValue)
            {
                report.Error(
                    $"Behaviour profile '{path}' requires firstAttackHesitationMax to be at least firstAttackHesitationMin.",
                    profile);
            }

            if (serialized.FindProperty("sharesAllyAlerts") == null
                || serialized.FindProperty("usesPressureSlot") == null)
            {
                report.Error(
                    $"Behaviour profile '{path}' must serialize ally-alert and pressure-slot settings.",
                    profile);
            }
        }

        private static void ValidateNonNegativeProfileFields(
            SerializedObject serialized,
            string path,
            EnemyBehaviourProfile profile,
            ValidationReport report,
            params string[] fieldNames)
        {
            foreach (string fieldName in fieldNames)
            {
                SerializedProperty field = serialized.FindProperty(fieldName);
                if (field == null || field.floatValue < 0f)
                {
                    report.Error(
                        $"Behaviour profile '{path}' requires a non-negative {fieldName}.",
                        profile);
                }
            }
        }

        private static void ValidateAction(
            AnimatorController controller,
            CharacterActionDefinition action,
            string label,
            ValidationReport report)
        {
            AnimatorState state = GetState(controller, action.ActionId.ToString());
            if (state == null)
            {
                report.Error($"{label} action '{action.ActionId}' has no matching Animator state.", controller);
                return;
            }

            if (state.motion == null)
            {
                report.Error($"{label} action state '{action.ActionId}' has no motion.", state);
            }

            if (!HasActionBehaviour(state, action.ActionId))
            {
                report.Error($"{label} action state '{action.ActionId}' is missing an {nameof(EnemyActionStateBehaviour)} with matching action ID.", state);
            }
            else
            {
                ValidateTrackingTiming(state, action.ActionId, label, report);
            }

            if (!HasOutgoingTransition(state))
            {
                report.Error($"{label} action state '{action.ActionId}' has no completion transition.", state);
            }
        }

        private static void ValidateFollowUps(
            EnemyMove move,
            IReadOnlyList<EnemyMove> moves,
            string label,
            ValidationReport report)
        {
            foreach (CharacterActionDefinition followUp in move.Action.FollowUps)
            {
                if (followUp == null)
                {
                    report.Error($"{label} action '{move.Action.ActionId}' has a null follow-up.", move.Action);
                    continue;
                }

                if (!moves.Any(candidate => candidate != null
                    && candidate.MoveUsage != EnemyMove.Usage.Opener
                    && candidate.Action == followUp))
                {
                    report.Error($"{label} action '{move.Action.ActionId}' follow-up '{followUp.name}' has no matching FollowUp or Any move entry.", move.Action);
                }
            }
        }

        private static void ValidateTrackingTiming(
            AnimatorState state,
            CharacterActionId actionId,
            string label,
            ValidationReport report)
        {
            foreach (EnemyActionStateBehaviour behaviour in state.behaviours
                         .OfType<EnemyActionStateBehaviour>())
            {
                var serialized = new SerializedObject(behaviour);
                SerializedProperty behaviourActionId = serialized.FindProperty("actionId");
                if (behaviourActionId == null || behaviourActionId.intValue != (int)actionId)
                {
                    continue;
                }

                SerializedProperty hasTrackingWindow = serialized.FindProperty("hasTrackingWindow");
                SerializedProperty trackingEnd = serialized.FindProperty("trackingEnd");
                SerializedProperty recoveryStart = serialized.FindProperty("recoveryStart");
                if (hasTrackingWindow == null || trackingEnd == null || recoveryStart == null
                    || trackingEnd.floatValue < 0f
                    || trackingEnd.floatValue > 1f
                    || recoveryStart.floatValue < 0f
                    || recoveryStart.floatValue > 1f)
                {
                    report.Error(
                        $"{label} action state '{actionId}' requires valid tracking timing fields.",
                        behaviour);
                    continue;
                }

                if (hasTrackingWindow.boolValue
                    && trackingEnd.floatValue > recoveryStart.floatValue)
                {
                    report.Error(
                        $"{label} action state '{actionId}' trackingEnd must not exceed recoveryStart.",
                        behaviour);
                }
            }
        }

        private static void ValidateReactionStates(
            AnimatorController controller,
            string label,
            ValidationReport report)
        {
            foreach (string triggerName in REACTION_TRIGGER_NAMES)
            {
                if (!HasTrigger(controller, triggerName))
                {
                    report.Error($"{label} is missing reaction trigger '{triggerName}'.", controller);
                }

                AnimatorState state = GetState(controller, triggerName);
                if (state == null)
                {
                    report.Error($"{label} is missing reaction state '{triggerName}'.", controller);
                    continue;
                }

                if (!state.behaviours.OfType<EnemyHitStateBehaviour>().Any())
                {
                    report.Error($"{label} reaction state '{triggerName}' is missing {nameof(EnemyHitStateBehaviour)}.", state);
                }

                if (!HasOutgoingTransition(state))
                {
                    report.Error($"{label} reaction state '{triggerName}' has no exit transition.", state);
                }
            }
        }

        private static void ValidateCriticalAndDeathStates(
            AnimatorController controller,
            CharacterActionDefinition deathAction,
            string label,
            ValidationReport report)
        {
            foreach (string triggerName in CRITICAL_TRIGGER_NAMES)
            {
                if (!HasTrigger(controller, triggerName))
                {
                    report.Error($"{label} is missing critical trigger '{triggerName}'.", controller);
                }
            }

            foreach (string stateName in CRITICAL_VICTIM_STATE_NAMES)
            {
                AnimatorState state = GetState(controller, stateName);
                if (state == null)
                {
                    report.Error($"{label} is missing critical victim state '{stateName}'.", controller);
                    continue;
                }

                if (!state.behaviours.OfType<EnemyCriticalVictimStateBehaviour>().Any())
                {
                    report.Error($"{label} critical victim state '{stateName}' is missing {nameof(EnemyCriticalVictimStateBehaviour)}.", state);
                }
            }

            AnimatorState getUpState = GetState(controller, "GetUp");
            if (getUpState == null)
            {
                report.Error($"{label} is missing get up state 'GetUp'.", controller);
            }
            else
            {
                if (!getUpState.behaviours.OfType<EnemyGetUpStateBehaviour>().Any())
                {
                    report.Error($"{label} get up state 'GetUp' is missing {nameof(EnemyGetUpStateBehaviour)}.", getUpState);
                }

                if (!HasOutgoingTransition(getUpState))
                {
                    report.Error($"{label} get up state 'GetUp' has no exit transition.", getUpState);
                }
            }

            if (deathAction == null)
            {
                return;
            }

            AnimatorState deathState = GetState(controller, deathAction.ActionId.ToString());
            if (deathState == null || !HasActionBehaviour(deathState, deathAction.ActionId))
            {
                report.Error($"{label} Death must have an action-state completion callback.", controller);
            }
        }

        private static void ValidateEncounterPrefabs(
            IReadOnlyDictionary<string, WeaponMovesetDefinition> movesets,
            IReadOnlyDictionary<string, EnemyBehaviourProfile> profiles,
            IReadOnlyDictionary<string, HealthData> healthData,
            ValidationReport report)
        {
            foreach (string path in FindPrefabPaths())
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                EnemyEncounterSystem[] encounters = prefab.GetComponentsInChildren<EnemyEncounterSystem>(true);
                EnemySpawnPoint[] spawnPoints = prefab.GetComponentsInChildren<EnemySpawnPoint>(true);
                if (encounters.Length == 0 && spawnPoints.Length == 0)
                {
                    continue;
                }

                report.EncounterPrefabCount += encounters.Length;
                foreach (EnemyEncounterSystem encounter in encounters)
                {
                    ValidateEncounterReferences(encounter, path, report);
                }

                foreach (EnemySpawnPoint spawnPoint in spawnPoints)
                {
                    ValidateSpawnPoint(spawnPoint, path, movesets, profiles, healthData, report);
                }
            }
        }

        private static void ValidateEncounterReferences(
            EnemyEncounterSystem encounter,
            string path,
            ValidationReport report)
        {
            var serialized = new SerializedObject(encounter);
            SerializedProperty spawnPoints = serialized.FindProperty("spawnPoints");
            SerializedProperty spawnOnStart = serialized.FindProperty("spawnOnStart");
            SerializedProperty respawnOnGrace = serialized.FindProperty("respawnOnGrace");
            SerializedProperty respawnOnGameEnded = serialized.FindProperty("respawnOnGameEnded");
            SerializedProperty maxPressureSlots = serialized.FindProperty("maxPressureSlots");
            SerializedProperty pressureSlotTimeoutSeconds = serialized.FindProperty(
                "pressureSlotTimeoutSeconds");
            if (spawnOnStart == null
                || respawnOnGrace == null
                || respawnOnGameEnded == null
                || maxPressureSlots == null
                || pressureSlotTimeoutSeconds == null)
            {
                report.Error(
                    $"Encounter '{path}' must serialize spawning and pressure-slot settings.",
                    encounter);
                return;
            }

            if (maxPressureSlots.intValue < 1)
            {
                report.Error($"Encounter '{path}' requires at least one pressure slot.", encounter);
            }

            if (pressureSlotTimeoutSeconds.floatValue <= 0f)
            {
                report.Error($"Encounter '{path}' requires a positive pressure slot timeout.", encounter);
            }

            if (spawnPoints == null || spawnPoints.arraySize == 0)
            {
                bool canSpawn = encounter.isActiveAndEnabled
                    && (spawnOnStart.boolValue
                        || respawnOnGrace.boolValue
                        || respawnOnGameEnded.boolValue);
                if (canSpawn)
                {
                    report.Error($"Encounter '{path}' has no spawn points.", encounter);
                }

                return;
            }

            var offsets = new HashSet<(EnemyBehaviourProfile Profile, int Offset)>();
            for (int index = 0; index < spawnPoints.arraySize; index++)
            {
                EnemySpawnPoint spawnPoint = spawnPoints.GetArrayElementAtIndex(index)
                    .objectReferenceValue as EnemySpawnPoint;
                if (spawnPoint == null)
                {
                    report.Error($"Encounter '{path}' has a null spawn point at index {index}.", encounter);
                    continue;
                }

                if (spawnPoint.BehaviourProfile != null
                    && !offsets.Add((spawnPoint.BehaviourProfile, spawnPoint.RandomSeedOffset)))
                {
                    report.Error(
                        $"Encounter '{path}' has duplicate randomSeedOffset {spawnPoint.RandomSeedOffset} for behaviour profile '{spawnPoint.BehaviourProfile.name}'.",
                        spawnPoint);
                }
            }
        }

        private static void ValidateEncounterScenes(
            IReadOnlyDictionary<string, WeaponMovesetDefinition> movesets,
            IReadOnlyDictionary<string, EnemyBehaviourProfile> profiles,
            IReadOnlyDictionary<string, HealthData> healthData,
            ValidationReport report)
        {
            foreach (string path in FindScenePaths())
            {
                Scene previewScene = EditorSceneManager.OpenPreviewScene(path);
                try
                {
                    ValidateEncounterScene(
                        previewScene,
                        path,
                        movesets,
                        profiles,
                        healthData,
                        report);
                }
                finally
                {
                    EditorSceneManager.ClosePreviewScene(previewScene);
                }
            }
        }

        private static void ValidateEncounterScene(
            Scene scene,
            string path,
            IReadOnlyDictionary<string, WeaponMovesetDefinition> movesets,
            IReadOnlyDictionary<string, EnemyBehaviourProfile> profiles,
            IReadOnlyDictionary<string, HealthData> healthData,
            ValidationReport report)
        {
            var sceneOffsets = new HashSet<(EnemyBehaviourProfile Profile, int Offset)>();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                EnemyEncounterSystem[] encounters = root.GetComponentsInChildren<EnemyEncounterSystem>(true);
                EnemySpawnPoint[] spawnPoints = root.GetComponentsInChildren<EnemySpawnPoint>(true);
                report.EncounterSceneCount += encounters.Length;

                foreach (EnemyEncounterSystem encounter in encounters)
                {
                    ValidateEncounterReferences(encounter, path, report);
                    ValidateSceneSpawnOffsets(encounter, path, sceneOffsets, report);
                }

                foreach (EnemySpawnPoint spawnPoint in spawnPoints)
                {
                    ValidateSpawnPoint(spawnPoint, path, movesets, profiles, healthData, report);
                }
            }
        }

        private static void ValidateSceneSpawnOffsets(
            EnemyEncounterSystem encounter,
            string path,
            HashSet<(EnemyBehaviourProfile Profile, int Offset)> sceneOffsets,
            ValidationReport report)
        {
            if (!encounter.isActiveAndEnabled)
            {
                return;
            }

            SerializedProperty spawnPoints = new SerializedObject(encounter)
                .FindProperty("spawnPoints");
            if (spawnPoints == null)
            {
                return;
            }

            for (int index = 0; index < spawnPoints.arraySize; index++)
            {
                EnemySpawnPoint spawnPoint = spawnPoints.GetArrayElementAtIndex(index)
                    .objectReferenceValue as EnemySpawnPoint;
                if (spawnPoint == null
                    || !spawnPoint.isActiveAndEnabled
                    || spawnPoint.BehaviourProfile == null)
                {
                    continue;
                }

                if (!sceneOffsets.Add((spawnPoint.BehaviourProfile, spawnPoint.RandomSeedOffset)))
                {
                    report.Error(
                        $"Scene encounter '{path}' has duplicate randomSeedOffset {spawnPoint.RandomSeedOffset} for behaviour profile '{spawnPoint.BehaviourProfile.name}'.",
                        spawnPoint);
                }
            }
        }

        private static void ValidateSpawnPoint(
            EnemySpawnPoint spawnPoint,
            string path,
            IReadOnlyDictionary<string, WeaponMovesetDefinition> movesets,
            IReadOnlyDictionary<string, EnemyBehaviourProfile> profiles,
            IReadOnlyDictionary<string, HealthData> healthData,
            ValidationReport report)
        {
            report.SpawnPointCount++;
            string label = $"Spawn point '{path}/{spawnPoint.name}'";
            RequireReference(spawnPoint.EnemyPrefab, "EnemyPrefab", label, report);
            RequireReference(spawnPoint.BehaviourProfile, "BehaviourProfile", label, report);
            RequireReference(spawnPoint.Moveset, "Moveset", label, report);
            RequireReference(spawnPoint.HealthData, "HealthData", label, report);

            if (spawnPoint.EnemyPrefab != null
                && AssetDatabase.GetAssetPath(spawnPoint.EnemyPrefab) == string.Empty)
            {
                report.Error($"{label} must reference a prefab asset, not a scene object.", spawnPoint);
            }

            ValidateReferencedAsset(spawnPoint.Moveset, movesets, "Moveset", label, spawnPoint, report);
            ValidateReferencedAsset(spawnPoint.BehaviourProfile, profiles, "BehaviourProfile", label, spawnPoint, report);
            ValidateReferencedAsset(spawnPoint.HealthData, healthData, "HealthData", label, spawnPoint, report);

            bool requiresTrigger = spawnPoint.BehaviourProfile != null
                && spawnPoint.BehaviourProfile.ActivationMode == EnemyActivationMode.Triggered;
            ValidateActivationTrigger(spawnPoint.EnemyPrefab, requiresTrigger, label, report);

        }

        private static void ValidateActivationTrigger(
            EnemyActor enemyPrefab,
            bool requiresTrigger,
            string label,
            ValidationReport report)
        {
            if (enemyPrefab == null)
            {
                return;
            }

            EnemyActivationTrigger[] triggers = enemyPrefab
                .GetComponentsInChildren<EnemyActivationTrigger>(true);
            if (triggers.Length == 0)
            {
                if (requiresTrigger)
                {
                    report.Error(
                        $"{label} uses Triggered activation but its EnemyPrefab has no {nameof(EnemyActivationTrigger)}.",
                        enemyPrefab);
                }

                return;
            }

            if (triggers.Length != 1)
            {
                report.Error(
                    $"{label} must author exactly one {nameof(EnemyActivationTrigger)} when one is present.",
                    enemyPrefab);
            }

            foreach (EnemyActivationTrigger trigger in triggers)
            {
                Collider collider = trigger.GetComponent<Collider>();
                Rigidbody body = trigger.GetComponent<Rigidbody>();
                if (!trigger.isActiveAndEnabled
                    || collider == null
                    || !collider.enabled
                    || !collider.gameObject.activeInHierarchy
                    || !collider.isTrigger
                    || body == null
                    || !body.isKinematic
                    || body.useGravity)
                {
                    report.Error(
                        $"{label} trigger activation requires an active enabled {nameof(EnemyActivationTrigger)}, enabled trigger Collider, and kinematic gravity-free Rigidbody.",
                        trigger);
                }
            }
        }

        private static void ValidateReferencedAsset<TAsset>(
            TAsset asset,
            IReadOnlyDictionary<string, TAsset> availableAssets,
            string assetName,
            string label,
            UnityEngine.Object context,
            ValidationReport report)
            where TAsset : UnityEngine.Object
        {
            if (asset != null && !availableAssets.Values.Contains(asset))
            {
                report.Error($"{label} references a {assetName} outside the project asset database.", context);
            }
        }

        private static AnimatorState GetState(AnimatorController controller, string name)
        {
            AnimatorControllerLayer baseLayer = controller.layers.FirstOrDefault(
                layer => layer.name == BASE_LAYER_NAME);
            if (baseLayer.stateMachine == null)
            {
                return null;
            }

            foreach (AnimatorState state in GetStates(baseLayer.stateMachine))
            {
                if (state.name == name)
                {
                    return state;
                }
            }

            return null;
        }

        private static IEnumerable<AnimatorState> GetStates(AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                yield return child.state;
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                foreach (AnimatorState state in GetStates(child.stateMachine))
                {
                    yield return state;
                }
            }
        }

        private static bool HasActionBehaviour(AnimatorState state, CharacterActionId actionId)
        {
            foreach (EnemyActionStateBehaviour behaviour in state.behaviours
                         .OfType<EnemyActionStateBehaviour>())
            {
                SerializedProperty actionIdProperty = new SerializedObject(behaviour)
                    .FindProperty("actionId");
                if (actionIdProperty != null && actionIdProperty.intValue == (int)actionId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasOutgoingTransition(AnimatorState state) =>
            state.transitions.Any(transition => transition.isExit
                || transition.destinationState != null
                || transition.destinationStateMachine != null);

        private static bool HasTrigger(AnimatorController controller, string parameterName) =>
            controller.parameters.Any(parameter => parameter.name == parameterName
                && parameter.type == AnimatorControllerParameterType.Trigger);

        private static void ValidateCriticalCompletionCallback(ValidationReport report)
        {
            MethodInfo callback = typeof(EnemyActionExecutor).GetMethod(
                nameof(EnemyActionExecutor.CompleteCriticalVictim),
                BindingFlags.Instance | BindingFlags.Public);
            if (callback == null)
            {
                report.Error("EnemyActionExecutor is missing the critical victim completion callback.", null);
            }
        }

        private static IEnumerable<string> FindPrefabPaths() =>
            AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath);

        private static IEnumerable<string> FindScenePaths() =>
            AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath);

        private static TComponent RequireComponent<TComponent>(
            GameObject gameObject,
            string label,
            ValidationReport report)
            where TComponent : Component
        {
            TComponent component = gameObject.GetComponent<TComponent>();
            if (component == null)
            {
                report.Error($"{label} is missing required {typeof(TComponent).Name}.", gameObject);
            }

            return component;
        }

        private static TComponent RequireComponentInChildren<TComponent>(
            GameObject gameObject,
            string label,
            ValidationReport report)
            where TComponent : Component
        {
            TComponent component = gameObject.GetComponentInChildren<TComponent>(true);
            if (component == null)
            {
                report.Error($"{label} is missing required child {typeof(TComponent).Name}.", gameObject);
            }

            return component;
        }

        private static void RequireReference(
            UnityEngine.Object reference,
            string referenceName,
            string label,
            ValidationReport report)
        {
            if (reference == null)
            {
                report.Error($"{label} is missing required {referenceName} reference.", null);
            }
        }

        private static TComponent GetObjectReference<TComponent>(
            SerializedObject serializedObject,
            string propertyName)
            where TComponent : UnityEngine.Object
        {
            return serializedObject.FindProperty(propertyName)?.objectReferenceValue as TComponent;
        }

        private sealed class ValidationReport
        {
            public int EnemyPrefabCount { get; set; }
            public int EncounterPrefabCount { get; set; }
            public int EncounterSceneCount { get; set; }
            public int SpawnPointCount { get; set; }
            public int ErrorCount { get; private set; }

            public void Error(string message, UnityEngine.Object context)
            {
                ErrorCount++;
                Debug.LogError($"[Enemy Authoring] {message}", context);
            }

            public void LogSummary(int movesetCount, int profileCount)
            {
                string summary = $"[Enemy Authoring] Validation complete: {EnemyPrefabCount} enemy prefabs, "
                    + $"{movesetCount} movesets, {profileCount} behaviour profiles, "
                    + $"{EncounterPrefabCount} encounter prefabs, {EncounterSceneCount} scene encounters, "
                    + $"{SpawnPointCount} spawn points, "
                    + $"{ErrorCount} errors.";
                if (ErrorCount == 0)
                {
                    Debug.Log(summary);
                    return;
                }

                Debug.LogWarning(summary);
            }
        }
    }
}
#endif
