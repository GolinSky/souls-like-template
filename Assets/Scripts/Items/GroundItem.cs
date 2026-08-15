using System;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components.Inventory;
using UnityEngine;

namespace SoulsLike.Items
{
    //todo: instead of some single GroundItem use main single GroundItemsSystem - which will invoked in OnTriggerEnter and by itself use entity locator etc....
    public sealed class GroundItem : MonoBehaviour
    {
        [SerializeField] private ItemId itemId;
        [SerializeField, Min(1)] private int quantity = 1;

        public ItemId ItemId => itemId;
        public int Quantity => quantity;

        public void Collect(InventoryComponent inventory)
        {
            if (inventory == null)
            {
                throw new ArgumentNullException(nameof(inventory));
            }

            if (itemId == ItemId.None)
            {
                throw new InvalidOperationException($"Ground item '{name}' requires an ItemId.");
            }

            inventory.Add(itemId, quantity);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            //todo: use entity locator to get ientity - then type of entity - compare if player entity - then access collect command and collect
            Character character = other.GetComponentInParent<Character>();
            if (character != null)
            {
                Collect(character.InventoryComponent);
            }
        }
    }
}
