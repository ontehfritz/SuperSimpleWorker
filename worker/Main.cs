using System;
using System.Configuration;
using SuperSimple.Worker;
using System.Reflection;
using System.Linq;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace SuperSimple.Worker
{
	[RunInstaller(true)]
	public class WorkerServiceInstaller : Installer
	{
		public WorkerServiceInstaller()
		{
			ServiceProcessInstaller process = new ServiceProcessInstaller();

			process.Account = ServiceAccount.LocalSystem;

			ServiceInstaller serviceAdmin = new ServiceInstaller();

			serviceAdmin.StartType = ServiceStartMode.Manual;

			serviceAdmin.ServiceName = "SuperSimpleWorker";
			serviceAdmin.DisplayName = "SuperSimpleWorker";
			serviceAdmin.Description = "SuperSimple worker service.";

			Installers.Add(process);
			Installers.Add(serviceAdmin);
		}
	}
	/// <summary>
	/// Main class.
	/// </summary>
	public class Program
	{
		private static Timer timer;

		public class WorkerService : ServiceBase
		{
			public WorkerService()
			{
				ServiceName = "SuperSimpleWorker";
			}

			protected override void OnStart(string[] args)
			{
				Program.Start(args);
			}

			protected override void OnStop()
			{
				Program.Stop();
			}
		}

		private static bool work = true;
		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments. 1 arg can be supplied. This arguement is the name of the
		/// worker. 
		/// </param>
		public static void Main (string[] args)
		{
			int platformID = (int) Environment.OSVersion.Platform;
			//platformIDs 4, 6, 128 *nix based systems
			//Environment.UserInteractive always false when running under mono
			if (Environment.UserInteractive ||
			    ((platformID == 4) || 
			 	 (platformID == 6) || 
			 	 (platformID == 128)))
			{

				try 
				{
					Start (args);
					//keep the program looping so the timer can run
					//timer must be used when worker is installed as 
					//a service.
					while(work){}

				}
				catch (Exception e) 
				{
					throw e;
				} 
				finally 
				{
					Stop ();
				}

				Console.WriteLine ("Exiting ...");

			}
			else 
			{
				//Console.WriteLine("Service");
				// running as service
				using (var service = new WorkerService())
					ServiceBase.Run (service);
			}
		}

		private static void Start(string[] args)
		{
			string connectionString = ConfigurationManager.ConnectionStrings ["ssw_db"].ConnectionString;
			string providerName = ConfigurationManager.ConnectionStrings ["ssw_db"].ProviderName;

			if (args.Length > 0) 
			{
				Job.WorkerName = args [0];
			}

			Assembly assembly = Assembly.Load(providerName);
			Type type = assembly.GetType ("SuperSimple.Worker." + providerName);

			//Assembly assembly = Assembly.Load("DelayedJob");
			//Type type = assembly.GetType ("DelayedJob." + providerName);

			Job.Repository = (IRepository)Activator.CreateInstance(type,connectionString);

			//Console.WriteLine (connectionString);
			Console.WriteLine (providerName);

			Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) 
			{
				e.Cancel = true;
				work = false;
			};

			Console.WriteLine ("*** Starting job worker " + Job.WorkerName);
			timer = new Timer (new TimerCallback (Work),null, 5000, 5000);

			//Console.ReadLine();

		}

		private static void Work(object state)
		{
			int SLEEP = 5;
			Stopwatch benchmark = new Stopwatch ();

			benchmark.Start ();
			Job.Report report = Job.WorkOff();
			benchmark.Stop ();
			TimeSpan ts = benchmark.Elapsed;

			if (report.failure == 0 &&
				report.success == 0) 
			{
					//sleep if no jobs. 1000, is milliseconds * how many seconds 
				System.Threading.Thread.Sleep (SLEEP * 1000); 
			} 
			else 
			{
				//if jobs have run then print out a report on success and failures
				int count = report.success + report.failure;
				double jobsPerSecond = (((double)ts.Milliseconds) / (count * 1.0)) / 1000;
				Console.WriteLine (count.ToString() + " jobs processed at " + jobsPerSecond.ToString() +  
					                   " j/s, " + report.failure.ToString() + 
					                   " failed ...");
			}
		}

		private static void Stop()
		{
			timer.Dispose();
			Job.ClearLocks();
		}
	}
}
