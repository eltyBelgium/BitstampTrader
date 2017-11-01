USE [BitstampTrader]
GO

/****** Object:  View [dbo].[VolumePerMonth]    Script Date: 17/10/2017 8:37:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[VolumePerMonth] AS
	WITH buyset AS(
		SELECT CONVERT(varchar, YEAR(buytimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(buytimestamp)),2) AS [Month], ABS(SUM(buyvalue)) AS BoughtUSD, SUM(buyamount) AS BoughtBTC from [order] 
		WHERE buytimestamp IS NOT NULL
		GROUP BY CONVERT(varchar, YEAR(buytimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(buytimestamp)),2)
	), sellset AS(
		SELECT CONVERT(varchar, YEAR(selltimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(selltimestamp)),2) AS [Month], SUM(sellvalue) AS SoldUSD, SUM(sellamount) AS SoldBTC from [order] 
		WHERE selltimestamp IS NOT NULL
		GROUP BY CONVERT(varchar, YEAR(selltimestamp)) + '-' +  RIGHT('00' + CONVERT(varchar, MONTH(selltimestamp)),2)
	)
	SELECT buyset.[Month], BoughtUSD, BoughtBTC, SoldUSD, SoldBTC FROM buyset
	LEFT JOIN sellset ON sellset.[Month] = buyset.[Month]

GO


