--UPDATE CHILD GRANTS
DECLARE
    CURSOR cur_src IS
        SELECT 
            c.PENSION_NO AS BENEFICIARY_ID,
            c.ID_NO AS CHILD_ID,
            C.GRANT_TYPE AS GRANT_TYPE
        FROM SASSA_ARCHIVE.socpen_personal_grants_archive a
        JOIN SASSA_ARCHIVE.SOCPEN_PERSONAL_ARCHIVE B ON LPAD(A.PENSION_NO, 13, '0') = LPAD(b.PENSION_NO, 13, '0')
        INNER JOIN VW_P12_CHILDREN c ON c.PENSION_NO = LPAD(A.PENSION_NO, 13, '0')
        WHERE EXISTS(
            SELECT 1
            FROM DC_SOCPEN d
            WHERE d.BENEFICIARY_ID = c.PENSION_NO
              AND d.GRANT_TYPE = c.GRANT_TYPE
              AND d.CHILD_ID = c.ID_NO
              AND d.SRD_NO IS NULL
              AND d.IS_SOCPEN = 0);
       --AND C.APPLICATION_DATE > add_months( trunc(sysdate), -12*3 ); --TO_DATE('2012-12-31', 'YYYY-MM-DD');      
BEGIN
    FOR rec IN cur_src LOOP
        BEGIN
            BEGIN
            UPDATE DC_SOCPEN d
            SET d.IS_SOCPEN = 1
            WHERE d.BENEFICIARY_ID = rec.BENEFICIARY_ID
              AND d.GRANT_TYPE = rec.GRANT_TYPE
              AND d.CHILD_ID = rec.CHILD_ID
              AND d.SRD_NO IS NULL
              AND d.IS_SOCPEN = 0;
            -- Optional: commit here if needed per record
            COMMIT;

            EXCEPTION
            WHEN OTHERS THEN 
                NULL;
            END; 
        END;
    END LOOP;
END;

--UPDATE MAIN GRANTS
DECLARE
    CURSOR cur_src IS
        SELECT 
            LPAD(A.PENSION_NO, 13, '0') AS BENEFICIARY_ID,
            A.GRANT_TYPE,
            NULL AS CHILD_ID,
            NULL AS SRD_NO
        FROM SASSA_ARCHIVE.SOCPEN_PERSONAL_GRANTS_ARCHIVE A
        INNER JOIN SASSA_ARCHIVE.SOCPEN_PERSONAL_ARCHIVE B ON A.PENSION_NO = B.PENSION_NO
        WHERE A.GRANT_TYPE IN ('0', '1','3', '4', '7', '8','6')                                  --Any Grant
        --AND A.APPLICATION_DATE > TO_DATE('2012-12-31', 'YYYY-MM-DD')
        AND EXISTS(
                    SELECT 1
            FROM DC_SOCPEN d
            WHERE d.BENEFICIARY_ID = LPAD(A.PENSION_NO, 13, '0')
              AND d.GRANT_TYPE = a.GRANT_TYPE
              AND d.CHILD_ID IS NULL
              AND d.SRD_NO IS NULL
              AND d.IS_SOCPEN = 0
              );
BEGIN
    FOR src IN cur_src LOOP
       BEGIN
            UPDATE DC_SOCPEN d
            SET d.IS_SOCPEN = 1
            WHERE d.BENEFICIARY_ID = src.BENEFICIARY_ID
              AND d.GRANT_TYPE = src.GRANT_TYPE
              AND d.CHILD_ID IS NULL
              AND d.SRD_NO IS NULL
              AND d.IS_SOCPEN = 0;

            -- Optional: COMMIT here if needed per record
            COMMIT;

        EXCEPTION
        WHEN OTHERS THEN 
            NULL;
        END; 
    END LOOP;
END;