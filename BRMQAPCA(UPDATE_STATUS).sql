update dc_socpen x
set x.status_code = 
(
SELECT CASE WHEN A.PRIM_STATUS IN ('B','A','9') AND A.SEC_STATUS IN ('2') THEN 'ACTIVE' ELSE 'INACTIVE' END from sassa.socpen_personal_grants A
where LPAD(A.PENSION_NO,13,0) = x.Beneficiary_id
and A.GRANT_TYPE = x.grant_type
and rownum = 1
)
where exists


select count(*) from dc_socpen where status_code = 'ACTIVE' --21806459