-- Insert last months payments intp the DC_PAYMENT table.
UPDATE CUST_PAYMENT SET HOMING_ACC_NAME = REPLACE(HOMING_ACC_NAME, '(QQ) ', '');
commit;
--manually check the count/records are for the correct payment period
TRUNCATE TABLE DC_PAYMENT;
commit;
INSERT INTO DC_PAYMENT
SELECT * FROM cust_payment
Where ACTION_DATE between sysdate - 30 and sysdate;
commit;

--set records INACTIVE
DECLARE
    TYPE rid_array IS TABLE OF ROWID INDEX BY PLS_INTEGER;
    v_rids rid_array;
BEGIN
    -- Collect all matching ROWIDs
    SELECT ROWID BULK COLLECT INTO v_rids
    FROM DC_SOCPEN
    WHERE STATUS_CODE = 'ACTIVE';

    -- Bulk update using FORALL
    FORALL i IN v_rids.FIRST .. v_rids.LAST
        UPDATE DC_SOCPEN
        SET STATUS_CODE = 'INACTIVE'
        WHERE ROWID = v_rids(i);

    COMMIT;
END;


--set new records ACTIVE
BEGIN
    UPDATE DC_SOCPEN sp
    SET STATUS_CODE = 'ACTIVE'
    WHERE EXISTS (
        SELECT 1
        FROM DC_PAYMENT dp
        WHERE dp.id_number = sp.beneficiary_id
          AND dp.GRANT_TYPE = sp.GRANT_TYPE
    );

    COMMIT;
END;


--merge old agestats to disability records
DECLARE 
    CURSOR c1 IS SELECT beneficiary_id, CAPTURE_REFERENCE, CAPTURE_DATE, SCAN_DATE, SOCPEN_DATE, CS_DATE, TDW_REC FROM DC_Socpen s 
    where grant_type = '0' 
    and exists (SELECT 1 FROM DC_Socpen where beneficiary_id = s.beneficiary_id AND grant_type = '3');
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
END;
-- delete orphaned disability records
-- This will delete records in DC_SOCPEN where grant_type is '3' and status is inactive
DECLARE 
    CURSOR c1 IS SELECT beneficiary_id FROM DC_SOCPEN WHERE grant_type = '0' AND status_code = 'ACTIVE';
BEGIN
    FOR rec IN c1 LOOP
        DELETE FROM DC_SOCPEN 
        WHERE grant_type = '3' 
        AND status_code = 'INACTIVE'
        AND beneficiary_id = rec.beneficiary_id;
        -- Also delete disability records from DC_FILE
        --Delete from dc_file
        --WHERE beneficiary_id = rec.beneficiary_id and grant_type = '3';
        COMMIT; -- Commit periodically to avoid undo log overflow
    END LOOP;
END;