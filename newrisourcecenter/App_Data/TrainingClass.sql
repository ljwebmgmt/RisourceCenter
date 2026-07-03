ALTER TABLE dbo.TrainingContents
ADD TrainingClass NVARCHAR(50) NULL;
GO

UPDATE dbo.TrainingContents
SET TrainingClass = 'basic'
WHERE TrainingClass IS NULL OR LTRIM(RTRIM(TrainingClass)) = '';
GO

ALTER TABLE dbo.TrainingContents
ADD CONSTRAINT DF_TrainingContents_TrainingClass
DEFAULT ('basic') FOR TrainingClass;
GO

ALTER TABLE dbo.UserProgress
ADD PdfOpenedAt  DATETIME NULL, VideoOpenedAt DATETIME NULL;
GO

CREATE TABLE dbo.UserTrainingTrackCompletions
(
    UserId        INT           NOT NULL,
    TrainingClass NVARCHAR(50)  NOT NULL,
    CompletedAt   DATETIME      NULL,
    EmailedAt     DATETIME      NULL,

    CONSTRAINT PK_UserTrainingTrackCompletions
        PRIMARY KEY (UserId, TrainingClass)
);
GO

ALTER TABLE dbo.TrainingContents
DROP CONSTRAINT DF_TrainingContents_TrainingClass;
GO

ALTER TABLE dbo.trainingTracks
ADD sort_order SMALLINT NOT NULL CONSTRAINT DF_trainingTracks_sort_order DEFAULT (0);
GO

ALTER TABLE dbo.trainingTracks
ADD image_url VARCHAR(500) NULL;
GO

ALTER TABLE dbo.TrainingContents
ADD sort_order SMALLINT NOT NULL CONSTRAINT DF_TrainingContents_sort_order DEFAULT (0);
GO