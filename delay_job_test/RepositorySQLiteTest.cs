using System;
using NUnit.Framework;
using Mono.Data.Sqlite;

namespace delayed_job
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

			//db.CreateJob(job, new );
		}

		[Test()]
		public void TestGetJob()
		{
			RepositorySQLite db = new RepositorySQLite();

			Job job = db.GetJob(1);
			Assert.AreEqual(job.id, 1);
		}

		[Test()]
		public void TestGetJobs()
		{
			RepositorySQLite db = new RepositorySQLite();
			
			Job [] jobs = db.GetJobs();
			Assert.Greater(jobs.Length, 0);
		}

		[Test()]
		public void TestClearJobs()
		{
			RepositorySQLite db = new RepositorySQLite();

			db.ClearJobs("test");
		}
	}
}

