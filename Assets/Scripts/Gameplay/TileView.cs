using UnityEngine;
using InkJam.Core;

namespace InkJam.Gameplay
{
    public class TileView : MonoBehaviour
    {
        private Tile _tile;
        private TextMesh _textMesh;

        public Tile Tile
        {
            get => _tile;
            set
            {
                if (_tile != null)
                {
                    _tile.OnLayerCountChanged -= UpdateLayerIndicator;
                }
                _tile = value;
                if (_tile != null)
                {
                    _tile.OnLayerCountChanged += UpdateLayerIndicator;
                    UpdateLayerIndicator(_tile.LayerCount);
                }
            }
        }

        private void Awake()
        {
            GameObject textObj = new GameObject("LayerIndicator");
            textObj.transform.SetParent(this.transform);
            textObj.transform.localPosition = new Vector3(0, 0, -0.1f);
            
            _textMesh = textObj.AddComponent<TextMesh>();
            _textMesh.characterSize = 0.1f;
            _textMesh.fontSize = 40;
            _textMesh.anchor = TextAnchor.MiddleCenter;
            _textMesh.alignment = TextAlignment.Center;
            _textMesh.color = Color.black;
        }

        private void OnDestroy()
        {
            if (_tile != null)
            {
                _tile.OnLayerCountChanged -= UpdateLayerIndicator;
            }
        }

        private void UpdateLayerIndicator(int layerCount)
        {
            if (_textMesh != null)
            {
                if (layerCount > 0)
                {
                    _textMesh.text = layerCount.ToString();
                }
                else
                {
                    _textMesh.text = "";
                }
            }
        }
    }
}
