using UnityEngine;
using UnityEngine.Rendering;

namespace SoulsLike.Rendering
{
    [ExecuteAlways]
    [RequireComponent(typeof(Volume))]
    public sealed class QualityVolumeProfileSelector : MonoBehaviour
    {
        private const string EDITOR_LOW_MEMORY_QUALITY_LEVEL_NAME = "Editor Low Memory";

        [SerializeField] private VolumeProfile lowMemoryProfile;

        private Volume _volume;
        private VolumeProfile _lowMemoryProfileClone;

        private void OnEnable()
        {
            _volume = GetComponent<Volume>();
            QualitySettings.activeQualityLevelChanged += OnActiveQualityLevelChanged;
            ApplyProfile();
        }

        private void OnDisable()
        {
            QualitySettings.activeQualityLevelChanged -= OnActiveQualityLevelChanged;
            _volume.profile = _volume.sharedProfile;
            DestroyLowMemoryProfileClone();
        }

        private void OnActiveQualityLevelChanged(int previousQualityLevel, int newQualityLevel)
        {
            ApplyProfile();
        }

        private void ApplyProfile()
        {
            string qualityLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()];
            if (qualityLevelName == EDITOR_LOW_MEMORY_QUALITY_LEVEL_NAME)
            {
                DestroyLowMemoryProfileClone();
                _lowMemoryProfileClone = Instantiate(lowMemoryProfile);
                _lowMemoryProfileClone.hideFlags = HideFlags.HideAndDontSave;
                _volume.profile = _lowMemoryProfileClone;
                return;
            }

            _volume.profile = _volume.sharedProfile;
            DestroyLowMemoryProfileClone();
        }

        private void DestroyLowMemoryProfileClone()
        {
            if (_lowMemoryProfileClone == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(_lowMemoryProfileClone);
            }
            else
            {
                DestroyImmediate(_lowMemoryProfileClone);
            }

            _lowMemoryProfileClone = null;
        }
    }
}
