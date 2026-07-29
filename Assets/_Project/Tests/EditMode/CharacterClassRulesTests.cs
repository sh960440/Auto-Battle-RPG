using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class CharacterClassRulesTests
    {
        [Test]
        public void GetPrimaryStat_MapsClassesPerDesign()
        {
            Assert.AreEqual(StatType.HP, CharacterClassRules.GetPrimaryStat(CharacterClass.Knight));
            Assert.AreEqual(StatType.Attack, CharacterClassRules.GetPrimaryStat(CharacterClass.Swordsman));
            Assert.AreEqual(StatType.Defense, CharacterClassRules.GetPrimaryStat(CharacterClass.ShieldGuard));
        }
    }
}