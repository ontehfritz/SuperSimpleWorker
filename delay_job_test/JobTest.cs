using System;
using NUnit.Framework;

namespace delayed_job
{
	[TestFixture()]
	public class JobTest
	{
		[Test()]
		public void TestEnqueue ()
		{
			Job.enqueue(new Ajob("Fritz"));
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
			System.Console.WriteLine(name);
		}
	}
}

