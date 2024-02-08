--CREATE DATABASE [Blog]
--GO

USE [Blog]
GO

-- DROP TABLE [User]
-- DROP TABLE [Role]
-- DROP TABLE [UserRole]
-- DROP TABLE [Post]
-- DROP TABLE [Category]
-- DROP TABLE [Tag]
-- DROP TABLE [PostTag]

CREATE TABLE [User] (
    [Id] INT NOT NULL IDENTITY(1, 1), --PK como int e incrementando de 1 em 1
    [Name] NVARCHAR(80) NOT NULL, --Permite caracteres especiais
    [Email] VARCHAR(200) NOT NULL,
    [PasswordHash] VARCHAR(255) NOT NULL,
    [Bio] TEXT NOT NULL, --TEXT nao possui limites
    [Image] VARCHAR(2000) NOT NULL, --vai receber a URL da imagem
    [Slug] VARCHAR(80) NOT NULL, --é uma url -> ex.: 'site.io/User/nome-sobrenome'

    --colocar aqui permite nomear as regras
    CONSTRAINT [PK_User] PRIMARY KEY([Id]),
    CONSTRAINT [UQ_User_Email] UNIQUE([Email]),
    CONSTRAINT [UQ_User_Slug] UNIQUE([Slug])
)
--cria índices p/ acelerar as consultas
CREATE NONCLUSTERED INDEX [IX_User_Email] ON [User]([Email])
CREATE NONCLUSTERED INDEX [IX_User_Slug] ON [User]([Slug])