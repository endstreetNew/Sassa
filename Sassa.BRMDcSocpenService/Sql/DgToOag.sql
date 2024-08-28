--ALL DG must be converted to Oag if DG and OAG exists
--1 Set all 3 to x where 0 exists
--2 Set tdw_rec where x to tdw_rec where 0 tdw_rec is not null


--Set tdw_rec from Dg to Oag
Update DC_Socpen s
Set (tdw_rec) = (
	SELECT f.tdw_rec
	FROM DC_Socpen f
	WHERE f.Beneficiary_id =  s.Beneficiary_id
	and f.Grant_TYPE = '3'
	and s.tdw_rec is not null
	AND ROWNUM = 1
)
where s.tdw_rec is null

update dc_socpen s
set s.grant_type = '0'
--select * from dc_socpen s
where s.grant_type ='3'
and status_code = 'INACTIVE'
and exists(
Select * from cust_payment p 
where not exists(SELECT * from dc_socpen where p.id_number = s.beneficiary_id and status_code = 'ACTIVE' and Grant_Type = '0'))