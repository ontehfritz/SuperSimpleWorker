using System;

namespace worker
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			//delayed_job.RepositorySQLite s = new delayed_job.RepositorySQLite ();

			//delayed_job.Job j = s.GetNextReadyJobs ()[0];

			//Console.WriteLine(j.assembly);
			//Console.WriteLine(@"/Users/Fritz/Documents/Projects/delayed_job/delay_job_test/bin/Debug/delay_job_test.dll");

			int SLEEP = 5;

			while(true)
			{
				//delayed_job.Job job = new delayed_job.Job ();
				delayed_job.Job.Report report = delayed_job.Job.WorkOff();

				if (report.failure == 0 &&
					report.success == 0) {
					System.Threading.Thread.Sleep (SLEEP * 1000); 
				} else {
					Console.WriteLine ("SUCCESS: " + report.success.ToString() + "\n" + 
					               "FAILURE: " + report.failure.ToString());
				}

				Console.WriteLine ("Done!");
				//job.ClearLocks();
			}
		}
	}
}
