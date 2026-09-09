using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Animations;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class AnimatorCallbackTraceTests
    {
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Models/Character/Character.prefab";
        private const float STEP_SECONDS = 0.05f;
        private const float ROUTE_TIMEOUT_SECONDS = 15f;

        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_instance);
        }

        [Test]
        public void SupportedController_AuthoredRoutesEmitOrderedCallbackSequences()
        {
            Animator animator = CreateAnimator(out AnimatorStateMachineReceiver receiver);
            var trace = new TraceObserver(animator);
            receiver.AddObserver(trace);

            try
            {
                int oneHandedLayer = RequireLayer(animator, "OneHandedLayer");
                int twoHandedLayer = RequireLayer(animator, "TwoHandedLayer");
                int upperBodyLayer = RequireLayer(animator, "UpperBodyActions");
                int fullBodyLayer = RequireLayer(animator, "FullBodyActions");

                AssertRoute(animator, trace, "Spawn", "Spawn", StateMachineName.Spawn, oneHandedLayer, false, false);
                RunAttackScenario(() =>
                {
                    AssertDifferentStateChain(animator, trace, oneHandedLayer);
                    AssertSameStateReplay(animator, trace, oneHandedLayer);
                    AssertRoute(animator, trace, "HeavyAttack", "HeavyAttack", StateMachineName.HeavyAttack,
                        oneHandedLayer, true, true);
                });
                AssertRoute(animator, trace, "Roll", "Roll", StateMachineName.Roll,
                    oneHandedLayer, false, true);
                AssertRoute(animator, trace, "BackStep", "BackStep", StateMachineName.BackStep,
                    oneHandedLayer, false, true);
                AssertRoute(animator, trace, "EquipmentSwapOut", "EquipmentSwapOut",
                    StateMachineName.EquipmentSwapOut, fullBodyLayer, true, false);
                AssertRoute(animator, trace, "EquipmentSwapIn", "EquipmentSwapIn",
                    StateMachineName.EquipmentSwapIn, fullBodyLayer, true, false);
                AssertRoute(animator, trace, "ItemDrink", "ItemDrink", StateMachineName.ItemDrink,
                    fullBodyLayer, true, true);
                AssertRoute(animator, trace, "ItemDrinkEmpty", "ItemDrinkEmpty",
                    StateMachineName.ItemDrinkEmpty, fullBodyLayer, false, true);
                AssertRoute(animator, trace, "BlockHit", "Blocked", StateMachineName.BlockHit,
                    oneHandedLayer, false, true);
                AssertRoute(animator, trace, "Parry", "Parry", StateMachineName.Parry,
                    oneHandedLayer, false, true);
                AssertRoute(animator, trace, "ParryStun", "Parried", StateMachineName.ParryStun,
                    oneHandedLayer, false, false);
                AssertRoute(animator, trace, "HitReaction", "HitFront", StateMachineName.HitReaction,
                    oneHandedLayer, false, false);
                AssertRoute(animator, trace, "GraceUnblock", "GraceUnblock", StateMachineName.GraceUnblock,
                    oneHandedLayer, false, false);
                AssertRoute(animator, trace, "GraceRestStart", "GraceRestStart", StateMachineName.GraceRestStart,
                    oneHandedLayer, false, false);
                AssertRoute(animator, trace, "GraceRestEnd", "GraceRestEnd", StateMachineName.GraceRestEnd,
                    oneHandedLayer, false, false);

                animator.SetLayerWeight(oneHandedLayer, 0f);
                animator.SetLayerWeight(twoHandedLayer, 1f);
                RunAttackScenario(() => AssertRoute(
                    animator,
                    trace,
                    "TwoHandedHeavyAttack",
                    "HeavyAttack",
                    StateMachineName.HeavyAttack,
                    oneHandedLayer,
                    true,
                    true,
                    allowZeroWeightAuthoritativeLayer: true));
                animator.SetLayerWeight(oneHandedLayer, 1f);
                animator.SetLayerWeight(twoHandedLayer, 0f);

                animator.SetLayerWeight(upperBodyLayer, 1f);
                animator.SetLayerWeight(fullBodyLayer, 0f);
                AssertRoute(animator, trace, "UpperBodyVisualBlendEquipmentSwapOut", "EquipmentSwapOut",
                    StateMachineName.EquipmentSwapOut, fullBodyLayer, true, false,
                    allowZeroWeightAuthoritativeLayer: true);
                animator.SetLayerWeight(upperBodyLayer, 0f);
                animator.SetLayerWeight(fullBodyLayer, 1f);
                AssertRoute(animator, trace, "FullBodyEquipmentSwapIn", "EquipmentSwapIn",
                    StateMachineName.EquipmentSwapIn, fullBodyLayer, true, false);

                // CriticalAttack has no StateMachineName mapping or AnimatorStateMachine behaviour.
                // Its authored trigger route is therefore asserted by reached state, not callback events.
                AssertTriggerReachesState(animator, "CriticalAttack", "CriticalAttack", oneHandedLayer);
                AssertRoute(animator, trace, "Death", "Death", StateMachineName.Death,
                    oneHandedLayer, false, false);
            }
            finally
            {
                receiver.RemoveObserver(trace);
            }
        }

        private static void AssertDifferentStateChain(Animator animator, TraceObserver trace, int layerIndex)
        {
            TraceSegment segment = trace.BeginSegment("LightAttackToLightAttackAlt", layerIndex);
            Trigger(animator, "LightAttack");
            AdvanceUntilEvent(animator, trace, segment, StateMachineName.LightAttack,
                StateMachineState.QueueCheck);
            Trigger(animator, "LightAttackAlt");
            AdvanceUntilExit(animator, trace, segment, StateMachineName.LightAttackAlt);

            CallbackRecord[] records = segment.Records.ToArray();
            AssertLifecycle(records, StateMachineName.LightAttack,
                Animator.StringToHash("LightAttack"), false, true, segment, trace);
            AssertLifecycle(records, StateMachineName.LightAttackAlt,
                Animator.StringToHash("LightAttack_Alt"), false, true, segment, trace);
            AssertOrdered(records, trace, segment,
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Enter),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.QueueCheck),
                new CallbackExpectation(StateMachineName.LightAttackAlt, StateMachineState.Enter),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Exit),
                new CallbackExpectation(StateMachineName.LightAttackAlt, StateMachineState.Exit));
            AssertSegmentDiagnostics(segment, trace);
        }

        private static void RunAttackScenario(Action scenario)
        {
            // PlayerMeleeAttackStateBehaviour requires gameplay DI/hitbox runtime outside this
            // callback-contract fixture; callback tuple assertions remain authoritative here.
            bool previousIgnoreFailingMessages = UnityEngine.TestTools.LogAssert.ignoreFailingMessages;
            var failingLogs = new List<CapturedLog>();
            Application.LogCallback captureFailingLog = (condition, stackTrace, type) =>
            {
                if (type is LogType.Error or LogType.Exception or LogType.Assert)
                {
                    failingLogs.Add(new CapturedLog(condition, stackTrace, type));
                }
            };

            Application.logMessageReceived += captureFailingLog;
            UnityEngine.TestTools.LogAssert.ignoreFailingMessages = true;
            try
            {
                scenario();
            }
            finally
            {
                Application.logMessageReceived -= captureFailingLog;
                UnityEngine.TestTools.LogAssert.ignoreFailingMessages = previousIgnoreFailingMessages;
            }

            Assert.That(failingLogs, Is.Not.Empty,
                "Attack-route isolation must capture the known unconfigured combat-relay diagnostic.");
            Assert.That(failingLogs.All(IsKnownUnconfiguredAttackRelayDiagnostic), Is.True,
                "Attack-route isolation captured an unrelated failing log:\n"
                + string.Join("\n", failingLogs.Select(log => log.ToString())));
            Assert.That(failingLogs.Any(log => log.Contains("PlayerMeleeCombatRelay")), Is.True,
                "Attack-route isolation did not capture the expected PlayerMeleeCombatRelay diagnostic.");
        }

        private static bool IsKnownUnconfiguredAttackRelayDiagnostic(CapturedLog log)
        {
            return log.Contains("PlayerMeleeAttackStateBehaviour")
                || log.Contains("PlayerMeleeCombatRelay");
        }

        private static void AssertSameStateReplay(Animator animator, TraceObserver trace, int layerIndex)
        {
            TraceSegment segment = trace.BeginSegment("LightAttackReplay", layerIndex);
            Trigger(animator, "LightAttack");
            AdvanceUntilEvent(animator, trace, segment, StateMachineName.LightAttack,
                StateMachineState.QueueCheck);
            Trigger(animator, "LightAttack");
            AdvanceUntilExit(animator, trace, segment, StateMachineName.LightAttack, requiredExitCount: 2);

            CallbackRecord[] records = segment.Records
                .Where(record => record.StateMachineName == StateMachineName.LightAttack)
                .ToArray();
            Assert.That(records.Count(record => record.Event == StateMachineState.Enter), Is.EqualTo(2),
                trace.BuildMessage(segment, "LightAttack replay must emit two Enter callbacks."));
            Assert.That(records.Count(record => record.Event == StateMachineState.Exit), Is.EqualTo(2),
                trace.BuildMessage(segment, "LightAttack replay must emit two Exit callbacks."));
            Assert.That(records.All(record => record.ShortNameHash == Animator.StringToHash("LightAttack")), Is.True,
                trace.BuildMessage(segment, "LightAttack replay emitted a callback for the wrong concrete state."));
            AssertOrdered(records, trace, segment,
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Enter),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.QueueCheck),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Exit),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Enter),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.QueueCheck),
                new CallbackExpectation(StateMachineName.LightAttack, StateMachineState.Exit));
            AssertSegmentDiagnostics(segment, trace);
        }

        private static void AssertRoute(
            Animator animator,
            TraceObserver trace,
            string name,
            string trigger,
            StateMachineName stateMachineName,
            int authoritativeLayer,
            bool requiresProgress,
            bool requiresQueueCheck,
            bool allowZeroWeightAuthoritativeLayer = false)
        {
            TraceSegment segment = trace.BeginSegment(
                name,
                authoritativeLayer,
                allowZeroWeightAuthoritativeLayer);
            Trigger(animator, trigger);
            AdvanceUntilExit(animator, trace, segment, stateMachineName);
            AssertLifecycle(
                segment.Records,
                stateMachineName,
                Animator.StringToHash(GetExpectedStateName(name)),
                requiresProgress,
                requiresQueueCheck,
                segment,
                trace);
            AssertSegmentDiagnostics(segment, trace);
        }

        private static void AssertTriggerReachesState(
            Animator animator,
            string name,
            string trigger,
            int layerIndex)
        {
            Trigger(animator, trigger);
            int expectedStateHash = Animator.StringToHash(name);
            int steps = Mathf.CeilToInt(ROUTE_TIMEOUT_SECONDS / STEP_SECONDS);
            for (int index = 0; index < steps; index++)
            {
                AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(layerIndex);
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(layerIndex);
                if (current.shortNameHash == expectedStateHash || next.shortNameHash == expectedStateHash) return;
                animator.Update(STEP_SECONDS);
            }

            Assert.Fail($"Trigger '{trigger}' did not reach '{name}' on layer {layerIndex}.");
        }

        private static void AdvanceUntilExit(
            Animator animator,
            TraceObserver trace,
            TraceSegment segment,
            StateMachineName stateMachineName,
            int requiredExitCount = 1)
        {
            int steps = Mathf.CeilToInt(ROUTE_TIMEOUT_SECONDS / STEP_SECONDS);
            for (int index = 0; index < steps; index++)
            {
                if (segment.Records.Count(record => record.StateMachineName == stateMachineName
                        && record.Event == StateMachineState.Exit) >= requiredExitCount)
                {
                    return;
                }

                animator.Update(STEP_SECONDS);
            }

            Assert.Fail(trace.BuildMessage(segment,
                $"{stateMachineName} did not emit {requiredExitCount} Exit callback(s) within {ROUTE_TIMEOUT_SECONDS}s."));
        }

        private static void AdvanceUntilEvent(
            Animator animator,
            TraceObserver trace,
            TraceSegment segment,
            StateMachineName stateMachineName,
            StateMachineState state)
        {
            int steps = Mathf.CeilToInt(ROUTE_TIMEOUT_SECONDS / STEP_SECONDS);
            for (int index = 0; index < steps; index++)
            {
                if (segment.Records.Any(record => record.StateMachineName == stateMachineName
                        && record.Event == state
                        && record.LayerIndex == segment.AuthoritativeLayer))
                {
                    return;
                }

                animator.Update(STEP_SECONDS);
            }

            Assert.Fail(trace.BuildMessage(segment,
                $"{stateMachineName} did not emit {state} on layer {segment.AuthoritativeLayer} within {ROUTE_TIMEOUT_SECONDS}s."));
        }

        private Animator CreateAnimator(out AnimatorStateMachineReceiver receiver)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHARACTER_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, $"Missing character prefab at {CHARACTER_PREFAB_PATH}.");
            _instance = UnityEngine.Object.Instantiate(prefab);
            Animator animator = _instance.GetComponentInChildren<Animator>(true);
            receiver = _instance.GetComponentInChildren<AnimatorStateMachineReceiver>(true);
            Assert.That(animator, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            receiver.InitializeStateMachines();
            animator.Update(0f);
            return animator;
        }

        private static int RequireLayer(Animator animator, string layerName)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            Assert.That(layerIndex, Is.GreaterThanOrEqualTo(0), $"Missing layer '{layerName}'.");
            return layerIndex;
        }

        private static void Trigger(Animator animator, string trigger)
        {
            animator.SetTrigger(trigger);
            animator.Update(0f);
        }

        private static void Advance(Animator animator, float seconds)
        {
            int steps = Mathf.CeilToInt(seconds / STEP_SECONDS);
            for (int index = 0; index < steps; index++) animator.Update(STEP_SECONDS);
        }

        private static void AssertLifecycle(
            IEnumerable<CallbackRecord> records,
            StateMachineName stateMachineName,
            int expectedShortNameHash,
            bool requiresProgress,
            bool requiresQueueCheck,
            TraceSegment segment,
            TraceObserver trace)
        {
            CallbackRecord[] matching = records
                .Where(record => record.StateMachineName == stateMachineName
                    && record.LayerIndex == segment.AuthoritativeLayer)
                .ToArray();
            Assert.That(matching.All(record => record.ShortNameHash == expectedShortNameHash), Is.True,
                trace.BuildMessage(segment,
                    $"{stateMachineName} expected hash {expectedShortNameHash} but received a different concrete state."));
            var expectations = new List<CallbackExpectation>
            {
                new(stateMachineName, StateMachineState.Enter)
            };
            if (requiresProgress) expectations.Add(new CallbackExpectation(stateMachineName, StateMachineState.Progress));
            if (requiresQueueCheck) expectations.Add(new CallbackExpectation(stateMachineName, StateMachineState.QueueCheck));
            expectations.Add(new CallbackExpectation(stateMachineName, StateMachineState.Exit));
            AssertOrdered(matching, trace, segment, expectations.ToArray());
        }

        private static string GetExpectedStateName(string routeName)
        {
            return routeName switch
            {
                "Spawn" => "Crouch_Idle_To_Stand_Idle_A",
                "Roll" => "LightRoll",
                "BlockHit" => "Blocked",
                "ParryStun" => "Parried",
                "HitReaction" => "HitFront",
                "GraceUnblock" => "TouchGrace",
                "TwoHandedHeavyAttack" => "HeavyAttack",
                "UpperBodyVisualBlendEquipmentSwapOut" => "EquipmentSwapOut",
                "FullBodyEquipmentSwapIn" => "EquipmentSwapIn",
                _ => routeName
            };
        }

        private static void AssertOrdered(
            IReadOnlyList<CallbackRecord> records,
            TraceObserver trace,
            TraceSegment segment,
            params CallbackExpectation[] expectations)
        {
            int index = -1;
            foreach (CallbackExpectation expectation in expectations)
            {
                index = FindNext(records, expectation, index + 1);
                Assert.That(index, Is.GreaterThanOrEqualTo(0),
                    trace.BuildMessage(segment, $"Missing ordered callback {expectation}."));
            }
        }

        private static int FindNext(
            IReadOnlyList<CallbackRecord> records,
            CallbackExpectation expectation,
            int startIndex)
        {
            for (int index = startIndex; index < records.Count; index++)
            {
                if (records[index].StateMachineName == expectation.StateMachineName
                    && records[index].Event == expectation.Event)
                {
                    return index;
                }
            }

            return -1;
        }

        private static void AssertSegmentDiagnostics(TraceSegment segment, TraceObserver trace)
        {
            Assert.That(segment.DuplicateCount, Is.Zero,
                trace.BuildMessage(segment, "Duplicate callback(s) detected."));
            Assert.That(segment.StaleExitCount, Is.Zero,
                trace.BuildMessage(segment, "Stale Exit callback(s) detected."));
            Assert.That(segment.WrongLayerCount, Is.Zero,
                trace.BuildMessage(segment, "Wrong-layer callback(s) detected."));
            Assert.That(segment.InactiveLayerCount, Is.Zero,
                trace.BuildMessage(segment, "Inactive-layer callback(s) detected."));
        }

        private sealed class TraceObserver : SoulsLike.Entities.Character.Components.Animations.IObserver<AnimatorStateMachineDto>
        {
            private readonly Animator _animator;
            private readonly List<CallbackRecord> _records = new();
            private readonly Dictionary<CallbackKey, int> _activeCallbacks = new();
            private TraceSegment _activeSegment;

            public TraceObserver(Animator animator)
            {
                _animator = animator;
            }

            public IReadOnlyList<CallbackRecord> Records => _records;

            public TraceSegment BeginSegment(
                string name,
                int authoritativeLayer,
                bool allowZeroWeightAuthoritativeLayer = false)
            {
                _activeSegment = new TraceSegment(
                    name,
                    authoritativeLayer,
                    allowZeroWeightAuthoritativeLayer,
                    _records);
                return _activeSegment;
            }

            public void UpdateState(AnimatorStateMachineDto state)
            {
                var record = new CallbackRecord(state.StateMachineName, state.State,
                    state.StateInfo.shortNameHash, state.LayerIndex);
                var key = new CallbackKey(record.StateMachineName, record.ShortNameHash, record.LayerIndex);
                if (_records.Count > 0 && _records[^1].Equals(record)) _activeSegment.DuplicateCount++;
                if (record.LayerIndex < 0 || record.LayerIndex >= _animator.layerCount)
                {
                    _activeSegment.WrongLayerCount++;
                }
                else if (_animator.GetLayerWeight(record.LayerIndex) <= 0f
                    && !(record.LayerIndex == _activeSegment.AuthoritativeLayer
                        && _activeSegment.AllowsZeroWeightAuthoritativeLayer))
                {
                    _activeSegment.InactiveLayerCount++;
                }

                if (record.Event == StateMachineState.Enter)
                {
                    _activeCallbacks.TryGetValue(key, out int count);
                    _activeCallbacks[key] = count + 1;
                }
                else if (record.Event == StateMachineState.Exit)
                {
                    if (!_activeCallbacks.TryGetValue(key, out int count) || count == 0) _activeSegment.StaleExitCount++;
                    else _activeCallbacks[key] = count - 1;
                }

                _records.Add(record);
            }

            public string BuildMessage(TraceSegment segment, string summary)
            {
                return $"{summary} Scenario={segment.Name}; authoritativeLayer={segment.AuthoritativeLayer}.\n"
                    + string.Join("\n", segment.Records.Select(record => record.ToString()));
            }
        }

        private sealed class TraceSegment
        {
            private readonly IReadOnlyList<CallbackRecord> _records;
            private readonly int _startIndex;

            public TraceSegment(
                string name,
                int authoritativeLayer,
                bool allowsZeroWeightAuthoritativeLayer,
                IReadOnlyList<CallbackRecord> records)
            {
                Name = name;
                AuthoritativeLayer = authoritativeLayer;
                AllowsZeroWeightAuthoritativeLayer = allowsZeroWeightAuthoritativeLayer;
                _records = records;
                _startIndex = records.Count;
            }

            public string Name { get; }
            public int AuthoritativeLayer { get; }
            public bool AllowsZeroWeightAuthoritativeLayer { get; }
            public int DuplicateCount { get; set; }
            public int StaleExitCount { get; set; }
            public int WrongLayerCount { get; set; }
            public int InactiveLayerCount { get; set; }
            public IEnumerable<CallbackRecord> Records => _records.Skip(_startIndex);
        }

        private readonly struct CallbackKey
        {
            public readonly StateMachineName StateMachineName;
            public readonly int ShortNameHash;
            public readonly int LayerIndex;

            public CallbackKey(StateMachineName stateMachineName, int shortNameHash, int layerIndex)
            {
                StateMachineName = stateMachineName;
                ShortNameHash = shortNameHash;
                LayerIndex = layerIndex;
            }
        }

        private readonly struct CapturedLog
        {
            public readonly string Condition;
            public readonly string StackTrace;
            public readonly LogType Type;

            public CapturedLog(string condition, string stackTrace, LogType type)
            {
                Condition = condition;
                StackTrace = stackTrace;
                Type = type;
            }

            public bool Contains(string value)
            {
                return Condition.IndexOf(value, StringComparison.Ordinal) >= 0
                    || StackTrace.IndexOf(value, StringComparison.Ordinal) >= 0;
            }

            public override string ToString() => $"{Type}: {Condition}\n{StackTrace}";
        }

        private readonly struct CallbackExpectation
        {
            public readonly StateMachineName StateMachineName;
            public readonly StateMachineState Event;

            public CallbackExpectation(StateMachineName stateMachineName, StateMachineState @event)
            {
                StateMachineName = stateMachineName;
                Event = @event;
            }

            public override string ToString() => $"({StateMachineName}, {Event})";
        }

        private readonly struct CallbackRecord
        {
            public readonly StateMachineName StateMachineName;
            public readonly StateMachineState Event;
            public readonly int ShortNameHash;
            public readonly int LayerIndex;

            public CallbackRecord(StateMachineName stateMachineName, StateMachineState @event, int shortNameHash, int layerIndex)
            {
                StateMachineName = stateMachineName;
                Event = @event;
                ShortNameHash = shortNameHash;
                LayerIndex = layerIndex;
            }

            public override string ToString() => $"({StateMachineName}, {Event}, {ShortNameHash}, {LayerIndex})";
        }
    }
}
