using System;
using NUnit.Framework;

namespace SuperSimple.Worker
{
	[TestFixture()]
	public class RepositoryPostgreSQLTest
	{
		string connectionString = 
			"Server=172.16.24.160;Port=5432;User Id=postgres;Password=;Database=ssw";

		[Test()]
		public void TestCreateJob()
		{
			RepositoryPostgreSQL db = new RepositoryPostgreSQL(connectionString);
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
			RepositoryPostgreSQL db = new RepositoryPostgreSQL(connectionString);

			Job job = db.GetJob(2);
			Assert.AreEqual(2, job.ID);
		}

		[Test()]
		public void TestGetJobs()
		{
			RepositoryPostgreSQL db = new RepositoryPostgreSQL(connectionString);

			Job [] jobs = db.GetJobs();
			Assert.Greater(jobs.Length, 0);
		}

		[Test()]
		public void TestClearJobs()
		{
			RepositoryPostgreSQL db = new RepositoryPostgreSQL(connectionString);

			db.ClearJobs("test");
		}

		[Test()]
		public void TestUpdateJob()
		{
			RepositoryPostgreSQL db = new RepositoryPostgreSQL(connectionString);

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






