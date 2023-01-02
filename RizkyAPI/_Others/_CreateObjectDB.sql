USE [TestDB]
GO

BEGIN TRY
BEGIN TRANSACTION
        
/****** Object:  Table [dbo].[tblItem] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblItem]') AND type in (N'U'))
DROP TABLE [dbo].[tblItem]

/****** Object:  Table [dbo].[tblSupplier] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblSupplier]') AND type in (N'U'))
DROP TABLE [dbo].[tblSupplier]

/****** Object:  Table [dbo].[tblPO] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblPO]') AND type in (N'U'))
DROP TABLE [dbo].[tblPO]

/****** Object:  Table [dbo].[tblUser] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblUser]') AND type in (N'U'))
DROP TABLE [dbo].[tblUser]

/****** Object:  Table [dbo].[tblRefreshToke] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblRefreshToke]') AND type in (N'U'))
DROP TABLE [dbo].[tblRefreshToke]

/****** Object: PROCEDURE [dbo].[spSetItem] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spSetItem]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spSetItem]

/****** Object: PROCEDURE [dbo].[spSetItem] ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spSetRefreshToken]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spSetRefreshToken]

--=================================================================================================================

--CREATE VIEW [dbo].[vGetListItems] AS SELECT * FROM [dbo].[tblItem]

/****** Object: PROCEDURE [dbo].[spSetItem] ******/
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spSetItem]') AND type in (N'P', N'PC'))
--BEGIN
--EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spSetItem] AS' 
--END
--//==========================================================================
--Alter PROCEDURE [dbo].[spSetItem]
--@Id bigInt,
--@ItemCode varchar(100),
--@ItemName varchar(200),
--@ItemDesc varchar(200)
--AS
--BEGIN
--IF NOT EXISTS (SELECT * FROM [dbo].[tblItem] WHERE Id = @Id) OR (@Id = 0)
--	BEGIN
--	Insert Into tblItem (ItemName, ItemCode, ItemDesc) values (@ItemName, @ItemCode, @ItemDesc);
--	SELECT SCOPE_IDENTITY();
--	END
--ELSE
--	BEGIN
--	Update tblItem Set ItemName=@ItemName, ItemCode=@ItemCode, ItemDesc=@ItemDesc where Id=@Id;
--	SELECT @Id
--	END
--END

/****** Object: PROCEDURE [dbo].[spSetRefreshToken] ******/
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spSetRefreshToken]') AND type in (N'P', N'PC'))
--BEGIN
--EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[spSetRefreshToken] AS' 
--END
--//==========================================================================
--Alter PROCEDURE [dbo].[spSetRefreshToken]
--@UserID bigInt,
--@RefreshToken varchar(100),
--@CreatedDate varchar(200),
--@ExpiredDate varchar(200)
--AS
--BEGIN
--IF NOT EXISTS (SELECT * FROM [dbo].[tblRefreshToken] WHERE UserID = @UserID)
--	BEGIN
--	Insert Into tblRefreshToken (UserID, RefreshToken, CreatedDate, ExpiredDate) values (@UserID, @RefreshToken, @CreatedDate, @ExpiredDate);
--	END
--ELSE
--	BEGIN
--	Update tblRefreshToken Set RefreshToken=@RefreshToken, CreatedDate=@CreatedDate, ExpiredDate=@ExpiredDate where UserID=@UserID;
--	END
--END

/****** Object:  Table [dbo].[tblRefreshToken] ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblRefreshToken]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tblRefreshToken](
	[UserID] [BIGINT] NOT NULL,
	[RefreshToken] [NVARCHAR](100) NOT NULL,
	[CreatedDate] [DATETIME] NOT NULL,
	[ExpiredDate] [DATETIME] NOT NULL,
	[IsUsed] [CHAR](1) NOT NULL
) ON [PRIMARY]
END
SET ANSI_PADDING OFF

/****** Object:  Table [dbo].[tblUser] ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblUser]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tblUser](
	[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	[SureName] [NVARCHAR](50) NOT NULL,
	[UserName] [NVARCHAR](50) NOT NULL,
	[Password] [NVARCHAR](50) NOT NULL,
	[Email] [NVARCHAR](50) NOT NULL,
	[Phone] [NVARCHAR](20) NULL,
	[Role] [NVARCHAR](50) NOT NULL,
	[Note] [NVARCHAR](200) NULL,
	[Status] [NVARCHAR](50) NULL
) ON [PRIMARY]
END
SET ANSI_PADDING OFF

/****** Object:  Table [dbo].[tblItem] ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblItem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tblItem](
	[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	[ItemCode] [NVARCHAR](100) NOT NULL,
	[ItemName] [NVARCHAR](200) NOT NULL,
	[ItemDesc] [NVARCHAR](200) NULL,
 CONSTRAINT [PK_tblItem] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
SET ANSI_PADDING OFF


/****** Object:  Table [dbo].[tblSupplier] ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblSupplier]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tblSupplier](
	[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	[SupplierName] [NVARCHAR](100) NOT NULL,
	[SupplierAddress] [NVARCHAR](500) NULL,
	[SupplierContactPerson] [NVARCHAR](100) NULL,
	[SupplierContactPhone] [NVARCHAR](100) NULL,
 CONSTRAINT [PK_tblSupplier] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
SET ANSI_PADDING OFF


/****** Object:  Table [dbo].[tblPO] ******/
SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
SET ANSI_PADDING ON
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tblPO]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[tblPO](
	[Id] [BIGINT] IDENTITY(1,1) NOT NULL,
	[ItemId] [BIGINT] NOT NULL,
	[SupplierId] [BIGINT] NOT NULL,
	[Qty] [DECIMAL](19, 4) NOT NULL,
	[UnitType] [NVARCHAR](50) NOT NULL,
	[CreatedDate] [DATETIME] NOT NULL,
	[CreatedBy] [NVARCHAR](100) NOT NULL,
 CONSTRAINT [PK_tblPO] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
SET ANSI_PADDING OFF


COMMIT TRAN -- Transaction Success!
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN --RollBack in case of Error

    -- <EDIT>: From SQL2008 on, you must raise error messages as follows:
    DECLARE @ErrorMessage NVARCHAR(4000);  
    DECLARE @ErrorSeverity INT;  
    DECLARE @ErrorState INT;  

    SELECT   
       @ErrorMessage = ERROR_MESSAGE(),  
       @ErrorSeverity = ERROR_SEVERITY(),  
       @ErrorState = ERROR_STATE();  

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);  
    -- </EDIT>
END CATCH

