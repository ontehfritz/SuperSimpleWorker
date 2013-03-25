using System;
using System.Data;

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

		public bool failed ()
		{
			return false;
		}

		public static void ClearLocks()
		{

		}
	}
}

