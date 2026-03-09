using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BoardOfEducation.Visuals
{
    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup _overlayGroup;

        private void Awake()
        {
            if (fadeOverlay != null)
            {
                _overlayGroup = fadeOverlay.GetComponent<CanvasGroup>();
                if (_overlayGroup == null)
                    _overlayGroup = fadeOverlay.gameObject.AddComponent<CanvasGroup>();
                _overlayGroup.alpha = 0f;
                _overlayGroup.blocksRaycasts = false;
            }
        }

        public void TransitionTo(ThemeConfig theme, System.Action onMidpoint)
        {
            StartCoroutine(FadeTransition(theme, onMidpoint));
        }

        private IEnumerator FadeTransition(ThemeConfig theme, System.Action onMidpoint)
        {
            if (_overlayGroup == null)
            {
                onMidpoint?.Invoke();
                yield break;
            }

            // Set overlay color to theme's background
            if (fadeOverlay != null)
                fadeOverlay.color = theme.backgroundBottom;

            _overlayGroup.blocksRaycasts = true;

            // Fade in
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _overlayGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            _overlayGroup.alpha = 1f;

            // Apply theme at midpoint
            onMidpoint?.Invoke();

            // Brief hold
            yield return new WaitForSeconds(0.15f);

            // Fade out
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _overlayGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            _overlayGroup.alpha = 0f;
            _overlayGroup.blocksRaycasts = false;
        }
    }
}
