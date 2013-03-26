using System;

namespace delayed_job
{
	interface IRepository
	{
		void CreateDb();
		Job CreateJob(Job job);
		Job GetJob(int pid);
		Job[] GetJobs();
	}
}

