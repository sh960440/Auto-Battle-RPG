using Combat;
using Data;
using NUnit.Framework;

namespace Combat.Tests
{
    public class StageProgressRulesTests
    {
        [TestCase(1, CombatResult.Victory, 2)]
        [TestCase(67, CombatResult.Victory, 68)]
        [TestCase(5, CombatResult.Defeat, 2)]
        [TestCase(4, CombatResult.Defeat, 1)]
        [TestCase(3, CombatResult.Defeat, 1)]
        [TestCase(2, CombatResult.Defeat, 1)]
        [TestCase(1, CombatResult.Defeat, 1)]
        public void GetNextStage_AppliesVictoryAndDefeatRules(int current, CombatResult result, int expected)
        {
            Assert.AreEqual(expected, StageProgressRules.GetNextStage(current, result));
        }
    }

    public class StageEncounterTableTests
    {
        [Test]
        public void Contains_RespectsClosedAndOpenEndedBands()
        {
            var early = StageEncounterBand.Create(1, 30);
            var late = StageEncounterBand.Create(31, 999, isOpenEnded: true);

            Assert.IsTrue(early.Contains(1));
            Assert.IsTrue(early.Contains(30));
            Assert.IsFalse(early.Contains(31));

            Assert.IsFalse(late.Contains(30));
            Assert.IsTrue(late.Contains(31));
            Assert.IsTrue(late.Contains(500));
        }

        [Test]
        public void GetBandForStage_UsesMatchingBand_ThenFallsBackToLast()
        {
            var early = StageEncounterBand.Create(1, 30);
            var mid = StageEncounterBand.Create(31, 70);
            var late = StageEncounterBand.Create(71, 100, isOpenEnded: true);
            var table = StageEncounterTable.CreateRuntime(early, mid, late);

            Assert.AreSame(early, table.GetBandForStage(15));
            Assert.AreSame(mid, table.GetBandForStage(50));
            Assert.AreSame(late, table.GetBandForStage(90));
            Assert.AreSame(late, table.GetBandForStage(9999));
        }
    }
}