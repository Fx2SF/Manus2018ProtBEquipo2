USE [master]
GO
/****** Object:  Database [manus]    Script Date: 17/03/2018 01:11:06 p. m. ******/
CREATE DATABASE [manus]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'manus', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\manus.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'manus_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\manus_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [manus] SET COMPATIBILITY_LEVEL = 130
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [manus].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [manus] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [manus] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [manus] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [manus] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [manus] SET ARITHABORT OFF 
GO
ALTER DATABASE [manus] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [manus] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [manus] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [manus] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [manus] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [manus] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [manus] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [manus] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [manus] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [manus] SET  DISABLE_BROKER 
GO
ALTER DATABASE [manus] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [manus] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [manus] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [manus] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [manus] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [manus] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [manus] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [manus] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [manus] SET  MULTI_USER 
GO
ALTER DATABASE [manus] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [manus] SET DB_CHAINING OFF 
GO
ALTER DATABASE [manus] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [manus] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [manus] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [manus] SET QUERY_STORE = OFF
GO
USE [manus]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [manus]
GO
/****** Object:  Table [dbo].[Cheques]    Script Date: 17/03/2018 01:11:06 p. m. ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cheques](
	[idCheque] [int] IDENTITY(1,1) NOT NULL,
	[imagePath] [varchar](150) NULL,
	[monto] [varchar](150) NULL,
	[cliente] [varchar](150) NULL,
	[montoEscrito] [varchar](150) NULL,
	[serie] [varchar](150) NULL,
	[numero] [varchar](150) NULL,
	[tipoCheque] [varchar](150) NULL,
	[fecha] [varchar](150) NULL,
 CONSTRAINT [PK_Cheque] PRIMARY KEY CLUSTERED 
(
	[idCheque] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [manus] SET  READ_WRITE 
GO
