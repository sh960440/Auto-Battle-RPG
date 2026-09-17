using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Converts between live <see cref="PlayerProfile"/> state and save data.
    /// </summary>
    public static class GameSaveCodec
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// Builds a save data from a live profile.
        /// </summary>
        public static GameSaveData ToSaveData(PlayerProfile profile, int currentStage)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var characters = profile.Characters;
            var characterData = new CharacterSaveData[characters.Count];
            for (var i = 0; i < characters.Count; i++)
                characterData[i] = ToCharacterSave(characters[i]);

            var inventory = profile.Inventory;
            var inventoryData = new EquipmentSaveData[inventory.Count];
            for (var i = 0; i < inventory.Count; i++)
                inventoryData[i] = ToEquipmentSave(inventory[i]);

            return new GameSaveData
            {
                version = CurrentVersion,
                gold = profile.Gold,
                currentStage = Math.Max(1, currentStage),
                selectedCharacterIndex = profile.SelectedIndex,
                characters = characterData,
                inventory = inventoryData
            };
        }

        /// <summary>
        /// Rebuilds a live profile from save data.
        /// </summary>
        public static PlayerProfile FromSaveData(GameSaveData data, ContentCatalog catalog)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var profile = new PlayerProfile(data.gold);
            var characters = data.characters ?? Array.Empty<CharacterSaveData>();
            for (var i = 0; i < characters.Length; i++)
            {
                var character = FromCharacterSave(characters[i], catalog);
                if (character != null)
                    profile.AddCharacter(character);
            }

            var inventory = data.inventory ?? Array.Empty<EquipmentSaveData>();
            for (var i = 0; i < inventory.Length; i++)
            {
                var item = FromEquipmentSave(inventory[i], catalog);
                if (item != null)
                    profile.AddToInventory(item);
            }

            if (profile.Characters.Count == 0)
                return profile;

            var selected = Mathf.Clamp(data.selectedCharacterIndex, 0, profile.Characters.Count - 1);
            profile.SelectCharacter(selected);
            return profile;
        }

        private static CharacterSaveData ToCharacterSave(CharacterInstance character)
        {
            return new CharacterSaveData
            {
                definitionId = character.Definition != null ? character.Definition.name : string.Empty,
                level = character.Level,
                leftHand = ToEquipmentSave(character.GetEquipment(EquipmentSlot.LeftHand)),
                rightHand = ToEquipmentSave(character.GetEquipment(EquipmentSlot.RightHand)),
                upperBody = ToEquipmentSave(character.GetEquipment(EquipmentSlot.UpperBody)),
                lowerBody = ToEquipmentSave(character.GetEquipment(EquipmentSlot.LowerBody))
            };
        }

        private static CharacterInstance FromCharacterSave(CharacterSaveData data, ContentCatalog catalog)
        {
            if (data == null || string.IsNullOrEmpty(data.definitionId))
                return null;

            var definition = catalog.FindCharacter(data.definitionId);
            if (definition == null)
            {
                Debug.LogWarning($"[GameSaveCodec] Missing character definition '{data.definitionId}'.");
                return null;
            }

            var character = new CharacterInstance(definition, data.level);
            TryEquip(character, data.leftHand, catalog);
            TryEquip(character, data.rightHand, catalog);
            TryEquip(character, data.upperBody, catalog);
            TryEquip(character, data.lowerBody, catalog);
            return character;
        }

        private static void TryEquip(CharacterInstance character, EquipmentSaveData data, ContentCatalog catalog)
        {
            var item = FromEquipmentSave(data, catalog);
            if (item == null)
                return;

            try
            {
                character.Equip(item);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GameSaveCodec] Skipped equip '{data.definitionId}': {ex.Message}");
            }
        }

        private static EquipmentSaveData ToEquipmentSave(EquipmentInstance item)
        {
            if (item == null || item.Definition == null)
                return new EquipmentSaveData();

            return new EquipmentSaveData
            {
                definitionId = item.Definition.name,
                level = item.Level
            };
        }

        private static EquipmentInstance FromEquipmentSave(EquipmentSaveData data, ContentCatalog catalog)
        {
            if (data == null || !data.HasItem)
                return null;

            var definition = catalog.FindEquipment(data.definitionId);
            if (definition == null)
            {
                Debug.LogWarning($"[GameSaveCodec] Missing equipment definition '{data.definitionId}'.");
                return null;
            }

            return new EquipmentInstance(definition, data.level);
        }
    }
}