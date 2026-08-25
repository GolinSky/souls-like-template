using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoulsLike.Items
{
    public sealed class GroundItemVfx : MonoBehaviour
    {
        private static readonly int _dissolveId = Shader.PropertyToID("_Dissolve");

        [SerializeField] private ParticleSystem[] ambientParticles;
        [SerializeField] private ParticleSystem pickupFlash;
        [SerializeField] private Renderer[] renderers;
        [SerializeField, Min(0.01f)] private float dissolveDuration = 0.45f;

        private MaterialPropertyBlock _propertyBlock;

        public async UniTask PlayPickupAsync(CancellationToken token)
        {
            foreach (ParticleSystem particleSystem in ambientParticles)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            pickupFlash.Play(true);

            float elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / dissolveDuration);
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
