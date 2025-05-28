--1. Invalidate Active grants.
DECLARE
    CURSOR c1 IS SELECT ROWID FROM DC_SOCPEN WHERE STATUS_CODE = 'ACTIVE' AND Grant_type <> 'S';
BEGIN
    FOR rec IN c1 LOOP
        UPDATE DC_SOCPEN 
        SET STATUS_CODE = 'INACTIVE' 
        WHERE ROWID = rec.ROWID;
    END LOOP;
END;

COMMIT;

--2. Set new Active grants from In-Payment Table
UPDATE DC_SOCPEN s
SET STATUS_CODE = 'ACTIVE'
WHERE EXISTS (
    SELECT 1 FROM DC_PAYMENT p 
    WHERE p.ID_NUMBER = s.beneficiary_id
    AND p.GRANT_TYPE = s.GRANT_TYPE
);
COMMIT;

--3. Migrate “INACTIVE” DG capture/scan data for each existing  OAG

delete from dc_socpen p
where p.grant_type ='3'
and status_code = 'INACTIVE'
and p.beneficiary_id in(SELECT beneficiary_id from dc_socpen where beneficiary_id = p.beneficiary_id and status_code = 'ACTIVE' and Grant_Type = '0');
commit;