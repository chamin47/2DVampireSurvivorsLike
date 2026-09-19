using System.Collections.Generic;
using UnityEngine;

namespace DawnFarm
{
    public sealed class FarmWorldTiler : MonoBehaviour
    {
        private readonly List<Transform> tiles = new List<Transform>();
        private Transform target;
        private Sprite[] sprites;
        private int columns;
        private int rows;
        private Vector2 cellSize;
        private Vector2Int lastCenter = new Vector2Int(int.MinValue, int.MinValue);

        public void Configure(Transform followTarget, Sprite[] tileSprites, int width = 13, int height = 9)
        {
            target = followTarget;
            sprites = tileSprites;
            columns = width;
            rows = height;
            if (sprites == null || sprites.Length == 0) return;
            cellSize = sprites[0].bounds.size;
            if (cellSize.x <= 0f || cellSize.y <= 0f) cellSize = Vector2.one * 1.6f;
            for (int i = 0; i < columns * rows; i++)
            {
                var tile = new GameObject($"FarmTile_{i:000}");
                tile.transform.SetParent(transform, false);
                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprites[i % sprites.Length];
                renderer.sortingOrder = -100;
                tiles.Add(tile.transform);
            }
            Refresh(true);
        }

        private void LateUpdate() => Refresh(false);

        private void Refresh(bool force)
        {
            if (target == null || tiles.Count == 0) return;
            var center = new Vector2Int(Mathf.RoundToInt(target.position.x / cellSize.x), Mathf.RoundToInt(target.position.y / cellSize.y));
            if (!force && center == lastCenter) return;
            lastCenter = center;
            int index = 0;
            for (int y = -rows / 2; y <= rows / 2; y++)
                for (int x = -columns / 2; x <= columns / 2 && index < tiles.Count; x++, index++)
                    tiles[index].position = new Vector3((center.x + x) * cellSize.x, (center.y + y) * cellSize.y, 1f);
        }
    }

    public sealed class FarmCameraFollow : MonoBehaviour
    {
        public Transform Target { get; set; }
        private void LateUpdate()
        {
            if (Target == null) return;
            Vector3 desired = new Vector3(Target.position.x, Target.position.y, -10f);
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
        }
    }
}
