using System;

namespace worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			delayed_job.Job.WorkOff();
			Console.WriteLine ("Done!");
		}
	}
}
