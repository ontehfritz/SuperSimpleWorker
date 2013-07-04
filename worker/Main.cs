using System;
using System.Configuration;
using DelayedJob;
using System.Reflection;
using System.Linq;
using System.ServiceProcess;
using System.Diagnostics;

namespace DelayJob.Worker
{
	/// <summary>
	/// Main class.
	/// </summary>
	class Program
	{
		private static bool work = true;
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments. 1 arg can be supplied. This arguement is the name of the
		/// worker. 
		/// </param>
		public static void Main (string[] args){

			try{
				Start (args);
			}catch(Exception e) {
				throw e;
			}
			finally{
				Stop ();
			}

			Console.WriteLine ("Exiting ...");
		}

		private static void Start(string[] args)
		{
			string connectionString = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ConnectionString;
			string providerName = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ProviderName;

			if (args.Length > 0) {
				Job.WorkerName = args [0];
			}

			int SLEEP = 5;

			Assembly assembly = Assembly.Load("DelayedJob");
			Type type = assembly.GetType ("DelayedJob." + providerName);

			Job.Repository = (IRepository)Activator.CreateInstance(type,connectionString);

			Console.WriteLine (connectionString);
			Console.WriteLine (providerName);

			Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
				e.Cancel = true;
				work = false;
			};

			Console.WriteLine ("*** Starting job worker " + Job.WorkerName);
			Stopwatch benchmark = new Stopwatch ();

			while(work)
			{
				benchmark.Start ();
				Job.Report report = Job.WorkOff();
				benchmark.Stop ();
				TimeSpan ts = benchmark.Elapsed;

				if (report.failure == 0 &&
				    report.success == 0) {
					//sleep if no jobs 
					System.Threading.Thread.Sleep (SLEEP * 1000); 
				} else {
					//if jobs have run then print out a report on success and failures
					int count = report.success + report.failure;
					double jobsPerSecond = (((double)ts.Milliseconds) / (count * 1.0)) / 1000;
					Console.WriteLine (count.ToString() + " jobs processed at " + jobsPerSecond.ToString() +  
					                   " j/s, " + report.failure.ToString() + 
					                   " failed ...");
				}
			}
		}

		private static void Stop()
		{
			Job.ClearLocks();
		}
	}
}
