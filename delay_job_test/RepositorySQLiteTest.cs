using System;
using NUnit.Framework;
using Mono.Data.Sqlite;

namespace delay_job
{
	[TestFixture()]
	public class RepositorySQLiteTest
	{
		[Test()]
		public void TestCreateDb ()
		{
			RepositorySQLite db = new RepositorySQLite();
			db.CreateDb();
		}

		[Test()]
		public void TestCreateJob()
		{
			RepositorySQLite db = new RepositorySQLite();
			Job job = new Job();
			job.attempts = 0; 
			job.failed_at = DateTime.Now;
			job.handler = "";
			job.last_error = "";
			job.locked_at = DateTime.Now;
			job.locked_by = "";
			job.priority = 0;
			job.run_at = DateTime.Now;

			db.CreateJob(job);
		}
	}
}

