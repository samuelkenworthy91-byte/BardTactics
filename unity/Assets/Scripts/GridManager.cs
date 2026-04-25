using UnityEngine;

namespace BardTactics
{
    /// <summary>
    /// Unity-side equivalent of Phaser board/tile math from src/main.js.
    /// Responsible for tile/world conversion and basic map walkability.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public const int TileSize = 48;
        public const int Cols = 8;
        public const int Rows = 8;

        // Mirrors MAP structure in main.js (street/cover/wall/gate).
        // [y, x] indexing.
        private readonly string[,] _map =
        {
            { "street", "cover", "street", "street", "street", "street", "gate",  "street" },
            { "street", "street", "cover", "street", "street", "cover", "street", "street" },
            { "street", "wall",  "wall",  "street", "street", "street", "street", "street" },
            { "street", "street", "street", "cover", "street", "street", "street", "street" },
            { "street", "street", "cover", "street", "street", "wall",  "wall",  "street" },
            { "street", "street", "street", "street", "cover", "street", "street", "street" },
            { "street", "cover", "street", "street", "street", "street", "street", "street" },
            { "street", "street", "street", "street", "street", "street", "street", "street" }
        };

        [Header("Board Origin (world)")]
        [SerializeField] private Vector2 boardOrigin = new Vector2(-192f, -192f);

        public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < Cols && y < Rows;

        public bool IsWalkable(int x, int y)
        {
            return IsInside(x, y) && _map[y, x] != "wall";
        }

        public string GetTileType(int x, int y)
        {
            if (!IsInside(x, y))
            {
                return "void";
            }

            return _map[y, x];
        }

        public Vector3 TileToWorldCenter(int x, int y)
        {
            float wx = boardOrigin.x + (x * TileSize) + (TileSize * 0.5f);
            float wy = boardOrigin.y + (y * TileSize) + (TileSize * 0.5f);
            return new Vector3(wx, wy, 0f);
        }

        public bool WorldToTile(Vector3 world, out Vector2Int tile)
        {
            float lx = world.x - boardOrigin.x;
            float ly = world.y - boardOrigin.y;

            int tx = Mathf.FloorToInt(lx / TileSize);
            int ty = Mathf.FloorToInt(ly / TileSize);
            tile = new Vector2Int(tx, ty);
            return IsInside(tx, ty);
        }
    }
}
