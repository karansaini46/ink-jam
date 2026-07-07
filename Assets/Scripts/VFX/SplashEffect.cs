using System.Collections;
using UnityEngine;

namespace InkJam.VFX
{
    public class SplashEffect : MonoBehaviour
    {
        private SpriteRenderer _sr;

        public static void Spawn(Sprite splashSprite, Vector3 position, Color color, float duration)
        {
            var go = new GameObject("SplashEffect");
            go.transform.position = position;
            go.transform.localScale = Vector3.zero;
            
            // Random rotation for variety
            go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            var effect = go.AddComponent<SplashEffect>();
            effect.Init(splashSprite, color, duration);
        }

        private void Init(Sprite splashSprite, Color color, float duration)
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = splashSprite;
            _sr.color = color;
            _sr.sortingOrder = 5; // Above background, below tiles

            StartCoroutine(AnimateSplash(duration));
        }

        private IEnumerator AnimateSplash(float duration)
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one * 1.5f;
            Color startColor = _sr.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Fast pop in scale, slower fade
                float scaleT = Mathf.Clamp01(t * 3f); // Reaches max scale at 33% of duration
                transform.localScale = Vector3.Lerp(startScale, endScale, Mathf.SmoothStep(0f, 1f, scaleT));
                
                // Linear fade
                _sr.color = Color.Lerp(startColor, endColor, t);
                
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
