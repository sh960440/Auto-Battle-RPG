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
            var sword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Priestess);

            Assert.IsTrue(service.TryEquipFromInventory(profile, knight, sword));
            Assert.AreSame(sword, knight.GetEquipment(EquipmentSlot.RightHand));
            Assert.AreEqual(0, profile.Inventory.Count);
        }

        [Test]
        public void TryEquipFromInventory_OverwritesSlot_AndDiscardsPrevious()
        {
            var (profile, knight, service) = CreateKnightProfile();
            var oldSword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Priestess, "Old Sword");
            var newSword = CreateInventoryItem(profile, EquipmentSlot.RightHand, CharacterClass.Priestess, "New Sword");

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
                    CharacterClass.Priestess,
                    baseMainStat: 3),
                level: 1);

            Assert.IsFalse(service.TryEquipFromInventory(profile, knight, sword));
            Assert.IsNull(knight.GetEquipment(EquipmentSlot.RightHand));
        }

        [Test]
        public void TrySellFromInventory_RemovesItemAndAddsGold()
        {
            var (profile, _, service) = CreateKnightProfile();
            var shield = CreateInventoryItem(
                profile,
                EquipmentSlot.LeftHand,
                CharacterClass.Swordsman,
                "Wooden Shield",
                level: 8);

            Assert.IsTrue(service.TrySellFromInventory(profile, shield));
            Assert.AreEqual(0, profile.Inventory.Count);
            Assert.AreEqual(8, profile.Gold);
        }

        [Test]
        public void TrySellFromInventory_RejectsItemNotInInventory()
        {
            var (profile, _, service) = CreateKnightProfile();
            var shield = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(
                    "Wooden Shield",
                    EquipmentSlot.LeftHand,
                    CharacterClass.Swordsman,
                    baseMainStat: 2),
                level: 8);

            Assert.IsFalse(service.TrySellFromInventory(profile, shield));
            Assert.AreEqual(0, profile.Gold);
        }

        private static (PlayerProfile profile, CharacterInstance knight, EquipmentService service) CreateKnightProfile()
        {
            var definition = CharacterDefinition.CreateRuntime(
                "Knight",
                CharacterClass.Priestess,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 });

            var profile = PlayerProfile.CreateStarter(0, definition);
            return (profile, profile.SelectedCharacter, new EquipmentService());
        }

        private static EquipmentInstance CreateInventoryItem(
            PlayerProfile profile,
            EquipmentSlot slot,
            CharacterClass requiredClass,
            string name = "Item",
            int level = 1)
        {
            var item = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(name, slot, requiredClass, baseMainStat: 3),
                level);
            profile.AddToInventory(item);
            return item;
        }
    }
}