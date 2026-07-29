using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EquipmentServiceTests
    {
        [Test]
        public void TryEquipFromInventory_EquipsEmptySlot_AndRemovesFromInventory()
        {
            var (profile, knight, service) = CreateKnightProfile();
            var sword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Knight);

            Assert.IsTrue(service.TryEquipFromInventory(profile, knight, sword));
            Assert.AreSame(sword, knight.GetEquipment(EquipmentSlot.RightHand));
            Assert.AreEqual(0, profile.Inventory.Count);
        }

        [Test]
        public void TryEquipFromInventory_OverwritesSlot_AndDiscardsPrevious()
        {
            var (profile, knight, service) = CreateKnightProfile();
            var oldSword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Knight, "Old Sword");
            var newSword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Knight, "New Sword");

            Assert.IsTrue(service.TryEquipFromInventory(profile, knight, oldSword));

            EquipmentInstance discarded = null;
            CharacterInstance discardedFrom = null;
            service.ItemDiscarded += (character, item) =>
            {
                discardedFrom = character;
                discarded = item;
            };

            Assert.IsTrue(service.TryEquipFromInventory(profile, knight, newSword));
            Assert.AreSame(newSword, knight.GetEquipment(EquipmentSlot.RightHand));
            Assert.AreSame(oldSword, discarded);
            Assert.AreSame(knight, discardedFrom);
            Assert.AreEqual(0, profile.Inventory.Count);
        }

        [Test]
        public void TryEquipFromInventory_RejectsWrongClass_LeavesInventoryUnchanged()
        {
            var (profile, knight, service) = CreateKnightProfile();
            var staff = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Swordsman, "Staff");

            Assert.IsFalse(service.CanEquipFromInventory(profile, knight, staff));
            Assert.IsFalse(service.TryEquipFromInventory(profile, knight, staff));
            Assert.IsNull(knight.GetEquipment(EquipmentSlot.RightHand));
            Assert.AreEqual(1, profile.Inventory.Count);
        }

        [Test]
        public void TryEquipFromInventory_RejectsItemNotInInventory()
        {
            var (profile, knight, service) = CreateKnightProfile();
            var sword = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(
                    "Sword",
                    EquipmentSlot.RightHand,
                    CharacterClass.Knight,
                    baseMainStat: 3),
                level: 1);

            Assert.IsFalse(service.TryEquipFromInventory(profile, knight, sword));
            Assert.IsNull(knight.GetEquipment(EquipmentSlot.RightHand));
        }

        private static (PlayerProfile profile, CharacterInstance knight, EquipmentService service) CreateKnightProfile()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Knight,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 });

            var profile = PlayerProfile.CreateStarter(0, definition);
            return (profile, profile.SelectedCharacter, new EquipmentService());
        }

        private static EquipmentInstance CreateInventoryItem(
            PlayerProfile profile,
            EquipmentSlot slot,
            CharacterClass requiredClass,
            string name = "Item")
        {
            var item = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(name, slot, requiredClass, baseMainStat: 3),
                level: 1);
            profile.AddToInventory(item);
            return item;
        }
    }
}