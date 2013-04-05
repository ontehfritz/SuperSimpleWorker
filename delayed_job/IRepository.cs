using System;

namespace delayed_job
{
	interface IRepository
	{
		void CreateDb();
		Job CreateJob(Job job, IJob j);
		Job GetJob(int pid);
		Job[] GetJobs();
		void ClearJobs(string workName);
		void UpdateJob(Job job);
	}
}

