/*using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using com.otm.manager;
using System;
using TMPro; // --- NEW: Added for TextMeshPro ---

namespace com.otm.game
{
    public class LoadingManager : Manager<LoadingManager>
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _progressBarFill;

        // --- NEW: The text that tells the player what is happening ---
        [SerializeField] private TMP_Text _statusText;

        [Header("Animation Settings")]
        [SerializeField] private float _fadeDuration = 0.25f;

        public override void Awake()
        {
            base.Awake();
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        public void ShowLoadingScreen(Action onFullyVisible = null)
        {
            if (_canvasGroup == null)
            {
                onFullyVisible?.Invoke();
                return;
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (_progressBarFill != null)
            {
                _progressBarFill.DOKill();
                _progressBarFill.fillAmount = 0f;
            }

            // --- NEW: Default start text ---
            SetStatus("Initializing...");

            _canvasGroup.DOFade(1f, _fadeDuration).OnComplete(() => {
                onFullyVisible?.Invoke();
            });
        }

        // --- NEW: Updates the text instantly ---
        public void SetStatus(string statusMessage)
        {
            if (_statusText != null)
            {
                _statusText.text = statusMessage;
            }
        }

        public void SetProgress(float targetValue)
        {
            if (_progressBarFill != null)
            {
                _progressBarFill.DOFillAmount(targetValue, 0.2f).SetEase(Ease.OutQuad);
            }
        }

        public void HideLoadingScreen()
        {
            if (_canvasGroup == null) return;

            // --- NEW: Final text before fading out ---
            SetStatus("Ready!");

            if (_progressBarFill != null)
            {
                _progressBarFill.DOKill();
                _progressBarFill.DOFillAmount(1f, 0.15f).OnComplete(FadeOutScreen);
            }
            else
            {
                FadeOutScreen();
            }
        }

        private void FadeOutScreen()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.DOFade(0f, _fadeDuration).OnComplete(() => {
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                });
            }
        }
    }
}*/