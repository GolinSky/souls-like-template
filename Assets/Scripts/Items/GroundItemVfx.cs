using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulsLike.Items
{
    public sealed class GroundItemVfx : MonoBehaviour
    {
        private static readonly int _dissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField] private Transform visualRoot;
        [SerializeField] private ParticleSystem[] ambientParticles;
        [SerializeField] private ParticleSystem pickupFlash;
        [SerializeField] private Renderer[] renderers;
        [SerializeField, Min(0.01f)] private float pullDuration = 0.22f;
        [SerializeField, Min(0.01f)] private float dissolveDuration = 0.45f;
        [SerializeField, Min(0f)] private float targetHeight = 1.1f;

        private MaterialPropertyBlock _propertyBlock;

        public async UniTask PlayPickupAsync(Transform target, CancellationToken token)
        {
            foreach (ParticleSystem particleSystem in ambientParticles)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            pickupFlash.Play(true);

            Vector3 startPosition = visualRoot.position;
            Vector3 startScale = visualRoot.localScale;
            float elapsed = 0f;
            while (elapsed < pullDuration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / pullDuration);
                visualRoot.position = Vector3.Lerp(
                    startPosition,
                    target.position + Vector3.up * targetHeight,
                    Mathf.SmoothStep(0f, 1f, progress));
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / dissolveDuration);
                visualRoot.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                SetDissolve(progress);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        private void SetDissolve(float progress)
        {
            _propertyBlock ??= new MaterialPropertyBlock();
            foreach (Renderer targetRenderer in renderers)
            {
                targetRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetFloat(_dissolveId, progress);
                targetRenderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}
