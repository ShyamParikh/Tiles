using com.otm.utility;
using Sirenix.OdinInspector;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace com.otm.animation
{
	public class ParticleSystemAnimation : BaseAnimation
	{
		#region private variables
		[SerializeField, ParticleSystemReference] ParticleSystem particleSystem;
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

		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].OnStart();
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

		protected override void OnAnimationOver()
		{
			base.OnAnimationOver();
			for (int i = 0; i < particleSystemModules.Length; i++)
			{
				particleSystemModules[i].OnComplete();
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

	[HideReferenceObjectPicker]
	public abstract class ParticleSystemModule
	{
		#region protected variables

		protected ParticleSystem particleSystem;

		#endregion

		#region virtual methods
		public virtual void Init(ParticleSystem particleSystem)
		{
			this.particleSystem = particleSystem;
		}

		public virtual void OnStart() { }

		public virtual void Play() { }

		public virtual void Stop() { }

		public virtual void OnComplete() { }

		#endregion

		#region abstract methods
		public abstract void Update(float evolution);

		#endregion
	}

	public class ParticleSystemPlayModule : ParticleSystemModule
	{
		#region private variables

		[SerializeField] private bool play;
		[SerializeField] private bool stop;
		[SerializeField] private bool stopOnComplete;

		#endregion

		#region overrided methods

		public override void Play()
		{
			base.Play();
			if (play)
			{
				particleSystem.Stop();
				particleSystem.Play();
			}
			else if (stop)
			{
				particleSystem.Stop();
			}
		}

		public override void OnComplete()
		{
			base.OnComplete();
			if (stopOnComplete)
				particleSystem.Stop();
		}


		public override void Stop()
		{
			particleSystem.Stop();
		}

		public override void Update(float evolution)
		{

		}
		#endregion
	}


	public class ParticleSystemEmissionRateOverTimeModule : ParticleSystemModule
	{
		#region private variables

		[SerializeField] private float startValue;
		[SerializeField] private float endValue;
		[SerializeField] private bool rateOverDistance;
		[SerializeField, HideReferenceObjectPicker] private AnimationCurve curve = new AnimationCurve();

		private EmissionModule emissionModule;

		#endregion

		#region overrided methods

		public override void Init(ParticleSystem particleSystem)
		{
			base.Init(particleSystem);
			emissionModule = particleSystem.emission;
		}

		public override void OnStart()
		{
			base.OnStart();
			if (!particleSystem.isPlaying)
			{
				particleSystem.Stop();
				particleSystem.Play();
			}
		}

		public override void Update(float evolution)
		{
			
			if (rateOverDistance)
				emissionModule.rateOverDistance = Mathf.Lerp(startValue, endValue, curve.Evaluate(evolution));
			else
				emissionModule.rateOverTime = Mathf.Lerp(startValue, endValue, curve.Evaluate(evolution));

		}

		public override void Stop()
		{
			emissionModule.rateOverTime = startValue;
		}

		#endregion
	}
}