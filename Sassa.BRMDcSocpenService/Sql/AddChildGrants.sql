DECLARE
    CURSOR cur_src IS
        SELECT DISTINCT
            c.PENSION_NO AS BENEFICIARY_ID,
            c.ID_NO AS CHILD_ID,
            B.NAME_EXT,
            B.SURNAME_EXT,
            C.GRANT_TYPE AS GRANT_TYPE,
            R.REGION_CODE AS REGION_ID,
            C.APPLICATION_DATE AS APPLICATION_DATE,
            C.STATUS_CODE,
            B.secondary_paypoint AS PAYPOINT  
        FROM SASSA.socpen_personal_grants a
        JOIN SASSA.SOCPEN_PERSONAL B ON LPAD(A.PENSION_NO, 13, '0') = LPAD(b.PENSION_NO, 13, '0')
        INNER JOIN VW_P12_CHILDREN c ON c.PENSION_NO = LPAD(A.PENSION_NO, 13, '0')
        LEFT JOIN SASSA.CUST_RESCODES R ON B.secondary_paypoint = R.RES_CODE
        AND C.APPLICATION_DATE > TO_DATE('2012-12-31', 'YYYY-MM-DD')
        WHERE NOT EXISTS(
            SELECT 1
            FROM DC_SOCPEN d
            WHERE d.BENEFICIARY_ID = c.PENSION_NO
              AND d.GRANT_TYPE = c.GRANT_TYPE
              AND d.CHILD_ID = c.ID_NO
              AND d.SRD_NO IS NULL);
BEGIN
    FOR rec IN cur_src LOOP
        BEGIN
            BEGIN
                INSERT INTO DC_SOCPEN (
                    BENEFICIARY_ID, CHILD_ID, GRANT_TYPE, SRD_NO, NAME, SURNAME, REGION_ID, APPLICATION_DATE, STATUS_CODE, PAYPOINT
                ) VALUES (
                    rec.BENEFICIARY_ID, rec.CHILD_ID, rec.GRANT_TYPE,null, rec.NAME_EXT, rec.SURNAME_EXT, rec.REGION_ID, rec.APPLICATION_DATE, rec.STATUS_CODE, rec.PAYPOINT
                );
                commit;
            EXCEPTION
            WHEN OTHERS THEN 
                NULL;
            END; 
        END;
    END LOOP;
END;