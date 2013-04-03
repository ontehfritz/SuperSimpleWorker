using System;

namespace delayed_job
{
	public interface IJob
	{
		string perform();
	}
}

