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
            var rules = CombatRules.CreateRuntimeDefaults();
            var player = CreateUnit("Warrior", hp: 100, attack: 20, defense: 5, speed: 10, isPlayer: true, rules);
            var enemy = CreateUnit("Goblin", hp: 30, attack: 5, defense: 2, speed: 8, isPlayer: false, rules);

            var sim = new CombatSimulator(
                new CombatContext(player, new[] { enemy }),
                rules,
                new System.Random(1));

            RunUntilFinished(sim);

            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(CombatResult.Victory, sim.Result);
            Assert.IsTrue(player.IsAlive);
            Assert.IsFalse(enemy.IsAlive);
        }

        [Test]
        public void MutualKill_CountsAsDefeat()
        {
            var rules = CombatRules.CreateRuntimeDefaults();
            var player = CreateUnit("Warrior", hp: 50, attack: 10, defense: 0, speed: 10, isPlayer: true, rules);
            var enemy = CreateUnit("Orc", hp: 50, attack: 10, defense: 0, speed: 10, isPlayer: false, rules);

            player.TakeDamage(50);
            enemy.TakeDamage(50);

            var sim = new CombatSimulator(
                new CombatContext(player, new[] { enemy }),
                rules,
                new System.Random(1));
            sim.Tick(0f);

            Assert.IsTrue(sim.IsFinished);
            Assert.AreEqual(CombatResult.Defeat, sim.Result);
            Assert.IsFalse(player.IsAlive);
            Assert.IsFalse(enemy.IsAlive);
        }

        [Test]
        public void Energy_StopsAtMaxEnergy()
        {
            var rules = CombatRules.CreateRuntimeDefaults();
            var unit = CreateUnit("Warrior", hp: 100, attack: 10, defense: 0, speed: 10, isPlayer: true, rules);

            var gained = unit.AddEnergy(rules.DefaultMaxEnergy + 50);

            Assert.AreEqual(rules.DefaultMaxEnergy, gained);
            Assert.AreEqual(rules.DefaultMaxEnergy, unit.Energy);

            Assert.AreEqual(0, unit.AddEnergy(10));
            Assert.AreEqual(rules.DefaultMaxEnergy, unit.Energy);
        }

        [Test]
        public void BasicAttack_GrantsEnergyFromCombatRules()
        {
            var rules = CombatRules.CreateRuntimeDefaults();
            var player = CreateUnit("Warrior", hp: 100, attack: 50, defense: 0, speed: 100, isPlayer: true, rules);
            var enemy = CreateUnit("Dummy", hp: 500, attack: 1, defense: 0, speed: 1, isPlayer: false, rules);

            var sim = new CombatSimulator(
                new CombatContext(player, new[] { enemy }),
                rules,
                new System.Random(1));

            // One player turn at Speed 100 fills ATB in 1 second.
            sim.Tick(1f);

            Assert.AreEqual(rules.EnergyOnAttack, player.Energy);
            Assert.AreEqual(rules.EnergyOnHit, enemy.Energy);
        }

        private static CombatUnit CreateUnit(
            string name,
            int hp,
            int attack,
            int defense,
            int speed,
            bool isPlayer,
            CombatRules rules)
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
                isPlayer,
                rules);
        }

        private static void RunUntilFinished(CombatSimulator sim, int maxTicks = 100000)
        {
            for (var i = 0; i < maxTicks && !sim.IsFinished; i++)
                sim.Tick(0.1f);

            Assert.IsTrue(sim.IsFinished, "Combat did not finish within tick budget.");
        }
    }
}