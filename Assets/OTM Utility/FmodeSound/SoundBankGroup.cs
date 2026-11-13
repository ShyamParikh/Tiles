#if UNITY_FMODE
using FMOD.Studio;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.otm.sfx
{
	[CreateAssetMenu(menuName = "OTM/SFX/MusicBank", fileName = "MusicBank")]
	public class SoundBankGroup : SerializedScriptableObject
	{
#region private variables

		[SerializeField] private float fadeTime;
		[SerializeField] private string paremeteName = "fade";
		[SerializeField] private bool stopLastSoundOnNewPlay;
		[SerializeField] private Dictionary<string, SoundBank> musics = new Dictionary<string, SoundBank>();

#endregion

#region propertice
		public SoundBank LastPlayedMusic { get; set; }

#endregion

#region unity callback

		private void OnEnable()
		{
			LastPlayedMusic = null;
		}

#endregion

#region public methods

		public EventInstance? GetInstance(string soundKey)
		{
			if (musics.ContainsKey(soundKey))
				return musics[soundKey].GetInstance();
			return null;
		}

		[Button]
		public void PlaySound()
		{
			if (musics.Count <= 0)
				return;
			string soundKey = musics.ElementAt(0).Key;
			PlaySound(soundKey);
		}

		public void PlaySound(string soundKey)
		{
			if (!musics.ContainsKey(soundKey))
				return;
			if (stopLastSoundOnNewPlay && LastPlayedMusic != null && LastPlayedMusic != musics[soundKey])
			{
				SoundBank lastBank = LastPlayedMusic;
				LastPlayedMusic.SetParameterWithLerp(paremeteName, 1f, 0, fadeTime, onComplete: lastBank.Stop);
			}
			LastPlayedMusic = musics[soundKey];
			LastPlayedMusic.Play();
			LastPlayedMusic.SetParameterWithLerp(paremeteName, 0, 1f, fadeTime);
		}

		public void StopSound()
		{
			if (LastPlayedMusic == null)
				return;
			LastPlayedMusic.SetParameterWithLerp(paremeteName, 1f, 0, fadeTime, onComplete: LastPlayedMusic.Stop);
			LastPlayedMusic = null;
		}

		public void StopSound(string soundKey)
		{
			if (!musics.ContainsKey(soundKey))
				return;
			musics[soundKey].SetParameterWithLerp(paremeteName, 1f, 0, fadeTime, onComplete: musics[soundKey].Stop);
			if (LastPlayedMusic == musics[soundKey])
				LastPlayedMusic = null;
		}

		public void StopAllSound()
		{
			foreach (var item in musics)
			{
				item.Value.Stop();
			}
			LastPlayedMusic = null;
		}

		public void PuaseAllSound()
		{
			foreach (var item in musics)
			{
				item.Value.Pause();
			}
		}

		public void ResumeAllSound()
		{
			foreach (var item in musics)
			{
				item.Value.Resume();
			}
		}

		public void ReleaseAllSound()
		{
			foreach (var item in musics)
			{
				item.Value.Release();
			}
		}


#endregion

	}
}
#endif