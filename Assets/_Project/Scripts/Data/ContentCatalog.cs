using System;
using UnityEngine;

namespace Data
{
    /// <summary>
    /// Lookup table for ScriptableObject definitions used when loading a save.
    /// </summary>
    [CreateAssetMenu(fileName = "ContentCatalog_", menuName = "Game Specific/Content Catalog")]
    public class ContentCatalog : ScriptableObject
    {
        [SerializeField] private CharacterDefinition[] _characters = Array.Empty<CharacterDefinition>();
        [SerializeField] private EquipmentDefinition[] _equipment = Array.Empty<EquipmentDefinition>();
        [SerializeField] private EquipmentDefinition[] _starterEquipment = Array.Empty<EquipmentDefinition>();
        [SerializeField] [Min(1)] private int _starterEquipmentStage = 3;

        public CharacterDefinition[] Characters => _characters;

        public EquipmentDefinition[] Equipment => _equipment;

        public EquipmentDefinition[] StarterEquipment => _starterEquipment;

        public int StarterEquipmentStage => Mathf.Max(1, _starterEquipmentStage);

        /// <summary>
        /// Creates a runtime catalog for tests.
        /// </summary>
        public static ContentCatalog CreateRuntime(
            CharacterDefinition[] characters,
            EquipmentDefinition[] equipment,
            EquipmentDefinition[] starterEquipment = null,
            int starterEquipmentStage = 3)
        {
            var catalog = CreateInstance<ContentCatalog>();
            catalog.name = "ContentCatalog_Runtime";
            catalog._characters = characters ?? Array.Empty<CharacterDefinition>();
            catalog._equipment = equipment ?? Array.Empty<EquipmentDefinition>();
            catalog._starterEquipment = starterEquipment ?? Array.Empty<EquipmentDefinition>();
            catalog._starterEquipmentStage = Mathf.Max(1, starterEquipmentStage);
            return catalog;
        }

        /// <summary>
        /// Finds a character definition by Unity asset name.
        /// </summary>
        public CharacterDefinition FindCharacter(string definitionId)
        {
            return FindByName(_characters, definitionId);
        }

        /// <summary>
        /// Finds an equipment definition by Unity asset name.
        /// </summary>
        public EquipmentDefinition FindEquipment(string definitionId)
        {
            return FindByName(_equipment, definitionId);
        }

        /// <summary>
        /// Adds catalog starter gear into an empty inventory.
        /// </summary>
        public void SeedStarterInventory(PlayerProfile profile)
        {
            if (profile == null || profile.Inventory.Count > 0 || _starterEquipment == null)
                return;

            var stage = StarterEquipmentStage;
            for (var i = 0; i < _starterEquipment.Length; i++)
            {
                if (_starterEquipment[i] == null)
                    continue;

                profile.AddToInventory(EquipmentInstance.CreateForStage(_starterEquipment[i], stage));
            }
        }

        private static T FindByName<T>(T[] items, string definitionId) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(definitionId) || items == null)
                return null;

            for (var i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].name == definitionId)
                    return items[i];
            }

            return null;
        }
    }
}