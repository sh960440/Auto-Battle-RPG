using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EquipmentSellRulesTests
    {
        [Test]
        public void GetQualityMultiplier_MatchesAgreedTable()
        {
            Assert.AreEqual(1, EquipmentSellRules.GetQualityMultiplier(EquipmentQuality.Common));
            Assert.AreEqual(2, EquipmentSellRules.GetQualityMultiplier(EquipmentQuality.Uncommon));
            Assert.AreEqual(3, EquipmentSellRules.GetQualityMultiplier(EquipmentQuality.Rare));
            Assert.AreEqual(5, EquipmentSellRules.GetQualityMultiplier(EquipmentQuality.Epic));
            Assert.AreEqual(8, EquipmentSellRules.GetQualityMultiplier(EquipmentQuality.Legendary));
        }

        [Test]
        public void GetSellPrice_IsMultiplierTimesLevel()
        {
            var shield = CreateItem(EquipmentQuality.Common, level: 8);
            var rare = CreateItem(EquipmentQuality.Rare, level: 8);

            Assert.AreEqual(8, EquipmentSellRules.GetSellPrice(shield));
            Assert.AreEqual(24, EquipmentSellRules.GetSellPrice(rare));
        }

        [Test]
        public void GetSellPrice_ReturnsZeroWhenItemIsNull()
        {
            Assert.AreEqual(0, EquipmentSellRules.GetSellPrice(null));
        }

        private static EquipmentInstance CreateItem(EquipmentQuality quality, int level)
        {
            var definition = EquipmentDefinition.CreateRuntime(
                "Wooden Shield",
                EquipmentSlot.LeftHand,
                CharacterClass.Priestess,
                baseMainStat: 2,
                quality: quality);
            return new EquipmentInstance(definition, level);
        }
    }
}