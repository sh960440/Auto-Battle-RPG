using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EnemyStageScalingTests
    {
        private static readonly StatBlock BaseStats = new StatBlock
        {
            HP = 50,
            Attack = 10,
            Defense = 4,
            Speed = 10
        };

        [Test]
        public void Scale_Stage1_LeavesBaseStatsUnchanged()
        {
            var scaled = EnemyStageScaling.Scale(BaseStats, 1);

            Assert.AreEqual(50, scaled.HP);
            Assert.AreEqual(10, scaled.Attack);
            Assert.AreEqual(4, scaled.Defense);
            Assert.AreEqual(10, scaled.Speed);
        }

        [Test]
        public void Scale_GrowsHpAttackDefenseLinearly()
        {
            // Stage 11 = +10 steps → HP +60%, Attack/Defense +50%.
            var scaled = EnemyStageScaling.Scale(BaseStats, 11);

            Assert.AreEqual(80, scaled.HP);
            Assert.AreEqual(15, scaled.Attack);
            Assert.AreEqual(6, scaled.Defense);
        }

        [Test]
        public void Scale_StopsGrowingSpeedAfterCapStage()
        {
            var atCap = EnemyStageScaling.Scale(BaseStats, EnemyStageScaling.SpeedScaleCapStage);
            var farPastCap = EnemyStageScaling.Scale(BaseStats, 500);

            Assert.AreEqual(atCap.Speed, farPastCap.Speed);
            Assert.Greater(farPastCap.HP, atCap.HP);
            Assert.Greater(farPastCap.Attack, atCap.Attack);
        }
    }
}
