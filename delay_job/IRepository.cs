using System;

namespace delay_job
{
	interface IRepository
	{
		void CreateDb();
		Job CreateJob(Job job);
		Job GetJob();
	}
}

