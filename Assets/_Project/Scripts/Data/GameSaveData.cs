using System;

namespace Data
{
    /// <summary>
    /// Serializable equipment entry for JSON saves.
    /// </summary>
    [Serializable]
    public class EquipmentSaveData
    {
        public string definitionId = string.Empty;
        public int level = 1;

        public bool HasItem => !string.IsNullOrEmpty(definitionId);
    }

    /// <summary>
    /// Serializable character entry for JSON saves.
    /// </summary>
    [Serializable]
    public class CharacterSaveData
    {
        public string definitionId = string.Empty;
        public int level = 1;
        public EquipmentSaveData leftHand = new();
        public EquipmentSaveData rightHand = new();
        public EquipmentSaveData upperBody = new();
        public EquipmentSaveData lowerBody = new();
    }

    /// <summary>
    /// Root JSON save payload.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int version = 1;
        public int gold;
        public int currentStage = 1;
        public int selectedCharacterIndex;
        public CharacterSaveData[] characters = Array.Empty<CharacterSaveData>();
        public EquipmentSaveData[] inventory = Array.Empty<EquipmentSaveData>();
    }
}