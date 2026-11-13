namespace com.otm.utility
{
	public abstract class RewardAdValidation
	{
		#region abstract methods
		public abstract bool IsValid();
		#endregion

		#region virtual methods
		public virtual void Init() { }
		public virtual void OnWatchAd() { }

		#endregion
	}

	public class NoneAdValidation : RewardAdValidation
	{
		public override bool IsValid()
		{
			return true;
		}
	}
}
