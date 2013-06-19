using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;

namespace DelayedJob
{
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

		//This struct is used to report how many jobs failed or succeeded 
		public struct Report{
			public int success; 
			public int failure;
		}

		/// <summary>
		/// Gets or sets the repository.
		/// This needs to be set before any operations can be done
		/// </summary>
		/// <value>The repository.</value>
		public static IRepository Repository
		{
			get {return _repository;}
			set {_repository = value; }
		}

		/// <summary>
		/// Gets or sets the ID.
		/// This is the ID of the job from the database. 
		/// If this is zero the ID has not been set. 
		/// </summary>
		/// <value>The ID</value>
		public int ID{
			get {return _id;}
			set {_id = value;}
		}

		public int Priority{
			get {return _priority;}
			set {_priority = value;}
		}

		public int Attempts{
			get {return _attempts;}
			set {_attempts = value;}
		}

		public string Handler{
			get {return _handler; }
			set {_handler = value; }
		}

		public string LastError{
			get { return _last_error; }
			set {_last_error = value; }
		}

		public DateTime? RunAt{
			get { return _run_at; }
			set {_run_at = value; }
		}

		public DateTime? LockedAt{
			get { return _locked_at; }
			set {_locked_at = value; }
		}
		public DateTime? FailedAt{
			get { return _failed_at; }
			set {_failed_at = value; }
		}

		public string LockedBy{
			get { return _locked_by; }
			set {_locked_by = value; }
		}

		public string ObjectType{
			get { return _type; }
			set {_type = value; }
		}

		public string JobAssembly{
			get { return _assembly; }
			set {_assembly = value; }
		}
		/*Object attributes */ 
		const int MAX_ATTEMPTS = 25;
		//const int MAX_RUN_TIME = 4; //hours
		private bool _destroyFailedJobs = false;

		public bool DestroyFailedJobs
		{
			get{
				return _destroyFailedJobs;
			}
			set{
				_destroyFailedJobs = value;
			}
		}

		private static string workerName = Guid.NewGuid().ToString(); 

		//string set_table_name = "delayed_jobs";

		public Job (){}

		private void Reschedule(string message, DateTime? time = null){
			if(_attempts < MAX_ATTEMPTS){
				time = (time == null ? DateTime.Now.AddSeconds(_attempts ^ 4 + 5) : time );
				_attempts += 1;
				_run_at = time;
				_last_error = message;
				this.unlock(); 
				_repository.UpdateJob(this);
			}
			else{
				if(_destroyFailedJobs){
					_repository.Remove(_id);
				}
				else{
					_failed_at = DateTime.Now;
					_repository.UpdateJob(this);
				}
			}
		}

		public void unlock(){
			_locked_at = null;
			_locked_by = null;
		}

		public bool LockExclusively(){
			_locked_by = workerName;
			_locked_at = DateTime.Now;
			try{
				_repository.UpdateJob(this);
			}
			catch(Exception e) {
				return false;
			}

			return true;
		}

		public static void ClearLocks(){
			_repository.ClearJobs(workerName);
		}

		public static Job[] FindAvailable(int limit = 5){
			return _repository.GetNextReadyJobs(limit);
		}

		public static bool? ReserveAndRunOneJob(){
			Job [] jobs = Job.FindAvailable();
			bool t = false;
			foreach(Job job in jobs){
				t = job.RunWithLock(4, workerName);
				if (t == true) {
					_repository.Remove (job.ID);
				}
				return t;
			}
			return null;
		}

		public static Report WorkOff(int num = 100){
			Report report = new Report();

			for(int i = 0; i < num; i++){
				bool? work = Job.ReserveAndRunOneJob ();

				if(work == true){
					report.success++;
				}
				else if(work == false){
					report.failure++;
				}
				else{
					break;
				}
			}

			return report;
		}
		/// <summary>
		/// Runs the with lock.
		/// </summary>
		/// <returns><c>true</c>, if with lock was run, <c>false</c> otherwise.</returns>
		/// <param name="max_run_time">Max_run_time.</param>
		/// <param name="workerName">Worker name.</param>
		public bool RunWithLock(int max_run_time, string workerName){
			if (this.LockExclusively()) {
				try {
					Type types = Assembly.LoadFrom (_assembly).GetType (_type, true);
					//ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
					XmlSerializer serializer = new XmlSerializer (types);
					IJob job = (IJob)serializer.Deserialize (new StringReader(_handler));
					//IJob job = (IJob)ci.Invoke(new object[0]);
					job.perform ();
				} catch (Exception e) {
					this.Reschedule (e.Message);
					//throw e;
					return false;
				}
			} else {
				return false;
			}

			return true;
		}

		private static string SerializeToXml(IJob job){
			StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			XmlSerializer serializer = new XmlSerializer(job.GetType());
			serializer.Serialize(writer, job);
			return writer.ToString();
		}

		private static string ParseType(Type type){
			if (type.AssemblyQualifiedName == null)
				throw new ArgumentException("Assembly Qualified Name is null");

			int idx = type.AssemblyQualifiedName.IndexOf(',');
			
			string retValue = type.AssemblyQualifiedName.Substring(0, idx);
			
			return retValue;
		}

		public static void Enqueue(IJob job, int priority = 0, DateTime? run_at = null){
			Job newJob = new Job();
			newJob.Priority = priority;
			newJob.ObjectType = ParseType(job.GetType());
			newJob.JobAssembly =  System.Reflection.Assembly.GetAssembly(job.GetType()).Location;
			newJob.Handler = SerializeToXml(job);
			newJob.RunAt = (run_at == null ? DateTime.Now : (DateTime)run_at);
			newJob.FailedAt = null;
			newJob.LockedAt = null;

			_repository.CreateJob(newJob);
		}
	}
}

