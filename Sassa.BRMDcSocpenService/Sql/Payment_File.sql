--1. Invalidate Active grants.
UPDATE DC_SOCPEN
SET STATUS_CODE = 'INACTIVE'
WHERE STATUS_CODE = 'ACTIVE';

COMMIT;

--2. Set new Active grants from In-Payment Table
UPDATE DC_SOCPEN
SET STATUS_CODE = 'ACTIVE'
WHERE (beneficiary_id,GRANT_TYPE) in (SELECT ID_NUMBER, GRANT_TYPE FROM CUST_PAYMENT)
and APPLICATION_DATE < to_date('01/JUL/2023');
COMMIT;

--3. Migrate “INACTIVE” DG capture/scan data for each existing  OAG

delete from dc_socpen p
where p.grant_type ='3'
and status_code = 'INACTIVE'
and p.beneficiary_id in(SELECT beneficiary_id from dc_socpen where beneficiary_id = p.beneficiary_id and status_code = 'ACTIVE' and Grant_Type = '0');
commit;