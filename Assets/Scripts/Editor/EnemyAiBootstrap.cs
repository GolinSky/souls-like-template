#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using SoulsLike.Components.Visibility;
using SoulsLike.EditorTools;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using SoulsLike.Items;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SoulsLike.Editor
{
    public static class EnemyAiBootstrap
    {
        private const string ENEMY_SETTINGS_FOLDER = "Assets/Settings/Enemy";
        private const string ENEMY_ANIMATION_FOLDER = "Assets/Art/Animation/Enemy";
        private const string ENEMY_PREFAB_FOLDER = "Assets/Prefabs/Enemy";
        private const string ACTION_FOLDER = ENEMY_SETTINGS_FOLDER + "/Actions";
        private const string CONTROLLER_PATH =
            ENEMY_ANIMATION_FOLDER + "/ErikaLongSwordEnemy.controller";
        private const string MOVESET_PATH =
            ENEMY_SETTINGS_FOLDER + "/ErikaLongSwordMoveset.asset";
        private const string BEHAVIOUR_PATH =
            ENEMY_SETTINGS_FOLDER + "/ErikaMeleeBehaviour.asset";
        private const string HEALTH_PATH =
            ENEMY_SETTINGS_FOLDER + "/ErikaMeleeHealth.asset";
        private const string ENEMY_PREFAB_PATH =
            ENEMY_PREFAB_FOLDER + "/ErikaMeleeEnemy.prefab";
        private const string CHARACTER_PREFAB_PATH =
            "Assets/Prefabs/Character/Character.prefab";
        private const string SWORD_PREFAB_PATH =
            "Assets/Prefabs/Swords/LongSword.prefab";
        private const string PLAYER_CONTROLLER_PATH =
            "Assets/Art/Animation/CharacterGreatSwordAnimator.controller";
        private const string ERIKA_MODEL_PATH =
            "Assets/Art/Models/Characters/Archer/Erika Archer With Bow Arrow.fbx";
        private const string T_POSE_PATH =
            "Assets/ThirdParty/DoubleL/Model/T-Pose.fbx";
        private const string DEFAULT_SCENE_PATH =
            "Assets/Scenes/DefaultLocation/DefaultLocaiton.unity";
        private const string OTHER_SCENE_PATH =
            "Assets/Scenes/DefaultLocation/Other.unity";
        private const string NAVIGATION_FOLDER =
            "Assets/Scenes/DefaultLocation/Navigation";
        private const string NAVIGATION_DATA_PATH =
            NAVIGATION_FOLDER + "/EnemyNavigation.asset";

        private const string IDLE_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Stand_Idle/Idle/Stand_Idle_A_1.fbx";
        private const string WALK_FORWARD_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Walk/Base/A/InPlace/Walk_A_F_InPlace.fbx";
        private const string WALK_BACKWARD_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Walk/Base/A/InPlace/Walk_A_B_InPlace.fbx";
        private const string WALK_LEFT_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Walk/Base/A/InPlace/Walk_A_F_L90_A_InPlace.fbx";
        private const string WALK_RIGHT_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Walk/Base/A/InPlace/Walk_A_F_R90_A_InPlace.fbx";
        private const string RUN_FORWARD_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Run/Base/A/InPlace/Run_A_F_InPlace.fbx";
        private const string RUN_BACKWARD_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Run/Base/A/InPlace/Run_A_B_InPlace.fbx";
        private const string RUN_LEFT_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Run/Base/A/InPlace/Run_A_F_L90_A_InPlace.fbx";
        private const string RUN_RIGHT_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Base Move/Run/Base/A/InPlace/Run_A_F_R90_A_InPlace.fbx";
        private const string LIGHT_ATTACK_1_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_1.fbx";
        private const string LIGHT_ATTACK_2_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_2.fbx";
        private const string COMBO_1_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_4_1.fbx";
        private const string COMBO_2_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_4_2_Combo.fbx";
        private const string COMBO_3_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_4_3_Combo.fbx";
        private const string HEAVY_ATTACK_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Enemy Attack/Enemy_Attack_5.fbx";
        private const string HIT_PATH =
            "Assets/ThirdParty/DoubleL/Demo Scenes/Demo Animations/Hit_F_1.anim";
        private const string DEATH_PATH =
            "Assets/ThirdParty/DoubleL/FBX_Animations/Hit/Die/Front/Die_F_1-A.fbx";

        private static readonly Vector3 ENEMY_SPAWN_POSITION =
            new(-18.37f, 271.81f, -156.04f);
        private static readonly Vector3 PATROL_POINT_A =
            new(-26f, 271.82f, -155f);
        private static readonly Vector3 PATROL_POINT_B =
            new(-9.5f, 271.8f, -156f);

        [MenuItem("Tools/SoulsLike/Bootstrap Enemy AI")]
        public static void Execute()
        {
            EnsureFolder(ACTION_FOLDER);
            EnsureFolder(ENEMY_ANIMATION_FOLDER);
            EnsureFolder(ENEMY_PREFAB_FOLDER);

            Avatar sourceAvatar = LoadRequiredAvatar(T_POSE_PATH);
            ConfigureSelectedAnimations(sourceAvatar);

            Dictionary<CharacterActionId, CharacterActionDefinition> actions =
                CreateActionDefinitions();
            AnimatorController controller = CreateEnemyAnimator(actions);
            WeaponMovesetDefinition moveset = CreateMoveset(controller, actions);
            EnemyBehaviourProfile behaviour = CreateBehaviour(actions);
            HealthData health = CreateHealthData();

            ConfigureSwordPrefab();
            ConfigurePlayerPrefab();
            ConfigurePlayerAnimator();
            EnemyActor enemyPrefab = CreateEnemyPrefab();
            ConfigureScenes(enemyPrefab, moveset, behaviour, health);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ForceReserializeAuthoredAssets(actions.Values);
            BakeNavigation();
            Debug.Log("Enemy AI bootstrap and navigation bake completed.");
        }

        [MenuItem("Tools/SoulsLike/Bake Enemy Navigation")]
        public static void BakeNavigation()
        {
            OpenNavigationScenes();
            Scene scene = SceneManager.GetSceneByPath(DEFAULT_SCENE_PATH);
            NavMeshSurface surface = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<NavMeshSurface>(true))
                .Single();
            surface.BuildNavMesh();
            FinalizeNavigationBake();
        }

        public static void FinalizeNavigationBake()
        {
            Scene scene = SceneManager.GetSceneByPath(DEFAULT_SCENE_PATH);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                throw new InvalidOperationException(
                    $"Navigation scene '{DEFAULT_SCENE_PATH}' must be loaded.");
            }

            NavMeshSurface surface = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<NavMeshSurface>(true))
                .Single();
            NavMeshData navigationData = surface.navMeshData
                ?? throw new InvalidOperationException(
                    "The navigation surface has not been baked.");
            if (!EditorUtility.IsPersistent(navigationData))
            {
                EnsureFolder(NAVIGATION_FOLDER);
                if (AssetDatabase.LoadAssetAtPath<NavMeshData>(NAVIGATION_DATA_PATH) != null)
                {
                    AssetDatabase.DeleteAsset(NAVIGATION_DATA_PATH);
                }

                AssetDatabase.CreateAsset(navigationData, NAVIGATION_DATA_PATH);
                EditorUtility.SetDirty(surface);
            }

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.ForceReserializeAssets(
                new[] { DEFAULT_SCENE_PATH },
                ForceReserializeAssetsOptions.ReserializeAssets);
            Debug.Log("Enemy navigation bake persisted successfully.");
        }

        private static void ConfigureSelectedAnimations(Avatar sourceAvatar)
        {
            string[] locomotionPaths =
            {
                IDLE_PATH,
                WALK_FORWARD_PATH,
                WALK_BACKWARD_PATH,
                WALK_LEFT_PATH,
                WALK_RIGHT_PATH,
                RUN_FORWARD_PATH,
                RUN_BACKWARD_PATH,
                RUN_LEFT_PATH,
                RUN_RIGHT_PATH
            };
            foreach (string path in locomotionPaths)
            {
                ConfigureAnimationImporter(path, sourceAvatar, true, true, true);
            }

            string[] actionPaths =
            {
                LIGHT_ATTACK_1_PATH,
                LIGHT_ATTACK_2_PATH,
                COMBO_1_PATH,
                COMBO_2_PATH,
                COMBO_3_PATH,
                HEAVY_ATTACK_PATH,
                DEATH_PATH
            };
            foreach (string path in actionPaths)
            {
                ConfigureAnimationImporter(path, sourceAvatar, false, false, false);
            }
        }

        private static void ConfigureAnimationImporter(
            string path,
            Avatar sourceAvatar,
            bool loop,
            bool inPlace,
            bool repairAvatar)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter
                ?? throw new InvalidOperationException(
                    $"Animation '{path}' requires a {nameof(ModelImporter)}.");
            importer.animationType = ModelImporterAnimationType.Human;
            if (repairAvatar)
            {
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                importer.sourceAvatar = sourceAvatar;
            }

            ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.loopTime = loop;
                clip.loopPose = loop;
                clip.lockRootHeightY = true;
                clip.keepOriginalPositionY = true;
                clip.lockRootPositionXZ = inPlace;
                clip.keepOriginalPositionXZ = inPlace;
                clip.lockRootRotation = inPlace;
                clip.keepOriginalOrientation = inPlace;
            }

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static Dictionary<CharacterActionId, CharacterActionDefinition>
            CreateActionDefinitions()
        {
            var actions = new Dictionary<CharacterActionId, CharacterActionDefinition>
            {
                [CharacterActionId.LightAttack1] = CreateAction(
                    CharacterActionId.LightAttack1, 1f, 14f, 180f, 20f, 70f),
                [CharacterActionId.LightAttack2] = CreateAction(
                    CharacterActionId.LightAttack2, 1.1f, 16f, 160f, 10f, 60f),
                [CharacterActionId.Combo1] = CreateAction(
                    CharacterActionId.Combo1, 1.05f, 18f, 180f, 15f, 65f),
                [CharacterActionId.Combo2] = CreateAction(
                    CharacterActionId.Combo2, 1.15f, 18f, 140f, 5f, 55f),
                [CharacterActionId.Combo3] = CreateAction(
                    CharacterActionId.Combo3, 1.35f, 24f, 100f, 0f, 35f),
                [CharacterActionId.HeavyAttack] = CreateAction(
                    CharacterActionId.HeavyAttack, 1.65f, 30f, 120f, 0f, 30f),
                [CharacterActionId.Death] = CreateAction(
                    CharacterActionId.Death, 0f, 0f, 0f, 0f, 0f)
            };

            SetFollowUps(
                actions[CharacterActionId.LightAttack1],
                actions[CharacterActionId.LightAttack2]);
            SetFollowUps(
                actions[CharacterActionId.Combo1],
                actions[CharacterActionId.Combo2]);
            SetFollowUps(
                actions[CharacterActionId.Combo2],
                actions[CharacterActionId.Combo3]);
            return actions;
        }

        private static CharacterActionDefinition CreateAction(
            CharacterActionId actionId,
            float damageMultiplier,
            float staminaCost,
            float windupTurnSpeed,
            float activeTurnSpeed,
            float recoveryTurnSpeed)
        {
            string path = $"{ACTION_FOLDER}/{actionId}.asset";
            CharacterActionDefinition action =
                LoadOrCreate<CharacterActionDefinition>(path);
            var serialized = new SerializedObject(action);
            SetInt(serialized, "actionId", (int)actionId, false);
            SetFloat(serialized, "damageMultiplier", damageMultiplier, false);
            SetFloat(serialized, "staminaCost", staminaCost, false);
            SetBool(serialized, "usesRootMotion", true, false);
            SetFloat(serialized, "windupTurnSpeed", windupTurnSpeed, false);
            SetFloat(serialized, "activeTurnSpeed", activeTurnSpeed, false);
            SetFloat(serialized, "recoveryTurnSpeed", recoveryTurnSpeed);
            return action;
        }

        private static void SetFollowUps(
            CharacterActionDefinition action,
            params CharacterActionDefinition[] followUps)
        {
            var serialized = new SerializedObject(action);
            SerializedProperty property = RequireProperty(serialized, "followUps");
            property.arraySize = followUps.Length;
            for (int index = 0; index < followUps.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = followUps[index];
            }

            Apply(serialized);
        }

        private static AnimatorController CreateEnemyAnimator(
            IReadOnlyDictionary<CharacterActionId, CharacterActionDefinition> actions)
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            if (controller != null)
            {
                AssetDatabase.DeleteAsset(CONTROLLER_PATH);
            }

            controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

            controller.parameters = new[]
            {
                new AnimatorControllerParameter
                {
                    name = "Speed",
                    type = AnimatorControllerParameterType.Float
                },
                new AnimatorControllerParameter
                {
                    name = "MoveX",
                    type = AnimatorControllerParameterType.Float
                },
                new AnimatorControllerParameter
                {
                    name = "MoveY",
                    type = AnimatorControllerParameterType.Float
                },
                new AnimatorControllerParameter
                {
                    name = "Hit",
                    type = AnimatorControllerParameterType.Trigger
                }
            };

            AnimatorControllerLayer layer = controller.layers[0];
            layer.name = "Base Layer";
            layer.defaultWeight = 1f;
            AnimatorStateMachine stateMachine = layer.stateMachine;
            foreach (ChildAnimatorState state in stateMachine.states.ToArray())
            {
                stateMachine.RemoveState(state.state);
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines.ToArray())
            {
                stateMachine.RemoveStateMachine(child.stateMachine);
            }

            BlendTree locomotionTree = GetOrCreateLocomotionTree(controller);
            AnimatorState locomotion = stateMachine.AddState("Locomotion");
            EnsureControllerSubAsset(locomotion, controller);
            locomotion.motion = locomotionTree;
            locomotion.writeDefaultValues = false;
            stateMachine.defaultState = locomotion;

            AnimatorState hit = stateMachine.AddState("Hit");
            EnsureControllerSubAsset(hit, controller);
            hit.motion = LoadClip(HIT_PATH);
            hit.writeDefaultValues = false;
            EnemyHitStateBehaviour hitBehaviour =
                hit.AddStateMachineBehaviour<EnemyHitStateBehaviour>();
            EnsureControllerSubAsset(hitBehaviour, controller);

            AnimatorStateTransition hitExit = hit.AddTransition(locomotion);
            EnsureControllerSubAsset(hitExit, controller);
            hitExit.hasExitTime = true;
            hitExit.exitTime = 0.9f;
            hitExit.hasFixedDuration = true;
            hitExit.duration = 0.08f;

            AnimatorStateTransition hitInterrupt =
                stateMachine.AddAnyStateTransition(hit);
            EnsureControllerSubAsset(hitInterrupt, controller);
            hitInterrupt.hasExitTime = false;
            hitInterrupt.hasFixedDuration = true;
            hitInterrupt.duration = 0f;
            hitInterrupt.canTransitionToSelf = true;
            hitInterrupt.AddCondition(AnimatorConditionMode.If, 0f, "Hit");

            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.LightAttack1],
                LoadClip(LIGHT_ATTACK_1_PATH), true, 0.15f, 0.5f, true, 0.43f, 0.7f, 0.64f);
            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.LightAttack2],
                LoadClip(LIGHT_ATTACK_2_PATH), true, 0.15f, 0.54f, false, 0f, 0f, 0.66f);
            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.Combo1],
                LoadClip(COMBO_1_PATH), true, 0.15f, 0.48f, true, 0.42f, 0.68f, 0.64f);
            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.Combo2],
                LoadClip(COMBO_2_PATH), true, 0.15f, 0.47f, true, 0.36f, 0.69f, 0.62f);
            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.Combo3],
                LoadClip(COMBO_3_PATH), true, 0.15f, 0.54f, false, 0f, 0f, 0.66f);
            AddActionState(
                controller, stateMachine, locomotion, actions[CharacterActionId.HeavyAttack],
                LoadClip(HEAVY_ATTACK_PATH), true, 0.15f, 0.62f, false, 0f, 0f, 0.72f);

            AnimatorState deathComplete = stateMachine.AddState("DeathComplete");
            EnsureControllerSubAsset(deathComplete, controller);
            deathComplete.writeDefaultValues = false;
            AddActionState(
                controller, stateMachine, deathComplete, actions[CharacterActionId.Death],
                LoadClip(DEATH_PATH), false, 0.15f, 0f, false, 0f, 0f, 0.92f);

            controller.layers = new[] { layer };
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(stateMachine);
            return controller;
        }

        private static BlendTree GetOrCreateLocomotionTree(AnimatorController controller)
        {
            BlendTree tree = AssetDatabase.LoadAllAssetsAtPath(CONTROLLER_PATH)
                .OfType<BlendTree>()
                .FirstOrDefault(candidate => candidate.name == "LocomotionBlendTree");
            if (tree == null)
            {
                tree = new BlendTree { name = "LocomotionBlendTree" };
                AssetDatabase.AddObjectToAsset(tree, controller);
            }

            tree.blendType = BlendTreeType.FreeformCartesian2D;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            tree.useAutomaticThresholds = false;
            tree.children = new[]
            {
                CreateChild(LoadClip(IDLE_PATH), Vector2.zero),
                CreateChild(LoadClip(WALK_FORWARD_PATH), new Vector2(0f, 1.5f)),
                CreateChild(LoadClip(WALK_BACKWARD_PATH), new Vector2(0f, -1.2f)),
                CreateChild(LoadClip(WALK_LEFT_PATH), new Vector2(-1.5f, 0f)),
                CreateChild(LoadClip(WALK_RIGHT_PATH), new Vector2(1.5f, 0f)),
                CreateChild(LoadClip(RUN_FORWARD_PATH), new Vector2(0f, 3.5f)),
                CreateChild(LoadClip(RUN_BACKWARD_PATH), new Vector2(0f, -3f)),
                CreateChild(LoadClip(RUN_LEFT_PATH), new Vector2(-3.5f, 0f)),
                CreateChild(LoadClip(RUN_RIGHT_PATH), new Vector2(3.5f, 0f))
            };
            EditorUtility.SetDirty(tree);
            return tree;
        }

        private static ChildMotion CreateChild(Motion motion, Vector2 position) =>
            new()
            {
                motion = motion,
                position = position,
                timeScale = 1f
            };

        private static void AddActionState(
            AnimatorController controller,
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            CharacterActionDefinition action,
            AnimationClip clip,
            bool hasHitbox,
            float activeStart,
            float activeEnd,
            bool hasCombo,
            float comboStart,
            float comboEnd,
            float recoveryStart)
        {
            AnimatorState state = stateMachine.AddState(action.ActionId.ToString());
            EnsureControllerSubAsset(state, controller);
            state.motion = clip;
            state.writeDefaultValues = false;
            EnemyActionStateBehaviour behaviour =
                state.AddStateMachineBehaviour<EnemyActionStateBehaviour>();
            EnsureControllerSubAsset(behaviour, controller);
            var serialized = new SerializedObject(behaviour);
            SetInt(serialized, "actionId", (int)action.ActionId, false);
            SetBool(serialized, "hasHitboxWindow", hasHitbox, false);
            SetFloat(serialized, "activeStart", activeStart, false);
            SetFloat(serialized, "activeEnd", activeEnd, false);
            SetBool(serialized, "hasComboWindow", hasCombo, false);
            SetFloat(serialized, "comboStart", comboStart, false);
            SetFloat(serialized, "comboEnd", comboEnd, false);
            SetFloat(serialized, "recoveryStart", recoveryStart);

            AnimatorStateTransition transition = state.AddTransition(destination);
            EnsureControllerSubAsset(transition, controller);
            transition.hasExitTime = true;
            transition.exitTime = 0.98f;
            transition.hasFixedDuration = true;
            transition.duration = 0.08f;
        }

        private static void EnsureControllerSubAsset(
            Object asset,
            AnimatorController controller)
        {
            if (!AssetDatabase.Contains(asset))
            {
                AssetDatabase.AddObjectToAsset(asset, controller);
            }
        }

        private static WeaponMovesetDefinition CreateMoveset(
            RuntimeAnimatorController controller,
            IReadOnlyDictionary<CharacterActionId, CharacterActionDefinition> actions)
        {
            WeaponMovesetDefinition moveset =
                LoadOrCreate<WeaponMovesetDefinition>(MOVESET_PATH);
            var serialized = new SerializedObject(moveset);
            SetInt(serialized, "weaponId", (int)ItemId.GreatSword, false);
            SetObject(serialized, "animatorController", controller, false);
            SetObject(serialized, "combatIdle", LoadClip(IDLE_PATH), false);
            SetObject(serialized, "walkForward", LoadClip(WALK_FORWARD_PATH), false);
            SetObject(serialized, "walkBackward", LoadClip(WALK_BACKWARD_PATH), false);
            SetObject(serialized, "walkLeft", LoadClip(WALK_LEFT_PATH), false);
            SetObject(serialized, "walkRight", LoadClip(WALK_RIGHT_PATH), false);
            SetObject(serialized, "runForward", LoadClip(RUN_FORWARD_PATH), false);
            SetObject(serialized, "runBackward", LoadClip(RUN_BACKWARD_PATH), false);
            SetObject(serialized, "runLeft", LoadClip(RUN_LEFT_PATH), false);
            SetObject(serialized, "runRight", LoadClip(RUN_RIGHT_PATH), false);
            SerializedProperty property = RequireProperty(serialized, "actions");
            CharacterActionId[] orderedActions =
            {
                CharacterActionId.LightAttack1,
                CharacterActionId.LightAttack2,
                CharacterActionId.Combo1,
                CharacterActionId.Combo2,
                CharacterActionId.Combo3,
                CharacterActionId.HeavyAttack,
                CharacterActionId.Death
            };
            property.arraySize = orderedActions.Length;
            for (int index = 0; index < orderedActions.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue =
                    actions[orderedActions[index]];
            }

            Apply(serialized);
            return moveset;
        }

        private static EnemyBehaviourProfile CreateBehaviour(
            IReadOnlyDictionary<CharacterActionId, CharacterActionDefinition> actions)
        {
            EnemyBehaviourProfile profile =
                LoadOrCreate<EnemyBehaviourProfile>(BEHAVIOUR_PATH);
            var serialized = new SerializedObject(profile);
            SetFloat(serialized, "perceptionRange", 14f, false);
            SetFloat(serialized, "fieldOfView", 115f, false);
            SetFloat(serialized, "eyeHeight", 1.45f, false);
            SetInt(serialized, "lineOfSightMask", ~(1 << 7), false);
            SetFloat(serialized, "targetMemorySeconds", 4.5f, false);
            SetFloat(serialized, "reactionDelayMin", 0.18f, false);
            SetFloat(serialized, "reactionDelayMax", 0.38f, false);
            SetBool(serialized, "startsDormant", false, false);
            SetFloat(serialized, "leashDistance", 22f, false);
            SetFloat(serialized, "patrolWaitSeconds", 1.4f, false);
            SetFloat(serialized, "searchSeconds", 4.5f, false);
            SetFloat(serialized, "searchTurnSpeed", 80f, false);
            SetFloat(serialized, "arrivalDistance", 0.35f, false);
            SetFloat(serialized, "decisionInterval", 0.18f, false);
            SetFloat(serialized, "preferredRangeMin", 1.15f, false);
            SetFloat(serialized, "preferredRangeMax", 2.8f, false);
            SetFloat(serialized, "strafeDistance", 1.6f, false);
            SetFloat(serialized, "waitSeconds", 0.4f, false);
            SetInt(serialized, "randomSeed", 1741, false);

            SerializedProperty rules = RequireProperty(serialized, "actionRules");
            rules.arraySize = 7;
            ConfigureRule(rules.GetArrayElementAtIndex(0),
                actions[CharacterActionId.LightAttack1], 0.8f, 2.35f, 60f, 3f, 0.55f, 0.35f);
            ConfigureRule(rules.GetArrayElementAtIndex(1),
                actions[CharacterActionId.LightAttack2], 0.8f, 2.2f, 60f, 2f, 0.75f, 0.4f);
            ConfigureRule(rules.GetArrayElementAtIndex(2),
                actions[CharacterActionId.LightAttack2], 0.7f, 2.45f, 75f, 4f, 0.45f, 0.5f,
                true, CharacterActionId.LightAttack1);
            ConfigureRule(rules.GetArrayElementAtIndex(3),
                actions[CharacterActionId.Combo1], 1f, 2.65f, 55f, 1.7f, 2.2f, 0.3f);
            ConfigureRule(rules.GetArrayElementAtIndex(4),
                actions[CharacterActionId.Combo2], 0.65f, 2.6f, 75f, 5f, 0f, 0.5f,
                true, CharacterActionId.Combo1);
            ConfigureRule(rules.GetArrayElementAtIndex(5),
                actions[CharacterActionId.Combo3], 0.65f, 2.8f, 80f, 5f, 0f, 0.5f,
                true, CharacterActionId.Combo2);
            ConfigureRule(rules.GetArrayElementAtIndex(6),
                actions[CharacterActionId.HeavyAttack], 1.35f, 3.1f, 45f, 1f, 3f, 0.25f);
            Apply(serialized);
            return profile;
        }

        private static void ConfigureRule(
            SerializedProperty rule,
            CharacterActionDefinition action,
            float minimumDistance,
            float maximumDistance,
            float maximumAngle,
            float baseWeight,
            float cooldown,
            float repetitionPenalty,
            bool requiresComboWindow = false,
            CharacterActionId requiredPreviousAction = CharacterActionId.None)
        {
            SetRelativeObject(rule, "action", action);
            SetRelativeFloat(rule, "minimumDistance", minimumDistance);
            SetRelativeFloat(rule, "maximumDistance", maximumDistance);
            SetRelativeFloat(rule, "maximumAngle", maximumAngle);
            SetRelativeBool(rule, "requiresLineOfSight", true);
            SetRelativeFloat(rule, "baseWeight", baseWeight);
            SetRelativeFloat(rule, "cooldown", cooldown);
            SetRelativeFloat(rule, "repetitionPenalty", repetitionPenalty);
            SetRelativeBool(rule, "requiresComboWindow", requiresComboWindow);
            SetRelativeInt(rule, "requiredPreviousAction", (int)requiredPreviousAction);
        }

        private static HealthData CreateHealthData()
        {
            HealthData health = LoadOrCreate<HealthData>(HEALTH_PATH);
            var serialized = new SerializedObject(health);
            SetFloat(serialized, "<MaxHealth>k__BackingField", 420f, false);
            SetFloat(serialized, "<StartingHealth>k__BackingField", 420f, false);
            SetFloat(serialized, "<MaxFocus>k__BackingField", 100f, false);
            SetFloat(serialized, "<StartingFocus>k__BackingField", 100f, false);
            SetFloat(serialized, "<MaxStamina>k__BackingField", 100f, false);
            SetFloat(serialized, "<StartingStamina>k__BackingField", 100f, false);
            SetBool(serialized, "<CanDie>k__BackingField", true, false);
            SetFloat(serialized, "<InvulnerableOnSpawnSeconds>k__BackingField", 0f);
            return health;
        }

        private static void ConfigureSwordPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(SWORD_PREFAB_PATH);
            try
            {
                MeshFilter meshFilter = RequireComponent<MeshFilter>(root);
                BoxCollider hitbox = root.GetComponent<BoxCollider>();
                if (hitbox == null)
                {
                    hitbox = root.AddComponent<BoxCollider>();
                }

                Bounds bounds = meshFilter.sharedMesh.bounds;
                hitbox.isTrigger = true;
                hitbox.center = new Vector3(bounds.center.x, 0.68f, bounds.center.z);
                hitbox.size = new Vector3(0.09f, 1.25f, 0.11f);
                hitbox.enabled = false;

                Rigidbody body = root.GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = root.AddComponent<Rigidbody>();
                }

                body.isKinematic = true;
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

                MeleeHitboxController controller =
                    root.GetComponent<MeleeHitboxController>();
                if (controller == null)
                {
                    controller = root.AddComponent<MeleeHitboxController>();
                }

                var serialized = new SerializedObject(controller);
                SetObject(serialized, "hitbox", hitbox, false);
                SetInt(serialized, "hitZone", 0);
                PrefabUtility.SaveAsPrefabAsset(root, SWORD_PREFAB_PATH);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigurePlayerPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CHARACTER_PREFAB_PATH);
            try
            {
                if (root.GetComponent<ViewEntity>() == null)
                {
                    root.AddComponent<ViewEntity>();
                }

                if (root.GetComponent<PlayerMeleeCombatRelay>() == null)
                {
                    root.AddComponent<PlayerMeleeCombatRelay>();
                }

                Transform lockNode = root.transform.Find("TargetLockNode");
                if (lockNode == null)
                {
                    lockNode = new GameObject(
                        "TargetLockNode",
                        typeof(TargetLockNode)).transform;
                    lockNode.SetParent(root.transform, false);
                }

                lockNode.localPosition = new Vector3(0f, 1.35f, 0f);
                lockNode.localRotation = Quaternion.identity;
                lockNode.localScale = Vector3.one;
                PrefabUtility.SaveAsPrefabAsset(root, CHARACTER_PREFAB_PATH);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ConfigurePlayerAnimator()
        {
            AnimatorController controller =
                LoadRequiredAsset<AnimatorController>(PLAYER_CONTROLLER_PATH);
            var actionByState = new Dictionary<string, CharacterActionId>
            {
                ["Attack"] = CharacterActionId.LightAttack1,
                ["LightAttack"] = CharacterActionId.LightAttack1,
                ["LightAttack_Alt"] = CharacterActionId.LightAttack2,
                ["HeavyAttack"] = CharacterActionId.HeavyAttack,
                ["HeavyAttack_Alt"] = CharacterActionId.HeavyAttack,
                ["RollAttack"] = CharacterActionId.LightAttack1,
                ["BackStepAttack"] = CharacterActionId.LightAttack1,
                ["RunAttack"] = CharacterActionId.LightAttack1,
                ["SpecialAttack"] = CharacterActionId.HeavyAttack
            };

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                ConfigurePlayerStateMachine(layer.stateMachine, actionByState);
            }

            EditorUtility.SetDirty(controller);
        }

        private static void ConfigurePlayerStateMachine(
            AnimatorStateMachine stateMachine,
            IReadOnlyDictionary<string, CharacterActionId> actionByState)
        {
            foreach (ChildAnimatorState child in stateMachine.states)
            {
                if (!actionByState.TryGetValue(child.state.name, out CharacterActionId actionId))
                {
                    continue;
                }

                PlayerMeleeAttackStateBehaviour behaviour = child.state.behaviours
                    .OfType<PlayerMeleeAttackStateBehaviour>()
                    .FirstOrDefault();
                if (behaviour == null)
                {
                    behaviour = child.state
                        .AddStateMachineBehaviour<PlayerMeleeAttackStateBehaviour>();
                }

                var serialized = new SerializedObject(behaviour);
                SetInt(serialized, "actionId", (int)actionId, false);
                SetFloat(serialized, "activeStart", 0.15f, false);
                SetFloat(serialized, "activeEnd", 0.58f);
            }

            foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
            {
                ConfigurePlayerStateMachine(child.stateMachine, actionByState);
            }
        }

        private static EnemyActor CreateEnemyPrefab()
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer < 0)
            {
                throw new InvalidOperationException("The Enemy layer is required.");
            }

            var root = new GameObject("ErikaMeleeEnemy");
            try
            {
                root.layer = enemyLayer;
                ViewEntity viewEntity = root.AddComponent<ViewEntity>();
                CharacterController characterController =
                    root.AddComponent<CharacterController>();
                characterController.center = new Vector3(0f, 0.9f, 0f);
                characterController.height = 1.8f;
                characterController.radius = 0.32f;
                characterController.stepOffset = 0.4f;
                characterController.skinWidth = 0.04f;
                characterController.excludeLayers = 1 << enemyLayer;

                NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
                agent.radius = 0.32f;
                agent.height = 1.8f;
                agent.speed = 3.5f;
                agent.acceleration = 14f;
                agent.angularSpeed = 540f;
                agent.stoppingDistance = 0.85f;
                agent.autoBraking = true;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

                HealthComponent healthComponent = root.AddComponent<HealthComponent>();
                root.AddComponent<VisibilityComponent>();
                root.AddComponent<EnemyHealthUiComponent>();
                EnemyNavigationMotor motor = root.AddComponent<EnemyNavigationMotor>();
                EnemyActor actor = root.AddComponent<EnemyActor>();

                GameObject modelPrefab = LoadRequiredAsset<GameObject>(ERIKA_MODEL_PATH);
                GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
                model.name = "ErikaVisual";
                model.transform.SetParent(root.transform, false);
                Animator animator = RequireComponent<Animator>(model);
                animator.applyRootMotion = true;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                EnemyAnimationController animation =
                    model.AddComponent<EnemyAnimationController>();

                FindRequiredChild(model.transform, "Erika_Archer_Bow_Mesh")
                    .gameObject.SetActive(false);
                FindRequiredChild(model.transform, "Erika_Archer_Arrow_Mesh")
                    .gameObject.SetActive(false);

                Transform rightHand = FindRequiredChild(
                    model.transform,
                    "mixamorig:RightHand");
                GameObject swordPrefab = LoadRequiredAsset<GameObject>(SWORD_PREFAB_PATH);
                GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab);
                sword.name = "LongSword";
                sword.transform.SetParent(rightHand, false);
                sword.transform.localPosition = swordPrefab.transform.localPosition;
                sword.transform.localRotation = swordPrefab.transform.localRotation;
                sword.transform.localScale = swordPrefab.transform.localScale;
                SetLayerRecursively(sword, enemyLayer);
                MeleeHitboxController hitbox =
                    RequireComponent<MeleeHitboxController>(sword);

                Transform lockNode = new GameObject(
                    "TargetLockNode",
                    typeof(TargetLockNode)).transform;
                lockNode.SetParent(root.transform, false);
                lockNode.localPosition = new Vector3(0f, 1.4f, 0f);

                SetLayerRecursively(root, enemyLayer);

                var motorSerialized = new SerializedObject(motor);
                SetObject(motorSerialized, "agent", agent, false);
                SetObject(motorSerialized, "controller", characterController);

                var actorSerialized = new SerializedObject(actor);
                SetObject(actorSerialized, "animator", animator, false);
                SetObject(actorSerialized, "meleeHitbox", hitbox);

                var animationSerialized = new SerializedObject(animation);
                SetObject(animationSerialized, "animator", animator, false);
                SetObject(animationSerialized, "motor", motor, false);
                SetObject(animationSerialized, "actor", actor, false);
                SetObject(animationSerialized, "meleeHitbox", hitbox);

                EnemyActor prefab = PrefabUtility.SaveAsPrefabAsset(root, ENEMY_PREFAB_PATH)
                    .GetComponent<EnemyActor>();
                if (prefab == null || viewEntity == null || healthComponent == null)
                {
                    throw new InvalidOperationException(
                        "The authored enemy prefab is missing required components.");
                }

                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void ConfigureScenes(
            EnemyActor enemyPrefab,
            WeaponMovesetDefinition moveset,
            EnemyBehaviourProfile behaviour,
            HealthData health)
        {
            Scene defaultScene = GetOrOpenScene(DEFAULT_SCENE_PATH);
            Scene otherScene = GetOrOpenScene(OTHER_SCENE_PATH);

            GameObject existingEncounter = FindRoot(otherScene, "EnemyEncounter");
            if (existingEncounter != null)
            {
                Object.DestroyImmediate(existingEncounter);
            }

            var encounter = new GameObject("EnemyEncounter");
            SceneManager.MoveGameObjectToScene(encounter, otherScene);
            EnemyEncounterSystem encounterSystem = encounter.AddComponent<EnemyEncounterSystem>();

            var spawnObject = new GameObject("ErikaMeleeSpawn");
            spawnObject.transform.SetParent(encounter.transform, false);
            spawnObject.transform.position = ENEMY_SPAWN_POSITION;
            spawnObject.transform.rotation = Quaternion.identity;
            EnemySpawnPoint spawn = spawnObject.AddComponent<EnemySpawnPoint>();

            Transform pointA = new GameObject("PatrolPointA").transform;
            pointA.SetParent(encounter.transform, false);
            pointA.position = PATROL_POINT_A;
            Transform pointB = new GameObject("PatrolPointB").transform;
            pointB.SetParent(encounter.transform, false);
            pointB.position = PATROL_POINT_B;

            var spawnSerialized = new SerializedObject(spawn);
            SetObject(spawnSerialized, "enemyPrefab", enemyPrefab, false);
            SetObject(spawnSerialized, "behaviourProfile", behaviour, false);
            SetObject(spawnSerialized, "moveset", moveset, false);
            SetObject(spawnSerialized, "healthData", health, false);
            SerializedProperty patrolPoints =
                RequireProperty(spawnSerialized, "patrolPoints");
            patrolPoints.arraySize = 2;
            patrolPoints.GetArrayElementAtIndex(0).objectReferenceValue = pointA;
            patrolPoints.GetArrayElementAtIndex(1).objectReferenceValue = pointB;
            Apply(spawnSerialized);

            var encounterSystemSerialized = new SerializedObject(encounterSystem);
            SerializedProperty spawnPoints =
                RequireProperty(encounterSystemSerialized, "spawnPoints");
            spawnPoints.arraySize = 1;
            spawnPoints.GetArrayElementAtIndex(0).objectReferenceValue = spawn;
            SetBool(encounterSystemSerialized, "spawnOnStart", true);

            GameObject navigation = FindRoot(defaultScene, "EnemyNavigation");
            if (navigation == null)
            {
                navigation = new GameObject("EnemyNavigation");
                SceneManager.MoveGameObjectToScene(navigation, defaultScene);
            }

            NavMeshSurface surface = navigation.GetComponent<NavMeshSurface>();
            if (surface == null)
            {
                surface = navigation.AddComponent<NavMeshSurface>();
            }

            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = ~((1 << LayerMask.NameToLayer("Player"))
                | (1 << LayerMask.NameToLayer("Enemy")));

            EditorSceneManager.MarkSceneDirty(otherScene);
            EditorSceneManager.MarkSceneDirty(defaultScene);
            EditorSceneManager.SaveScene(otherScene);
            EditorSceneManager.SaveScene(defaultScene);
        }

        private static void OpenNavigationScenes()
        {
            foreach (string scenePath in LocationBakeTool.AllScenes)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }
            }

            Scene defaultScene = SceneManager.GetSceneByPath(DEFAULT_SCENE_PATH);
            EditorSceneManager.SetActiveScene(defaultScene);
        }

        private static void ForceReserializeAuthoredAssets(
            IEnumerable<CharacterActionDefinition> actions)
        {
            var paths = new List<string>
            {
                CONTROLLER_PATH,
                MOVESET_PATH,
                BEHAVIOUR_PATH,
                HEALTH_PATH,
                ENEMY_PREFAB_PATH,
                CHARACTER_PREFAB_PATH,
                SWORD_PREFAB_PATH,
                PLAYER_CONTROLLER_PATH,
                DEFAULT_SCENE_PATH,
                OTHER_SCENE_PATH
            };
            paths.AddRange(actions.Select(AssetDatabase.GetAssetPath));
            AssetDatabase.ForceReserializeAssets(
                paths,
                ForceReserializeAssetsOptions.ReserializeAssets);
        }

        private static AnimationClip LoadClip(string path)
        {
            AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<AnimationClip>()
                .FirstOrDefault(candidate => !candidate.name.StartsWith("__preview__"));
            return clip ?? throw new InvalidOperationException(
                $"Animation clip '{path}' was not imported.");
        }

        private static Avatar LoadRequiredAvatar(string path)
        {
            Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Avatar>()
                .FirstOrDefault(candidate => candidate.isValid && candidate.isHuman);
            return avatar ?? throw new InvalidOperationException(
                $"Humanoid avatar '{path}' was not imported.");
        }

        private static TAsset LoadOrCreate<TAsset>(string path)
            where TAsset : ScriptableObject
        {
            TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            if (asset != null)
            {
                return asset;
            }

            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            asset = ScriptableObject.CreateInstance<TAsset>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static TAsset LoadRequiredAsset<TAsset>(string path)
            where TAsset : Object
        {
            TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
            return asset != null
                ? asset
                : throw new InvalidOperationException(
                    $"Required asset '{path}' was not found.");
        }

        private static Scene GetOrOpenScene(string path)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            return scene.IsValid() && scene.isLoaded
                ? scene
                : EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }

        private static GameObject FindRoot(Scene scene, string name) =>
            scene.GetRootGameObjects().FirstOrDefault(root => root.name == name);

        private static Transform FindRequiredChild(Transform root, string name)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            throw new InvalidOperationException(
                $"Hierarchy '{root.name}' requires child '{name}'.");
        }

        private static TComponent RequireComponent<TComponent>(GameObject root)
            where TComponent : Component
        {
            TComponent component = root.GetComponent<TComponent>();
            return component != null
                ? component
                : throw new InvalidOperationException(
                    $"'{root.name}' requires {typeof(TComponent).Name}.");
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = layer;
            }
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }

                current = next;
            }
        }

        private static SerializedProperty RequireProperty(
            SerializedObject serialized,
            string propertyName) =>
            serialized.FindProperty(propertyName)
            ?? throw new InvalidOperationException(
                $"{serialized.targetObject.GetType().Name} requires property '{propertyName}'.");

        private static SerializedProperty RequireRelative(
            SerializedProperty property,
            string propertyName) =>
            property.FindPropertyRelative(propertyName)
            ?? throw new InvalidOperationException(
                $"Serialized property '{property.propertyPath}' requires '{propertyName}'.");

        private static void SetObject(
            SerializedObject serialized,
            string propertyName,
            Object value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).objectReferenceValue = value;
            if (apply)
            {
                Apply(serialized);
            }
        }

        private static void SetFloat(
            SerializedObject serialized,
            string propertyName,
            float value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).floatValue = value;
            if (apply)
            {
                Apply(serialized);
            }
        }

        private static void SetInt(
            SerializedObject serialized,
            string propertyName,
            int value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).intValue = value;
            if (apply)
            {
                Apply(serialized);
            }
        }

        private static void SetBool(
            SerializedObject serialized,
            string propertyName,
            bool value,
            bool apply = true)
        {
            RequireProperty(serialized, propertyName).boolValue = value;
            if (apply)
            {
                Apply(serialized);
            }
        }

        private static void SetRelativeObject(
            SerializedProperty property,
            string propertyName,
            Object value) =>
            RequireRelative(property, propertyName).objectReferenceValue = value;

        private static void SetRelativeFloat(
            SerializedProperty property,
            string propertyName,
            float value) =>
            RequireRelative(property, propertyName).floatValue = value;

        private static void SetRelativeInt(
            SerializedProperty property,
            string propertyName,
            int value) =>
            RequireRelative(property, propertyName).intValue = value;

        private static void SetRelativeBool(
            SerializedProperty property,
            string propertyName,
            bool value) =>
            RequireRelative(property, propertyName).boolValue = value;

        private static void Apply(SerializedObject serialized)
        {
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(serialized.targetObject);
        }
    }
}
#endif
