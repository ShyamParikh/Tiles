#if UNITY_FMODE
using FMOD;
using UnityEngine;
using UnityEngine.UI;

namespace com.otm.sfx
{
	public class ButtonSFX : MonoBehaviour
	{
		#region private variables

		[SerializeField] private SoundBank buttonSound;
		private Button button;

		#endregion

		#region unity callback

		private void Start()
		{
			button = GetComponent<Button>();
			if (button != null)
				button.onClick.AddListener(PlayButtonSound);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (buttonSound == null)
				buttonSound = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundBank>("Assets/MainGame/SFX/Banks/Button-SFX.asset");
		}

		public void SetButtonSoundBank(SoundBank soundBank)
		{
			if (buttonSound != null || soundBank == null)
				return;
			buttonSound = soundBank;
		}

#endif
		#endregion

		#region private methods

		private void PlayButtonSound()
		{
			if (buttonSound != null)
			{
				buttonSound.Play();
			}
		}

		#endregion
	}
}
#endif