using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class StatCalculatorTests
    {
        [Test]
        public void Calculate_LevelOneBare_ReturnsBaseStats()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Warrior",
                CharacterClass.Warrior,
                new StatBlock { HP = 100, Attack = 12, Defense = 4, Speed = 10 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }));

            var character = new CharacterInstance(definition, level: 1);
            var stats = StatCalculator.Calculate(character);

            Assert.AreEqual(100, stats.HP);
            Assert.AreEqual(12, stats.Attack);
            Assert.AreEqual(4, stats.Defense);
            Assert.AreEqual(10, stats.Speed);
        }

        [Test]
        public void Calculate_AppliesLevelBonusFromUpgradeCurve()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Warrior",
                CharacterClass.Warrior,
                new StatBlock { HP = 100, Attack = 12, Defense = 4, Speed = 10 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }));

            var character = new CharacterInstance(definition, level: 5);
            var stats = StatCalculator.Calculate(character);

            // Level 5 => 4 * per-level bonus
            Assert.AreEqual(120, stats.HP);
            Assert.AreEqual(16, stats.Attack);
            Assert.AreEqual(8, stats.Defense);
            Assert.AreEqual(10, stats.Speed);
        }

        [Test]
        public void Calculate_AddsEquippedMainStats_UsingEquipmentLevelCurve()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Warrior",
                CharacterClass.Warrior,
                new StatBlock { HP = 100, Attack = 12, Defense = 4, Speed = 10 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }));

            var growth = EquipmentGrowthCurve.CreateRuntime(
                tier1MaxLevel: 10,
                tier1BonusPerLevel: 1);

            var sword = EquipmentDefinition.CreateRuntime(
                "Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Warrior,
                StatType.Attack,
                baseMainStat: 3,
                growth);

            var vest = EquipmentDefinition.CreateRuntime(
                "Vest",
                EquipmentSlot.UpperBody,
                CharacterClass.Warrior,
                StatType.Defense,
                baseMainStat: 2,
                growth);

            var pants = EquipmentDefinition.CreateRuntime(
                "Pants",
                EquipmentSlot.LowerBody,
                CharacterClass.Warrior,
                StatType.HP,
                baseMainStat: 10,
                growth);

            var character = new CharacterInstance(definition, level: 3);
            character.Equip(new EquipmentInstance(sword, level: 5)); // 3 + 4 = 7 Attack
            character.Equip(new EquipmentInstance(vest, level: 3));  // 2 + 2 = 4 Defense
            character.Equip(new EquipmentInstance(pants, level: 1)); // 10 HP

            var stats = StatCalculator.Calculate(character);

            // Base + level(2 * bonus) + gear
            Assert.AreEqual(100 + 10 + 10, stats.HP);
            Assert.AreEqual(12 + 2 + 7, stats.Attack);
            Assert.AreEqual(4 + 2 + 4, stats.Defense);
            Assert.AreEqual(10, stats.Speed);
        }
    }
}