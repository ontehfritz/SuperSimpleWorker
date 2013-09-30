namespace SuperSimple.Worker
{
	using System;
	/// <summary>
	/// This interface is used for creating repositories for a database.
	/// </summary>
	public interface IRepository
	{
		/// <summary>
		/// Pass a job object and that object will be created in the database. 
		/// </summary>
		/// <returns>After creation of the object it will return the object with its ID</returns>
		/// <param name="job">Job.</param>
		Job CreateJob(Job job);
		/// <summary>
		/// Gets a job with the specified id.
		/// </summary>
		/// <returns>A job object with that has the id provided.</returns>
		/// <param name="pid">Pid.</param>
		Job GetJob(int pid);
		/// <summary>
		/// This will get all jobs.
		/// </summary>
		/// <returns>an array of job objects</returns>
		Job[] GetJobs();
		/// <summary>
		/// Gets the next ready jobs.
		/// </summary>
		/// <returns>.</returns>
		/// <param name="limit">Limit is how many jobs will be returned</param>
		Job[] GetNextReadyJobs(int limit = 1);
		/// <summary>
		/// Clears the jobs.
		/// </summary>
		/// <param name="workerName">Worker name.</param>
		void ClearJobs(string workerName);
		/// <summary>
		/// Updates the job.
		/// </summary>
		/// <param name="job">Job.</param>
		void UpdateJob(Job job);
		/// <summary>
		/// Remove the specified job with ID.
		/// </summary>
		/// <param name="jobID">Job ID</param>
		void Remove(int jobID);
	}
}

