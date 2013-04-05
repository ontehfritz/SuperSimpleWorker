using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;

namespace delayed_job
{
	public class Job
	{
		public int id;
		public int priority;  //Allows some jobs to jump to the front of the queue
		public int attempts;  //Provides for retries, but still fail eventually.
		public string handler; //YAML-encoded string of the object that will do work
		public string last_error; //reason for last failure (See Note below)
		public DateTime run_at; //When to run. Could be Time.now for immediately, or sometime in the future.
		public DateTime locked_at; //Set when a client is working on this object
		public DateTime failed_at; //Set when all retries have failed (actually, by default, the record is deleted instead)
		public string locked_by; //Who is working on this object (if locked)
		public string type;

		const int MAX_ATTEMPTS = 25;
		const int MAX_RUN_TIME = 4; //hours
		string set_table_name = "delayed_jobs";

		public Job ()
		{

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

		public static bool RunWithLock(int max_run_time, string worker_name)
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			Job[] jobs = sqlite.GetJobs();
			Type types = Type.GetType(jobs[0].type, true);
			//ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

			XmlSerializer serializer = new XmlSerializer(types);

			IJob job = (IJob)serializer.Deserialize(new StringReader(jobs[0].handler));
			//IJob job = (IJob)ci.Invoke(new object[0]);
			try
			{
				job.perform();
			}
			catch(Exception e)
			{
				throw e;
				return false;
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

		public static void Enqueue(IJob job, int priority = 0, DateTime? run_at = null)
		{
			Job newJob = new Job();
			newJob.priority = priority;
			//newJob.type = job.GetType();
			newJob.handler = SerializeToXml(job);
			newJob.run_at = (run_at == null ? DateTime.Now : (DateTime)run_at);
			RepositorySQLite sqlite = new RepositorySQLite();
			sqlite.CreateJob(newJob, job);
		}

		public bool Failed ()
		{
			return false;
		}

		public static void ClearLocks()
		{

		}
	}
}

