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