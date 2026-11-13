using UnityEngine;

namespace com.otm.rate_us
{
	public abstract class BaseRateShowCondition
	{

		#region virtual methods

		public virtual void Init()
		{

		}

		#endregion

		#region abstract methods

		public abstract bool IsValid();

		#endregion
	}
}
