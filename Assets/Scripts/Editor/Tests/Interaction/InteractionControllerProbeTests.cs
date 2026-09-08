#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Interactions;
using SoulsLike.Services.Layer;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Interaction
{
    public sealed class InteractionControllerProbeTests
    {
        private const int INTERACTION_LAYER = 31;

        private static readonly MethodInfo RefreshTargetMethod =
            typeof(InteractionController).GetMethod(
                "RefreshTarget",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly List<GameObject> _objects = new();
        private readonly List<Entity> _entities = new();
        private readonly List<InteractionController> _controllers = new();
        private EntityLocator _locator;

        [TearDown]
        public void TearDown()
        {
            foreach (InteractionController controller in _controllers)
            {
                controller.Dispose();
            }

            foreach (Entity entity in _entities)
            {
                entity.Dispose();
            }

            foreach (GameObject target in _objects)
            {
                if (target != null)
                {
                    Object.DestroyImmediate(target);
                }
            }

            _entities.Clear();
            _objects.Clear();
            _controllers.Clear();
        }

        [Test]
        public void RefreshTarget_RejectsInteractableBeyondOnePointFiveMeters()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractable(
                "Out of range",
                new Vector3(0f, 0.5f, 1f),
                new Vector3(0.3f, 1f, 0.3f),
                new Vector3(0f, 0f, 1.51f));

            Refresh(controller);

            Assert.That(controller.CurrentPrompt.IsVisible, Is.False);
        }

        [Test]
        public void RefreshTarget_RejectsLargeVolumeWhenStableInteractionAnchorIsOutOfRange()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractable(
                "Far ladder",
                new Vector3(0f, 0.5f, 1f),
                new Vector3(0.3f, 1f, 0.3f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, 3f));

            Refresh(controller);

            Assert.That(controller.CurrentPrompt.IsVisible, Is.False);
        }

        [Test]
        public void RefreshTarget_KeepsLadderVolumeWhenActorStartsInsideIt()
        {
            InteractionController controller = CreateController(
                new Vector3(0f, 0f, -1f),
                Vector3.forward);
            CreateInteractable(
                "Climb ladder",
                new Vector3(0f, 0.5f, -1.1f),
                new Vector3(1.6f, 1.8f, 1.2f),
                new Vector3(0f, 0f, -1.5f),
                Vector3.zero);

            Refresh(controller);

            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Climb ladder"));
        }

        [Test]
        public void RefreshTarget_RejectsVolumeBehindActorEvenWhenOverlappingProbeOrigin()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractable(
                "Behind",
                new Vector3(0f, 0.5f, -0.2f),
                new Vector3(0.2f, 1f, 0.2f),
                new Vector3(0f, 0f, -0.2f));

            Refresh(controller);

            Assert.That(controller.CurrentPrompt.IsVisible, Is.False);
        }

        [Test]
        public void RefreshTarget_ClearsTargetWhenActorTurnsAwayInsideInteractionVolume()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractable(
                "Ladder",
                new Vector3(0f, 0.5f, 0.6f),
                new Vector3(1.6f, 1f, 2f),
                new Vector3(0f, 0f, 1f));

            Refresh(controller);
            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Ladder"));

            _objects[0].transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Refresh(controller);

            Assert.That(controller.CurrentPrompt.IsVisible, Is.False);
        }

        [Test]
        public void RefreshTarget_SwitchesNearbyTargetsWhenActorRotates()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractableAtAngle("Left", -5f);
            CreateInteractableAtAngle("Right", 5f);
            Transform actorTransform = _objects[0].transform;

            actorTransform.rotation = Quaternion.Euler(0f, -15f, 0f);
            Refresh(controller);
            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Left"));

            actorTransform.rotation = Quaternion.Euler(0f, 15f, 0f);
            Refresh(controller);
            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Right"));
        }

        [Test]
        public void RefreshTarget_SwitchesOverlappingVolumesByInteractionAnchor()
        {
            InteractionController controller = CreateController(Vector3.zero, Vector3.forward);
            CreateInteractable(
                "Left",
                new Vector3(-0.1f, 0.5f, 0.6f),
                new Vector3(1.6f, 1f, 2f),
                new Vector3(-0.1f, 0f, 1f));
            CreateInteractable(
                "Right",
                new Vector3(0.1f, 0.5f, 0.6f),
                new Vector3(1.6f, 1f, 2f),
                new Vector3(0.1f, 0f, 1f));
            Transform actorTransform = _objects[0].transform;

            actorTransform.rotation = Quaternion.Euler(0f, -15f, 0f);
            Refresh(controller);
            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Left"));

            actorTransform.rotation = Quaternion.Euler(0f, 15f, 0f);
            Refresh(controller);
            Assert.That(controller.CurrentPrompt.Text, Is.EqualTo("Right"));
        }

        private InteractionController CreateController(Vector3 position, Vector3 forward)
        {
            _locator = new EntityLocator();
            GameObject actorObject = Track(new GameObject("Actor"));
            actorObject.transform.SetPositionAndRotation(
                position,
                Quaternion.LookRotation(forward));
            Character character = actorObject.AddComponent<Character>();
            ViewEntity actorView = actorObject.AddComponent<ViewEntity>();
            actorView.Construct(1, EntityType.Player);

            Entity actorEntity = RegisterEntity(actorView.Id, EntityType.Player);
            var interactionCommand = new InteractionCommand(actorEntity);
            interactionCommand.Initialize();

            var controller = new InteractionController(
                null,
                _locator,
                actorView,
                character,
                new TestLayerService());
            controller.Initialize();
            _controllers.Add(controller);
            return controller;
        }

        private void CreateInteractable(
            string prompt,
            Vector3 colliderPosition,
            Vector3 colliderSize,
            Vector3 anchorPosition,
            Vector3? interactionAnchorPosition = null)
        {
            GameObject target = Track(new GameObject(prompt));
            target.layer = INTERACTION_LAYER;
            target.transform.position = colliderPosition;
            ViewEntity view = target.AddComponent<ViewEntity>();
            view.Construct(_entities.Count + 1, EntityType.Ladder);
            BoxCollider collider = target.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = colliderSize;

            GameObject anchor = Track(new GameObject($"{prompt} Anchor"));
            anchor.transform.position = anchorPosition;
            GameObject interactionAnchor = Track(new GameObject($"{prompt} Interaction Anchor"));
            interactionAnchor.transform.position = interactionAnchorPosition ?? colliderPosition;
            Entity entity = RegisterEntity(view.Id, EntityType.Ladder);
            entity.RegisterComponent(new TestInteractableCommand(
                interactionAnchor.transform,
                anchor.transform,
                prompt));
        }

        private void CreateInteractableAtAngle(string prompt, float angle)
        {
            Vector3 position = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            CreateInteractable(
                prompt,
                new Vector3(position.x, 0.5f, position.z),
                new Vector3(0.1f, 1f, 0.1f),
                position);
        }

        private Entity RegisterEntity(long id, EntityType entityType)
        {
            var entity = new Entity(id, _locator, entityType);
            entity.Initialize();
            _entities.Add(entity);
            return entity;
        }

        private static void Refresh(InteractionController controller)
        {
            Physics.SyncTransforms();
            RefreshTargetMethod.Invoke(controller, null);
        }

        private GameObject Track(GameObject target)
        {
            _objects.Add(target);
            return target;
        }

        private sealed class TestLayerService : ILayerService
        {
            public LayerMask GetLayerMask(LayerName name) => default;
            public int GetLayer(LayerName name) => INTERACTION_LAYER;
            public LayerMask GetMask(LayerMaskName name) => 1 << INTERACTION_LAYER;
            public void SetLayer(GameObject gameObject, LayerName name, bool recursive = true) { }
        }

        private sealed class TestInteractableCommand : IInteractableCommand
        {
            private readonly Transform _interactionAnchor;
            private readonly Transform _anchor;
            private readonly InteractionPrompt _prompt;

            public Transform InteractionAnchor => _interactionAnchor;

            public TestInteractableCommand(
                Transform interactionAnchor,
                Transform anchor,
                string prompt)
            {
                _interactionAnchor = interactionAnchor;
                _anchor = anchor;
                _prompt = new InteractionPrompt(prompt);
            }

            public bool CanInteract(IEntity actor) => true;
            public InteractionPrompt GetPrompt(IEntity actor) => _prompt;
            public InteractionPrompt GetFailurePrompt(IEntity actor) => default;
            public Transform GetInteractionAnchor(IEntity actor) => _anchor;
            public UniTask InteractAsync(IEntity actor, CancellationToken token) => UniTask.CompletedTask;
        }
    }
}
#endif
