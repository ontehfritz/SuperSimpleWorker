# Delayed Job.net 

Delayed_job.net (or DJ.NET) encapsulates the common pattern of asynchronously executing longer tasks in the background.
This is ported and inspired from the original project:
https://github.com/tobi/delayed_job

I needed to have a simple job scheduler (cross platform) in my ASP.NET MVC or Nancy (http://nancyfx.org) projects.
I have seen time after time .Net developers having long running processes in their .net web apps. Main reason is there 
is no simple way of doing this and no solution that works cross platform. 

This can be used with Mono or Microsoft .Net framework. As well as cross platform Windows, OSX, and Linux. It can be 
used with other .NET projects as it is stand alone. 

I worked from the original repo as the code is more straight forward and due to difference between ruby and c# it 
allowed me to focus on functionality rather than keeping the code the same. Although I structured it as close as I 
could.

If you would like more indepth information on use and FAQ please see the wiki: 

https://github.com/fritzcoder/delayed_job.net/wiki

There are two major components to Delayed_job.net 
1. The delayed_job assembly which gives your program access to creating jobs for scheduling
2. worker.exe, this runs the jobs scheduled by your program. It can be run in the background.
It must also currently be run on the system as the program or webserver as it 
needs access to the assemblies where the jobs where created. This will hopefully be solved in the future.

Like the ruby version of delayed_job some examples of use are: 
 
* sending massive newsletters
* image resizing
* http downloads
* batch imports 
* spam checks 

## Setup

The library evolves around a delayed_jobs table which can be created by using:
```
  script .sql in the database you wish to use. 
```

The created table looks as follows: 

```
  CREATE TABLE delay_jobs(
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
