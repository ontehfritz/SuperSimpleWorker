using System;
using System.Data;
using Mono.Data.Sqlite;

namespace delay_job
{
	public class RepositorySQLite : IRepository 
	{
		public RepositorySQLite ()
		{

		}

		public void CreateDb()
		{
			string connectionString = "URI=file:delay_job.db";

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string createTable = "CREATE TABLE delay_jobs(" +
					"id integer not null primary key," + 
					"priority integer," + 
					"attempts integer," + 
					"handler varchar(255)," + 
					"last_error varchar(255)," + 
					"run_at datetime," + 
					"locked_at datetime," + 
					"failed_at datetime," + 
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

		public Job CreateJob(Job job)
		{
			string connectionString = "URI=file:delay_job.db";

			using(SqliteConnection dbcon = new SqliteConnection(connectionString)){
				dbcon.Open();
				SqliteCommand dbcmd = dbcon.CreateCommand();

				string insertRecord = "insert into delay_jobs (" +
						"priority," + 
						"attempts," + 
						"handler," + 
						"last_error," + 
						"run_at," + 
						"locked_at," + 
						"failed_at," + 
						"locked_by" + 
						") values (" + 
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

		public Job GetJob()
		{
			Job job = new Job();
			return job;
		}
	}
}

