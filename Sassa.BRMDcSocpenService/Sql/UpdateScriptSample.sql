Update DC_SOCPEN target SET (CAPTURE_Reference,Capture_date,brm_barcode)  
 = (
    SELECT s.UNQ_FILE_NO,updated_date,brm_barcode
    FROM DC_FILE s
    INNER JOIN
    (
        SELECT APPLICANT_NO,GRANT_TYPE
        FROM DC_FILE
        GROUP BY APPLICANT_NO,GRANT_TYPE
        HAVING COUNT(*) = 1 
    ) t
    ON s.APPLICANT_NO =  t.APPLICANT_NO and s.Grant_Type = t.Grant_Type
    AND s.APPLICANT_NO = target.beneficiary_id
    AND s.Grant_TYPE = target.Grant_Type
    --AND to_number(s.srd_no) = target.srd_no
    AND s.CHILD_ID_NO = target.CHILD_ID
 )
 WHERE EXISTS (
    SELECT s.UNQ_FILE_NO
    FROM DC_FILE s
    INNER JOIN
    (
        SELECT APPLICANT_NO,GRANT_TYPE
        FROM DC_FILE
        GROUP BY APPLICANT_NO,GRANT_TYPE
        HAVING COUNT(*) = 1 
    ) t
    ON s.APPLICANT_NO =  t.APPLICANT_NO and s.Grant_Type = t.Grant_Type
    WHERE s.APPLICANT_NO = target.beneficiary_id
    AND s.Grant_TYPE = target.Grant_Type
    --AND to_number(s.srd_no) = target.srd_no
    AND s.CHILD_ID_NO = target.CHILD_ID
)
AND capture_reference is null 
and grant_type in ('5','6','9','C');
commit;