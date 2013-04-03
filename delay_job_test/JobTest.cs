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
			//Assert.AreEqual("Fritz",Job.RunWithLock(typeof(Ajob),"worker1"));
			Assert.AreEqual("Fritz",Job.RunWithLock(Type.GetType("delay_job.Ajob"),"worker1"));
			//Assert.AreEqual("Hello",typeof(Ajob).ToString());
			
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
			return name;
		}
	}
}

