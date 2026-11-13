using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.otm.ui
{
	public class AutoHideSubView : SubView
	{
		#region protected variables
		[SerializeField] protected float duration;
		protected float wait;
		protected Coroutine waitCoroutine;
		#endregion

		#region overrided methods

		public virtual void ShowView(float duration)
		{
			wait = duration;
			base.ShowView();
		}

		public override void ShowView()
		{
			wait = duration;
			base.ShowView();
		}

		protected override void OnViewShowDone()
		{
			base.OnViewShowDone();
			waitCoroutine = StartCoroutine(DoAnimation(wait));
		}

		#endregion

		#region Coroutine

		IEnumerator DoAnimation(float wait)
		{
			yield return new WaitForSeconds(wait);
			HideView();
		}

		#endregion
	}
}
