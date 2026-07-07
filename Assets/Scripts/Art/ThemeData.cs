using System;
using System.Collections.Generic;
using UnityEngine;
using InkJam.Core;

namespace InkJam.Art
{
    [Serializable]
    public struct TileSpriteMapping
    {
        public TileColor color;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "NewTheme", menuName = "Ink Jam/Art Theme")]
    public class ThemeData : ScriptableObject
    {
        [Header("Board Elements")]
        public Sprite boardBackgroundSprite;
        public Sprite gridCellSprite;
        public Sprite exitFrameSprite;

        [Header("Obstacles")]
        public Sprite fixedObstacleSprite;
        public Sprite inkBleedSprite;

        [Header("Tile Skins")]
        public List<TileSpriteMapping> tileSprites;

        private Dictionary<TileColor, Sprite> _tileLookup;

        public void Initialize()
        {
            if (_tileLookup == null)
            {
                _tileLookup = new Dictionary<TileColor, Sprite>();
                foreach (var mapping in tileSprites)
                {
                    _tileLookup[mapping.color] = mapping.sprite;
                }
            }
        }

        public Sprite GetTileSprite(TileColor color)
        {
            if (_tileLookup == null) Initialize();
            
            if (_tileLookup != null && _tileLookup.TryGetValue(color, out var sprite))
            {
                return sprite;
            }
            return null; // Fallback to missing sprite
        }
    }
}
