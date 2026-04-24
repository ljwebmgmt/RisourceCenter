SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID('[dbo].[TrainingContents]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[TrainingContents] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [Title] NVARCHAR(200) NOT NULL,
            [Description] NVARCHAR(MAX) NULL,
            [PdfPath] NVARCHAR(500) NULL,
            [VideoPath] NVARCHAR(500) NULL,
            [PassingPercentage] INT NULL,
            CONSTRAINT [PK_TrainingContents] PRIMARY KEY CLUSTERED ([Id] ASC)
        );
    END;

    IF OBJECT_ID('[dbo].[QuizQuestions]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[QuizQuestions] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [TrainingContentId] INT NULL,
            [QuestionText] NVARCHAR(MAX) NOT NULL,
            CONSTRAINT [PK_QuizQuestions] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[QuizQuestions] WITH CHECK
        ADD CONSTRAINT [FK__QuizQuest__Train__763775D2]
            FOREIGN KEY ([TrainingContentId])
            REFERENCES [dbo].[TrainingContents] ([Id]);

        ALTER TABLE [dbo].[QuizQuestions] CHECK CONSTRAINT [FK__QuizQuest__Train__763775D2];

        CREATE NONCLUSTERED INDEX [IX_QuizQuestions_TrainingContentId]
            ON [dbo].[QuizQuestions]([TrainingContentId]);
    END;

    IF OBJECT_ID('[dbo].[QuizOptions]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[QuizOptions] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [QuestionId] INT NULL,
            [OptionText] NVARCHAR(500) NOT NULL,
            [IsCorrect] BIT NOT NULL,
            CONSTRAINT [PK_QuizOptions] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[QuizOptions] WITH CHECK
        ADD CONSTRAINT [FK__QuizOptio__Quest__7913E27D]
            FOREIGN KEY ([QuestionId])
            REFERENCES [dbo].[QuizQuestions] ([Id]);

        ALTER TABLE [dbo].[QuizOptions] CHECK CONSTRAINT [FK__QuizOptio__Quest__7913E27D];

        CREATE NONCLUSTERED INDEX [IX_QuizOptions_QuestionId]
            ON [dbo].[QuizOptions]([QuestionId]);
    END;

    IF OBJECT_ID('[dbo].[UserProgress]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[UserProgress] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [UserId] INT NOT NULL,
            [TrainingContentId] INT NULL,
            [StartTime] DATETIME NOT NULL,
            [EndTime] DATETIME NULL,
            [ScorePercentage] INT NULL,
            [IsPassed] BIT NULL,
            CONSTRAINT [PK_UserProgress] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[UserProgress] WITH CHECK
        ADD CONSTRAINT [FK__UserProgr__Train__7BF04F28]
            FOREIGN KEY ([TrainingContentId])
            REFERENCES [dbo].[TrainingContents] ([Id]);

        ALTER TABLE [dbo].[UserProgress] CHECK CONSTRAINT [FK__UserProgr__Train__7BF04F28];

        CREATE NONCLUSTERED INDEX [IX_UserProgress_UserId]
            ON [dbo].[UserProgress]([UserId]);

        CREATE NONCLUSTERED INDEX [IX_UserProgress_TrainingContentId]
            ON [dbo].[UserProgress]([TrainingContentId]);
    END;

    IF OBJECT_ID('[dbo].[TrainingQuizAttempt]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[TrainingQuizAttempt] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [TrainingContentId] INT NOT NULL,
            [UserId] INT NOT NULL,
            [UserProgressId] INT NULL,
            [StartedAt] DATETIME NOT NULL,
            [SubmittedAt] DATETIME NOT NULL,
            [TotalQuestions] INT NOT NULL,
            [CorrectQuestions] INT NOT NULL,
            [ScorePercentage] INT NOT NULL,
            [PassingPercentage] INT NOT NULL,
            [IsPassed] BIT NOT NULL,
            CONSTRAINT [PK_TrainingQuizAttempt] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[TrainingQuizAttempt] WITH CHECK
        ADD CONSTRAINT [FK_TrainingQuizAttempt_TrainingContent]
            FOREIGN KEY ([TrainingContentId])
            REFERENCES [dbo].[TrainingContents] ([Id]);

        ALTER TABLE [dbo].[TrainingQuizAttempt] CHECK CONSTRAINT [FK_TrainingQuizAttempt_TrainingContent];

        ALTER TABLE [dbo].[TrainingQuizAttempt] WITH CHECK
        ADD CONSTRAINT [FK_TrainingQuizAttempt_UserProgress]
            FOREIGN KEY ([UserProgressId])
            REFERENCES [dbo].[UserProgress] ([Id]);

        ALTER TABLE [dbo].[TrainingQuizAttempt] CHECK CONSTRAINT [FK_TrainingQuizAttempt_UserProgress];

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttempt_TrainingContentId]
            ON [dbo].[TrainingQuizAttempt]([TrainingContentId]);

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttempt_UserId]
            ON [dbo].[TrainingQuizAttempt]([UserId]);

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttempt_UserProgressId]
            ON [dbo].[TrainingQuizAttempt]([UserProgressId]);
    END;

    IF OBJECT_ID('[dbo].[TrainingQuizAttemptAnswer]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[TrainingQuizAttemptAnswer] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [AttemptId] INT NOT NULL,
            [QuestionId] INT NOT NULL,
            [OptionId] INT NOT NULL,
            [IsCorrect] BIT NOT NULL,
            CONSTRAINT [PK_TrainingQuizAttemptAnswer] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] WITH CHECK
        ADD CONSTRAINT [FK_TrainingQuizAttemptAnswer_Attempt]
            FOREIGN KEY ([AttemptId])
            REFERENCES [dbo].[TrainingQuizAttempt] ([Id]);

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] CHECK CONSTRAINT [FK_TrainingQuizAttemptAnswer_Attempt];

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] WITH CHECK
        ADD CONSTRAINT [FK_TrainingQuizAttemptAnswer_Question]
            FOREIGN KEY ([QuestionId])
            REFERENCES [dbo].[QuizQuestions] ([Id]);

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] CHECK CONSTRAINT [FK_TrainingQuizAttemptAnswer_Question];

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] WITH CHECK
        ADD CONSTRAINT [FK_TrainingQuizAttemptAnswer_Option]
            FOREIGN KEY ([OptionId])
            REFERENCES [dbo].[QuizOptions] ([Id]);

        ALTER TABLE [dbo].[TrainingQuizAttemptAnswer] CHECK CONSTRAINT [FK_TrainingQuizAttemptAnswer_Option];

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttemptAnswer_AttemptId]
            ON [dbo].[TrainingQuizAttemptAnswer]([AttemptId]);

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttemptAnswer_QuestionId]
            ON [dbo].[TrainingQuizAttemptAnswer]([QuestionId]);

        CREATE NONCLUSTERED INDEX [IX_TrainingQuizAttemptAnswer_OptionId]
            ON [dbo].[TrainingQuizAttemptAnswer]([OptionId]);
    END;

    IF OBJECT_ID('[dbo].[TrainingRoleAssignment]', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[TrainingRoleAssignment] (
            [Id] INT IDENTITY(1,1) NOT NULL,
            [TrainingContentId] INT NOT NULL,
            [RoleId] NVARCHAR(128) NOT NULL,
            [CreatedAt] DATETIME NOT NULL,
            CONSTRAINT [PK_TrainingRoleAssignment] PRIMARY KEY CLUSTERED ([Id] ASC)
        );

        ALTER TABLE [dbo].[TrainingRoleAssignment] WITH CHECK
        ADD CONSTRAINT [FK_TrainingRoleAssignment_TrainingContent]
            FOREIGN KEY ([TrainingContentId])
            REFERENCES [dbo].[TrainingContents] ([Id]);

        ALTER TABLE [dbo].[TrainingRoleAssignment] CHECK CONSTRAINT [FK_TrainingRoleAssignment_TrainingContent];

        ALTER TABLE [dbo].[TrainingRoleAssignment] WITH CHECK
        ADD CONSTRAINT [FK_TrainingRoleAssignment_AspNetRoles]
            FOREIGN KEY ([RoleId])
            REFERENCES [dbo].[AspNetRoles] ([Id]);

        ALTER TABLE [dbo].[TrainingRoleAssignment] CHECK CONSTRAINT [FK_TrainingRoleAssignment_AspNetRoles];

        CREATE NONCLUSTERED INDEX [IX_TrainingRoleAssignment_TrainingContentId]
            ON [dbo].[TrainingRoleAssignment]([TrainingContentId]);

        CREATE NONCLUSTERED INDEX [IX_TrainingRoleAssignment_RoleId]
            ON [dbo].[TrainingRoleAssignment]([RoleId]);

        CREATE UNIQUE NONCLUSTERED INDEX [UX_TrainingRoleAssignment_TrainingContentId_RoleId]
            ON [dbo].[TrainingRoleAssignment]([TrainingContentId], [RoleId]);
    END;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrState INT = ERROR_STATE();

    RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
END CATCH;
