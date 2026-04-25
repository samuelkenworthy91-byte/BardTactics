using System.Collections.Generic;
using UnityEngine;

namespace BardTactics
{
    /// <summary>
    /// First-pass Unity port of the Phaser turn/click flow.
    /// Keeps selection/move/target logic centralized and grid-based.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        public enum Phase
        {
            Player,
            Enemy
        }

        [SerializeField] private Camera worldCamera;
        [SerializeField] private GridManager grid;
        [SerializeField] private List<UnitController> units = new();

        [Header("Runtime State")]
        [SerializeField] private Phase phase = Phase.Player;
        [SerializeField] private bool busy;
        [SerializeField] private bool previewOpen;
        [SerializeField] private UnitController selectedUnit;

        private readonly List<Vector2Int> moveTiles = new();
        private readonly List<Vector2Int> targetTiles = new();

        private void Awake()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleBoardClickFromScreen(Input.mousePosition);
            }
        }

        public void HandleBoardClickFromScreen(Vector3 screenPosition)
        {
            if (phase != Phase.Player || busy || previewOpen || worldCamera == null || grid == null)
            {
                return;
            }

            Vector3 world = worldCamera.ScreenToWorldPoint(screenPosition);
            world.z = 0f;

            if (!grid.WorldToTile(world, out Vector2Int tile))
            {
                return;
            }

            HandleBoardTileClick(tile.x, tile.y);
        }

        public void HandleBoardTileClick(int tileX, int tileY)
        {
            if (phase != Phase.Player || busy || previewOpen)
            {
                return;
            }

            UnitController clickedUnit = GetUnitAt(tileX, tileY);

            if (clickedUnit != null && clickedUnit.Team == Team.Player && !clickedUnit.Acted)
            {
                selectedUnit = clickedUnit;
                moveTiles.Clear();
                moveTiles.AddRange(GetReachableTiles(clickedUnit));
                targetTiles.Clear();
                targetTiles.AddRange(GetAttackableEnemyTiles(clickedUnit));
                return;
            }

            if (selectedUnit != null &&
                clickedUnit != null &&
                clickedUnit.Team == Team.Enemy &&
                ContainsTile(targetTiles, tileX, tileY))
            {
                // Combat preview / resolution hook.
                return;
            }

            if (selectedUnit != null && ContainsTile(moveTiles, tileX, tileY))
            {
                MoveUnit(selectedUnit, tileX, tileY);
                return;
            }

            ClearSelection();
        }

        public void HandleUnitClicked(UnitController unit)
        {
            if (unit == null)
            {
                return;
            }

            HandleBoardTileClick(unit.TileX, unit.TileY);
        }

        private UnitController GetUnitAt(int x, int y)
        {
            for (int i = 0; i < units.Count; i++)
            {
                UnitController u = units[i];
                if (u != null && u.TileX == x && u.TileY == y)
                {
                    return u;
                }
            }

            return null;
        }

        private void MoveUnit(UnitController unit, int x, int y)
        {
            unit.SetTilePosition(x, y, grid);
            selectedUnit = unit;
            moveTiles.Clear();
            targetTiles.Clear();
            targetTiles.AddRange(GetAttackableEnemyTiles(unit));

            if (targetTiles.Count == 0)
            {
                unit.Acted = true;
                ClearSelection();
            }
        }

        private void ClearSelection()
        {
            selectedUnit = null;
            moveTiles.Clear();
            targetTiles.Clear();
        }

        private IEnumerable<Vector2Int> GetReachableTiles(UnitController unit)
        {
            List<Vector2Int> results = new();
            Queue<(Vector2Int tile, int dist)> queue = new();
            HashSet<Vector2Int> visited = new();

            Vector2Int start = new(unit.TileX, unit.TileY);
            queue.Enqueue((start, 0));
            visited.Add(start);

            Vector2Int[] dirs =
            {
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(0, 1),
                new Vector2Int(0, -1)
            };

            while (queue.Count > 0)
            {
                (Vector2Int pos, int dist) = queue.Dequeue();

                if (dist > 0)
                {
                    results.Add(pos);
                }

                if (dist >= unit.Move)
                {
                    continue;
                }

                for (int i = 0; i < dirs.Length; i++)
                {
                    Vector2Int next = pos + dirs[i];
                    if (visited.Contains(next))
                    {
                        continue;
                    }

                    if (!grid.IsWalkable(next.x, next.y))
                    {
                        continue;
                    }

                    UnitController occupant = GetUnitAt(next.x, next.y);
                    if (occupant != null && occupant != unit)
                    {
                        continue;
                    }

                    visited.Add(next);
                    queue.Enqueue((next, dist + 1));
                }
            }

            return results;
        }

        private IEnumerable<Vector2Int> GetAttackableEnemyTiles(UnitController unit)
        {
            List<Vector2Int> results = new();
            WeaponData active = unit.ActiveWeapon;
            int range = active == null ? 1 : active.Range;

            for (int i = 0; i < units.Count; i++)
            {
                UnitController other = units[i];
                if (other == null || other.Team == unit.Team)
                {
                    continue;
                }

                int md = Mathf.Abs(other.TileX - unit.TileX) + Mathf.Abs(other.TileY - unit.TileY);
                if (md == range)
                {
                    results.Add(new Vector2Int(other.TileX, other.TileY));
                }
            }

            return results;
        }

        private static bool ContainsTile(List<Vector2Int> list, int x, int y)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].x == x && list[i].y == y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
