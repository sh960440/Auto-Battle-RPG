using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class LootServiceTests
    {
        [Test]
        public void RollGold_StaysInsideTableRange()
        {
            var table = LootTable.CreateRuntime(10, 12, equipmentDropChance: 0f);
            var service = new LootService();
            var random = new System.Random(1);

            for (var i = 0; i < 20; i++)
            {
                var drop = service.Roll(table, stage: 3, random: random);
                Assert.GreaterOrEqual(drop.Gold, 10);
                Assert.LessOrEqual(drop.Gold, 12);
                Assert.IsFalse(drop.HasItem);
            }
        }

        [Test]
        public void Roll_CreatesItemAtStageLevel()
        {
            var sword = EquipmentDefinition.CreateRuntime(
                "Iron Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 5,
                quality: EquipmentQuality.Common);
            var table = LootTable.CreateRuntime(5, 5, 1f, LootEntry.Create(sword, 1));
            var drop = new LootService().Roll(table, stage: 8, random: new System.Random(2));

            Assert.IsTrue(drop.HasItem);
            Assert.AreEqual(8, drop.Item.Level);
            Assert.AreSame(sword, drop.Item.Definition);
        }

        [Test]
        public void GetQualityWeightMultiplier_ShiftsTowardRareAtHighStage()
        {
            var commonEarly = LootService.GetQualityWeightMultiplier(EquipmentQuality.Common, 1);
            var rareEarly = LootService.GetQualityWeightMultiplier(EquipmentQuality.Rare, 1);
            var commonLate = LootService.GetQualityWeightMultiplier(EquipmentQuality.Common, 40);
            var rareLate = LootService.GetQualityWeightMultiplier(EquipmentQuality.Rare, 40);

            Assert.Greater(commonEarly, rareEarly);
            Assert.Greater(rareLate, commonLate);
        }

        [Test]
        public void Roll_PrefersMatchingClassWhenWeightsAreEqual()
        {
            var knightItem = EquipmentDefinition.CreateRuntime(
                "Knight Blade",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 4);
            var swordItem = EquipmentDefinition.CreateRuntime(
                "Sword Blade",
                EquipmentSlot.RightHand,
                CharacterClass.Swordsman,
                baseMainStat: 4);

            var table = LootTable.CreateRuntime(
                1,
                1,
                1f,
                LootEntry.Create(knightItem, 1),
                LootEntry.Create(swordItem, 1));

            var knightCount = 0;
            var random = new System.Random(11);
            var service = new LootService();
            const int rolls = 80;

            for (var i = 0; i < rolls; i++)
            {
                var drop = service.Roll(table, stage: 1, preferredClass: CharacterClass.Knight, random: random);
                if (drop.Item != null && drop.Item.RequiredClass == CharacterClass.Knight)
                    knightCount++;
            }

            Assert.Greater(knightCount, rolls / 2);
        }

        [Test]
        public void FormatSummary_IncludesGoldAndItemName()
        {
            var sword = EquipmentDefinition.CreateRuntime(
                "Iron Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 5);
            var drop = new LootDrop(12, new EquipmentInstance(sword, 8));

            var text = drop.FormatSummary();

            Assert.IsTrue(text.Contains("+12 Gold"));
            Assert.IsTrue(text.Contains("Iron Sword"));
            Assert.IsTrue(text.Contains("Lv8"));
        }

        [Test]
        public void ApplyLoot_AddsGoldAndInventoryItem()
        {
            var sword = EquipmentDefinition.CreateRuntime(
                "Iron Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 5);
            var profile = PlayerProfile.CreateStarter(100);
            var drop = new LootDrop(12, new EquipmentInstance(sword, 3));

            profile.ApplyLoot(drop);

            Assert.AreEqual(112, profile.Gold);
            Assert.AreEqual(1, profile.Inventory.Count);
            Assert.AreEqual("Iron Sword", profile.Inventory[0].DisplayName);
        }
    }
}