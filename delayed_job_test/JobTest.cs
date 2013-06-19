using System;
using System.IO;
using NUnit.Framework;
using DelayedJob;

namespace DelayedJob
{
	[TestFixture()]
	public class JobTest
	{
		string connectionString = 
		 "URI=file:delay_job.db";

		[Test()]
		public void TestEnqueue ()
		{
			Job.Repository = new RepositorySQLite (connectionString);
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
			RepositorySQLite sqlite = new RepositorySQLite(connectionString);
			//Assert.AreEqual("Fritz",Job.RunWithLock(typeof(Ajob),"worker1"));
			//Assert.AreEqual("Fritz",Job.RunWithLock("worker1"));
			//Assert.AreEqual("Hello",typeof(Ajob).ToString());
			Job.Enqueue(new Ajob("TestRunWithLock"));
			Job[] newJobs = sqlite.GetNextReadyJobs(1);
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
			RepositorySQLite sqlite = new RepositorySQLite(connectionString);
			Job.Enqueue(new Ajob("Reschedule"));
			Job[] newJobs = sqlite.GetNextReadyJobs(1);
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

		public string perform()
		{
			File.Create(name);

			return name;
		}
	}
}

