using Data;
using NUnit.Framework;
using UnityEngine;

namespace Data.Tests
{
    public class GameSaveCodecTests
    {
        [Test]
        public void RoundTrip_PreservesGoldStageLevelsAndEquipment()
        {
            var knight = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 120, Attack = 10, Defense = 4, Speed = 9 });
            knight.name = "Character_Knight";

            var sword = EquipmentDefinition.CreateRuntime(
                "Iron Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 5);
            sword.name = "Equipment_Iron Sword";

            var vest = EquipmentDefinition.CreateRuntime(
                "Padded Coat",
                EquipmentSlot.UpperBody,
                CharacterClass.Knight,
                baseMainStat: 3);
            vest.name = "Equipment_Padded Coat";

            var catalog = ContentCatalog.CreateRuntime(
                new[] { knight },
                new[] { sword, vest });

            var profile = PlayerProfile.CreateStarter(250, knight);
            profile.Characters[0].SetLevel(4);
            profile.Characters[0].Equip(new EquipmentInstance(sword, 7));
            profile.AddToInventory(new EquipmentInstance(vest, 3));

            var data = GameSaveCodec.ToSaveData(profile, currentStage: 12);
            var json = JsonUtility.ToJson(data);
            var loadedData = JsonUtility.FromJson<GameSaveData>(json);
            var loaded = GameSaveCodec.FromSaveData(loadedData, catalog);

            Assert.AreEqual(250, loaded.Gold);
            Assert.AreEqual(12, loadedData.currentStage);
            Assert.AreEqual(1, loaded.Characters.Count);
            Assert.AreEqual(4, loaded.Characters[0].Level);
            Assert.AreEqual(7, loaded.Characters[0].GetEquipment(EquipmentSlot.RightHand).Level);
            Assert.AreEqual("Equipment_Iron Sword", loaded.Characters[0].GetEquipment(EquipmentSlot.RightHand).Definition.name);
            Assert.AreEqual(1, loaded.Inventory.Count);
            Assert.AreEqual("Equipment_Padded Coat", loaded.Inventory[0].Definition.name);
            Assert.AreEqual(3, loaded.Inventory[0].Level);
        }

        [Test]
        public void FromSaveData_SkipsMissingDefinitions()
        {
            var knight = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 8, Defense = 3, Speed = 8 });
            knight.name = "Character_Knight";

            var catalog = ContentCatalog.CreateRuntime(new[] { knight }, System.Array.Empty<EquipmentDefinition>());
            var data = new GameSaveData
            {
                gold = 10,
                currentStage = 2,
                characters = new[]
                {
                    new CharacterSaveData { definitionId = "Character_Knight", level = 2 },
                    new CharacterSaveData { definitionId = "MissingHero", level = 9 }
                },
                inventory = new[]
                {
                    new EquipmentSaveData { definitionId = "MissingItem", level = 5 }
                }
            };

            var profile = GameSaveCodec.FromSaveData(data, catalog);

            Assert.AreEqual(1, profile.Characters.Count);
            Assert.AreEqual(0, profile.Inventory.Count);
            Assert.AreEqual(2, profile.Characters[0].Level);
        }
    }
}