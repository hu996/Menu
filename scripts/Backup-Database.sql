-- Execute with sqlcmd variables supplied by the database operator.
-- Example: sqlcmd -S <server> -E -v DatabaseName="RestaurantMenuPlatform" BackupFile="D:\\backups\\RestaurantMenuPlatform.bak" -i Backup-Database.sql
BACKUP DATABASE [$(DatabaseName)]
TO DISK = N'$(BackupFile)'
WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10;
RESTORE VERIFYONLY FROM DISK = N'$(BackupFile)' WITH CHECKSUM;
