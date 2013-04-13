using System;
using NUnit.Framework;
using delayed_job;

namespace delay_job
{
	[TestFixture()]
	public class JobTest
	{
		[Test()]
		public void TestEnqueue ()
		{
			Job.Enqueue(new Ajob("Fritz"));
		}

		[Test()]
		public void TestRunWithLock ()
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			//Assert.AreEqual("Fritz",Job.RunWithLock(typeof(Ajob),"worker1"));
			//Assert.AreEqual("Fritz",Job.RunWithLock("worker1"));
			//Assert.AreEqual("Hello",typeof(Ajob).ToString());
			Job.Enqueue(new Ajob("TestRunWithLock"));
			Job newJob = sqlite.GetNextReadyJobs("test");
			Assert.AreEqual(true, newJob.RunWithLock(12,"awesome"));
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
			RepositorySQLite sqlite = new RepositorySQLite();
			Job job = sqlite.GetNextReadyJobs("test");
			job.locked_by = "run";
			sqlite.UpdateJob(job);

			return name;
		}
	}
}

