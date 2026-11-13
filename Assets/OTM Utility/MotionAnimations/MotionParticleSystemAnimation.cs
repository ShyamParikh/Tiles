using UnityEngine;

namespace com.otm.animation
{
	public class MotionParticleSystemAnimation : BaseMotionAnimation
	{
		#region private variables
		[SerializeField] ParticleSystem particleSystem;
		[SerializeField] private bool onStartDelayOver;
		[SerializeField] ParticleSystemModule[] particleSystemModules = new ParticleSystemModule[0];
		#endregion

		#region overrided methods

		public override void Init(MonoBehaviour monoBehaviour, bool useUnscaleTime = false)
		{
			base.Init(monoBehaviour, useUnscaleTime);
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].Init(particleSystem);
			}
		}

		public override void OnUpdate(float evolution)
		{
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].Update(evolution);
			}
		}

		public override void Play()
		{
			base.Play();
			if (onStartDelayOver)
				return;
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].Play();
			}
		}

		protected override void OnAnimationStartDelayOver()
		{
			base.OnAnimationStartDelayOver();
			if (!onStartDelayOver)
				return;
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].Play();
			}
		}

		public override void Stop()
		{
			base.Stop();
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].Stop();
			}
		}

		#endregion
	}
}
