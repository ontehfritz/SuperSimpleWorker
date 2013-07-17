using System;
using NUnit.Framework;

namespace DelayedJob
{
	[TestFixture()]
	public class RepositoryMsSQLTest
	{
		string connectionString = 
			"Server=172.16.24.136;Database=delayed_job_test;User ID=sa;Password=";

		[Test()]
		public void TestCreateJob()
		{
			RepositoryMsSQL db = new RepositoryMsSQL(connectionString);
			Job job = new Job();
			job.Attempts = 0; 
			job.FailedAt = DateTime.Now;
			job.Handler = "";
			job.LastError = "";
			job.LockedAt = DateTime.Now;
			job.LockedBy = "";
			job.Priority = 0;
			job.RunAt = DateTime.Now;

			job = db.CreateJob(job);
			Assert.Greater (job.ID, 0);
		}

		[Test()]
		public void TestGetJob()
		{
			RepositoryMsSQL db = new RepositoryMsSQL(connectionString);

			Job job = db.GetJob(2);
			Assert.AreEqual(2, job.ID);
		}

		[Test()]
		public void TestGetJobs()
		{
			RepositoryMsSQL db = new RepositoryMsSQL(connectionString);

			Job [] jobs = db.GetJobs();
			Assert.Greater(jobs.Length, 0);
		}

		[Test()]
		public void TestClearJobs()
		{
			RepositoryMsSQL db = new RepositoryMsSQL(connectionString);

			db.ClearJobs("test");
		}

		[Test()]
		public void TestUpdateJob()
		{
			RepositoryMsSQL db = new RepositoryMsSQL(connectionString);

			Job job = new Job();
			job.Attempts = 0; 
			job.FailedAt = DateTime.Now;
			job.Handler = "";
			job.LastError = "";
			job.LockedAt = DateTime.Now;
			job.LockedBy = "";
			job.Priority = 0;
			job.RunAt = DateTime.Now;

			job = db.CreateJob(job);

			job.LockedBy = "TestUpdateJob";
			db.UpdateJob(job);
			Assert.AreEqual (db.GetJob(job.ID).LockedBy, "TestUpdateJob");
		}
	}
}



