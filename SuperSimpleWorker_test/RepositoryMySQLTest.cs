using System;
using NUnit.Framework;

namespace SuperSimple.Worker
{
	[TestFixture()]
	public class RepositoryMySQLTest
	{
		string connectionString = 
            "Data Source=172.16.24.168;Database=test;User ID=root;Password=password";

		[Test()]
		public void TestCreateJob()
		{
			RepositoryMySQL db = new RepositoryMySQL(connectionString);
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
			RepositoryMySQL db = new RepositoryMySQL(connectionString);

			Job job = db.GetJob(2);
			Assert.AreEqual(2, job.ID);
		}

		[Test()]
		public void TestGetJobs()
		{
			RepositoryMySQL db = new RepositoryMySQL(connectionString);

			Job [] jobs = db.GetJobs();
			Assert.Greater(jobs.Length, 0);
		}

		[Test()]
		public void TestClearJobs()
		{
			RepositoryMySQL db = new RepositoryMySQL(connectionString);

			db.ClearJobs("test");
		}

		[Test()]
		public void TestUpdateJob()
		{
			RepositoryMySQL db = new RepositoryMySQL(connectionString);

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




