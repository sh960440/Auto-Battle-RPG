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

        public CharacterDefinition[] Characters => _characters;

        public EquipmentDefinition[] Equipment => _equipment;

        /// <summary>
        /// Creates a runtime catalog for tests.
        /// </summary>
        public static ContentCatalog CreateRuntime(
            CharacterDefinition[] characters,
            EquipmentDefinition[] equipment)
        {
            var catalog = CreateInstance<ContentCatalog>();
            catalog.name = "ContentCatalog_Runtime";
            catalog._characters = characters ?? Array.Empty<CharacterDefinition>();
            catalog._equipment = equipment ?? Array.Empty<EquipmentDefinition>();
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