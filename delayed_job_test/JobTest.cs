using System;
using System.IO;
using NUnit.Framework;
using DelayedJob;

namespace DelayedJob
{
	[TestFixture()]
	public class JobTest
	{
		//IRepository repo = new RepositoryMonoSQLite("URI=file:delay_job.db");
		IRepository repo = 
			new RepositoryMySQL("Data Source=172.16.24.160;Database=delayed_job_test;User ID=root;Password=");
		//IRepository repo = 
		//	new RepositoryMsSQL("Server=172.16.24.136;Database=delayed_job_test;User ID=sa;Password=");
		//IRepository repo = 
		//	new RepositoryPostgreSQL("Server=172.16.24.160;Port=5432;User Id=postgres;Password=;Database=delayed_job_test");

		[Test()]
		public void TestEnqueue ()
		{
			Job.Repository = repo;
			Job test = Job.Enqueue(new Ajob());
			Assert.Greater (test.ID, 0);
		}

		[Test()]
		public void TestWorkOff ()
		{
			Job.Repository = repo;
			Job.Enqueue (new Ajob());
			DelayedJob.Job.Report report = Job.WorkOff();
			Assert.Greater (report.success, 0);
		}

		[Test()]
		public void TestRunWithLock ()
		{
			Job.Repository = repo;
			Job test = Job.Enqueue(new Ajob());
			Assert.AreEqual(true, test.RunWithLock());
		}

		[Test()]
		public void TestFindAvailable ()
		{
			Job.Repository = repo;
			Job.Enqueue(new Ajob());
			Job[] jobs = Job.FindAvailable();
			Assert.GreaterOrEqual(jobs.Length,0);
		}

		[Test()]
		public void TestReserveAndRunOneJob()
		{
			Job.Repository = repo;
			Job.Enqueue(new Ajob());
			Assert.AreEqual(true, Job.ReserveAndRunOneJob());
		}

		[Test()]
		public void TestReschedule()
		{
			Job.Repository = repo;
			Job test = Job.Enqueue(new Ajob());
			test.Reschedule("test");
		}
	}

	public class Ajob : IJob
	{
		public string name;
		public Ajob() {}

		public void perform()
		{
			Console.WriteLine("Unit testing");
		}
	}
}
