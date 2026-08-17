using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class ProgressionServiceTests
    {
        [Test]
        public void TryUpgrade_SpendsGoldAndRaisesLevel()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 4, Speed = 9 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }));

            var profile = PlayerProfile.CreateStarter(200, definition);
            var character = profile.SelectedCharacter;
            var service = new ProgressionService();

            Assert.AreEqual(1, character.Level);
            Assert.IsTrue(service.TryUpgrade(profile, character));
            Assert.AreEqual(2, character.Level);
            Assert.AreEqual(150, profile.Gold);
        }

        [Test]
        public void TryUpgrade_FailsWhenGoldIsNotEnough()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 4, Speed = 9 },
                UpgradeCurve.CreateRuntime(new StatBlock { HP = 5, Attack = 1, Defense = 1 }));

            var profile = PlayerProfile.CreateStarter(10, definition);
            var service = new ProgressionService();

            Assert.IsFalse(service.TryUpgrade(profile, profile.SelectedCharacter));
            Assert.AreEqual(1, profile.SelectedCharacter.Level);
            Assert.AreEqual(10, profile.Gold);
        }
    }
}
