CREATE UNIQUE NONCLUSTERED INDEX [IX_words] ON [dbo].[words] ([n]) ON [PRIMARY]
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
