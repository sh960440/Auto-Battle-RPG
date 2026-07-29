using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Equips inventory items onto characters.
    /// </summary>
    public class EquipmentService
    {
        /// <summary>
        /// Fired when an equipped item is overwritten and discarded.
        /// </summary>
        public event Action<CharacterInstance, EquipmentInstance> ItemDiscarded;

        /// <summary>
        /// Whether the item can be moved from inventory onto the character.
        /// </summary>
        public bool CanEquipFromInventory(
            PlayerProfile profile,
            CharacterInstance character,
            EquipmentInstance item)
        {
            if (profile == null || character == null || item == null)
                return false;

            if (!IsInRoster(profile, character))
                return false;

            if (!IsInInventory(profile, item))
                return false;

            return character.CanEquip(item);
        }

        /// <summary>
        /// Removes <paramref name="item"/> from inventory and equips it, overwriting the slot.
        /// </summary>
        /// <returns><c>true</c> when equip succeeded.</returns>
        public bool TryEquipFromInventory(
            PlayerProfile profile,
            CharacterInstance character,
            EquipmentInstance item)
        {
            if (!CanEquipFromInventory(profile, character, item))
                return false;

            if (!profile.RemoveFromInventory(item))
                return false;

            var previous = character.Equip(item);
            if (previous != null)
                Discard(character, previous);

            return true;
        }

        private void Discard(CharacterInstance character, EquipmentInstance previous)
        {
            ItemDiscarded?.Invoke(character, previous);
            Debug.Log(
                $"[EquipmentService] Discarded {previous.DisplayName} (Lv{previous.Level}) " +
                $"from {character.DisplayName} [{previous.Slot}].");
        }

        private static bool IsInRoster(PlayerProfile profile, CharacterInstance character)
        {
            var characters = profile.Characters;
            for (var i = 0; i < characters.Count; i++)
            {
                if (characters[i] == character)
                    return true;
            }

            return false;
        }

        private static bool IsInInventory(PlayerProfile profile, EquipmentInstance item)
        {
            var inventory = profile.Inventory;
            for (var i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == item)
                    return true;
            }

            return false;
        }
    }
}