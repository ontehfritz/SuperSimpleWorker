using System;
using System.Configuration;
using DelayedJob;

namespace Worker
{
	class MainClass
	{
		public static void Main (string[] args){
			//How many seconds to sleep
			string connectionString = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ConnectionString;
			string providerName = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ProviderName;

			int SLEEP = 5;

			switch (providerName) {
			case "RepositorySQLite":
				Job.Repository = new RepositorySQLite (connectionString);
				break;
			default:
				throw new ArgumentException (String.Format("No such provider {0}", providerName));
				break;
			}

			Console.WriteLine (connectionString);
			Console.WriteLine (providerName);

			while(true)
			{
				Job.Report report = Job.WorkOff();

				if (report.failure == 0 &&
					report.success == 0) {
					//sleep if no jobs 
					System.Threading.Thread.Sleep (SLEEP * 1000); 
				} else {
					//if jobs have run then print out a report on success and failures
					Console.WriteLine ("SUCCESS: " + report.success.ToString() + "\n" + 
					               "FAILURE: " + report.failure.ToString());
				}

				Console.WriteLine ("Done!");
				//Clear all jobs that have been locked and run successfully
				//delayed_job.Job.ClearLocks();
			}
		}
	}
}
