ALTER TABLE [dbo].[EmailResponses] ADD CONSTRAINT [PK_EmailResponses] PRIMARY KEY CLUSTERED  ([Id]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
