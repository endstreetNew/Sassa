--Delete disability grants where the beneficiary has an active old age grant
DECLARE
    CURSOR c1 IS SELECT beneficiary_id,CAPTURE_REFERENCE,CAPTURE_DATE,SCAN_DATE,SOCPEN_DATE,CS_DATE,TDW_REC FROM DC_SOCPEN s
    WHERE grant_type = '0' AND status_code = 'ACTIVE'
    and exists (SELECT 1 FROM DC_Socpen where beneficiary_id = s.beneficiary_id AND grant_type = '3' AND status_code = 'INACTIVE');
BEGIN
    FOR rec IN c1 LOOP
        DELETE FROM DC_SOCPEN 
        WHERE grant_type = '3' 
        AND status_code = 'INACTIVE'
        AND beneficiary_id = rec.beneficiary_id;
        COMMIT; -- Commit periodically to avoid undo log overflow
    END LOOP;
END;
