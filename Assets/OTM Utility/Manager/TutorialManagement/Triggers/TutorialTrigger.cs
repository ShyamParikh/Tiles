namespace com.otm.tutorial
{
	public abstract class TutorialTrigger
	{
		#region protected variables
		protected TutorialMono tutorial;

		#endregion

		#region virtual methods

		public virtual void Init(TutorialMono tutorial)
		{
			this.tutorial = tutorial;
		}

		public virtual void OnComplete()
		{

		}

		public virtual void OnDestroy()
		{
		}

		#endregion

		#region public methods

		public void StartTutorial()
		{
			if (TutorialManager.Instance.IsTutorialRunning)
				return;
			if (!tutorial.IsTutorialPlayed)
				tutorial.StartTutorial();
		}

		public void StopTutorial()
		{
			if (!tutorial.IsTutorialPlayed)
				tutorial.StopTutorial();
		}

		#endregion
	}
}