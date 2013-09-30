
namespace SuperSimple.Worker
{
	using System;
	using System.Data;
	using System.Collections.Generic;
	#if WINDOWS
	public class RepositoryMonoSQLite 
	{
		public RepositoryMonoSQLite (){
			throw new NotImplementedException("Mono SQLite is not implemented for Windows.");
		}
	}
	#else
	using Mono.Data.Sqlite;
	/// <summary>
	/// Repository mono SQ lite.
	/// </summary>
	public class RepositoryMonoSQLite : IRepository 
	{
		private string _connectionString;
		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString
		{
			get 
			{
				return _connectionString;
			}

			set
			{
				_connectionString = value;
			}
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="DelayedJob.RepositoryMonoSQLite"/> class.
		/// </summary>
		public RepositoryMonoSQLite (){ }
		/// <summary>
		/// Initializes a new instance of the <see cref="DelayedJob.RepositoryMonoSQLite"/> class.
		/// </summary>
		/// <param name="connectionString">Connection string.</param>
		public RepositoryMonoSQLite(string connectionString)
		{
			_connectionString = connectionString;
		}
		/// <summary>
		/// Remove the specified job with ID.
		/// </summary>
		/// <param name="jobID">Job I.</param>
		public void Remove(int jobID)
		{
			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
			{
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
                string delete = "DELETE FROM ssw WHERE id = @JobID";
				
				dbcmd.CommandText = delete;
				dbcmd.Parameters.AddWithValue("@JobID", jobID);
				
				dbcmd.ExecuteNonQuery();
				
				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

		}
		/// <summary>
		/// Gets the next ready jobs.
		/// </summary>
		/// <returns>.</returns>
		/// <param name="limit">Limit is how many jobs will be returned</param>
		public Job[] GetNextReadyJobs(int limit = 1)
		{
			List<Job> jobs = new List<Job>();

			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
			{
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

                string next = "select * from ssw where " +
					"locked_by is null and " +
						"run_at <= @time " +
						"order by priority desc, run_at asc limit @limit";
			   
				dbcmd.CommandText = next;
				dbcmd.Parameters.AddWithValue("@limit", limit);
				dbcmd.Parameters.AddWithValue("@time", DateTime.Now);

				Job job = new Job();
				IDataReader reader = dbcmd.ExecuteReader();

				while(reader.Read()) 
                {
					job = new Job();
					job.Attempts = int.Parse(reader["attempts"].ToString());
					job.ID = int.Parse(reader["id"].ToString());

					if(reader["failed_at"].ToString() != "")
                    {
						job.FailedAt = DateTime.Parse(reader["failed_at"].ToString());
					}

					job.ObjectType = reader["type"].ToString();
					job.JobAssembly = reader["assembly"].ToString();
					job.Handler = reader["handler"].ToString();
					job.LastError = reader["last_error"].ToString();

					if(reader["locked_at"].ToString() != "")
                    {
						job.LockedAt = DateTime.Parse(reader["locked_at"].ToString());
					}

					job.LockedBy = reader["locked_by"].ToString();
					job.Priority = int.Parse(reader["priority"].ToString());
					job.RunAt = DateTime.Parse(reader["run_at"].ToString());
					jobs.Add(job);
				}

				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

			return jobs.ToArray();
		}
		/// <summary>
		/// Updates the job.
		/// </summary>
		/// <param name="job">Job.</param>
		public void UpdateJob(Job job)
        {
			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
            {
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string update = "update ssw " +
					"set " +
					"priority = @priority," + 
					"attempts = @attempts," + 
					"last_error = @last_error," +
					"run_at = @run_at," + 
					"failed_at = @failed_at," + 
					"locked_by = @locked_by, " +
					"locked_at = @locked_at " + 
					"where ID = @ID";

				dbcmd.CommandText = update;
				dbcmd.Parameters.AddWithValue("@ID", job.ID);
				dbcmd.Parameters.AddWithValue("@priority", job.Priority);
				dbcmd.Parameters.AddWithValue("@attempts", job.Attempts);
				dbcmd.Parameters.AddWithValue("@last_error", job.LastError);
				dbcmd.Parameters.AddWithValue("@run_at", job.RunAt);
				dbcmd.Parameters.AddWithValue("@failed_at", job.FailedAt);
				dbcmd.Parameters.AddWithValue("@locked_by", job.LockedBy);
				dbcmd.Parameters.AddWithValue("@locked_at", job.LockedAt);
				
				dbcmd.ExecuteNonQuery();
				
				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}
		}
		/// <summary>
		/// Clears the jobs.
		/// </summary>
		/// <param name="workerName">Worker name.</param>
		public void ClearJobs(string workerName)
        {
			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
            {
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string update = "update ssw " +
						"set locked_by = null, " +
						"locked_at = null " + 
						"where locked_by = @WorkerName";

				dbcmd.CommandText = update;
				dbcmd.Parameters.AddWithValue("@WorkerName", workerName);

				dbcmd.ExecuteNonQuery();

				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}
		}
		/// <summary>
		/// Pass a job object and that object will be created in the database.
		/// </summary>
		/// <returns>After creation of the object it will return the object with its ID</returns>
		/// <param name="job">Job.</param>
		public Job CreateJob(Job job)
        {
			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
            {
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string insertRecord = "insert into ssw (" +
						"type," + 
						"assembly," + 
						"priority," + 
						"attempts," + 
						"handler," + 
						"last_error," + 
						"run_at," + 
						"locked_at," + 
						"failed_at," + 
						"locked_by" + 
						") values (" +
						"@type," + 
						"@assembly," + 
						"@priority," + 
						"@attempts," + 
						"@handler," + 
						"@last_error," + 
						"@run_at," + 
						"@locked_at," + 
						"@failed_at," + 
						"@locked_by" + 
						");select last_insert_rowid();";

				dbcmd.CommandText = insertRecord;
				dbcmd.Parameters.AddWithValue("@type",job.ObjectType);
				dbcmd.Parameters.AddWithValue("@assembly",job.JobAssembly);
				dbcmd.Parameters.AddWithValue("@priority",job.Priority);
				dbcmd.Parameters.AddWithValue("@attempts",job.Attempts);
				dbcmd.Parameters.AddWithValue("@handler", job.Handler);
				dbcmd.Parameters.AddWithValue("@last_error",job.LastError);
				dbcmd.Parameters.AddWithValue("@run_at",job.RunAt);
				dbcmd.Parameters.AddWithValue("@locked_at", job.LockedAt);
				dbcmd.Parameters.AddWithValue("@failed_at", job.FailedAt);
				dbcmd.Parameters.AddWithValue("@locked_by", job.LockedBy);

				var id = dbcmd.ExecuteScalar();

				job.ID = int.Parse(id.ToString());

				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

			return job;
		}
		/// <summary>
		/// Gets a job with the specified id.
		/// </summary>
		/// <returns>A job object with that has the id provided.</returns>
		/// <param name="pid">Pid.</param>
		public Job GetJob(int pid)
        {
			Job job = new Job();

			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
            {
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
				string query = "select * from ssw where id = @pid";

				dbcmd.CommandText = query;
				
				dbcmd.Parameters.AddWithValue("@pid",pid);

				IDataReader reader = dbcmd.ExecuteReader();
				while(reader.Read()) 
                {
					job.Attempts = int.Parse(reader["attempts"].ToString());
					job.ID = int.Parse(reader["id"].ToString());
					job.ObjectType = reader["type"].ToString();
					job.JobAssembly = reader["assembly"].ToString();

					if(reader["failed_at"].ToString() != "")
                    {
						job.FailedAt = DateTime.Parse(reader["failed_at"].ToString());
					}
					job.Handler = reader["handler"].ToString();
					job.LastError = reader["last_error"].ToString();

					if(reader["failed_at"].ToString() != "")
                    {
						job.LockedAt = DateTime.Parse(reader["locked_at"].ToString());
					}

					job.LockedBy = reader["locked_by"].ToString();
					job.Priority = int.Parse(reader["priority"].ToString());
					job.RunAt = DateTime.Parse(reader["run_at"].ToString());
				}
			}

			return job;
		}
		/// <summary>
		/// This will get all jobs.
		/// </summary>
		/// <returns>an array of job objects</returns>
		public Job[] GetJobs(){
			List<Job> jobs = new List<Job>();

			using(SqliteConnection dbcon = new SqliteConnection(_connectionString))
            {
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
				string query = "select * from ssw";
				
				dbcmd.CommandText = query;
			
				IDataReader reader = dbcmd.ExecuteReader();
				Job job = new Job();
				while(reader.Read()) 
                {
					job = new Job();
					job.ObjectType = reader["type"].ToString();
					job.JobAssembly = reader["assembly"].ToString();
					job.Attempts = int.Parse(reader["attempts"].ToString());
					job.ID = int.Parse(reader["id"].ToString());
					if (reader ["failed_at"].ToString () != "") 
                    {
						job.FailedAt = DateTime.Parse (reader["failed_at"].ToString());
					}
					job.Handler = reader["handler"].ToString();
					job.LastError = reader["last_error"].ToString();

					if (reader ["locked_at"].ToString () != "") 
                    {
						job.LockedAt = DateTime.Parse (reader["locked_at"].ToString());
					}

					job.LockedBy = reader["locked_by"].ToString();
					job.Priority = int.Parse(reader["priority"].ToString());
					job.RunAt = DateTime.Parse(reader["run_at"].ToString());
					jobs.Add(job);
				}
			}

			return jobs.ToArray();
		}
	}
	#endif
}


