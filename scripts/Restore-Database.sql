-- Execute only against an isolated restore target after obtaining change approval.
-- Supply the logical data/log names returned by RESTORE FILELISTONLY.
RESTORE DATABASE [$(RestoreDatabaseName)]
FROM DISK = N'$(BackupFile)'
WITH MOVE N'$(LogicalDataName)' TO N'$(RestoreDataFile)',
     MOVE N'$(LogicalLogName)' TO N'$(RestoreLogFile)',
     RECOVERY, REPLACE, CHECKSUM, STATS = 10;
DBCC CHECKDB ([$(RestoreDatabaseName)]) WITH NO_INFOMSGS;
