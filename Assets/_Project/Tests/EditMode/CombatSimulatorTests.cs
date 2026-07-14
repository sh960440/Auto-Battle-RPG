using Combat;
using Data;
using NUnit.Framework;

namespace Combat.Tests
{
    public class CombatSimulatorTests
    {
        [Test]
        public void Duel_PlayerWinsAgainstWeakerEnemy()
        {
            var player = CreateUnit("Warrior", hp: 100, attack: 20, defense: 5, speed: 10, isPlayer: true);
            var enemy = CreateUnit("Goblin", hp: 30, attack: 5, defense: 2, speed: 8, isPlayer: false);

            var sim = new CombatSimulator(new CombatContext(player, new[] { enemy }), new System.Random(1));

            RunUntilFinished(sim);

            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(CombatResult.Victory, sim.Result);
            Assert.IsTrue(player.IsAlive);
            Assert.IsFalse(enemy.IsAlive);
        }

        [Test]
        public void MutualKill_CountsAsDefeat()
        {
            var player = CreateUnit("Warrior", hp: 50, attack: 10, defense: 0, speed: 10, isPlayer: true);
            var enemy = CreateUnit("Orc", hp: 50, attack: 10, defense: 0, speed: 10, isPlayer: false);

            player.TakeDamage(50);
            enemy.TakeDamage(50);

            var sim = new CombatSimulator(new CombatContext(player, new[] { enemy }), new System.Random(1));
            sim.Tick(0f);

            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(CombatResult.Defeat, sim.Result);
            Assert.IsFalse(player.IsAlive);
            Assert.IsFalse(enemy.IsAlive);
        }

        [Test]
        public void Energy_StopsAtMaxEnergy()
        {
            var unit = CreateUnit("Warrior", hp: 100, attack: 10, defense: 0, speed: 10, isPlayer: true);

            var gained = unit.AddEnergy(CombatUnit.DefaultMaxEnergy + 50);

            Assert.AreEqual(CombatUnit.DefaultMaxEnergy, gained);
            Assert.AreEqual(CombatUnit.DefaultMaxEnergy, unit.Energy);

            Assert.AreEqual(0, unit.AddEnergy(10));
            Assert.AreEqual(CombatUnit.DefaultMaxEnergy, unit.Energy);
        }

        private static CombatUnit CreateUnit(
            string name,
            int hp,
            int attack,
            int defense,
            int speed,
            bool isPlayer)
        {
            return new CombatUnit(
                name,
                new StatBlock
                {
                    HP = hp,
                    Attack = attack,
                    Defense = defense,
                    Speed = speed
                },
                isPlayer);
        }

        private static void RunUntilFinished(CombatSimulator sim, int maxTicks = 100000)
        {
            for (var i = 0; i < maxTicks && !sim.IsFinished; i++)
                sim.Tick(0.1f);

            Assert.IsTrue(sim.IsFinished, "Combat did not finish within tick budget.");
        }
    }
}