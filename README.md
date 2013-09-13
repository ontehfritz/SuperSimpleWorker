# Delayed Job.net v0.1.9
### Simple light weight background job runner

Cross platform Status:

Mono 3.2.1
[![Build Status](https://travis-ci.org/fritzcoder/delayed_job.net.png?branch=master)](https://travis-ci.org/fritzcoder/delayed_job.net)

This is ported and inspired from the original project:
https://github.com/tobi/delayed_job

Currently being developed using mono and OSX. The goal of the project is to have it work equally across platforms.

delayed_job.net (or DJ.NET) encapsulates the common pattern of asynchronously executing longer tasks in the background. This is useful for off loading tasks from ASP.NET/MVC or Nancy (http://nancyfx.org) projects. It can also be used with any .Net technologies. 

It is ready to be used with Mono or Microsoft .Net framework; As well as cross platform Windows, OSX, and Linux. 

Like the ruby version of delayed_job some examples of use are: 
 
* sending massive newsletters
* image resizing
* http downloads
* batch imports 
* spam checks

If you would like more in-depth information on use and FAQ please see the wiki: 
https://github.com/fritzcoder/delayed_job.net/wiki

There are three major components to Delayed_job.net:

1. The DelayedJob.dll assembly which gives your program access to creating jobs for scheduling

2. Repository[databasename].dll assemblies, this gives DJ.Net and worker.exe access to an array of databases.

3. worker.exe, this runs the jobs scheduled by your program. It can be run in the background. 

Some important notes:
* worker.exe must currently be run on the same system as the code that is scheduling jobs. This is so it can resolve and deserialize the job objects.

* classes and/or there instantiated objects must by serialisable.

## Setup

1. Copy the DelayedJob.dll in your project bin folder.

2. Using MonoDevelop or Visual Studio reference the DelayedJob.dll

3. Copy a Repository[databasename].dll assembly in your bin folder and reference it with visual studio or MonoDevelop.

4. Create the delayed_job table in one of the supported databases. The sql table script can be found in the sql folder.

5. Start creating jobs use The IJob interface and create your class:

```
public class EmailJob : DelayedJob.IJob
{
	//Make sure information you want to persist in the database is public
	//There is no way to serialize and deserialize private data.
	public string fromAddress = "email@gmail.com";
	public string toAddress = "toemail@gmail.com";
	public const string fromPassword = "";
	public const string subject = "DelayJob.net test";
	public const string body = "A automated test";
	
	public void perform(){
		var smtp = new SmtpClient
		{
			Host = "smtp.gmail.com",
			Port = 587,
			EnableSsl = true,
			DeliveryMethod = SmtpDeliveryMethod.Network,
			UseDefaultCredentials = false,
			Credentials = new NetworkCredential(fromAddress, fromPassword)
		};
		using (var message = new MailMessage(fromAddress, toAddress)
		       {
			Subject = subject,
			Body = body
		})
		{

			ServicePointManager.ServerCertificateValidationCallback = 
				delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) 
			{ return true; };
			smtp.Send(message);
		}
	}
}

```

5. Now schedule the job:

```
//Set the Repository with the database you would like to use with a
//connection string. 
Job.Repository = new RepositoryMonoSQLite("URI=file:delayed_job.db");
//Enqueue the Job now. When the worker process runs it will execute the 
//perform method. In this case it will send an email.
Job.Enqueue(new EmailJob());
```

6. The worker process needs to be configured. The worker.exe has a file called
worker.exe.config. Configure the database you want to use and the connection string. 
It Should be the same database you are enqueueing your jobs in. You can now 
run worker process by:

```
mono worker.exe [optional name] or mono worker.exe [optional name] &
```

or on windows simply:

```
worker.exe [optional name]
```
Also on windows you can install it as a service:

```
InstallUtil.exe worker.exe 
```

Please see the wiki for more detailed information. 

The library evolves around a delayed_jobs table

```
  In the sql directory there is {db server type}.sql script. Run the script in         the database you wish to use. 
  Currently supporting: 
	sqlite3 - Linux and OSX only (no windows support)
	MySql
	PostgreSQL
	Microsoft SQL Server (Tested on version 2012)
```

The create table script looks as follows:
* May differ slightly between database types *

```
  CREATE TABLE delayed_jobs(
  	id integer not null primary key,  
    assembly varchar(8000), 
	type varchar(255), 
	priority integer default 0,
	attempts integer default 0, 
	handler varchar(255),
	last_error varchar(255),
	run_at datetime default null,
	locked_at datetime default null,
	failed_at datetime default null,
	locked_by varchar(255), 
	created_at timestamp default current_timestamp, 
	modified_at timestamp default current_timestamp
  )
```

On failure, the job is scheduled again in 5 seconds + N ** 4, where N is the number of retries.

By default, it will delete failed jobs

Please report questions, feature requests and bugs to: 
https://github.com/fritzcoder/delayed_job.net/issues
