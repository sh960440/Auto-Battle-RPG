using System;
using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// Runtime player save data.
    /// </summary>
    public class PlayerProfile
    {
        private readonly List<CharacterInstance> _characters = new();
        private readonly List<EquipmentInstance> _inventory = new();
        private int _gold;
        private int _selectedIndex;

        public PlayerProfile(int startingGold = 0)
        {
            _gold = Math.Max(0, startingGold);
        }

        public int Gold => _gold;

        public IReadOnlyList<CharacterInstance> Characters => _characters;

        public IReadOnlyList<EquipmentInstance> Inventory => _inventory;

        /// <summary>
        /// Currently selected character.
        /// </summary>
        public CharacterInstance SelectedCharacter
        {
            get
            {
                if (_characters.Count == 0)
                    return null;

                return _characters[_selectedIndex];
            }
        }

        /// <summary>
        /// Index of the selected character in <see cref="Characters"/>.
        /// </summary>
        public int SelectedIndex => _selectedIndex;

        /// <summary>
        /// Adds a character to the roster. Selects it when it is the first character.
        /// </summary>
        public void AddCharacter(CharacterInstance character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            _characters.Add(character);

            if (_characters.Count == 1)
                _selectedIndex = 0;
        }

        /// <summary>
        /// Selects the character at <paramref name="index"/>.
        /// </summary>
        public void SelectCharacter(int index)
        {
            if (index < 0 || index >= _characters.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            _selectedIndex = index;
        }

        /// <summary>
        /// Selects the given roster character.
        /// </summary>
        public void SelectCharacter(CharacterInstance character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            var index = _characters.IndexOf(character);
            if (index < 0)
                throw new InvalidOperationException("Character is not in this profile roster.");

            _selectedIndex = index;
        }

        /// <summary>
        /// Adds gold.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            _gold += amount;
        }

        /// <summary>
        /// Adds victory gold and an optional inventory item.
        /// </summary>
        public void ApplyLoot(LootDrop drop)
        {
            if (drop == null)
                return;

            AddGold(drop.Gold);
            if (drop.HasItem)
                AddToInventory(drop.Item);
        }

        /// <summary>
        /// Spends gold when the balance is enough.
        /// </summary>
        /// <returns><c>true</c> when the spend succeeded.</returns>
        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
                return true;

            if (_gold < amount)
                return false;

            _gold -= amount;
            return true;
        }

        /// <summary>
        /// Adds an unequipped item to the equipment center.
        /// </summary>
        public void AddToInventory(EquipmentInstance item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _inventory.Add(item);
        }

        /// <summary>
        /// Removes an item from inventory when present.
        /// </summary>
        public bool RemoveFromInventory(EquipmentInstance item)
        {
            if (item == null)
                return false;

            return _inventory.Remove(item);
        }

        /// <summary>
        /// Creates a starter profile from character definitions.
        /// </summary>
        public static PlayerProfile CreateStarter(int startingGold, params CharacterDefinition[] definitions)
        {
            var profile = new PlayerProfile(startingGold);
            if (definitions == null)
                return profile;

            for (var i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] != null)
                    profile.AddCharacter(new CharacterInstance(definitions[i], level: 1));
            }

            return profile;
        }
    }
}