CREATE TABLE [dbo].[delayed_jobs](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[assembly] [varchar](8000) NULL,
	[type] [varchar](255) NULL,
	[priority] [int] NULL,
	[attempts] [int] NULL,
	[handler] [varchar](255) NULL,
	[last_error] [varchar](255) NULL,
	[run_at] [datetime] NULL,
	[locked_at] [datetime] NULL,
	[failed_at] [datetime] NULL,
	[locked_by] [varchar](255) NULL,
	[created_at] [datetime] NULL,
	[modified_at] [datetime] NULL,
 CONSTRAINT [PK__delayed___3213E83F87A1EB57] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]