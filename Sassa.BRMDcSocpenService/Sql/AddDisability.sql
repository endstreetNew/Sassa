-- NO semicolons(;) at the end of the SQL statements in this file!
DECLARE
    CURSOR cur_src IS
        SELECT 
            LPAD(A.PENSION_NO, 13, '0') AS BENEFICIARY_ID,
            A.GRANT_TYPE,
            NULL AS CHILD_ID,
            NULL AS SRD_NO,
            B.NAME_EXT,
            B.SURNAME_EXT,
            D.REGION_CODE AS REGION_ID,
            A.APPLICATION_DATE,
            CASE 
                WHEN A.PRIM_STATUS IN ('B', 'A', '9') AND A.SEC_STATUS = '2' THEN 'ACTIVE' 
                ELSE 'INACTIVE' 
            END AS STATUS_CODE,
            B.SECONDARY_PAYPOINT AS PAYPOINT,
            ROW_NUMBER() OVER (PARTITION BY A.PENSION_NO, A.GRANT_TYPE ORDER BY A.APPLICATION_DATE DESC) AS rn
        FROM SASSA.SOCPEN_PERSONAL_GRANTS A
        INNER JOIN SASSA.SOCPEN_PERSONAL B ON A.PENSION_NO = B.PENSION_NO
        LEFT JOIN SASSA.CUST_RESCODES D ON B.SECONDARY_PAYPOINT = D.RES_CODE
        WHERE GRANT_TYPE = '3' AND A.PRIM_STATUS IN ('B', 'A', '9') AND A.SEC_STATUS = '2') --Active disability
             OR (GRANT_TYPE = '3' AND NOT EXISTS(SELECT 1                                 --Disability where no Old age grant exists
            FROM DC_SOCPEN d
            WHERE d.BENEFICIARY_ID = LPAD(A.PENSION_NO, 13, '0')
              AND d.GRANT_TYPE = '0')
              )
        AND A.APPLICATION_DATE > TO_DATE('2012-12-31', 'YYYY-MM-DD')
        AND NOT EXISTS(
                    SELECT 1
            FROM DC_SOCPEN d
            WHERE d.BENEFICIARY_ID = LPAD(A.PENSION_NO, 13, '0')
              AND d.GRANT_TYPE = a.GRANT_TYPE
              AND d.CHILD_ID IS NULL
              AND d.SRD_NO IS NULL
              );
BEGIN
    FOR src IN cur_src LOOP
       BEGIN
            INSERT INTO DC_SOCPEN(
                BENEFICIARY_ID, 
                CHILD_ID, 
                NAME, 
                SURNAME, 
                GRANT_TYPE, 
                REGION_ID, 
                APPLICATION_DATE, 
                STATUS_CODE, 
                PAYPOINT
            )
            VALUES (
                src.BENEFICIARY_ID, 
                NULL, 
                src.NAME_EXT, 
                src.SURNAME_EXT, 
                src.GRANT_TYPE, 
                src.REGION_ID, 
                src.APPLICATION_DATE, 
                src.STATUS_CODE, 
                src.PAYPOINT);
            commit;
        EXCEPTION
        WHEN OTHERS THEN 
            NULL;
        END; 
    END LOOP;
END;
