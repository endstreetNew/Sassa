--merge old agestats to disability records
DECLARE 
    CURSOR c1 IS SELECT beneficiary_id, CAPTURE_REFERENCE, CAPTURE_DATE, SCAN_DATE, SOCPEN_DATE, CS_DATE, TDW_REC FROM DC_Socpen where grant_type = '0';
BEGIN
    FOR source IN c1 LOOP
           UPDATE DC_SOCPEN target 
           SET target.CAPTURE_REFERENCE = NVL(target.CAPTURE_REFERENCE,source.CAPTURE_REFERENCE),
           target.CAPTURE_DATE = NVL(target.CAPTURE_DATE,source.CAPTURE_DATE),
           target.SCAN_DATE = NVL(target.SCAN_DATE,source.SCAN_DATE),
           target.SOCPEN_DATE = NVL(target.SOCPEN_DATE,source.SOCPEN_DATE),
           target.CS_DATE = NVL(target.CS_DATE,source.CS_DATE),
           target.TDW_REC = NVL(target.TDW_REC,source.TDW_REC)
           WHERE target.beneficiary_id = source.beneficiary_id 
           and target.grant_type = '3';           
        COMMIT; -- Commit periodically to avoid undo log overflow
    END LOOP;
END