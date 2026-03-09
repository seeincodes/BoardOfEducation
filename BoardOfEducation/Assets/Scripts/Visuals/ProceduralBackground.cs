using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Visuals
{
    [RequireComponent(typeof(RawImage))]
    public class ProceduralBackground : MonoBehaviour
    {
        private RawImage _bgImage;
        private Texture2D _gradientTex;
        private ThemeConfig _theme;
        private float _animOffset;

        // Particle children
        private RectTransform[] _particles;
        private Vector2[] _particleVelocities;
        private Image[] _particleImages;

        private const int GradientHeight = 64;

        public void Initialize(ThemeConfig theme)
        {
            _theme = theme;
            _bgImage = GetComponent<RawImage>();

            GenerateGradientTexture();
            SpawnParticles();
        }

        private void GenerateGradientTexture()
        {
            if (_gradientTex != null)
                Destroy(_gradientTex);

            _gradientTex = new Texture2D(1, GradientHeight, TextureFormat.RGBA32, false);
            _gradientTex.wrapMode = TextureWrapMode.Clamp;
            _gradientTex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < GradientHeight; y++)
            {
                float t = (float)y / (GradientHeight - 1);
                _gradientTex.SetPixel(0, y, Color.Lerp(_theme.backgroundBottom, _theme.backgroundTop, t));
            }
            _gradientTex.Apply();
            _bgImage.texture = _gradientTex;
            _bgImage.color = Color.white;
        }

        private void SpawnParticles()
        {
            ClearParticles();

            int count = _theme != null ? _theme.particleCount : 10;
            _particles = new RectTransform[count];
            _particleVelocities = new Vector2[count];
            _particleImages = new Image[count];

            var parentRect = GetComponent<RectTransform>();

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Particle_{i}");
                var rt = go.AddComponent<RectTransform>();
                rt.SetParent(transform, false);

                float size = _theme.particleSize * Mathf.Min(parentRect.rect.width, parentRect.rect.height);
                size = Mathf.Max(size, 8f);
                rt.sizeDelta = new Vector2(size, size);

                // Random start position within parent bounds
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.zero;
                rt.anchoredPosition = new Vector2(
                    Random.Range(0f, parentRect.rect.width),
                    Random.Range(0f, parentRect.rect.height)
                );

                var img = go.AddComponent<Image>();
                img.color = Color.Lerp(_theme.particleColorA, _theme.particleColorB, Random.value);
                img.raycastTarget = false;

                _particles[i] = rt;
                _particleImages[i] = img;

                // Random drift direction
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = _theme.particleSpeed * Random.Range(5f, 20f);
                _particleVelocities[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            }
        }

        private void Update()
        {
            if (_theme == null || _particles == null) return;

            var parentRect = GetComponent<RectTransform>();
            float w = parentRect.rect.width;
            float h = parentRect.rect.height;

            // Animate gradient hue shift
            _animOffset += Time.deltaTime * _theme.gradientAnimSpeed * 0.01f;

            // Animate particles
            for (int i = 0; i < _particles.Length; i++)
            {
                if (_particles[i] == null) continue;

                var pos = _particles[i].anchoredPosition;
                pos += _particleVelocities[i] * Time.deltaTime;

                // Wrap around edges
                if (pos.x < -20f) pos.x = w + 20f;
                if (pos.x > w + 20f) pos.x = -20f;
                if (pos.y < -20f) pos.y = h + 20f;
                if (pos.y > h + 20f) pos.y = -20f;

                _particles[i].anchoredPosition = pos;

                // Gentle alpha pulse
                float alpha = 0.3f + 0.3f * Mathf.Sin(Time.time * 0.8f + i * 0.5f);
                var c = _particleImages[i].color;
                _particleImages[i].color = new Color(c.r, c.g, c.b, alpha);
            }
        }

        private void ClearParticles()
        {
            if (_particles != null)
            {
                foreach (var p in _particles)
                {
                    if (p != null) Destroy(p.gameObject);
                }
            }
            _particles = null;
            _particleVelocities = null;
            _particleImages = null;
        }

        private void OnDestroy()
        {
            ClearParticles();
            if (_gradientTex != null)
                Destroy(_gradientTex);
        }
    }
}
