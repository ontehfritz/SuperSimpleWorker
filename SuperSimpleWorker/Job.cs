namespace SuperSimple.Worker
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;
	using System.Xml.Serialization;
	using System.Reflection;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	/// This class is used to create/schedule/delete jobs. Also contains static methods for worker.exe.
	/// </summary>
	public class Job
	{
		/* database fields */
		private int _id;
		private int _priority;  //Allows some jobs to jump to the front of the queue
		private int _attempts;  //Provides for retries, but still fail eventually.
		private string _handler; //xml string of the object that will do work
		private string _last_error; //reason for last failure (See Note below)
		private DateTime? _run_at; //When to run. Could be Time.now for immediately, or sometime in the future.
		private DateTime? _locked_at; //Set when a client is working on this object
		private DateTime? _failed_at; //Set when all retries have failed (actually, by default, the record is deleted instead)
		private string _locked_by; //Who is working on this object (if locked)
		private string _type;
		private string _assembly;
		/************************/

		private static IRepository _repository;

		/// <summary>
		/// The inner report class is used to track how many successful and failed jobs. 
		/// </summary>
		public struct Report
		{
			/// <summary>
			/// The number of successful jobs
			/// </summary>
			public int success; 
			/// <summary>
			/// The number of failed jobs.
			/// </summary>
			public int failure;
		}

		/// <summary>
		/// Gets or sets the repository.
		/// This needs to be set before any operations can be done
		/// </summary>
		/// <value>The repository.</value>
		public static IRepository  Repository
		{
			get 
			{
				return _repository;
			}
			set 
			{
				_repository = value; 
			}
		}

		/// <summary>
		/// Gets or sets the ID.
		/// This is the ID of the job from the database. 
		/// If this is zero the ID has not been set. 
		/// </summary>
		/// <value>The ID</value>
		public int ID
		{
			get 
			{
				return _id;
			}
			set 
			{
				_id = value;
			}
		}

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority
		{
			get 
			{
				return _priority;
			}
			set 
			{
				_priority = value;
			}
		}
		/// <summary>
		/// Gets or sets the attempts.
		/// </summary>
		/// <value>The attempts.</value>
		public int Attempts
		{
			get 
			{
				return _attempts;
			}
			set 
			{
				_attempts = value;
			}
		}
		/// <summary>
		/// Gets or sets the handler.
		/// </summary>
		/// <value>The handler.</value>
		public string Handler
		{
			get 
			{
				return _handler; 
			}
			set 
			{
				_handler = value; 
			}
		}
		/// <summary>
		/// Gets or sets the last error.
		/// </summary>
		/// <value>The last error.</value>
		public string LastError
		{
			get 
			{ 
				return _last_error; 
			}
			set 
			{
				_last_error = value; 
			}
		}
		/// <summary>
		/// Gets or sets the run at.
		/// </summary>
		/// <value>The run at.</value>
		public DateTime? RunAt
		{
			get 
			{ 
				return _run_at; 
			}
			set 
			{
				_run_at = value; 
			}
		}
		/// <summary>
		/// Gets or sets the locked at.
		/// </summary>
		/// <value>The locked at.</value>
		public DateTime? LockedAt
		{
			get 
			{ 
				return _locked_at; 
			}
			set 
			{
				_locked_at = value; 
			}
		}
		/// <summary>
		/// Gets or sets the failed at.
		/// </summary>
		/// <value>The failed at.</value>
		public DateTime? FailedAt
		{
			get 
			{ 
				return _failed_at; 
			}
			set 
			{
				_failed_at = value; 
			}
		}
		/// <summary>
		/// Gets or sets the locked by.
		/// </summary>
		/// <value>The locked by.</value>
		public string LockedBy
		{
			get 
			{ 
				return _locked_by; 
			}
			set 
			{
				_locked_by = value; 
			}
		}
		/// <summary>
		/// Gets or sets the type of the object.
		/// </summary>
		/// <value>The type of the object.</value>
		public string ObjectType
		{
			get 
			{ 
				return _type; 
			}
			set 
			{
				_type = value; 
			}
		}
		/// <summary>
		/// Gets or sets the job assembly.
		/// </summary>
		/// <value>The job assembly.</value>
		public string JobAssembly
		{
			get 
			{ 
				return _assembly; 
			}
			set 
			{
				_assembly = value; 
			}
		}
		/*Object attributes */ 
		const int MAX_ATTEMPTS = 25;
		//const int MAX_RUN_TIME = 4; //hours
		private bool _destroyFailedJobs = false;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="DelayedJob.Job"/> destroy failed jobs.
		/// </summary>
		/// <value><c>true</c> if destroy failed jobs; otherwise, <c>false</c>.</value>
		public bool DestroyFailedJobs
		{
			get
			{
				return _destroyFailedJobs;
			}
			set
			{
				_destroyFailedJobs = value;
			}
		}
		//Set a default name a guid
		private static string workerName = Guid.NewGuid().ToString(); 
		/// <summary>
		/// Gets or sets the name of the worker. This is useful for server reboots. If not set a guid is used, which 
		/// will be different each time the worker service is restarted. 
		/// </summary>
		/// <value>The name of the worker.</value>
		public static string WorkerName
		{
			get
			{
				return workerName;
			}
			set
			{
				workerName = value;
			}
		}

		//string set_table_name = "delayed_jobs";
		/// <summary>
		/// Initializes a new instance of the <see cref="DelayedJob.Job"/> class.
		/// </summary>
		public Job (){}

		/// <summary>
		/// Log the specified message.
		/// </summary>
		/// <param name="message">Message.</param>
		private void Log(string message)
		{
			using (System.IO.StreamWriter file = 
			       new System.IO.StreamWriter(workerName + ".log",true)) 
			{
				file.WriteLine (message);
			}
		}

		/// <summary>
		/// Reschedule the specified message and time.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="time">Time.</param>
		public void Reschedule(string message, DateTime? time = null)
		{
			if(_attempts < MAX_ATTEMPTS)
			{
				time = (time == null ? DateTime.Now.AddSeconds(_attempts ^ 4 + 5) : time );
				_attempts += 1;
				_run_at = time;
				_last_error = message;
				this.unlock(); 
				_repository.UpdateJob(this);
			}
			else
			{
				Log (string.Format("* [Job] PERMANENTLY removing {0} because of {1} consecutive failures.",
				                   _type, _attempts));
				if(_destroyFailedJobs){
					_repository.Remove(_id);
				}
				else{
					_failed_at = DateTime.Now;
					_repository.UpdateJob(this);
				}
			}
		}
		/// <summary>
		/// Unlock this instance.
		/// </summary>
		private void unlock()
		{
			_locked_at = null;
			_locked_by = null;
		}
		/// <summary>
		/// Locks the exclusively.
		/// </summary>
		/// <returns><c>true</c>, if exclusively was locked, <c>false</c> otherwise.</returns>
		public bool LockExclusively()
		{
			_locked_by = workerName;
			_locked_at = DateTime.Now;
			try
			{
				_repository.UpdateJob(this);
			}
			catch(Exception e) 
			{
				Log (string.Format("* [JOB] {0} failed with {1}: {2} -" + 
				                   "{3} failed attempts",_type,_assembly,e.Message,_attempts));
				return false;
			}

			return true;
		}
		/// <summary>
		/// Clears the locks.
		/// </summary>
		public static void ClearLocks()
		{
			_repository.ClearJobs(workerName);
		}
		/// <summary>
		/// Finds the available.
		/// </summary>
		/// <returns>The available.</returns>
		/// <param name="limit">Limit.</param>
		public static Job[] FindAvailable(int limit = 5)
		{
			return _repository.GetNextReadyJobs(limit);
		}
		/// <summary>
		/// Reserves the and run one job.
		/// </summary>
		/// <returns>The and run one job.</returns>
		public static bool? ReserveAndRunOneJob()
		{
			Job [] jobs = Job.FindAvailable();
			bool t = false;
			foreach(Job job in jobs)
			{
				t = job.RunWithLock();
				if (t == true) 
				{
					_repository.Remove (job.ID);
				}
				return t;
			}
			return null;
		}

		/// <summary>
		/// Works the off.
		/// </summary>
		/// <returns>The off.</returns>
		/// <param name="num">Number.</param>
		public static Report WorkOff(int num = 100)
		{
			Report report = new Report();

			for(int i = 0; i < num; i++)
			{
				bool? work = Job.ReserveAndRunOneJob ();

				if(work == true)
				{
					report.success++;
				}
				else if(work == false)
				{
					report.failure++;
				}
				else
				{
					break;
				}
			}

			return report;
		}
		/// <summary>
		/// Runs the with lock.
		/// </summary>
		/// <returns><c>true</c>, if with lock was run, <c>false</c> otherwise.</returns>
		public bool RunWithLock()
		{
			Log (string.Format ("* [JOB] aquiring lock on {0}",_type));
			if (this.LockExclusively()) 
			{
				try 
				{
					Type types = Assembly.LoadFrom (_assembly).GetType (_type, true);
					//ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
					XmlSerializer serializer = new XmlSerializer (types);
					IJob job = (IJob)serializer.Deserialize (new StringReader(_handler));
					//IJob job = (IJob)ci.Invoke(new object[0]);
					Stopwatch benchmark = new Stopwatch ();
					benchmark.Start();
					job.perform ();
					benchmark.Stop();
					TimeSpan ts = benchmark.Elapsed;
					Log(string.Format("* [JOB] {0} completed after {1}",
					                  _type, (ts.TotalMilliseconds / 1000).ToString()));

				} 
				catch (Exception e) 
				{
					Log (string.Format("* [JOB] {0} failed with {1}: {2} -" + 
						"{3} failed attempts",_type,_assembly,e.Message,_attempts));
					this.Reschedule (e.Message);
					//throw e;
					return false;
				}
			} 
			else 
			{
				Log (string.Format ("* [JOB] failed to aquire exclusive lock for {0}",_type));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Enqueue the specified job, priority, run_at and dllPath.
		/// </summary>
		/// <param name="job">The object that uses the IJob interface.</param>
		/// <param name="priority">Give the job priority. 0 is default and higher the number the more priority.
		/// </param>
		/// <param name="run_at">If no time is specfied, the job will be scheduled to run ASAP</param>
		/// <param name="dllPath">Supplying a path will override the path set automatically. The path 
		/// that is automatically generated is the path of the dll of the running program.
		/// When using with asp.net/MVC the dll of the running web application is in a temporary directory. This is 
		/// usually not a problem. However, on server reboots it may not be the same directory in most cases it is. 
		/// Most of the time you will not need to set this.
		/// </param>
		public static Job Enqueue(IJob job, int priority = 0, 
		                           DateTime? run_at = null,
		                           string dllPath = null)
		{
			Job newJob = new Job();
			newJob.Priority = priority;
			newJob.ObjectType = ParseType(job.GetType());
			if (dllPath == null) 
			{
				newJob.JobAssembly = System.Reflection.Assembly.GetAssembly (job.GetType()).Location;
			} 
			else 
			{
				newJob.JobAssembly = dllPath;
			}

			newJob.LastError = null;
			newJob.Handler = SerializeToXml(job);
			newJob.RunAt = (run_at == null ? DateTime.Now : (DateTime)run_at);
			newJob.FailedAt = null;
			newJob.LockedAt = null;

			return _repository.CreateJob(newJob);
		}
		/// <summary>
		/// Serializes to xml.
		/// </summary>
		/// <returns>The to xml.</returns>
		/// <param name="job">Job.</param>
		private static string SerializeToXml(IJob job)
		{
			StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			XmlSerializer serializer = new XmlSerializer(job.GetType());
			serializer.Serialize(writer, job);
			return writer.ToString();
		}

		/// <summary>
		/// Parses the type.
		/// </summary>
		/// <returns>The type.</returns>
		/// <param name="type">Type.</param>
		private static string ParseType(Type type)
		{
			if (type.AssemblyQualifiedName == null)
				throw new ArgumentException("Assembly Qualified Name is null");

			int idx = type.AssemblyQualifiedName.IndexOf(',');

			string retValue = type.AssemblyQualifiedName.Substring(0, idx);

			return retValue;
		}
	}
}

