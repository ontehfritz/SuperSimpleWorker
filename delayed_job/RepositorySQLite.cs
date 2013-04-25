using System;
using System.Data;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace delayed_job
{
	public class RepositorySQLite : IRepository 
	{
		public string connectionString = "URI=file:/Users/Fritz/Documents/Projects/delayed_job/delay_job_test/bin/Debug/delay_job.db";
		public RepositorySQLite (){}

//		private string ParseType(Type type)
//		{
//			if (type.AssemblyQualifiedName == null)
//				throw new ArgumentException("Assembly Qualified Name is null");
//
//			int idx = type.AssemblyQualifiedName.IndexOf(',', 
//			          type.AssemblyQualifiedName.IndexOf(',') + 1);
//			
//			string retValue = type.AssemblyQualifiedName.Substring(0, idx);
//			
//			return retValue;
//		}

		public void Remove(int jobID){

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
				string delete = "DELETE FROM delay_jobs WHERE id = @JobID";
				
				dbcmd.CommandText = delete;
				dbcmd.Parameters.AddWithValue("@JobID", jobID);
				
				dbcmd.ExecuteNonQuery();
				
				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

		}


		public Job[] GetNextReadyJobs(int limit = 1)
		{
			List<Job> jobs = new List<Job>();

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string next = "select * from delay_jobs where " +
					"locked_by is null " + 
						"order by priority desc, run_at asc limit @limit";
			   
				dbcmd.CommandText = next;
				dbcmd.Parameters.AddWithValue("@limit", limit);
				Job job = new Job();
				IDataReader reader = dbcmd.ExecuteReader();
				while(reader.Read()) {
					job = new Job();
					job.attempts = int.Parse(reader["attempts"].ToString());
					job.id = int.Parse(reader["id"].ToString());
					job.failed_at = DateTime.Parse(reader["failed_at"].ToString());
					job.type = reader["type"].ToString();
					job.handler = reader["handler"].ToString();
					job.last_error = reader["last_error"].ToString();
					job.locked_at = DateTime.Parse(reader["locked_at"].ToString());
					job.locked_by = reader["locked_by"].ToString();
					job.priority = int.Parse(reader["priority"].ToString());
					job.run_at = DateTime.Parse(reader["run_at"].ToString());
					jobs.Add(job);
				}

				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

			return jobs.ToArray();
		}

		public void UpdateJob(Job job)
		{
			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string update = "update delay_jobs " +
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
				dbcmd.Parameters.AddWithValue("@ID", job.id);
				dbcmd.Parameters.AddWithValue("@priority", job.priority);
				dbcmd.Parameters.AddWithValue("@attempts", job.attempts);
				dbcmd.Parameters.AddWithValue("@last_error", job.last_error);
				dbcmd.Parameters.AddWithValue("@run_at", job.run_at);
				dbcmd.Parameters.AddWithValue("@failed_at", job.failed_at);
				dbcmd.Parameters.AddWithValue("@locked_by", job.locked_by);
				dbcmd.Parameters.AddWithValue("@locked_at", job.locked_at);
				
				dbcmd.ExecuteNonQuery();
				
				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}
		}

		public void ClearJobs(string workerName)
		{
			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string update = "update delay_jobs " +
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

		public void CreateDb()
		{
			//string connectionString = "URI=file:delay_job.db";

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string createTable = "CREATE TABLE delay_jobs(" +
					"id integer not null primary key," + 
					"type varchar(255)," + 
					"priority integer default 0," + 
					"attempts integer default 0," + 
					"handler varchar(255)," + 
					"last_error varchar(255)," + 
					"run_at datetime default null," + 
					"locked_at datetime default null," + 
					"failed_at datetime default null," + 
					"locked_by varchar(255)," + 
					"created_at timestamp default current_timestamp," + 
					"modified_at timestamp default current_timestamp" + 
					")";

				dbcmd.CommandText = createTable;
				dbcmd.ExecuteNonQuery();
				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}
		}

		public Job CreateJob(Job job/*, IJob j*/)
		{
			//string connectionString = "URI=file:delay_job.db";

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string insertRecord = "insert into delay_jobs (" +
						"type," + 
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
				dbcmd.Parameters.AddWithValue("@type",job.type);
				dbcmd.Parameters.AddWithValue("@priority",job.priority);
				dbcmd.Parameters.AddWithValue("@attempts",job.attempts);
				dbcmd.Parameters.AddWithValue("@handler", job.handler);
				dbcmd.Parameters.AddWithValue("@last_error",job.last_error);
				dbcmd.Parameters.AddWithValue("@run_at",job.run_at);
				dbcmd.Parameters.AddWithValue("@locked_at", job.locked_at);
				dbcmd.Parameters.AddWithValue("@failed_at", job.failed_at);
				dbcmd.Parameters.AddWithValue("@locked_by", job.locked_by);

				var id = dbcmd.ExecuteScalar();

				job.id = int.Parse(id.ToString());

				dbcmd.Dispose();
				dbcmd = null;
				dbcon.Close();
			}

			return job;
		}

		public Job GetJob(int pid)
		{
			Job job = new Job();

			//string connectionString = "URI=file:delay_job.db";
			
			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
				string query = "select * from delay_jobs where id = @pid";
				
				dbcmd.CommandText = query;
				
				dbcmd.Parameters.AddWithValue("@pid",pid);

				IDataReader reader = dbcmd.ExecuteReader();
				while(reader.Read()) {
					job.attempts = int.Parse(reader["attempts"].ToString());
					job.id = int.Parse(reader["id"].ToString());
					job.failed_at = DateTime.Parse(reader["failed_at"].ToString());
					job.handler = reader["handler"].ToString();
					job.last_error = reader["last_error"].ToString();
					job.locked_at = DateTime.Parse(reader["locked_at"].ToString());
					job.locked_by = reader["locked_by"].ToString();
					job.priority = int.Parse(reader["priority"].ToString());
					job.run_at = DateTime.Parse(reader["run_at"].ToString());
				}
			}

			return job;
		}

		public Job[] GetJobs()
		{
			List<Job> jobs = new List<Job>();

			//string connectionString = "URI=file:delay_job.db";
			
			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();
				
				string query = "select * from delay_jobs";
				
				dbcmd.CommandText = query;
			
				IDataReader reader = dbcmd.ExecuteReader();
				Job job = new Job();
				while(reader.Read()) {
					job = new Job();
					job.type = reader["type"].ToString();
					job.attempts = int.Parse(reader["attempts"].ToString());
					job.id = int.Parse(reader["id"].ToString());
					job.failed_at = DateTime.Parse(reader["failed_at"].ToString());
					job.handler = reader["handler"].ToString();
					job.last_error = reader["last_error"].ToString();
					job.locked_at = DateTime.Parse(reader["locked_at"].ToString());
					job.locked_by = reader["locked_by"].ToString();
					job.priority = int.Parse(reader["priority"].ToString());
					job.run_at = DateTime.Parse(reader["run_at"].ToString());
					jobs.Add(job);
				}
			}

			return jobs.ToArray();
		}
	}
}

