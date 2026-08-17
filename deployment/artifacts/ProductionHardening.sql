BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [Users]
        WHERE DATALENGTH([Email]) > 640
           OR DATALENGTH([NormalizedEmail]) > 640
           OR DATALENGTH([DisplayName]) > 240
           OR DATALENGTH([PasswordHash]) > 1024
           OR DATALENGTH([SecurityStamp]) > 128)
        THROW 51000, 'Production hardening cannot shorten one or more Users values. Clean the oversized data and retry the migration.', 1;

    IF EXISTS (
        SELECT 1
        FROM [PaymentTransactions]
        WHERE DATALENGTH([Provider]) > 128
           OR DATALENGTH([ProviderReference]) > 400
           OR DATALENGTH([Status]) > 64)
        THROW 51001, 'Production hardening cannot shorten one or more PaymentTransactions values. Clean the oversized data and retry the migration.', 1;

    IF EXISTS (SELECT 1 FROM [Orders] WHERE DATALENGTH([Status]) > 64)
        THROW 51002, 'Production hardening cannot shorten one or more Orders status values. Clean the oversized data and retry the migration.', 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE TABLE [DistributedCache] (
        [Id] nvarchar(449) NOT NULL,
        [Value] varbinary(max) NOT NULL,
        [ExpiresAtTime] datetimeoffset NOT NULL,
        [SlidingExpirationInSeconds] bigint NULL,
        [AbsoluteExpiration] datetimeoffset NULL,
        CONSTRAINT [PK_DistributedCache] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_DistributedCache_ExpiresAtTime] ON [DistributedCache] ([ExpiresAtTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_Orders_TenantId_BranchId] ON [Orders];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_MenuItems_TenantId_MenuCategoryId] ON [MenuItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_MenuCategories_TenantId_MenuId] ON [MenuCategories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_BranchMenus_TenantId_MenuId] ON [BranchMenus];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'SecurityStamp');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Users] ALTER COLUMN [SecurityStamp] nvarchar(64) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(512) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_Users_NormalizedEmail] ON [Users];
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'NormalizedEmail');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Users] ALTER COLUMN [NormalizedEmail] nvarchar(320) NOT NULL;
    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(320) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'DisplayName');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Users] ALTER COLUMN [DisplayName] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Status');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions];
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'ProviderReference');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [ProviderReference] nvarchar(200) NOT NULL;
    CREATE UNIQUE INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions] ([Provider], [ProviderReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions];
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Provider');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [Provider] nvarchar(64) NOT NULL;
    CREATE UNIQUE INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions] ([Provider], [ProviderReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [CheckoutUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Orders] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    ALTER TABLE [Orders] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_TenantId_Status_CreatedAtUtc] ON [PaymentTransactions] ([TenantId], [Status], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_UserId_UsedAtUtc_ExpiresAtUtc] ON [PasswordResetTokens] ([UserId], [UsedAtUtc], [ExpiresAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_Orders_TenantId_BranchId_Status_CreatedAtUtc] ON [Orders] ([TenantId], [BranchId], [Status], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_MenuItems_TenantId_MenuCategoryId_IsAvailable_SortOrder] ON [MenuItems] ([TenantId], [MenuCategoryId], [IsAvailable], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_MenuCategories_TenantId_MenuId_IsActive_SortOrder] ON [MenuCategories] ([TenantId], [MenuId], [IsActive], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    CREATE INDEX [IX_BranchMenus_TenantId_MenuId_IsActive] ON [BranchMenus] ([TenantId], [MenuId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814233447_ProductionHardening', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    IF EXISTS (
        SELECT 1 FROM [Tenants]
        WHERE DATALENGTH([Slug]) > 240 OR DATALENGTH([Phone]) > 80
           OR DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320
           OR DATALENGTH([LogoUrl]) > 2000 OR DATALENGTH([Email]) > 640
           OR DATALENGTH([DefaultLanguage]) > 20 OR DATALENGTH([Currency]) > 16
           OR DATALENGTH([CoverImageUrl]) > 2000 OR DATALENGTH([Address]) > 1000)
        THROW 51010, 'Operational string constraints found oversized Tenants data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [Subscriptions]
        WHERE DATALENGTH([Status]) > 64 OR DATALENGTH([PaymentProvider]) > 128
           OR DATALENGTH([ExternalSubscriptionId]) > 400)
        THROW 51011, 'Operational string constraints found oversized Subscriptions data. Clean the values and retry the migration.', 1;

    IF EXISTS (SELECT 1 FROM [PaymentTransactions] WHERE DATALENGTH([Currency]) > 16)
        THROW 51012, 'Operational string constraints found oversized PaymentTransactions currency data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [OrderStatusHistories]
        WHERE DATALENGTH([ToStatus]) > 64 OR DATALENGTH([FromStatus]) > 64)
        THROW 51013, 'Operational string constraints found oversized order status history data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [Menus]
        WHERE DATALENGTH([Status]) > 64 OR DATALENGTH([Slug]) > 240
           OR DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320)
        THROW 51014, 'Operational string constraints found oversized Menus data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [MenuItems]
        WHERE DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320
           OR DATALENGTH([DescriptionEn]) > 4000 OR DATALENGTH([DescriptionAr]) > 4000
           OR DATALENGTH([Description]) > 4000 OR DATALENGTH([Currency]) > 16)
        THROW 51015, 'Operational string constraints found oversized MenuItems data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [MenuCategories]
        WHERE DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320
           OR DATALENGTH([DescriptionAr]) > 2000 OR DATALENGTH([Description]) > 2000)
        THROW 51016, 'Operational string constraints found oversized MenuCategories data. Clean the values and retry the migration.', 1;

    IF EXISTS (SELECT 1 FROM [Memberships] WHERE DATALENGTH([Role]) > 64)
        THROW 51017, 'Operational string constraints found oversized Memberships role data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [BranchSpecificMenuItems]
        WHERE DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320
           OR DATALENGTH([DescriptionEn]) > 4000 OR DATALENGTH([DescriptionAr]) > 4000
           OR DATALENGTH([Description]) > 4000 OR DATALENGTH([Currency]) > 16)
        THROW 51018, 'Operational string constraints found oversized branch-specific menu item data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [Branches]
        WHERE DATALENGTH([Slug]) > 240 OR DATALENGTH([Phone]) > 80 OR DATALENGTH([OpeningHours]) > 2000
           OR DATALENGTH([NameEn]) > 320 OR DATALENGTH([NameAr]) > 320 OR DATALENGTH([Name]) > 320
           OR DATALENGTH([Address]) > 1000)
        THROW 51019, 'Operational string constraints found oversized Branches data. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [AuditLogs]
        WHERE DATALENGTH([EntityType]) > 240 OR DATALENGTH([ActorDisplayName]) > 320 OR DATALENGTH([Action]) > 240)
        THROW 51020, 'Operational string constraints found oversized AuditLogs metadata. Clean the values and retry the migration.', 1;

    IF EXISTS (
        SELECT 1 FROM [AnalyticsEvents]
        WHERE DATALENGTH([EventType]) > 64 OR DATALENGTH([Device]) > 1024)
        THROW 51021, 'Operational string constraints found oversized AnalyticsEvents data. Clean the values and retry the migration.', 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Tenants_Slug] ON [Tenants];
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Slug');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Tenants_Slug] ON [Tenants] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Phone');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Phone] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'NameEn');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'NameAr');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Name');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'LogoUrl');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [LogoUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Email');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Email] nvarchar(320) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'DefaultLanguage');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [DefaultLanguage] nvarchar(10) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Currency');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Currency] nvarchar(8) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'CoverImageUrl');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [CoverImageUrl] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Address');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Address] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'Status');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'PaymentProvider');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [PaymentProvider] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'ExternalSubscriptionId');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [ExternalSubscriptionId] nvarchar(200) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Currency');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderStatusHistories]') AND [c].[name] = N'ToStatus');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [OrderStatusHistories] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [OrderStatusHistories] ALTER COLUMN [ToStatus] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderStatusHistories]') AND [c].[name] = N'FromStatus');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [OrderStatusHistories] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [OrderStatusHistories] ALTER COLUMN [FromStatus] nvarchar(32) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Status');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Menus_TenantId_Slug] ON [Menus];
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Slug');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Menus_TenantId_Slug] ON [Menus] ([TenantId], [Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'NameEn');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'NameAr');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Name');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'NameEn');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'NameAr');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Name');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'DescriptionEn');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [DescriptionEn] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'DescriptionAr');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [DescriptionAr] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Description');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Description] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Currency');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var38 sysname;
    SELECT @var38 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'NameEn');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var39 sysname;
    SELECT @var39 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'NameAr');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var40 sysname;
    SELECT @var40 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'Name');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var41 sysname;
    SELECT @var41 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'DescriptionAr');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [DescriptionAr] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'Description');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [Description] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var43 sysname;
    SELECT @var43 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Memberships]') AND [c].[name] = N'Role');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Memberships] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [Memberships] ALTER COLUMN [Role] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var44 sysname;
    SELECT @var44 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'NameEn');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var45 sysname;
    SELECT @var45 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'NameAr');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var46 sysname;
    SELECT @var46 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Name');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var47 sysname;
    SELECT @var47 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'DescriptionEn');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [DescriptionEn] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var48 sysname;
    SELECT @var48 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'DescriptionAr');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [DescriptionAr] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var49 sysname;
    SELECT @var49 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Description');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Description] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var50 sysname;
    SELECT @var50 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Currency');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Branches_TenantId_Slug] ON [Branches];
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Slug');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Branches_TenantId_Slug] ON [Branches] ([TenantId], [Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var52 sysname;
    SELECT @var52 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Phone');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Phone] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var53 sysname;
    SELECT @var53 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'OpeningHours');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [OpeningHours] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var54 sysname;
    SELECT @var54 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'NameEn');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var54 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var55 sysname;
    SELECT @var55 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'NameAr');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Branches_TenantId_Name] ON [Branches];
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Name');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var56 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE INDEX [IX_Branches_TenantId_Name] ON [Branches] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var57 sysname;
    SELECT @var57 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Address');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var57 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Address] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var58 sysname;
    SELECT @var58 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'EntityType');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var58 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [EntityType] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var59 sysname;
    SELECT @var59 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'ActorDisplayName');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var59 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [ActorDisplayName] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var60 sysname;
    SELECT @var60 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'Action');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var60 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [Action] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_AnalyticsEvents_TenantId_EventType_CreatedAtUtc] ON [AnalyticsEvents];
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnalyticsEvents]') AND [c].[name] = N'EventType');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [AnalyticsEvents] DROP CONSTRAINT [' + @var61 + '];');
    ALTER TABLE [AnalyticsEvents] ALTER COLUMN [EventType] nvarchar(32) NOT NULL;
    CREATE INDEX [IX_AnalyticsEvents_TenantId_EventType_CreatedAtUtc] ON [AnalyticsEvents] ([TenantId], [EventType], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var62 sysname;
    SELECT @var62 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnalyticsEvents]') AND [c].[name] = N'Device');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [AnalyticsEvents] DROP CONSTRAINT [' + @var62 + '];');
    ALTER TABLE [AnalyticsEvents] ALTER COLUMN [Device] nvarchar(512) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814235803_ConstrainOperationalStrings', N'8.0.19');
END;
GO

COMMIT;
GO

