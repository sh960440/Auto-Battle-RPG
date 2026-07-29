using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EquipmentLevelRulesTests
    {
        [Test]
        public void GetLevelForStage_TracksStageOneToOne_UntilCap()
        {
            Assert.AreEqual(1, EquipmentLevelRules.GetLevelForStage(1));
            Assert.AreEqual(10, EquipmentLevelRules.GetLevelForStage(10));
            Assert.AreEqual(20, EquipmentLevelRules.GetLevelForStage(20));
            Assert.AreEqual(20, EquipmentLevelRules.GetLevelForStage(99));
        }

        [Test]
        public void GetLevelForStage_ClampsNonPositiveStageToMinLevel()
        {
            Assert.AreEqual(EquipmentLevelRules.MinLevel, EquipmentLevelRules.GetLevelForStage(0));
            Assert.AreEqual(EquipmentLevelRules.MinLevel, EquipmentLevelRules.GetLevelForStage(-5));
        }

        [Test]
        public void CreateForStage_UsesStageDerivedLevel()
        {
            var definition = EquipmentDefinition.CreateRuntime(
                "Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 3);

            var item = EquipmentInstance.CreateForStage(definition, stage: 7);

            Assert.AreEqual(7, item.Level);
            Assert.AreSame(definition, item.Definition);
        }

        [Test]
        public void CreateForStage_CapsLevelAtMax()
        {
            var definition = EquipmentDefinition.CreateRuntime(
                "Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 3);

            var item = EquipmentInstance.CreateForStage(definition, stage: 50);

            Assert.AreEqual(EquipmentLevelRules.MaxLevel, item.Level);
        }
    }
}