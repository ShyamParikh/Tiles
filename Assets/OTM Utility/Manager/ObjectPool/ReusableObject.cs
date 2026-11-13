using com.otm.manager;
using UnityEngine;

namespace com.otm.objectPool
{
	public class ReusableObject : MonoBehaviour, IObjectPool
	{
		#region private variables

		[SerializeField] private float aliveDuration;

		#endregion

		#region public methods

		public void RecycleObject()
		{
			ObjectPoolManager.Recycle(gameObject);
		}

		public void InitObject()
		{
		}

		public void OnSpwan()
		{
			if (aliveDuration <= 0)
				return;
			Invoke("RecycleObject", aliveDuration);
		}

		#endregion
	}
}