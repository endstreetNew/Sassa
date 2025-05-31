-- Insert last months payments intp the DC_PAYMENT table.


DECLARE
    CURSOR c1 IS SELECT ROWID FROM DC_SOCPEN WHERE STATUS_CODE = 'ACTIVE';
BEGIN
    FOR rec IN c1 LOOP
        UPDATE DC_SOCPEN 
        SET STATUS_CODE = 'INACTIVE' 
        WHERE ROWID = rec.ROWID;
    END LOOP;
    COMMIT;
END;

DECLARE 
    CURSOR c1 IS SELECT id_number, GRANT_TYPE FROM DC_PAYMENT;
BEGIN
    FOR rec IN c1 LOOP
        UPDATE DC_SOCPEN
        SET STATUS_CODE = 'ACTIVE'
        WHERE  beneficiary_id = rec.id_number
        AND GRANT_TYPE = rec.GRANT_TYPE;
    END LOOP;
    COMMIT;
END;

DECLARE 
    CURSOR c1 IS SELECT beneficiary_id FROM DC_SOCPEN WHERE grant_type = '0' AND status_code = 'ACTIVE';
BEGIN
    FOR rec IN c1 LOOP
        DELETE FROM DC_SOCPEN 
        WHERE grant_type = '3' 
        AND status_code = 'INACTIVE'
        AND beneficiary_id = rec.beneficiary_id;
        COMMIT; -- Commit periodically to avoid undo log overflow
    END LOOP;
END;