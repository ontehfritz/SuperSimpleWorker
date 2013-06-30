CREATE TABLE delayed_jobs(
	id integer not null primary key AUTO_INCREMENT, 
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
	modified_at timestamp default null 
);