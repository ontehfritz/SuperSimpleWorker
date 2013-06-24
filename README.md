# Delayed Job.net 

Delayed_job.net (or DJ.NET) encapsulates the common pattern of asynchronously executing longer tasks in the background.

This is ported and inspired from the original project:
https://github.com/tobi/delayed_job

I needed to have a simple job scheduler (cross platform) in my ASP.NET/MVC or Nancy (http://nancyfx.org) projects. There is no simple way of doing this without something heavy like MSMQ and no solution that works cross platform. Ruby and RoR have had many available solutions. 

This can be used with Mono or Microsoft .Net framework. As well as cross platform Windows, OSX, and Linux. It can be used with other .NET projects as it is stand alone. 

I worked from the original repo as the code is more straight forward and due to difference between ruby and c# it 
allowed me to focus on functionality rather than keeping the code the same. Although I structured it as close as I 
could.

If you would like more in-depth information on use and FAQ please see the wiki: 

https://github.com/fritzcoder/delayed_job.net/wiki

There are two major components to Delayed_job.net 
1. The delayed_job assembly which gives your program access to creating jobs for scheduling
2. worker.exe, this runs the jobs scheduled by your program. It can be run in the background. This process had to be separate as there is no rake interface with .net, where as ruby the worker process is run through rake. 

Some important notes:
* worker.exe must currently be run on the same system as the code that is scheduling jobs. Unlike ruby, the serialisation process is different. 
C# cannot execute code stored in the database. It must be compiled into byte-code. Instead objects are serialised into xml in the database.
Then the objects are deserialized using the dll file the job objects are defined. 
* a potential issue is when using this with mono and ASP.NET or ASP.NET MVC; when the webserver runs the web application it stores the assembly (dll) in a temporary directory. This temp directory is where delay_job.net will store the path of the assembly to instantiate the objects when deserializing. Currently in testing this has not been a problem even during reboots of system or webserver. However, it may be possible for this directory to change making the jobs fail.  
* classes and/or there instantiated objects must by serialisable, this also means that any class members are not serialisable, you must instantiate them in the perform method or a default constructor. To be serialisable you must have a default constructor and all values you want to be assigned on deserialization must be public members or public properties of a class. 


Like the ruby version of delayed_job some examples of use are: 
 
* sending massive newsletters
* image resizing
* http downloads
* batch imports 
* spam checks 

## Setup

The library evolves around a delayed_jobs table which can be created by using:

```
  In the sql directory there is {db server type}.sql script. Run the script in    the database you wish to use. 
  Currently supporting: 
	sqlite3
```

The create table script looks as follows:
*May differ slightly between database types 

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
