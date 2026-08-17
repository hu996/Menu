IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Ingredients] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [IsAllergen] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Ingredients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Plans] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [MonthlyPrice] decimal(18,2) NOT NULL,
        [MaxBranches] int NOT NULL,
        [MaxMenuItems] int NOT NULL,
        [MaxUsers] int NOT NULL,
        [AdvancedAnalytics] bit NOT NULL,
        [CustomBranding] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Plans] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Tenants] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Slug] nvarchar(450) NOT NULL,
        [LogoUrl] nvarchar(max) NULL,
        [DefaultLanguage] nvarchar(max) NOT NULL,
        [SubscriptionStatus] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Subscriptions] (
        [Id] uniqueidentifier NOT NULL,
        [PlanId] uniqueidentifier NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [StartsAtUtc] datetime2 NOT NULL,
        [EndsAtUtc] datetime2 NULL,
        [PaymentProvider] nvarchar(max) NULL,
        [ExternalSubscriptionId] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Subscriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Subscriptions_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Branches] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Address] nvarchar(max) NULL,
        [Phone] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Branches_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [Menus] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Slug] nvarchar(450) NOT NULL,
        [IsGlobal] bit NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Menus] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Menus_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [QrCodes] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [Code] nvarchar(450) NOT NULL,
        [TargetType] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_QrCodes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QrCodes_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [BranchMenus] (
        [BranchId] uniqueidentifier NOT NULL,
        [MenuId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BranchMenus] PRIMARY KEY ([BranchId], [MenuId]),
        CONSTRAINT [FK_BranchMenus_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_BranchMenus_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [MenuCategories] (
        [Id] uniqueidentifier NOT NULL,
        [MenuId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuCategories_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [MenuItems] (
        [Id] uniqueidentifier NOT NULL,
        [MenuCategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuItems_MenuCategories_MenuCategoryId] FOREIGN KEY ([MenuCategoryId]) REFERENCES [MenuCategories] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [MenuItemImages] (
        [Id] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [Url] nvarchar(max) NOT NULL,
        [IsPrimary] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuItemImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MenuItemImages_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE TABLE [MenuItemIngredients] (
        [MenuItemId] uniqueidentifier NOT NULL,
        [IngredientId] uniqueidentifier NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuItemIngredients] PRIMARY KEY ([MenuItemId], [IngredientId]),
        CONSTRAINT [FK_MenuItemIngredients_Ingredients_IngredientId] FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MenuItemIngredients_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Branches_TenantId_Name] ON [Branches] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_BranchMenus_MenuId] ON [BranchMenus] ([MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MenuCategories_MenuId] ON [MenuCategories] ([MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MenuItemImages_MenuItemId] ON [MenuItemImages] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MenuItemIngredients_IngredientId] ON [MenuItemIngredients] ([IngredientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MenuItems_MenuCategoryId] ON [MenuItems] ([MenuCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Menus_TenantId_Slug] ON [Menus] ([TenantId], [Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_QrCodes_BranchId] ON [QrCodes] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QrCodes_Code] ON [QrCodes] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Subscriptions_PlanId] ON [Subscriptions] ([PlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tenants_Slug] ON [Tenants] ([Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812171755_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812171755_InitialCreate', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [NormalizedEmail] nvarchar(450) NOT NULL,
        [DisplayName] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE TABLE [Memberships] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [Role] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Memberships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Memberships_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_Memberships_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE INDEX [IX_Memberships_BranchId] ON [Memberships] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Memberships_TenantId_UserId] ON [Memberships] ([TenantId], [UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE INDEX [IX_Memberships_UserId] ON [Memberships] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812174029_AddMembership'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812174029_AddMembership', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812175548_AddBranchSlug'
)
BEGIN
    ALTER TABLE [Branches] ADD [Slug] nvarchar(450) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812175548_AddBranchSlug'
)
BEGIN

                    UPDATE Branches
                    SET Slug = CASE
                        WHEN LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'), '/', '-'))) = ''
                            THEN 'branch-' + RIGHT(CONVERT(varchar(36), Id), 8)
                        ELSE LOWER(LTRIM(RTRIM(REPLACE(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'), '/', '-'))))
                    END;

                    WITH DuplicateSlugs AS
                    (
                        SELECT Id,
                               ROW_NUMBER() OVER (PARTITION BY TenantId, Slug ORDER BY CreatedAtUtc, Id) AS RowNumber
                        FROM Branches
                    )
                    UPDATE b
                    SET Slug = CONCAT(b.Slug, '-', d.RowNumber)
                    FROM Branches b
                    INNER JOIN DuplicateSlugs d ON d.Id = b.Id
                    WHERE d.RowNumber > 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812175548_AddBranchSlug'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_TenantId_Slug] ON [Branches] ([TenantId], [Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812175548_AddBranchSlug'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812175548_AddBranchSlug', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE TABLE [BranchMenuItemOverrides] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [PriceOverride] decimal(18,2) NULL,
        [IsAvailableOverride] bit NULL,
        [IsVisibleOverride] bit NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BranchMenuItemOverrides] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BranchMenuItemOverrides_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_BranchMenuItemOverrides_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE TABLE [BranchSpecificMenuItems] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [CategoryId] uniqueidentifier NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Price] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [IsVisible] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_BranchSpecificMenuItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BranchSpecificMenuItems_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_BranchSpecificMenuItems_MenuCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [MenuCategories] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE INDEX [IX_BranchMenuItemOverrides_BranchId] ON [BranchMenuItemOverrides] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE INDEX [IX_BranchMenuItemOverrides_MenuItemId] ON [BranchMenuItemOverrides] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE UNIQUE INDEX [IX_BranchMenuItemOverrides_TenantId_BranchId_MenuItemId] ON [BranchMenuItemOverrides] ([TenantId], [BranchId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE INDEX [IX_BranchSpecificMenuItems_BranchId] ON [BranchSpecificMenuItems] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    CREATE INDEX [IX_BranchSpecificMenuItems_CategoryId] ON [BranchSpecificMenuItems] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812181324_AddBranchMenuOverrides'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812181324_AddBranchMenuOverrides', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812182744_AddImageMetadata'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD [ContentType] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812182744_AddImageMetadata'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD [OriginalFileName] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812182744_AddImageMetadata'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD [SortOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812182744_AddImageMetadata'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812182744_AddImageMetadata', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812183959_AddPlanFeatures'
)
BEGIN
    CREATE TABLE [PlanFeatures] (
        [Id] uniqueidentifier NOT NULL,
        [PlanId] uniqueidentifier NOT NULL,
        [FeatureKey] nvarchar(450) NOT NULL,
        [Enabled] bit NOT NULL,
        [LimitValue] int NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PlanFeatures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlanFeatures_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812183959_AddPlanFeatures'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlanFeatures_PlanId_FeatureKey] ON [PlanFeatures] ([PlanId], [FeatureKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812183959_AddPlanFeatures'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812183959_AddPlanFeatures', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812184659_AddPaymentTransactions'
)
BEGIN
    CREATE TABLE [PaymentTransactions] (
        [Id] uniqueidentifier NOT NULL,
        [SubscriptionId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Currency] nvarchar(max) NOT NULL,
        [Provider] nvarchar(450) NOT NULL,
        [ProviderReference] nvarchar(450) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PaymentTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PaymentTransactions_Subscriptions_SubscriptionId] FOREIGN KEY ([SubscriptionId]) REFERENCES [Subscriptions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812184659_AddPaymentTransactions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions] ([Provider], [ProviderReference]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812184659_AddPaymentTransactions'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_SubscriptionId] ON [PaymentTransactions] ([SubscriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812184659_AddPaymentTransactions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812184659_AddPaymentTransactions', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185216_AddAuditLogs'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [ActorUserId] uniqueidentifier NULL,
        [ActorDisplayName] nvarchar(max) NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityType] nvarchar(max) NOT NULL,
        [EntityId] uniqueidentifier NULL,
        [OldValueJson] nvarchar(max) NULL,
        [NewValueJson] nvarchar(max) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185216_AddAuditLogs'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_TenantId_CreatedAtUtc] ON [AuditLogs] ([TenantId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185216_AddAuditLogs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812185216_AddAuditLogs', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185732_AddAnalyticsEvents'
)
BEGIN
    CREATE TABLE [AnalyticsEvents] (
        [Id] uniqueidentifier NOT NULL,
        [EventType] nvarchar(450) NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [MenuId] uniqueidentifier NULL,
        [MenuItemId] uniqueidentifier NULL,
        [Device] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_AnalyticsEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185732_AddAnalyticsEvents'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_TenantId_BranchId_CreatedAtUtc] ON [AnalyticsEvents] ([TenantId], [BranchId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185732_AddAnalyticsEvents'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_TenantId_EventType_CreatedAtUtc] ON [AnalyticsEvents] ([TenantId], [EventType], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812185732_AddAnalyticsEvents'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812185732_AddAnalyticsEvents', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812192119_AddLookupValues'
)
BEGIN
    CREATE TABLE [LookupValues] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(64) NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [NameEn] nvarchar(160) NOT NULL,
        [NameAr] nvarchar(160) NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LookupValues] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812192119_AddLookupValues'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LookupValues_TenantId_Type_Code] ON [LookupValues] ([TenantId], [Type], [Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812192119_AddLookupValues'
)
BEGIN
    CREATE INDEX [IX_LookupValues_TenantId_Type_IsActive_SortOrder] ON [LookupValues] ([TenantId], [Type], [IsActive], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812192119_AddLookupValues'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812192119_AddLookupValues', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    ALTER TABLE [LookupValues] ADD [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    CREATE TABLE [PriceHistories] (
        [Id] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [PreviousPrice] decimal(18,2) NOT NULL,
        [NewPrice] decimal(18,2) NOT NULL,
        [OperationCode] nvarchar(64) NOT NULL,
        [ChangeAmount] decimal(18,2) NULL,
        [ChangePercentage] decimal(9,4) NULL,
        [Reason] nvarchar(500) NULL,
        [ActorUserId] uniqueidentifier NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_PriceHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PriceHistories_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [Branches] ([Id]),
        CONSTRAINT [FK_PriceHistories_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    CREATE INDEX [IX_PriceHistories_BranchId] ON [PriceHistories] ([BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    CREATE INDEX [IX_PriceHistories_MenuItemId] ON [PriceHistories] ([MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    CREATE INDEX [IX_PriceHistories_TenantId_CreatedAtUtc] ON [PriceHistories] ([TenantId], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812193854_AddPricingAndLookupDescriptions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812193854_AddPricingAndLookupDescriptions', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    ALTER TABLE [Users] ADD [FailedLoginCount] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    ALTER TABLE [Users] ADD [LastLoginAtUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    ALTER TABLE [Users] ADD [LockoutEndUtc] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    ALTER TABLE [Users] ADD [SecurityStamp] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    ALTER TABLE [LookupValues] ADD [IsGlobal] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812194910_AddAuthenticationSecurityAndGlobalLookups'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812194910_AddAuthenticationSecurityAndGlobalLookups', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200135_AddIngredientManagement'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ingredients]') AND [c].[name] = N'Name');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Ingredients] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Ingredients] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200135_AddIngredientManagement'
)
BEGIN
    ALTER TABLE [Ingredients] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200135_AddIngredientManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Ingredients_TenantId_Name] ON [Ingredients] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200135_AddIngredientManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812200135_AddIngredientManagement', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200225_NormalizeIngredientActiveDefault'
)
BEGIN
    UPDATE [Ingredients] SET [IsActive] = 1 WHERE [IsActive] = 0
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200225_NormalizeIngredientActiveDefault'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ingredients]') AND [c].[name] = N'IsActive');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Ingredients] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Ingredients] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200225_NormalizeIngredientActiveDefault'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812200225_NormalizeIngredientActiveDefault', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE TABLE [Modifiers] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [IsRequired] bit NOT NULL,
        [MinSelections] int NOT NULL,
        [MaxSelections] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Modifiers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE TABLE [MenuItemModifiers] (
        [MenuItemId] uniqueidentifier NOT NULL,
        [ModifierId] uniqueidentifier NOT NULL,
        [SortOrder] int NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuItemModifiers] PRIMARY KEY ([MenuItemId], [ModifierId]),
        CONSTRAINT [FK_MenuItemModifiers_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MenuItemModifiers_Modifiers_ModifierId] FOREIGN KEY ([ModifierId]) REFERENCES [Modifiers] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE TABLE [ModifierOptions] (
        [Id] uniqueidentifier NOT NULL,
        [ModifierId] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [PriceAdjustment] decimal(18,2) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_ModifierOptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ModifierOptions_Modifiers_ModifierId] FOREIGN KEY ([ModifierId]) REFERENCES [Modifiers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE INDEX [IX_MenuItemModifiers_ModifierId] ON [MenuItemModifiers] ([ModifierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE INDEX [IX_MenuItemModifiers_TenantId_ModifierId] ON [MenuItemModifiers] ([TenantId], [ModifierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE INDEX [IX_ModifierOptions_ModifierId] ON [ModifierOptions] ([ModifierId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ModifierOptions_TenantId_ModifierId_Name] ON [ModifierOptions] ([TenantId], [ModifierId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Modifiers_TenantId_Name] ON [Modifiers] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812200559_AddModifiers'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812200559_AddModifiers', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812201057_AddPlanCurrency'
)
BEGIN
    ALTER TABLE [Plans] ADD [Currency] nvarchar(3) NOT NULL DEFAULT N'EGP';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812201057_AddPlanCurrency'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812201057_AddPlanCurrency', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Address] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [CoverImageUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Currency] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Email] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Tenants] ADD [Phone] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Menus] ADD [MenuTypeCode] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Menus] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Menus] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Menus] ADD [ScopeCode] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [DescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuItems] ADD [ProductTypeCode] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [ClassificationCode] nvarchar(64) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD [ParentCategoryId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Branches] ADD [Latitude] decimal(9,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Branches] ADD [Longitude] decimal(9,6) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Branches] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Branches] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    ALTER TABLE [Branches] ADD [OpeningHours] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    CREATE INDEX [IX_Menus_TenantId_MenuTypeCode_ScopeCode] ON [Menus] ([TenantId], [MenuTypeCode], [ScopeCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202529_AddBilingualMenuClassification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812202529_AddBilingualMenuClassification', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202641_NormalizeBilingualActiveDefaults'
)
BEGIN
    UPDATE [Tenants] SET [IsActive] = 1 WHERE [IsActive] = 0
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202641_NormalizeBilingualActiveDefaults'
)
BEGIN
    UPDATE [MenuCategories] SET [IsActive] = 1 WHERE [IsActive] = 0
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202641_NormalizeBilingualActiveDefaults'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'IsActive');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Tenants] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202641_NormalizeBilingualActiveDefaults'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'IsActive');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [MenuCategories] ADD DEFAULT CAST(1 AS bit) FOR [IsActive];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812202641_NormalizeBilingualActiveDefaults'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812202641_NormalizeBilingualActiveDefaults', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812205018_AddSeparateAllergens'
)
BEGIN
    CREATE TABLE [Allergens] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Allergens] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812205018_AddSeparateAllergens'
)
BEGIN
    CREATE TABLE [MenuItemAllergens] (
        [MenuItemId] uniqueidentifier NOT NULL,
        [AllergenId] uniqueidentifier NOT NULL,
        [Id] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_MenuItemAllergens] PRIMARY KEY ([MenuItemId], [AllergenId]),
        CONSTRAINT [FK_MenuItemAllergens_Allergens_AllergenId] FOREIGN KEY ([AllergenId]) REFERENCES [Allergens] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_MenuItemAllergens_MenuItems_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [MenuItems] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812205018_AddSeparateAllergens'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Allergens_TenantId_Name] ON [Allergens] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812205018_AddSeparateAllergens'
)
BEGIN
    CREATE INDEX [IX_MenuItemAllergens_AllergenId] ON [MenuItemAllergens] ([AllergenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812205018_AddSeparateAllergens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812205018_AddSeparateAllergens', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812210837_RemovePlanCurrencyDefault'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Plans]') AND [c].[name] = N'Currency');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Plans] DROP CONSTRAINT [' + @var4 + '];');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812210837_RemovePlanCurrencyDefault'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812210837_RemovePlanCurrencyDefault', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812212909_AddPasswordResetTokens'
)
BEGIN
    CREATE TABLE [PasswordResetTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(450) NOT NULL,
        [ExpiresAtUtc] datetime2 NOT NULL,
        [UsedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PasswordResetTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812212909_AddPasswordResetTokens'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PasswordResetTokens_TokenHash] ON [PasswordResetTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812212909_AddPasswordResetTokens'
)
BEGIN
    CREATE INDEX [IX_PasswordResetTokens_UserId] ON [PasswordResetTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812212909_AddPasswordResetTokens'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812212909_AddPasswordResetTokens', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenuItemOverrides] DROP CONSTRAINT [FK_BranchMenuItemOverrides_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenuItemOverrides] DROP CONSTRAINT [FK_BranchMenuItemOverrides_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenus] DROP CONSTRAINT [FK_BranchMenus_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenus] DROP CONSTRAINT [FK_BranchMenus_Menus_MenuId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [FK_BranchSpecificMenuItems_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [FK_BranchSpecificMenuItems_MenuCategories_CategoryId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Memberships] DROP CONSTRAINT [FK_Memberships_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuCategories] DROP CONSTRAINT [FK_MenuCategories_Menus_MenuId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemAllergens] DROP CONSTRAINT [FK_MenuItemAllergens_Allergens_AllergenId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemAllergens] DROP CONSTRAINT [FK_MenuItemAllergens_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemImages] DROP CONSTRAINT [FK_MenuItemImages_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemIngredients] DROP CONSTRAINT [FK_MenuItemIngredients_Ingredients_IngredientId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemIngredients] DROP CONSTRAINT [FK_MenuItemIngredients_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemModifiers] DROP CONSTRAINT [FK_MenuItemModifiers_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemModifiers] DROP CONSTRAINT [FK_MenuItemModifiers_Modifiers_ModifierId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItems] DROP CONSTRAINT [FK_MenuItems_MenuCategories_MenuCategoryId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [ModifierOptions] DROP CONSTRAINT [FK_ModifierOptions_Modifiers_ModifierId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [FK_PaymentTransactions_Subscriptions_SubscriptionId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PriceHistories] DROP CONSTRAINT [FK_PriceHistories_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PriceHistories] DROP CONSTRAINT [FK_PriceHistories_MenuItems_MenuItemId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [QrCodes] DROP CONSTRAINT [FK_QrCodes_Branches_BranchId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_QrCodes_BranchId] ON [QrCodes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_PriceHistories_BranchId] ON [PriceHistories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_PriceHistories_MenuItemId] ON [PriceHistories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_SubscriptionId] ON [PaymentTransactions];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_ModifierOptions_ModifierId] ON [ModifierOptions];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuItems_MenuCategoryId] ON [MenuItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuItemModifiers_ModifierId] ON [MenuItemModifiers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuItemIngredients_IngredientId] ON [MenuItemIngredients];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuItemImages_MenuItemId] ON [MenuItemImages];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuItemAllergens_AllergenId] ON [MenuItemAllergens];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_MenuCategories_MenuId] ON [MenuCategories];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_Memberships_BranchId] ON [Memberships];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_BranchSpecificMenuItems_BranchId] ON [BranchSpecificMenuItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_BranchSpecificMenuItems_CategoryId] ON [BranchSpecificMenuItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_BranchMenus_MenuId] ON [BranchMenus];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_BranchMenuItemOverrides_BranchId] ON [BranchMenuItemOverrides];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    DROP INDEX [IX_BranchMenuItemOverrides_MenuItemId] ON [BranchMenuItemOverrides];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Subscriptions] ADD CONSTRAINT [AK_Subscriptions_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Modifiers] ADD CONSTRAINT [AK_Modifiers_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Menus] ADD CONSTRAINT [AK_Menus_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItems] ADD CONSTRAINT [AK_MenuItems_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD CONSTRAINT [AK_MenuCategories_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Ingredients] ADD CONSTRAINT [AK_Ingredients_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Branches] ADD CONSTRAINT [AK_Branches_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Allergens] ADD CONSTRAINT [AK_Allergens_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_QrCodes_TenantId_BranchId] ON [QrCodes] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_PriceHistories_TenantId_BranchId] ON [PriceHistories] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_PriceHistories_TenantId_MenuItemId] ON [PriceHistories] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_TenantId_SubscriptionId] ON [PaymentTransactions] ([TenantId], [SubscriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItems_TenantId_MenuCategoryId] ON [MenuItems] ([TenantId], [MenuCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemModifiers_TenantId_MenuItemId] ON [MenuItemModifiers] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemIngredients_TenantId_IngredientId] ON [MenuItemIngredients] ([TenantId], [IngredientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemIngredients_TenantId_MenuItemId] ON [MenuItemIngredients] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemImages_TenantId_MenuItemId] ON [MenuItemImages] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemAllergens_TenantId_AllergenId] ON [MenuItemAllergens] ([TenantId], [AllergenId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuItemAllergens_TenantId_MenuItemId] ON [MenuItemAllergens] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuCategories_TenantId_MenuId] ON [MenuCategories] ([TenantId], [MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_MenuCategories_TenantId_ParentCategoryId] ON [MenuCategories] ([TenantId], [ParentCategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_Memberships_TenantId_BranchId] ON [Memberships] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_BranchSpecificMenuItems_TenantId_BranchId] ON [BranchSpecificMenuItems] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_BranchSpecificMenuItems_TenantId_CategoryId] ON [BranchSpecificMenuItems] ([TenantId], [CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_BranchMenus_TenantId_BranchId] ON [BranchMenus] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_BranchMenus_TenantId_MenuId] ON [BranchMenus] ([TenantId], [MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_BranchMenuItemOverrides_TenantId_MenuItemId] ON [BranchMenuItemOverrides] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_TenantId_MenuId] ON [AnalyticsEvents] ([TenantId], [MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    CREATE INDEX [IX_AnalyticsEvents_TenantId_MenuItemId] ON [AnalyticsEvents] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [AnalyticsEvents] ADD CONSTRAINT [FK_AnalyticsEvents_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [AnalyticsEvents] ADD CONSTRAINT [FK_AnalyticsEvents_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [AnalyticsEvents] ADD CONSTRAINT [FK_AnalyticsEvents_Menus_TenantId_MenuId] FOREIGN KEY ([TenantId], [MenuId]) REFERENCES [Menus] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenuItemOverrides] ADD CONSTRAINT [FK_BranchMenuItemOverrides_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenuItemOverrides] ADD CONSTRAINT [FK_BranchMenuItemOverrides_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenus] ADD CONSTRAINT [FK_BranchMenus_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchMenus] ADD CONSTRAINT [FK_BranchMenus_Menus_TenantId_MenuId] FOREIGN KEY ([TenantId], [MenuId]) REFERENCES [Menus] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD CONSTRAINT [FK_BranchSpecificMenuItems_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD CONSTRAINT [FK_BranchSpecificMenuItems_MenuCategories_TenantId_CategoryId] FOREIGN KEY ([TenantId], [CategoryId]) REFERENCES [MenuCategories] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [Memberships] ADD CONSTRAINT [FK_Memberships_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD CONSTRAINT [FK_MenuCategories_MenuCategories_TenantId_ParentCategoryId] FOREIGN KEY ([TenantId], [ParentCategoryId]) REFERENCES [MenuCategories] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuCategories] ADD CONSTRAINT [FK_MenuCategories_Menus_TenantId_MenuId] FOREIGN KEY ([TenantId], [MenuId]) REFERENCES [Menus] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemAllergens] ADD CONSTRAINT [FK_MenuItemAllergens_Allergens_TenantId_AllergenId] FOREIGN KEY ([TenantId], [AllergenId]) REFERENCES [Allergens] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemAllergens] ADD CONSTRAINT [FK_MenuItemAllergens_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD CONSTRAINT [FK_MenuItemImages_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemIngredients] ADD CONSTRAINT [FK_MenuItemIngredients_Ingredients_TenantId_IngredientId] FOREIGN KEY ([TenantId], [IngredientId]) REFERENCES [Ingredients] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemIngredients] ADD CONSTRAINT [FK_MenuItemIngredients_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemModifiers] ADD CONSTRAINT [FK_MenuItemModifiers_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItemModifiers] ADD CONSTRAINT [FK_MenuItemModifiers_Modifiers_TenantId_ModifierId] FOREIGN KEY ([TenantId], [ModifierId]) REFERENCES [Modifiers] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [MenuItems] ADD CONSTRAINT [FK_MenuItems_MenuCategories_TenantId_MenuCategoryId] FOREIGN KEY ([TenantId], [MenuCategoryId]) REFERENCES [MenuCategories] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [ModifierOptions] ADD CONSTRAINT [FK_ModifierOptions_Modifiers_TenantId_ModifierId] FOREIGN KEY ([TenantId], [ModifierId]) REFERENCES [Modifiers] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD CONSTRAINT [FK_PaymentTransactions_Subscriptions_TenantId_SubscriptionId] FOREIGN KEY ([TenantId], [SubscriptionId]) REFERENCES [Subscriptions] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PriceHistories] ADD CONSTRAINT [FK_PriceHistories_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [PriceHistories] ADD CONSTRAINT [FK_PriceHistories_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    ALTER TABLE [QrCodes] ADD CONSTRAINT [FK_QrCodes_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812215021_Module03TenantIsolationConstraints'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812215021_Module03TenantIsolationConstraints', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812221132_Module04OnboardingBranding'
)
BEGIN
    ALTER TABLE [Tenants] ADD [BrandAccentColor] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812221132_Module04OnboardingBranding'
)
BEGIN
    ALTER TABLE [Tenants] ADD [BrandPrimaryColor] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812221132_Module04OnboardingBranding'
)
BEGIN
    ALTER TABLE [Branches] ADD [BrandAccentColorOverride] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812221132_Module04OnboardingBranding'
)
BEGIN
    ALTER TABLE [Branches] ADD [BrandPrimaryColorOverride] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812221132_Module04OnboardingBranding'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812221132_Module04OnboardingBranding', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    ALTER TABLE [Menus] ADD [BrandAccentColor] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    ALTER TABLE [Menus] ADD [BrandPrimaryColor] nvarchar(16) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    ALTER TABLE [Menus] ADD [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    ALTER TABLE [Menus] ADD [DescriptionAr] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    ALTER TABLE [Menus] ADD [SortOrder] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    CREATE INDEX [IX_Menus_TenantId_SortOrder] ON [Menus] ([TenantId], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812222850_Module05MenuArchitecture'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812222850_Module05MenuArchitecture', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    DROP INDEX [IX_MenuItemImages_TenantId_MenuItemId] ON [MenuItemImages];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItemImages]') AND [c].[name] = N'Url');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [MenuItemImages] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [MenuItemImages] ALTER COLUMN [Url] nvarchar(500) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItemImages]') AND [c].[name] = N'OriginalFileName');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [MenuItemImages] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [MenuItemImages] ALTER COLUMN [OriginalFileName] nvarchar(260) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItemImages]') AND [c].[name] = N'ContentType');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [MenuItemImages] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [MenuItemImages] ALTER COLUMN [ContentType] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD [AltText] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    ALTER TABLE [MenuItemImages] ADD [StorageKey] nvarchar(260) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    CREATE INDEX [IX_MenuItemImages_TenantId_MenuItemId_SortOrder] ON [MenuItemImages] ([TenantId], [MenuItemId], [SortOrder]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812225524_Module06ProductCatalogImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812225524_Module06ProductCatalogImages', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    DROP INDEX [IX_Modifiers_TenantId_Name] ON [Modifiers];
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Modifiers]') AND [c].[name] = N'Name');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Modifiers] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Modifiers] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE UNIQUE INDEX [IX_Modifiers_TenantId_Name] ON [Modifiers] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Modifiers] ADD [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Modifiers] ADD [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    DROP INDEX [IX_ModifierOptions_TenantId_ModifierId_Name] ON [ModifierOptions];
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ModifierOptions]') AND [c].[name] = N'Name');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ModifierOptions] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [ModifierOptions] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE UNIQUE INDEX [IX_ModifierOptions_TenantId_ModifierId_Name] ON [ModifierOptions] ([TenantId], [ModifierId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [ModifierOptions] ADD [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [ModifierOptions] ADD [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    DROP INDEX [IX_Ingredients_TenantId_Name] ON [Ingredients];
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ingredients]') AND [c].[name] = N'Name');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Ingredients] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Ingredients] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE UNIQUE INDEX [IX_Ingredients_TenantId_Name] ON [Ingredients] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Ingredients] ADD [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Ingredients] ADD [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    DROP INDEX [IX_Allergens_TenantId_Name] ON [Allergens];
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Allergens]') AND [c].[name] = N'Name');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Allergens] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Allergens] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE UNIQUE INDEX [IX_Allergens_TenantId_Name] ON [Allergens] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Allergens] ADD [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    ALTER TABLE [Allergens] ADD [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    UPDATE Ingredients SET NameEn = Name WHERE NameEn IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    UPDATE Allergens SET NameEn = Name WHERE NameEn IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    UPDATE Modifiers SET NameEn = Name WHERE NameEn IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    UPDATE ModifierOptions SET NameEn = Name WHERE NameEn IS NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN

    INSERT INTO Allergens (Id, Name, NameEn, NameAr, IsActive, CreatedAtUtc, UpdatedAtUtc, TenantId)
    SELECT NEWID(), i.Name, i.Name, NULL, 1, i.CreatedAtUtc, NULL, i.TenantId
    FROM Ingredients i
    WHERE i.IsAllergen = 1
      AND i.IsActive = 1
      AND NOT EXISTS (SELECT 1 FROM Allergens a WHERE a.TenantId = i.TenantId AND a.Name = i.Name);

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN

    INSERT INTO MenuItemAllergens (MenuItemId, AllergenId, Id, CreatedAtUtc, UpdatedAtUtc, TenantId)
    SELECT link.MenuItemId, a.Id, NEWID(), link.CreatedAtUtc, NULL, link.TenantId
    FROM MenuItemIngredients link
    INNER JOIN Ingredients i ON i.Id = link.IngredientId AND i.TenantId = link.TenantId
    INNER JOIN Allergens a ON a.TenantId = i.TenantId AND a.Name = i.Name
    WHERE i.IsAllergen = 1
      AND NOT EXISTS (SELECT 1 FROM MenuItemAllergens existing WHERE existing.MenuItemId = link.MenuItemId AND existing.AllergenId = a.Id);

END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ingredients]') AND [c].[name] = N'IsAllergen');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Ingredients] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Ingredients] DROP COLUMN [IsAllergen];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812231550_Module07ManagedCatalogRelationships'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812231550_Module07ManagedCatalogRelationships', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812233340_Module08LookupConfigurationCenter'
)
BEGIN
    CREATE TABLE [LookupTypes] (
        [Id] uniqueidentifier NOT NULL,
        [IsGlobal] bit NOT NULL,
        [Code] nvarchar(64) NOT NULL,
        [NameEn] nvarchar(160) NOT NULL,
        [NameAr] nvarchar(160) NULL,
        [Description] nvarchar(500) NULL,
        [IsActive] bit NOT NULL,
        [SortOrder] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_LookupTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812233340_Module08LookupConfigurationCenter'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LookupTypes_TenantId_Code] ON [LookupTypes] ([TenantId], [Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812233340_Module08LookupConfigurationCenter'
)
BEGIN

    INSERT INTO [LookupTypes]
        ([Id], [IsGlobal], [Code], [NameEn], [NameAr], [Description], [IsActive], [SortOrder], [CreatedAtUtc], [UpdatedAtUtc], [TenantId])
    SELECT NEWID(), CAST(1 AS bit), [Type], [Type], NULL, N'Migrated from the existing lookup catalog.', CAST(1 AS bit), MIN([SortOrder]), SYSUTCDATETIME(), NULL, '00000000-0000-0000-0000-000000000000'
    FROM [LookupValues] AS value
    WHERE value.[IsGlobal] = 1
      AND value.[TenantId] = '00000000-0000-0000-0000-000000000000'
      AND NOT EXISTS (
          SELECT 1 FROM [LookupTypes] AS existing
          WHERE existing.[IsGlobal] = 1
            AND existing.[TenantId] = '00000000-0000-0000-0000-000000000000'
            AND existing.[Code] = value.[Type])
    GROUP BY [Type];

    INSERT INTO [LookupTypes]
        ([Id], [IsGlobal], [Code], [NameEn], [NameAr], [Description], [IsActive], [SortOrder], [CreatedAtUtc], [UpdatedAtUtc], [TenantId])
    SELECT NEWID(), CAST(0 AS bit), value.[Type], value.[Type], NULL, N'Migrated from the existing lookup catalog.', CAST(1 AS bit), MIN(value.[SortOrder]), SYSUTCDATETIME(), NULL, value.[TenantId]
    FROM [LookupValues] AS value
    WHERE value.[IsGlobal] = 0
      AND NOT EXISTS (
          SELECT 1 FROM [LookupTypes] AS existing
          WHERE existing.[Code] = value.[Type]
            AND (existing.[IsGlobal] = 1 OR existing.[TenantId] = value.[TenantId]))
    GROUP BY value.[TenantId], value.[Type];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260812233340_Module08LookupConfigurationCenter'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260812233340_Module08LookupConfigurationCenter', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813000906_Module10QrManagement'
)
BEGIN
    DROP INDEX [IX_QrCodes_TenantId_BranchId] ON [QrCodes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813000906_Module10QrManagement'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[QrCodes]') AND [c].[name] = N'TargetType');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [QrCodes] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [QrCodes] ALTER COLUMN [TargetType] nvarchar(64) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813000906_Module10QrManagement'
)
BEGIN
    ALTER TABLE [QrCodes] ADD [TableLabel] nvarchar(120) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813000906_Module10QrManagement'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_QrCodes_TenantId_BranchId_TargetType_TableLabel] ON [QrCodes] ([TenantId], [BranchId], [TargetType], [TableLabel]) WHERE [TableLabel] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813000906_Module10QrManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813000906_Module10QrManagement', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Plans]') AND [c].[name] = N'Name');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Plans] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Plans] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    ALTER TABLE [Plans] ADD [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD [RequestedPlanId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    CREATE INDEX [IX_PaymentTransactions_RequestedPlanId] ON [PaymentTransactions] ([RequestedPlanId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    ALTER TABLE [PaymentTransactions] ADD CONSTRAINT [FK_PaymentTransactions_Plans_RequestedPlanId] FOREIGN KEY ([RequestedPlanId]) REFERENCES [Plans] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813003620_Module12CommercialLayer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813003620_Module12CommercialLayer', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051241_AddBranchSpecificItemLocalization'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813051241_AddBranchSpecificItemLocalization', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051948_AddBranchSpecificItemLocalizationColumns'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD [DescriptionAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051948_AddBranchSpecificItemLocalizationColumns'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD [DescriptionEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051948_AddBranchSpecificItemLocalizationColumns'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD [NameAr] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051948_AddBranchSpecificItemLocalizationColumns'
)
BEGIN
    ALTER TABLE [BranchSpecificMenuItems] ADD [NameEn] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813051948_AddBranchSpecificItemLocalizationColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813051948_AddBranchSpecificItemLocalizationColumns', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    ALTER TABLE [Memberships] ADD CONSTRAINT [AK_Memberships_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE TABLE [PermissionDefinitions] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(120) NOT NULL,
        [GroupCode] nvarchar(64) NOT NULL,
        [NameEn] nvarchar(160) NOT NULL,
        [NameAr] nvarchar(160) NOT NULL,
        [SortOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_PermissionDefinitions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [Role] nvarchar(450) NOT NULL,
        [PermissionCode] nvarchar(120) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE TABLE [UserPermissions] (
        [Id] uniqueidentifier NOT NULL,
        [MembershipId] uniqueidentifier NOT NULL,
        [PermissionCode] nvarchar(120) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPermissions_Memberships_TenantId_MembershipId] FOREIGN KEY ([TenantId], [MembershipId]) REFERENCES [Memberships] ([TenantId], [Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PermissionDefinitions_Code] ON [PermissionDefinitions] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RolePermissions_Role_PermissionCode] ON [RolePermissions] ([Role], [PermissionCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UserPermissions_TenantId_MembershipId_PermissionCode] ON [UserPermissions] ([TenantId], [MembershipId], [PermissionCode]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813155411_AddUserPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813155411_AddUserPermissions', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    ALTER TABLE [ModifierOptions] ADD CONSTRAINT [AK_ModifierOptions_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE TABLE [Orders] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [MenuId] uniqueidentifier NULL,
        [OrderNumber] nvarchar(32) NOT NULL,
        [IdempotencyKey] nvarchar(120) NOT NULL,
        [CustomerName] nvarchar(160) NOT NULL,
        [CustomerPhone] nvarchar(40) NOT NULL,
        [Notes] nvarchar(500) NULL,
        [Total] decimal(18,2) NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CompletedAtUtc] datetime2 NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_Orders_TenantId_Id] UNIQUE ([TenantId], [Id]),
        CONSTRAINT [FK_Orders_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id]),
        CONSTRAINT [FK_Orders_Menus_TenantId_MenuId] FOREIGN KEY ([TenantId], [MenuId]) REFERENCES [Menus] ([TenantId], [Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [ProductName] nvarchar(160) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [Quantity] int NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_OrderItems_TenantId_Id] UNIQUE ([TenantId], [Id]),
        CONSTRAINT [FK_OrderItems_MenuItems_TenantId_MenuItemId] FOREIGN KEY ([TenantId], [MenuItemId]) REFERENCES [MenuItems] ([TenantId], [Id]),
        CONSTRAINT [FK_OrderItems_Orders_TenantId_OrderId] FOREIGN KEY ([TenantId], [OrderId]) REFERENCES [Orders] ([TenantId], [Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE TABLE [OrderStatusHistories] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [FromStatus] nvarchar(max) NULL,
        [ToStatus] nvarchar(max) NOT NULL,
        [ActorUserId] uniqueidentifier NULL,
        [ActorDisplayName] nvarchar(160) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_OrderStatusHistories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderStatusHistories_Orders_TenantId_OrderId] FOREIGN KEY ([TenantId], [OrderId]) REFERENCES [Orders] ([TenantId], [Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE TABLE [OrderItemModifiers] (
        [Id] uniqueidentifier NOT NULL,
        [OrderItemId] uniqueidentifier NOT NULL,
        [ModifierOptionId] uniqueidentifier NOT NULL,
        [OptionName] nvarchar(160) NOT NULL,
        [PriceAdjustment] decimal(18,2) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_OrderItemModifiers] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_OrderItemModifiers_TenantId_Id] UNIQUE ([TenantId], [Id]),
        CONSTRAINT [FK_OrderItemModifiers_ModifierOptions_TenantId_ModifierOptionId] FOREIGN KEY ([TenantId], [ModifierOptionId]) REFERENCES [ModifierOptions] ([TenantId], [Id]),
        CONSTRAINT [FK_OrderItemModifiers_OrderItems_TenantId_OrderItemId] FOREIGN KEY ([TenantId], [OrderItemId]) REFERENCES [OrderItems] ([TenantId], [Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_OrderItemModifiers_TenantId_ModifierOptionId] ON [OrderItemModifiers] ([TenantId], [ModifierOptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_OrderItemModifiers_TenantId_OrderItemId] ON [OrderItemModifiers] ([TenantId], [OrderItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_OrderItems_TenantId_MenuItemId] ON [OrderItems] ([TenantId], [MenuItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_OrderItems_TenantId_OrderId] ON [OrderItems] ([TenantId], [OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_Orders_TenantId_BranchId] ON [Orders] ([TenantId], [BranchId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_TenantId_IdempotencyKey] ON [Orders] ([TenantId], [IdempotencyKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_Orders_TenantId_MenuId] ON [Orders] ([TenantId], [MenuId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_TenantId_OrderNumber] ON [Orders] ([TenantId], [OrderNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    CREATE INDEX [IX_OrderStatusHistories_TenantId_OrderId] ON [OrderStatusHistories] ([TenantId], [OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813172855_AddOrderLifecycle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813172855_AddOrderLifecycle', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813182841_AddTenantBrandingImages'
)
BEGIN
    CREATE TABLE [TenantBrandingImages] (
        [Id] uniqueidentifier NOT NULL,
        [Kind] nvarchar(32) NOT NULL,
        [Url] nvarchar(500) NOT NULL,
        [StorageKey] nvarchar(260) NOT NULL,
        [OriginalFileName] nvarchar(260) NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [SizeBytes] bigint NOT NULL,
        [AltText] nvarchar(300) NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_TenantBrandingImages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TenantBrandingImages_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813182841_AddTenantBrandingImages'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TenantBrandingImages_TenantId_Kind] ON [TenantBrandingImages] ([TenantId], [Kind]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260813182841_AddTenantBrandingImages'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260813182841_AddTenantBrandingImages', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [QrCodes] ADD [TableId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [Orders] ADD [QrCodeId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [Orders] ADD [TableId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [QrCodes] ADD CONSTRAINT [AK_QrCodes_TenantId_Id] UNIQUE ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE TABLE [RestaurantTables] (
        [Id] uniqueidentifier NOT NULL,
        [BranchId] uniqueidentifier NOT NULL,
        [Name] nvarchar(120) NOT NULL,
        [NameAr] nvarchar(120) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [UpdatedAtUtc] datetime2 NULL,
        [TenantId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RestaurantTables] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_RestaurantTables_TenantId_Id] UNIQUE ([TenantId], [Id]),
        CONSTRAINT [FK_RestaurantTables_Branches_TenantId_BranchId] FOREIGN KEY ([TenantId], [BranchId]) REFERENCES [Branches] ([TenantId], [Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE INDEX [IX_QrCodes_TenantId_BranchId_TableId_TargetType] ON [QrCodes] ([TenantId], [BranchId], [TableId], [TargetType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE INDEX [IX_QrCodes_TenantId_TableId] ON [QrCodes] ([TenantId], [TableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE INDEX [IX_Orders_TenantId_QrCodeId] ON [Orders] ([TenantId], [QrCodeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE INDEX [IX_Orders_TenantId_TableId] ON [Orders] ([TenantId], [TableId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RestaurantTables_TenantId_BranchId_Name] ON [RestaurantTables] ([TenantId], [BranchId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_QrCodes_TenantId_QrCodeId] FOREIGN KEY ([TenantId], [QrCodeId]) REFERENCES [QrCodes] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_RestaurantTables_TenantId_TableId] FOREIGN KEY ([TenantId], [TableId]) REFERENCES [RestaurantTables] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    ALTER TABLE [QrCodes] ADD CONSTRAINT [FK_QrCodes_RestaurantTables_TenantId_TableId] FOREIGN KEY ([TenantId], [TableId]) REFERENCES [RestaurantTables] ([TenantId], [Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814001000_AddRestaurantTablesAndOrderReferences'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814001000_AddRestaurantTablesAndOrderReferences', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814004826_EnforceTableSpecificQrUniqueness'
)
BEGIN
    DROP INDEX [IX_QrCodes_TenantId_BranchId_TableId_TargetType] ON [QrCodes];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814004826_EnforceTableSpecificQrUniqueness'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_QrCodes_TenantId_BranchId_TableId_TargetType] ON [QrCodes] ([TenantId], [BranchId], [TableId], [TargetType]) WHERE [TableId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814004826_EnforceTableSpecificQrUniqueness'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814004826_EnforceTableSpecificQrUniqueness', N'8.0.19');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814123000_RetireLegacyBranchQrCodes'
)
BEGIN
    UPDATE [QrCodes] SET [IsActive] = 0, [UpdatedAtUtc] = SYSUTCDATETIME() WHERE [TableId] IS NULL AND [IsActive] = 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814123000_RetireLegacyBranchQrCodes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260814123000_RetireLegacyBranchQrCodes', N'8.0.19');
END;
GO

COMMIT;
GO

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
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'SecurityStamp');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [Users] ALTER COLUMN [SecurityStamp] nvarchar(64) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(512) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_Users_NormalizedEmail] ON [Users];
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'NormalizedEmail');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [Users] ALTER COLUMN [NormalizedEmail] nvarchar(320) NOT NULL;
    CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Email');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Users] ALTER COLUMN [Email] nvarchar(320) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'DisplayName');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Users] ALTER COLUMN [DisplayName] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Status');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814233447_ProductionHardening'
)
BEGIN
    DROP INDEX [IX_PaymentTransactions_Provider_ProviderReference] ON [PaymentTransactions];
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'ProviderReference');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var21 + '];');
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
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Provider');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var22 + '];');
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
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Orders]') AND [c].[name] = N'Status');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [Orders] DROP CONSTRAINT [' + @var23 + '];');
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
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Slug');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Tenants_Slug] ON [Tenants] ([Slug]);
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Phone');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Phone] nvarchar(40) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'NameEn');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'NameAr');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [NameAr] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Name');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'LogoUrl');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [LogoUrl] nvarchar(1000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Email');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Email] nvarchar(320) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'DefaultLanguage');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [DefaultLanguage] nvarchar(10) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Currency');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Currency] nvarchar(8) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'CoverImageUrl');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [CoverImageUrl] nvarchar(1000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Address');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [Tenants] ALTER COLUMN [Address] nvarchar(500) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'Status');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'PaymentProvider');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [PaymentProvider] nvarchar(64) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Subscriptions]') AND [c].[name] = N'ExternalSubscriptionId');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [Subscriptions] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [Subscriptions] ALTER COLUMN [ExternalSubscriptionId] nvarchar(200) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PaymentTransactions]') AND [c].[name] = N'Currency');
    IF @var38 IS NOT NULL EXEC(N'ALTER TABLE [PaymentTransactions] DROP CONSTRAINT [' + @var38 + '];');
    ALTER TABLE [PaymentTransactions] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderStatusHistories]') AND [c].[name] = N'ToStatus');
    IF @var39 IS NOT NULL EXEC(N'ALTER TABLE [OrderStatusHistories] DROP CONSTRAINT [' + @var39 + '];');
    ALTER TABLE [OrderStatusHistories] ALTER COLUMN [ToStatus] nvarchar(32) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrderStatusHistories]') AND [c].[name] = N'FromStatus');
    IF @var40 IS NOT NULL EXEC(N'ALTER TABLE [OrderStatusHistories] DROP CONSTRAINT [' + @var40 + '];');
    ALTER TABLE [OrderStatusHistories] ALTER COLUMN [FromStatus] nvarchar(32) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Status');
    IF @var41 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var41 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Status] nvarchar(32) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Menus_TenantId_Slug] ON [Menus];
    DECLARE @var42 sysname;
    SELECT @var42 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Slug');
    IF @var42 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var42 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Menus_TenantId_Slug] ON [Menus] ([TenantId], [Slug]);
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'NameEn');
    IF @var43 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var43 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [NameEn] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'NameAr');
    IF @var44 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var44 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [NameAr] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Menus]') AND [c].[name] = N'Name');
    IF @var45 IS NOT NULL EXEC(N'ALTER TABLE [Menus] DROP CONSTRAINT [' + @var45 + '];');
    ALTER TABLE [Menus] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'NameEn');
    IF @var46 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var46 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [NameEn] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'NameAr');
    IF @var47 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var47 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [NameAr] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Name');
    IF @var48 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var48 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'DescriptionEn');
    IF @var49 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var49 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [DescriptionEn] nvarchar(2000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'DescriptionAr');
    IF @var50 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var50 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [DescriptionAr] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var51 sysname;
    SELECT @var51 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Description');
    IF @var51 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var51 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Description] nvarchar(2000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuItems]') AND [c].[name] = N'Currency');
    IF @var52 IS NOT NULL EXEC(N'ALTER TABLE [MenuItems] DROP CONSTRAINT [' + @var52 + '];');
    ALTER TABLE [MenuItems] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'NameEn');
    IF @var53 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var53 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [NameEn] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'NameAr');
    IF @var54 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var54 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [NameAr] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'Name');
    IF @var55 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var55 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var56 sysname;
    SELECT @var56 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'DescriptionAr');
    IF @var56 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var56 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [DescriptionAr] nvarchar(1000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MenuCategories]') AND [c].[name] = N'Description');
    IF @var57 IS NOT NULL EXEC(N'ALTER TABLE [MenuCategories] DROP CONSTRAINT [' + @var57 + '];');
    ALTER TABLE [MenuCategories] ALTER COLUMN [Description] nvarchar(1000) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Memberships]') AND [c].[name] = N'Role');
    IF @var58 IS NOT NULL EXEC(N'ALTER TABLE [Memberships] DROP CONSTRAINT [' + @var58 + '];');
    ALTER TABLE [Memberships] ALTER COLUMN [Role] nvarchar(32) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'NameEn');
    IF @var59 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var59 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [NameEn] nvarchar(160) NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'NameAr');
    IF @var60 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var60 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var61 sysname;
    SELECT @var61 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Name');
    IF @var61 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var61 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
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
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'DescriptionEn');
    IF @var62 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var62 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [DescriptionEn] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var63 sysname;
    SELECT @var63 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'DescriptionAr');
    IF @var63 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var63 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [DescriptionAr] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var64 sysname;
    SELECT @var64 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Description');
    IF @var64 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var64 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Description] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var65 sysname;
    SELECT @var65 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[BranchSpecificMenuItems]') AND [c].[name] = N'Currency');
    IF @var65 IS NOT NULL EXEC(N'ALTER TABLE [BranchSpecificMenuItems] DROP CONSTRAINT [' + @var65 + '];');
    ALTER TABLE [BranchSpecificMenuItems] ALTER COLUMN [Currency] nvarchar(8) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Branches_TenantId_Slug] ON [Branches];
    DECLARE @var66 sysname;
    SELECT @var66 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Slug');
    IF @var66 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var66 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Slug] nvarchar(120) NOT NULL;
    CREATE UNIQUE INDEX [IX_Branches_TenantId_Slug] ON [Branches] ([TenantId], [Slug]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var67 sysname;
    SELECT @var67 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Phone');
    IF @var67 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var67 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Phone] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var68 sysname;
    SELECT @var68 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'OpeningHours');
    IF @var68 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var68 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [OpeningHours] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var69 sysname;
    SELECT @var69 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'NameEn');
    IF @var69 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var69 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [NameEn] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var70 sysname;
    SELECT @var70 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'NameAr');
    IF @var70 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var70 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [NameAr] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_Branches_TenantId_Name] ON [Branches];
    DECLARE @var71 sysname;
    SELECT @var71 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Name');
    IF @var71 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var71 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Name] nvarchar(160) NOT NULL;
    CREATE INDEX [IX_Branches_TenantId_Name] ON [Branches] ([TenantId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var72 sysname;
    SELECT @var72 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Branches]') AND [c].[name] = N'Address');
    IF @var72 IS NOT NULL EXEC(N'ALTER TABLE [Branches] DROP CONSTRAINT [' + @var72 + '];');
    ALTER TABLE [Branches] ALTER COLUMN [Address] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var73 sysname;
    SELECT @var73 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'EntityType');
    IF @var73 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var73 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [EntityType] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var74 sysname;
    SELECT @var74 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'ActorDisplayName');
    IF @var74 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var74 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [ActorDisplayName] nvarchar(160) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var75 sysname;
    SELECT @var75 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AuditLogs]') AND [c].[name] = N'Action');
    IF @var75 IS NOT NULL EXEC(N'ALTER TABLE [AuditLogs] DROP CONSTRAINT [' + @var75 + '];');
    ALTER TABLE [AuditLogs] ALTER COLUMN [Action] nvarchar(120) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DROP INDEX [IX_AnalyticsEvents_TenantId_EventType_CreatedAtUtc] ON [AnalyticsEvents];
    DECLARE @var76 sysname;
    SELECT @var76 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnalyticsEvents]') AND [c].[name] = N'EventType');
    IF @var76 IS NOT NULL EXEC(N'ALTER TABLE [AnalyticsEvents] DROP CONSTRAINT [' + @var76 + '];');
    ALTER TABLE [AnalyticsEvents] ALTER COLUMN [EventType] nvarchar(32) NOT NULL;
    CREATE INDEX [IX_AnalyticsEvents_TenantId_EventType_CreatedAtUtc] ON [AnalyticsEvents] ([TenantId], [EventType], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260814235803_ConstrainOperationalStrings'
)
BEGIN
    DECLARE @var77 sysname;
    SELECT @var77 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AnalyticsEvents]') AND [c].[name] = N'Device');
    IF @var77 IS NOT NULL EXEC(N'ALTER TABLE [AnalyticsEvents] DROP CONSTRAINT [' + @var77 + '];');
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

