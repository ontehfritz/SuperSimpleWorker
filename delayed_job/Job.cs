using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Collections.Generic;

namespace delayed_job
{
	public class Job
	{

		/*database fields*/
		public int id;
		public int priority;  //Allows some jobs to jump to the front of the queue
		public int attempts;  //Provides for retries, but still fail eventually.
		public string handler; //xml string of the object that will do work
		public string last_error; //reason for last failure (See Note below)
		public DateTime? run_at; //When to run. Could be Time.now for immediately, or sometime in the future.
		public DateTime? locked_at; //Set when a client is working on this object
		public DateTime? failed_at; //Set when all retries have failed (actually, by default, the record is deleted instead)
		public string locked_by; //Who is working on this object (if locked)
		public string type;
		public string assembly;

		/*Object attributes */ 
		const int MAX_ATTEMPTS = 25;
		const int MAX_RUN_TIME = 4; //hours
		public bool destroyFailedJobs = true;
		public string workerName; 

		public int minPriority;
		public int maxPriority;

		string set_table_name = "delayed_jobs";

		public Job ()
		{
			workerName = Guid.NewGuid().ToString();
		}


		private void Reschedule(string message, DateTime? time = null)
		{
			RepositorySQLite sqlite = new RepositorySQLite();

			if(attempts < MAX_ATTEMPTS)
			{
				time = (time == null ? DateTime.Now.AddSeconds(attempts ^ 4 + 5) : time );
				this.attempts += 1;
				this.run_at = time;
				this.last_error = message;
				this.unlock(); 
				sqlite.UpdateJob(this);
			}
			else
			{
				//if(destroyFailedJobs)
				//{
					//sqlite.Remove(this.id);
				//}
				//else
				//{
					this.failed_at = DateTime.Now;
					sqlite.UpdateJob(this);
				//}
			}
		}

		public void unlock()
		{
			locked_at = null;
			locked_by = null;
		}
//		public static T InstantiateType<T>(Type type)
//		{
//			if (type == null)
//			{
//				throw new ArgumentNullException("type", "Cannot instantiate null");
//			}
//			ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
//			if (ci == null)
//			{
//				throw new ArgumentException("Cannot instantiate type which has no empty constructor", type.Name);
//			}
//			return (T) ci.Invoke(new object[0]);
//		}

		public static bool LockExclusively(int max_run_time, 
		                                   string worker_name)
		{
			return true;
		}

		public void ClearLocks()
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			sqlite.ClearJobs(workerName);
		}

		public static Job[] FindAvailable(int limit = 5, int max_run_time = MAX_RUN_TIME)
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			return sqlite.GetNextReadyJobs(limit);
		}

		public bool? ReserveAndRunOneJob(int max_run_time = MAX_RUN_TIME)
		{
			Job [] jobs = Job.FindAvailable();
			bool t = false;
			foreach(Job job in jobs)
			{
				t = job.RunWithLock(4, workerName);
				return t;
			}

			return null;
		}

		public struct Report
		{
			public int success; 
			public int failure;
		}

		public Report WorkOff(int num = 100)
		{
			Report report = new Report();
			for(int i = 0; i < num; i++)
			{
				bool? work = this.ReserveAndRunOneJob ();

				if(work == true){
					report.success++;
				}
				else if(work == false){
					report.failure++;
				}
				else{
					break;
				}
			}

			return report;
		}
		/// <summary>
		/// Runs the with lock.
		/// </summary>
		/// <returns><c>true</c>, if with lock was run, <c>false</c> otherwise.</returns>
		/// <param name="max_run_time">Max_run_time.</param>
		/// <param name="workerName">Worker name.</param>
		public bool RunWithLock(int max_run_time, string workerName)
		{
			RepositorySQLite sqlite = new RepositorySQLite();
			     

			this.locked_by = workerName;
			this.locked_at = DateTime.Now;
			sqlite.UpdateJob(this);
		
			//@"/Users/Fritz/Documents/Projects/delayed_job/delay_job_test/bin/Debug/delay_job_test.dll"			
			Type types = Assembly.LoadFrom(this.assembly).GetType(this.type, true);
			//ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);

			XmlSerializer serializer = new XmlSerializer(types);

			IJob job = (IJob)serializer.Deserialize(new StringReader(this.handler));
			//IJob job = (IJob)ci.Invoke(new object[0]);
			try
			{
				job.perform();
			}
			catch(Exception e)
			{
				this.Reschedule (e.Message);
				//throw e;
				return false;
			}

			return true;
		}

		private static string SerializeToXml(IJob job)
		{
			StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
			XmlSerializer serializer = new XmlSerializer(job.GetType());
			serializer.Serialize(writer, job);
			return writer.ToString();
		}

		private static string ParseType(Type type)
		{
			if (type.AssemblyQualifiedName == null)
				throw new ArgumentException("Assembly Qualified Name is null");
			
			//int idx = type.AssemblyQualifiedName.IndexOf(',', 
			//                                             type.AssemblyQualifiedName.IndexOf(',') + 1);
			int idx = type.AssemblyQualifiedName.IndexOf(',');
			
			string retValue = type.AssemblyQualifiedName.Substring(0, idx);
			
			return retValue;
		}

		public static void Enqueue(IJob job, int priority = 0, DateTime? run_at = null)
		{
			Job newJob = new Job();
			newJob.priority = priority;
			newJob.type = ParseType(job.GetType());
			newJob.assembly =  System.Reflection.Assembly.GetAssembly(job.GetType()).Location;
			newJob.handler = SerializeToXml(job);
			newJob.run_at = (run_at == null ? DateTime.Now : (DateTime)run_at);
			newJob.failed_at = null;
			newJob.locked_at = null;

			RepositorySQLite sqlite = new RepositorySQLite();
			sqlite.CreateJob(newJob/*, job*/);
		}

		public DateTime? Failed()
		{
			return this.failed_at;
		}
	}
}

