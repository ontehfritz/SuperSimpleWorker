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
			job.Attempts = 0; 
			job.FailedAt = DateTime.Now;
			job.Handler = "";
			job.LastError = "";
			job.LockedAt = DateTime.Now;
			job.LockedBy = "";
			job.Priority = 0;
			job.RunAt = DateTime.Now;

			//db.CreateJob(job, new );
		}

		[Test()]
		public void TestGetJob()
		{
			RepositorySQLite db = new RepositorySQLite();

			Job job = db.GetJob(2);
			Assert.AreEqual(2, job.ID);
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

		[Test()]
		public void TestUpdateJob()
		{
			RepositorySQLite db = new RepositorySQLite();
			Job job = db.GetJob(1);
			job.LockedBy = "TestUpdateJob";
			db.UpdateJob(job);
		}

	}
}

