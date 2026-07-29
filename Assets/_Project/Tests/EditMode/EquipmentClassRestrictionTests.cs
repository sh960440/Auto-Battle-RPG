using System;
using Data;
using NUnit.Framework;

namespace Data.Tests
{
    public class EquipmentClassRestrictionTests
    {
        [Test]
        public void CanBeEquippedBy_OnlyMatchingClass()
        {
            var sword = EquipmentDefinition.CreateRuntime(
                "Sword",
                EquipmentSlot.RightHand,
                CharacterClass.Knight,
                baseMainStat: 3);

            Assert.IsTrue(sword.CanBeEquippedBy(CharacterClass.Knight));
            Assert.IsFalse(sword.CanBeEquippedBy(CharacterClass.Swordsman));
            Assert.IsFalse(sword.CanBeEquippedBy(CharacterClass.ShieldGuard));
        }

        [Test]
        public void Equip_AllowsMatchingClass()
        {
            var knight = CreateCharacter(CharacterClass.Knight);
            var sword = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(
                    "Sword",
                    EquipmentSlot.RightHand,
                    CharacterClass.Knight,
                    baseMainStat: 3),
                level: 1);

            Assert.IsTrue(knight.CanEquip(sword));
            Assert.IsNull(knight.Equip(sword));
            Assert.AreSame(sword, knight.GetEquipment(EquipmentSlot.RightHand));
        }

        [Test]
        public void Equip_RejectsMismatchedClass()
        {
            var swordsman = CreateCharacter(CharacterClass.Swordsman);
            var sword = new EquipmentInstance(
                EquipmentDefinition.CreateRuntime(
                    "Sword",
                    EquipmentSlot.RightHand,
                    CharacterClass.Knight,
                    baseMainStat: 3),
                level: 1);

            Assert.IsFalse(swordsman.CanEquip(sword));
            Assert.Throws<InvalidOperationException>(() => swordsman.Equip(sword));
            Assert.IsNull(swordsman.GetEquipment(EquipmentSlot.RightHand));
        }

        private static CharacterInstance CreateCharacter(CharacterClass characterClass)
        {
            var definition = CharacterDefinition.CreateRuntime(
                characterClass.ToString(),
                characterClass,
                new StatBlock { HP = 100, Attack = 10, Defense = 5, Speed = 10 });

            return new CharacterInstance(definition, level: 1);
        }
    }
}