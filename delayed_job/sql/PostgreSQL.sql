CREATE TABLE delayed_jobs(
	id serial not null primary key, 
	assembly varchar(8000), 
	type varchar(255), 
	priority integer default 0, 
	attempts integer default 0,  
	handler varchar(255), 
	last_error varchar(255), 
	run_at timestamp default null, 
	locked_at timestamp default null, 
	failed_at timestamp default null,
	locked_by varchar(255), 
	created_at timestamp default current_timestamp, 
	modified_at timestamp default null 
);