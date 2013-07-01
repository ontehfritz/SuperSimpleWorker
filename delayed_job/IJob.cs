namespace DelayedJob
{
	using System;
	/// <summary>
	/// When scheduling jobs. The object must implement this interface. The perform method is executed by the worker 
	/// process. All job actions must go into the perform method.
	/// </summary>
	public interface IJob
	{
		/// <summary>
		/// Perform this instance.
		/// </summary>
		void perform();
	}
}

