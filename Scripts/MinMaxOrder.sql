/****** Object:  Table [dbo].[MinMaxLog]    Script Date: 13/09/2017 18:27:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MinMaxLog](
	[Day] [date] NOT NULL,
	[Minimum] [decimal](8, 2) NOT NULL,
	[Maximum] [decimal](8, 2) NOT NULL,
 CONSTRAINT [PK_MinimumMaximumLog] PRIMARY KEY CLUSTERED 
(
	[Day] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO
