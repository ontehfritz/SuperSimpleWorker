using System;
using System.Data;
using System.Collections.Generic;
using Mono.Data.Sqlite;

namespace delayed_job
{
	public class RepositorySQLite : IRepository 
	{
		public RepositorySQLite (){}


		protected virtual string GetStorableJobTypeName(Type jobType)
		{
			if (jobType.AssemblyQualifiedName == null)
			{
				throw new ArgumentException("Cannot determine job type name when type's AssemblyQualifiedName is null");
			}
			
			int idx = jobType.AssemblyQualifiedName.IndexOf(',');
			// find next
			idx = jobType.AssemblyQualifiedName.IndexOf(',', idx + 1);
			
			string retValue = jobType.AssemblyQualifiedName.Substring(0, idx);
			
			return retValue;
		}


		public void CreateDb()
		{
			string connectionString = "URI=file:delay_job.db";

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

		public Job CreateJob(Job job, IJob j)
		{
			string connectionString = "URI=file:delay_job.db";

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
				dbcmd.Parameters.AddWithValue("@type",GetStorableJobTypeName(j.GetType()));
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

			string connectionString = "URI=file:delay_job.db";
			
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

			string connectionString = "URI=file:delay_job.db";
			
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

