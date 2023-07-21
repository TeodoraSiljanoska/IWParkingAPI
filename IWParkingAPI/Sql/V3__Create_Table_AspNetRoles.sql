CREATE TABLE [AspNetRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(256) NOT NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
