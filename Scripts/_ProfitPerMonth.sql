USE [BitstampTrader]
GO

/****** Object:  View [dbo].[ProfitPerMonth]    Script Date: 17/10/2017 8:36:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[ProfitPerMonth] AS
	SELECT CONVERT(varchar, YEAR(selltimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(selltimestamp)),2) AS [Month], 
		   SUM(buyvalue + sellvalue - buyfee - sellfee) AS Profit  
	FROM [order] 
	WHERE selltimestamp IS NOT NULL
	GROUP BY CONVERT(varchar, YEAR(selltimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(selltimestamp)),2)



GO


