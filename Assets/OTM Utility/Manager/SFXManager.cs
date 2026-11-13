#if UNITY_FMODE
using com.otm.manager;
using com.otm.sfx;
using com.otm.utility;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace com.otm.sfx
{
	public class SFXManager : Manager<SFXManager>
	{
		#region private variables
		[SerializeField] private SoundBankGroup startMusicBank;

		private Bus musicBus;
		private Bus sfxBus;

		#endregion

		#region propertices
		public bool IsMusicOn
		{
			get { return PlayerPrefsHelper.GetBool("MusicBus", true); }
			private set { PlayerPrefsHelper.SetBool("MusicBus", value); }
		}

		public bool IsSFXOn
		{
			get { return PlayerPrefsHelper.GetBool("SFXBus", true); }
			private set { PlayerPrefsHelper.SetBool("SFXBus", value); }
		}

		#endregion

		#region unity callback

		private void Start()
		{
			SetBus();
		}

		#endregion

		#region private methods

		private void SetBus()
		{
			musicBus = RuntimeManager.GetBus("bus:/Music");
			sfxBus = RuntimeManager.GetBus("bus:/SFX");
			musicBus.setMute(!IsMusicOn);
			sfxBus.setMute(!IsSFXOn);
			//PlayStartMusic();
		}

		#endregion

		#region public methods

		public void SetBackGroundMusic(string musicKey)
		{
			startMusicBank.PlaySound(musicKey);
		}

		public void StopBackGrounMusic()
		{
			startMusicBank.StopAllSound();
		}

		public void SetMusicVolume(float volume)
		{
			musicBus.setVolume(volume);
		}

		public void SetActiveMusic(bool active)
		{
			IsMusicOn = active;
			musicBus.setMute(!active);
		}

		public void SetActiveSFX(bool active)
		{
			IsSFXOn = active;
			sfxBus.setMute(!active);
		}

		#endregion
	}
}
#endif