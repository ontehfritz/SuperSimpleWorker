using System;
using System.Configuration;
using DelayedJob;
using System.Reflection;
using System.Linq;
using System.ServiceProcess;

namespace DelayJob.Worker
{
	public class WorkerService : ServiceBase
	{
		public WorkerService()
		{
			ServiceName = "DelayJob_Worker";
		}

		protected override void OnStart(string[] args)
		{
			MainClass.Start(args);
		}

		protected override void OnStop()
		{
			MainClass.Stop();
		}
	}

	class MainClass
	{
		private static bool work = true;
		public static void Main (string[] args){

			Start (args);
			Stop ();

			Console.WriteLine ("Exiting ...");
		}

		public static void Start(string[] args)
		{
			string connectionString = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ConnectionString;
			string providerName = ConfigurationManager.ConnectionStrings ["delayed_job_db"].ProviderName;

			if (args.Length > 0) {
				Job.WorkerName = args [0];
				Console.WriteLine ("set workername");
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

			while(work)
			{
				Job.Report report = Job.WorkOff();

				if (report.failure == 0 &&
				    report.success == 0) {
					//sleep if no jobs 
					System.Threading.Thread.Sleep (SLEEP * 1000); 
				} else {
					//if jobs have run then print out a report on success and failures
					int count = report.success + report.failure;
					Console.WriteLine (count.ToString() + " jobs processed at %.4f j/s, " + report.failure.ToString() + 
					                   " failed ...");
				}
			}
		}

		public static void Stop()
		{
			Job.ClearLocks();
		}
	}
}
