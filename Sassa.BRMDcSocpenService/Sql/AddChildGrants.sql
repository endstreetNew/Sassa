-- NO semicolons(;) at the end of the SQL statements in this file!
MERGE INTO DC_SOCPEN d
USING (
    SELECT DISTINCT
        c.PENSION_NO AS BENEFICIARY_ID,
        c.ID_NO AS CHILD_ID,
        B.NAME_EXT,
        B.SURNAME_EXT,
        C.GRANT_TYPE AS GRANT_TYPE,
        R.REGION_CODE AS REGION_ID,
        C.APPLICATION_DATE AS APPLICATION_DATE,
        C.STATUS_CODE,
        B.secondary_paypoint AS paypoint  
    FROM SASSA.socpen_personal_grants a
    JOIN SASSA.SOCPEN_PERSONAL B ON  LPAD(A.PENSION_NO, 13, '0') =  LPAD(b.PENSION_NO, 13, '0')
    INNER JOIN VW_P12_CHILDREN c ON c.PENSION_NO =  LPAD(A.PENSION_NO, 13, '0')
    LEFT JOIN SASSA.CUST_RESCODES R ON B.secondary_paypoint = R.RES_CODE
    WHERE C.APPLICATION_DATE > TRUNC(SYSDATE - 30)
) src
ON (d.BENEFICIARY_ID = src.BENEFICIARY_ID AND d.GRANT_TYPE = src.GRANT_TYPE AND d.CHILD_ID = src.CHILD_ID AND d.SRD_NO is null)
WHEN NOT MATCHED THEN 
INSERT (BENEFICIARY_ID, CHILD_ID, NAME, SURNAME, GRANT_TYPE, REGION_ID, APPLICATION_DATE, STATUS_CODE, PAYPOINT)
VALUES (src.BENEFICIARY_ID, src.CHILD_ID, src.NAME_EXT, src.SURNAME_EXT, src.GRANT_TYPE, src.REGION_ID, src.APPLICATION_DATE, src.STATUS_CODE, src.PAYPOINT)

--with cursor

--DECLARE
--    CURSOR cur_src IS
--        SELECT DISTINCT
--            LPAD(c.PENSION_NO, 13, '0') AS BENEFICIARY_ID,
--            c.ID_NO AS CHILD_ID,
--            B.NAME_EXT,
--            B.SURNAME_EXT,
--            C.GRANT_TYPE AS GRANT_TYPE,
--            R.REGION_CODE AS REGION_ID,
--            C.APPLICATION_DATE AS APPLICATION_DATE,
--            C.STATUS_CODE,
--            B.secondary_paypoint AS PAYPOINT  
--        FROM SASSA.socpen_personal_grants a
--        JOIN SASSA.SOCPEN_PERSONAL B ON LPAD(A.PENSION_NO, 13, '0') = LPAD(b.PENSION_NO, 13, '0')
--        INNER JOIN VW_P12_CHILDREN c ON c.PENSION_NO = LPAD(A.PENSION_NO, 13, '0')
--        LEFT JOIN SASSA.CUST_RESCODES R ON B.secondary_paypoint = R.RES_CODE;
--BEGIN
--    FOR rec IN cur_src LOOP
--        -- Check if a matching record exists
--        DECLARE
--            v_count INTEGER;
--        BEGIN
--            SELECT COUNT(*)
--            INTO v_count
--            FROM DC_SOCPEN d
--            WHERE d.BENEFICIARY_ID = rec.BENEFICIARY_ID
--              AND d.GRANT_TYPE = rec.GRANT_TYPE
--              AND d.CHILD_ID = rec.CHILD_ID
--              AND d.SRD_NO IS NULL;

--            IF v_count = 0 THEN
--                INSERT INTO DC_SOCPEN (
--                    BENEFICIARY_ID, CHILD_ID, GRANT_TYPE, SRD_NO, NAME, SURNAME, REGION_ID, APPLICATION_DATE, STATUS_CODE, PAYPOINT
--                ) VALUES (
--                    rec.BENEFICIARY_ID, rec.CHILD_ID, rec.GRANT_TYPE,null, rec.NAME_EXT, rec.SURNAME_EXT, rec.REGION_ID, rec.APPLICATION_DATE, rec.STATUS_CODE, rec.PAYPOINT
--                );
--                commit;
--            END IF;
--        END;
--    END LOOP;
--END