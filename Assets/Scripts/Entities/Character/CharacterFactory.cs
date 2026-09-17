using System;
using SoulsLike.Services.Repository;
using SoulsLike.Services.VContainer;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class CharacterFactory : IDisposable
    {
        private const string CHARACTER_PREFAB_KEY = "Character";

        private readonly LifetimeScope _parentScope;
        private readonly CharacterScopeInstaller _characterScopePrefab;
        private readonly IAssetService _assetService;

        private CharacterScopeInstaller _characterScope;

        public CharacterFactory(
            LifetimeScope parentScope,
            CharacterScopeInstaller characterScopePrefab,
            IAssetService assetService)
        {
            _parentScope = parentScope;
            _characterScopePrefab = characterScopePrefab;
            _assetService = assetService;
        }

        public Character CreateCharacter(Vector3? spawnPosition = null)
        {
            if (_characterScope != null)
            {
                throw new InvalidOperationException("A local player is already created for this factory.");
            }

            GameObject prefab = _assetService.LoadPrefab(CHARACTER_PREFAB_KEY);
            CharacterScopeInstaller scope = _parentScope.CreateChildFromPrefab(_characterScopePrefab);
            GameObject instance = UnityEngine.Object.Instantiate(prefab, scope.transform, true);
            instance.name = $"{nameof(Character)}_Instance";
            Character character = instance.GetComponent<Character>();
            character.StageSpawn(spawnPosition);
            scope.gameObject.SetActive(true);
            scope.BuildOnce();
            _characterScope = scope;
            return character;
        }

        public void Dispose()
        {
            if (_characterScope == null)
            {
                return;
            }

            CharacterScopeInstaller scope = _characterScope;
            _characterScope = null;
            scope.Dispose();
        }
    }
}
