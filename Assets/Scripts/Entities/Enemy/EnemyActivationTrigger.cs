using SoulsLike.Entities.BaseEntity;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Enemy
{
    [RequireComponent(typeof(Collider), typeof(Rigidbody))]
    public sealed class EnemyActivationTrigger : MonoBehaviour
    {
        private IEntityLocator _entityLocator;
        private EnemyController _controller;

        [Inject]
        public void Construct(IEntityLocator entityLocator, EnemyController controller)
        {
            _entityLocator = entityLocator;
            _controller = controller;
        }

        private void Reset()
        {
            ConfigureTriggerBody();
        }

        private void Awake()
        {
            ConfigureTriggerBody();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_entityLocator.TryGetEntity(other, out IEntity entity)
                || entity.EntityType != EntityType.Player)
            {
                return;
            }

            _controller.ActivateFromTrigger();
        }

        private void ConfigureTriggerBody()
        {
            Collider collider = GetComponent<Collider>();
            Rigidbody body = GetComponent<Rigidbody>();
            if (collider == null || body == null)
            {
                Debug.LogError(
                    $"{nameof(EnemyActivationTrigger)} requires a Collider and Rigidbody.",
                    this);
                return;
            }

            collider.isTrigger = true;
            body.isKinematic = true;
            body.useGravity = false;
        }
    }
}
