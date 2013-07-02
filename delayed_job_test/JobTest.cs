using System;
using System.IO;
using NUnit.Framework;
using DelayedJob;

namespace DelayedJob
{
	[TestFixture()]
	public class JobTest
	{
		//string connectionString = 
		 //"URI=file:delay_job.db";
		//"Data Source=172.16.24.128;Database=delayed_job_test;User ID=;Password=";
		IRepository repo = new RepositoryMonoSQLite("URI=file:delay_job.db");
		//IRepository repo = 
		//	new RepositoryMySQL("Data Source=172.16.24.131;Database=delayed_job_test;User ID=;Password=");
		//IRepository repo = 
		//	new RepositoryMsSQL("Data Source=172.16.24.129;Database=delayed_job_test;User ID=;Password=");


		[Test()]
		public void TestEnqueue ()
		{
			Job.Repository = repo;
			Job.Enqueue(new Ajob("Fritz"));
		}

		[Test()]
		public void TestWorkOff ()
		{
			Job.WorkOff();
		}

		[Test()]
		public void TestRunWithLock ()
		{

			//Assert.AreEqual("Fritz",Job.RunWithLock(typeof(Ajob),"worker1"));
			//Assert.AreEqual("Fritz",Job.RunWithLock("worker1"));
			//Assert.AreEqual("Hello",typeof(Ajob).ToString());
			Job.Enqueue(new Ajob("TestRunWithLock"));
			Job[] newJobs = repo.GetNextReadyJobs(1);
			Assert.AreEqual(true, newJobs[0].RunWithLock(12,"awesome"));
		}

		[Test()]
		public void TestFindAvailable ()
		{
			Job[] jobs = Job.FindAvailable();
			Assert.GreaterOrEqual(jobs.Length,0);
		}

		[Test()]
		public void TestReserveAndRunOneJob()
		{

			Job.ReserveAndRunOneJob();
		}

		[Test()]
		public void TestReschedule()
		{

			Job.Enqueue(new Ajob("Reschedule"));
			Job[] newJobs = repo.GetNextReadyJobs(1);
			//newJobs[0].Reschedule("test");
		}
	}

	public class Ajob : IJob
	{
		public string name;
		public Ajob() {}
		public Ajob(string n)
		{
			name = n;
		}

		public void perform()
		{
			File.Create(name);
		}
	}
}

