using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

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

		const int MAX_ATTEMPTS = 25;
		const int MAX_RUN_TIME = 4; //hours
		string set_table_name = "delayed_jobs";

		public Job ()
		{

		}

		private static string SerializeToXml(IJob job)
		{
			StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			XmlSerializer serializer = new XmlSerializer(job.GetType());
			serializer.Serialize(writer, job);
			return writer.ToString();
		}

		public static void enqueue(IJob job, int priority = 0, DateTime? run_at = null)
		{
			Job newJob = new Job();
			newJob.priority = priority;
			newJob.handler = SerializeToXml(job);
			newJob.run_at = (run_at == null ? DateTime.Now : (DateTime)run_at);
			RepositorySQLite sqlite = new RepositorySQLite();
			sqlite.CreateJob(newJob);
		}

		public bool failed ()
		{
			return false;
		}

		public static void ClearLocks()
		{

		}
	}
}

