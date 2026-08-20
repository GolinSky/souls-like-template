using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using SoulsLike.Ui.PlayerHud;
using UnityEngine;
using PlayerCharacter = SoulsLike.Entities.Character.Character;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public sealed class GroundItemCollectionCommand : EntityCommand
    {
        private readonly PlayerCharacter _character;
        private readonly InventoryComponent _inventory;
        private readonly ItemCatalog _itemCatalog;
        private readonly PlayerHudUiController _playerHud;

        public Transform CollectionTarget => _character.transform;

        public GroundItemCollectionCommand(
            Entity actor,
            PlayerCharacter character,
            InventoryComponent inventory,
            ItemCatalog itemCatalog,
            PlayerHudUiController playerHud)
            : base(actor)
        {
            _character = character;
            _inventory = inventory;
            _itemCatalog = itemCatalog;
            _playerHud = playerHud;
        }

        public void Collect(GroundItem groundItem)
        {
            switch (groundItem.RewardType)
            {
                case GroundItemRewardType.Item:
                    _inventory.Add(groundItem.ItemId, groundItem.Quantity);
                    ItemDefinition item = _itemCatalog.GetItem(groundItem.ItemId);
                    _playerHud.ShowAcquisition(
                        item.DisplayName,
                        item.Icon,
                        groundItem.Quantity);
                    break;
                case GroundItemRewardType.Currency:
                    _character.GrantCurrency(groundItem.CurrencyAmount);
                    _playerHud.ShowAcquisition(
                        "Runes",
                        null,
                        groundItem.CurrencyAmount);
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(
                        nameof(groundItem.RewardType),
                        groundItem.RewardType,
                        null);
            }
        }
    }
}
