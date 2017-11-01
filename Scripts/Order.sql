/****** Object:  Table [dbo].[Order]    Script Date: 13/09/2017 18:27:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Order](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BuyAmount] [decimal](12, 8) NOT NULL,
	[BuyTimestamp] [datetime] NULL,
	[BuyPrice] [decimal](8, 2) NOT NULL,
	[BuyValue] [decimal](8, 2) NULL,
	[BuyFee] [decimal](8, 2) NULL,
	[BuyId] [bigint] NOT NULL,
	[SellAmount] [decimal](12, 8) NULL,
	[SellTimestamp] [datetime] NULL,
	[SellPrice] [decimal](8, 2) NULL,
	[SellValue] [decimal](8, 2) NULL,
	[SellFee] [decimal](8, 2) NULL,
	[SellId] [bigint] NULL,
 CONSTRAINT [PK__Order] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO