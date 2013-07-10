using System;
using NUnit.Framework;

namespace DelayedJob
{
#if WINDOWS
	public class RepositorySQLiteTest
	{
		public RepositorySQLiteTest(){}
	}
#else
	[TestFixture()]
	public class RepositorySQLiteTest
	{
		string connectionString = 
				"URI=file:delay_job.db";

		[Test()]
		public void TestCreateJob()
		{
			RepositoryMonoSQLite db = new RepositoryMonoSQLite(connectionString);
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
			RepositoryMonoSQLite db = new RepositoryMonoSQLite(connectionString);

			Job job = db.GetJob(2);
			Assert.AreEqual(2, job.ID);
		}

		[Test()]
		public void TestGetJobs()
		{
			RepositoryMonoSQLite db = new RepositoryMonoSQLite(connectionString);
			
			Job [] jobs = db.GetJobs();
			Assert.Greater(jobs.Length, 0);
		}

		[Test()]
		public void TestClearJobs()
		{
			RepositoryMonoSQLite db = new RepositoryMonoSQLite(connectionString);

			db.ClearJobs("test");
		}

		[Test()]
		public void TestUpdateJob()
		{
			RepositoryMonoSQLite db = new RepositoryMonoSQLite(connectionString);

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
#endif
}

