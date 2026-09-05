using UnityEngine;

namespace SoulsLike.Services.Settings
{
    [CreateAssetMenu(fileName = "SettingsDefaultsData", menuName = "Data/SettingsDefaultsData")]
    public sealed class SettingsDefaultsData : ScriptableObject
    {
        [SerializeField] private GameSettingsData defaults = new();

        public GameSettingsData CreateCopy()
        {
            return SettingsDataUtility.Copy(defaults);
        }
    }
}
