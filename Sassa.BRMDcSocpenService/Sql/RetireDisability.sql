DECLARE 
    CURSOR c1 IS SELECT beneficiary_id,CAPTURE_REFERENCE,CAPTURE_DATE,SCAN_DATE,SOCPEN_DATE,CS_DATE,TDW_REC FROM DC_SOCPEN WHERE grant_type = '0' AND status_code = 'ACTIVE';
BEGIN
    FOR rec IN c1 LOOP
    --TRANSFER THE disability DOC STATUS TO THE NEW OLD AGE RECORD (UNTESTED)
--MERGE INTO DC_Socpen target
--USING (
--    SELECT source.beneficiary_id, source.CAPTURE_REFERENCE, source.CAPTURE_DATE, 
--           source.SCAN_DATE, source.SOCPEN_DATE, source.CS_DATE, source.TDW_REC
--    FROM DC_Socpen source
--    JOIN DC_SOCPEN s ON source.beneficiary_id = s.beneficiary_id
--    WHERE source.grant_type = '3' 
--    AND source.status_code = 'INACTIVE'
--) source_data
--ON (target.beneficiary_id = source_data.beneficiary_id AND target.grant_type = '0' AND target.status_code = 'ACTIVE' AND ROWNUM =1)
--WHEN MATCHED THEN
--UPDATE SET target.CAPTURE_REFERENCE = NVL(target.CAPTURE_REFERENCE,source_data.CAPTURE_REFERENCE),
--           target.CAPTURE_DATE = NVL(target.CAPTURE_DATE,source_data.CAPTURE_DATE),
--           target.SCAN_DATE = NVL(target.SCAN_DATE,source_data.SCAN_DATE),
--           target.SOCPEN_DATE = NVL(target.SOCPEN_DATE,source_data.SOCPEN_DATE),
--           target.CS_DATE = NVL(target.CS_DATE,source_data.CS_DATE),
--           target.TDW_REC = NVL(target.TDW_REC,source_data.TDW_REC);

        DELETE FROM DC_SOCPEN 
        WHERE grant_type = '3' 
        AND status_code = 'INACTIVE'
        AND beneficiary_id = rec.beneficiary_id;
        COMMIT; -- Commit periodically to avoid undo log overflow
    END LOOP;
END;
