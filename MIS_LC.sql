SELECT TOP (1000) l.[LCID]
      ,a.Idnumber
      ,a.FirstName
      ,a.Surname
      ,g.SocpenCode as GrantType
      ,[TypeLCID] AS LCType
      ,l.[ProvinceID] as RegionID
      ,l.[TownID] As officeId
      ,l.[DateCreated]
  FROM [NatLive].[dbo].[LooseCorrespondence] l
  inner join [NatLive].[dbo].[Applicant] a on  a.ApplicantID = l.ApplicantId
  inner join dbo.Application ap on ap.ApplicationId = l.ApplicationID
  inner join typecharacteristic g on ap.CharacteristicID = g.CharacteristicID