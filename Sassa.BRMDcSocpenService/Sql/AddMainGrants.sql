-- NO semicolons(;) at the end of the SQL statements in this file!

--with rn = 1 filter
MERGE INTO DC_SOCPEN d
USING (
    SELECT 
        BENEFICIARY_ID,
        GRANT_TYPE,
        CHILD_ID,
        SRD_NO,
        NAME_EXT,
        SURNAME_EXT,
        REGION_ID,
        APPLICATION_DATE,
        STATUS_CODE,
        PAYPOINT
    FROM (
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
        INNER JOIN SASSA.SOCPEN_PERSONAL B 
            ON A.PENSION_NO = B.PENSION_NO
        INNER JOIN SASSA.CUST_RESCODES D 
            ON B.SECONDARY_PAYPOINT = D.RES_CODE
        WHERE A.GRANT_TYPE IN ('0', '1', '3', '4', '7', '8')
            AND A.APPLICATION_DATE > TO_DATE('2012-12-31', 'YYYY-MM-DD')
    ) t
    WHERE rn = 1
) src
ON (d.BENEFICIARY_ID = src.BENEFICIARY_ID 
    AND d.GRANT_TYPE = src.GRANT_TYPE 
    AND d.CHILD_ID IS NULL 
    AND d.SRD_NO IS NULL)
WHEN NOT MATCHED THEN 
INSERT (
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
    src.CHILD_ID, 
    src.NAME_EXT, 
    src.SURNAME_EXT, 
    src.GRANT_TYPE, 
    src.REGION_ID, 
    src.APPLICATION_DATE, 
    src.STATUS_CODE, 
    src.PAYPOINT
)