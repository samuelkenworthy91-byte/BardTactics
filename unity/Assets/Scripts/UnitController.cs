using System.Collections.Generic;
using UnityEngine;

namespace BardTactics
{
    public enum Team
    {
        Player,
        Enemy
    }

    [System.Serializable]
    public class WeaponData
    {
        public string Name;
        public int Damage;
        public int Range;
        public string Kind;
    }

    /// <summary>
    /// Lightweight data + state container for a single unit.
    /// Mirrors important properties from main.js UNITS entries.
    /// </summary>
    public class UnitController : MonoBehaviour
    {
        [Header("Identity")]
        public string UnitId;
        public string DisplayName;
        public Team Team;
        public bool Boss;

        [Header("Grid Position")]
        public int TileX;
        public int TileY;

        [Header("Stats")]
        public int Move = 5;
        public int HP = 10;
        public int MaxHP = 10;
        public int Strength = 3;
        public int Magic = 0;
        public int Defense = 1;
        public int Resistance = 0;
        public int Speed = 4;
        public bool Acted;

        [Header("Weapons")]
        public List<WeaponData> Weapons = new();
        public int ActiveWeaponIndex = 0;

        public WeaponData ActiveWeapon
        {
            get
            {
                if (Weapons == null || Weapons.Count == 0)
                {
                    return null;
                }

                int idx = Mathf.Clamp(ActiveWeaponIndex, 0, Weapons.Count - 1);
                return Weapons[idx];
            }
        }

        public void SetTilePosition(int x, int y, GridManager grid)
        {
            TileX = x;
            TileY = y;
            transform.position = grid.TileToWorldCenter(x, y);
        }
    }
}
