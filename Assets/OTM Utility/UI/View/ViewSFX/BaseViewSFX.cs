#if UNITY_FMODE
using com.otm.sfx;
#endif
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.ui
{
	public abstract class BaseViewSFX : SerializedMonoBehaviour
	{
		#region protected variables

#if UNITY_FMODE
		[SerializeField] protected SoundBank showSound;
		[SerializeField] protected SoundBank hideSound;
		[SerializeField] protected Dictionary<string, SoundBank> otherSound = new Dictionary<string, SoundBank>();
#endif

		#endregion

		#region virtual methods
		public virtual void PlayShowSound()
		{
#if UNITY_FMODE
			if (showSound != null)
				showSound.Play();
#endif
		}
		public virtual void PlayHideSound()
		{
#if UNITY_FMODE
			if (hideSound != null)
				hideSound.Play();
#endif
		}
		public virtual void PlayOtherSound(string soundKey)
		{
#if UNITY_FMODE
			if (!otherSound.ContainsKey(soundKey))
				return;
			otherSound[soundKey].Play();
#endif
		}

		public virtual void StopShowSound()
		{
#if UNITY_FMODE
			if (showSound != null)
				showSound.Stop();
#endif
		}
		public virtual void StopHideSound()
		{
#if UNITY_FMODE
			if (hideSound != null)
				hideSound.Stop();
#endif
		}
		public virtual void StopOtherSound(string soundKey)
		{
#if UNITY_FMODE
			if (!otherSound.ContainsKey(soundKey))
				return;
			otherSound[soundKey].Stop();
#endif
		}
		public virtual void StopAllSound()
		{
#if UNITY_FMODE
			showSound.Stop();
			hideSound.Stop();
			foreach (var item in otherSound)
			{
				item.Value.Stop();
			}
#endif
		}

#if UNITY_FMODE
		public virtual SoundBank GetShowSound()
		{
			return showSound;
		}

		public virtual SoundBank GetHideSound()
		{
			return hideSound;
		}
#endif

#if UNITY_FMODE
		public virtual SoundBank GetOtherSound(string soundKey)
		{
			if (otherSound.ContainsKey(soundKey))
				return otherSound[soundKey];
			return null;
		}
#endif
		#endregion

	}
}