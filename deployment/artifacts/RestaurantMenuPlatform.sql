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

