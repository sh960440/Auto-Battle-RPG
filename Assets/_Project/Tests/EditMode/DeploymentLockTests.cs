using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class DeploymentLockTests
    {
        [Test]
        public void TryLockDeployment_SnapshotsSelectedCharacter_AndBlocksSwitching()
        {
            var knight = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 });
            var swordsman = CharacterDefinition.CreateRuntime(
                "Swordsman",
                CharacterClass.Swordsman,
                new StatBlock { HP = 80, Attack = 16, Defense = 3, Speed = 12 });

            var profile = PlayerProfile.CreateStarter(0, knight, swordsman);
            var service = new PlayerProfileService(profile);

            Assert.IsTrue(service.TryLockDeployment());
            Assert.IsTrue(service.IsDeploymentLocked);
            Assert.AreSame(profile.Characters[0], service.DeployedCharacter);

            service.SelectNext();
            Assert.AreEqual(0, profile.SelectedIndex);
            Assert.AreSame(profile.Characters[0], service.DeployedCharacter);
        }

        [Test]
        public void TryLockDeployment_SnapshotsFinalStatsAndSkill()
        {
            var skill = SkillDefinition.CreateRuntime("Heavy Strike", 80, 1.5f);
            var knight = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }),
                skill);

            var profile = PlayerProfile.CreateStarter(0, knight);
            profile.SelectedCharacter.SetLevel(3);
            var service = new PlayerProfileService(profile);

            var expected = StatCalculator.Calculate(profile.SelectedCharacter);
            Assert.IsTrue(service.TryLockDeployment());

            Assert.AreEqual(expected.HP, service.DeployedStats.HP);
            Assert.AreEqual(expected.Attack, service.DeployedStats.Attack);
            Assert.AreSame(skill, service.DeployedSkill);

            // Live character changes must not affect the locked combat snapshot.
            profile.SelectedCharacter.SetLevel(10);
            Assert.AreEqual(expected.HP, service.DeployedStats.HP);
            Assert.AreEqual(expected.Attack, service.DeployedStats.Attack);
        }

        [Test]
        public void UnlockDeployment_AllowsSwitchingAgain()
        {
            var knight = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 });
            var swordsman = CharacterDefinition.CreateRuntime(
                "Swordsman",
                CharacterClass.Swordsman,
                new StatBlock { HP = 80, Attack = 16, Defense = 3, Speed = 12 });

            var profile = PlayerProfile.CreateStarter(0, knight, swordsman);
            var service = new PlayerProfileService(profile);

            Assert.IsTrue(service.TryLockDeployment());
            service.UnlockDeployment();

            Assert.IsFalse(service.IsDeploymentLocked);
            service.SelectNext();
            Assert.AreEqual(1, profile.SelectedIndex);
            Assert.AreSame(profile.Characters[1], service.DeployedCharacter);
        }
    }
}