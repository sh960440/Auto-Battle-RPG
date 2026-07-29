using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EquipmentMainStatRulesTests
    {
        [Test]
        public void GetMainStatType_MapsSlotsPerGdd()
        {
            Assert.AreEqual(StatType.Attack, EquipmentMainStatRules.GetMainStatType(EquipmentSlot.LeftHand));
            Assert.AreEqual(StatType.Attack, EquipmentMainStatRules.GetMainStatType(EquipmentSlot.RightHand));
            Assert.AreEqual(StatType.Defense, EquipmentMainStatRules.GetMainStatType(EquipmentSlot.UpperBody));
            Assert.AreEqual(StatType.HP, EquipmentMainStatRules.GetMainStatType(EquipmentSlot.LowerBody));
        }

        [Test]
        public void EquipmentDefinition_MainStatType_FollowsSlot()
        {
            var left = EquipmentDefinition.CreateRuntime("Dagger", EquipmentSlot.LeftHand, CharacterClass.ShieldGuard, 2);
            var right = EquipmentDefinition.CreateRuntime("Sword", EquipmentSlot.RightHand, CharacterClass.Knight, 3);
            var upper = EquipmentDefinition.CreateRuntime("Vest", EquipmentSlot.UpperBody, CharacterClass.Knight, 2);
            var lower = EquipmentDefinition.CreateRuntime("Pants", EquipmentSlot.LowerBody, CharacterClass.Swordsman, 10);

            Assert.AreEqual(StatType.Attack, left.MainStatType);
            Assert.AreEqual(StatType.Attack, right.MainStatType);
            Assert.AreEqual(StatType.Defense, upper.MainStatType);
            Assert.AreEqual(StatType.HP, lower.MainStatType);
        }
    }
}