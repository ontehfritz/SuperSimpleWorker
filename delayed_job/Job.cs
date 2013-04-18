using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;

namespace delayed_job
{
	public class Job
	{

		/*database fields*/
		public int id;
		public int priority;  //Allows some jobs to jump to the front of the queue
		public int attempts;  //Provides for retries, but still fail eventually.
		public string handler; //xml string of the object that will do work
		public string last_error; //reason for last failure (See Note below)
		public DateTime run_at; //When to run. Could be Time.now for immediately, or sometime in the future.
		public DateTime locked_at; //Set when a client is working on this object
		public DateTime failed_at; //Set when all retries have failed (actually, by default, the record is deleted instead)
		public string locked_by; //Who is working on this object (if locked)
		public string type;

		/*Object attributes */ 
		const int MAX_ATTEMPTS = 25;
		const int MAX_RUN_TIME = 4; //hours
		public bool destroyFailedJobs = true;
		public string workerName; 

		public int minPriority;
		public int maxPriority;

		string set_table_name = "delayed_jobs";

		public Job ()
		{
			workerName = Guid.NewGuid().ToString();
		}

//		public static T InstantiateType<T>(Type type)
//		{
//			if (type == null)
//			{
//				throw new ArgumentNullException("type", "Cannot instantiate null");
//			}
//			ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
//			if (ci == null)
//			{
//				throw new ArgumentException("Cannot instantiate type which has no empty constructor", type.Name);
//			}
//			return (T) ci.Invoke(new object[0]);
//		}

		public static bool LockExclusively(int max_run_time, 
		                                   string worker_name)
		{
			return true;
		}

		public static void ClearLocks(string workerName)
		{
			RepositorySQLite sqlite = new RepositorySQLite();

			
			sqlite.ClearJobs(workerName);
		}

		public static Job[] FindAvailable(int limit = 5, int max_run_time = MAX_RUN_TIME)
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			return sqlite.GetNextReadyJobs(limit);
		}

		public static bool? ReserveAndRunOneJob(int max_run_time = MAX_RUN_TIME)
		{
			Job [] jobs = Job.FindAvailable();
			bool t = false;
			foreach(Job job in jobs)
			{
				t = job.RunWithLock(4, job.workerName);
				return t;
			}

			return null;
		}

		public struct Report
		{
			public int success; 
			public int failure;
		}

		public static Report WorkOff(int num = 100)
		{
			Report report = new Report();
			for(int i = 0; i < num; i++)
			{
				if(Job.ReserveAndRunOneJob() == true)
					report.success++;
				else if(Job.ReserveAndRunOneJob() == false)
					report.failure++;
				else
					break;

			}

			return report;
		}

		public bool RunWithLock(int max_run_time, string workerName)
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			     

			this.locked_by = workerName;
			this.locked_at = DateTime.Now;
			sqlite.UpdateJob(this);
		

			Type types = Type.GetType(this.type, true);
			//ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

			XmlSerializer serializer = new XmlSerializer(types);

			IJob job = (IJob)serializer.Deserialize(new StringReader(this.handler));
			//IJob job = (IJob)ci.Invoke(new object[0]);
			try
			{
				job.perform();
			}
			catch(Exception e)
			{
				throw e;
				//return false;
			}

			return true;
		}

		private static string SerializeToXml(IJob job)
		{
			StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			XmlSerializer serializer = new XmlSerializer(job.GetType());
			serializer.Serialize(writer, job);
			return writer.ToString();
		}

		private static string ParseType(Type type)
		{
			if (type.AssemblyQualifiedName == null)
				throw new ArgumentException("Assembly Qualified Name is null");
			
			int idx = type.AssemblyQualifiedName.IndexOf(',', 
			                                             type.AssemblyQualifiedName.IndexOf(',') + 1);
			
			string retValue = type.AssemblyQualifiedName.Substring(0, idx);
			
			return retValue;
		}

		public static void Enqueue(IJob job, int priority = 0, DateTime? run_at = null)
		{
			Job newJob = new Job();
			newJob.priority = priority;
			newJob.type = ParseType(job.GetType());
			newJob.handler = SerializeToXml(job);

			newJob.run_at = (run_at == null ? DateTime.Now : (DateTime)run_at);
			RepositorySQLite sqlite = new RepositorySQLite();
			sqlite.CreateJob(newJob/*, job*/);
		}

		public bool Failed ()
		{
			return false;
		}
	}
}

