using System;

namespace Data
{
    /// <summary>
    /// Flat stat bundle for characters, enemies, and equipment.
    /// </summary>
    [Serializable]
    public struct StatBlock
    {
        public int HP;
        public int Attack;
        public int Defense;
        public int Speed;

        // Hooks for future features.
        public int LifeSteal;
        public int Tenacity;
        public int Evasion;
        public int Crit;
        public int Accuracy;
        public int MaxEnergy;

        public static StatBlock Zero => default;

        /// <summary>
        /// Reads a stat value by type.
        /// </summary>
        public int Get(StatType type)
        {
            return type switch
            {
                StatType.HP => HP,
                StatType.Defense => Defense,
                StatType.LifeSteal => LifeSteal,
                StatType.Tenacity => Tenacity,
                StatType.Evasion => Evasion,
                StatType.Attack => Attack,
                StatType.Crit => Crit,
                StatType.Accuracy => Accuracy,
                StatType.MaxEnergy => MaxEnergy,
                StatType.Speed => Speed,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        /// <summary>
        /// Writes a stat value by type.
        /// </summary>
        public void Set(StatType type, int value)
        {
            switch (type)
            {
                case StatType.HP: HP = value; break;
                case StatType.Defense: Defense = value; break;
                case StatType.LifeSteal: LifeSteal = value; break;
                case StatType.Tenacity: Tenacity = value; break;
                case StatType.Evasion: Evasion = value; break;
                case StatType.Attack: Attack = value; break;
                case StatType.Crit: Crit = value; break;
                case StatType.Accuracy: Accuracy = value; break;
                case StatType.MaxEnergy: MaxEnergy = value; break;
                case StatType.Speed: Speed = value; break;
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        /// <summary>
        /// Adds two stat blocks field by field. Used when combining base stats and bonuses.
        /// </summary>
        public static StatBlock operator +(StatBlock a, StatBlock b)
        {
            return new StatBlock
            {
                HP = a.HP + b.HP,
                Attack = a.Attack + b.Attack,
                Defense = a.Defense + b.Defense,
                Speed = a.Speed + b.Speed,
                LifeSteal = a.LifeSteal + b.LifeSteal,
                Tenacity = a.Tenacity + b.Tenacity,
                Evasion = a.Evasion + b.Evasion,
                Crit = a.Crit + b.Crit,
                Accuracy = a.Accuracy + b.Accuracy,
                MaxEnergy = a.MaxEnergy + b.MaxEnergy
            };
        }
    }
}