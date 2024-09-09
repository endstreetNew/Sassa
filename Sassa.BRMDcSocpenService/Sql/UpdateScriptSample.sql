Update DC_SOCPEN s SET 
(CAPTURE_Reference,capture_date)  
=
(
    SELECT f.UNQ_FILE_NO,f.Updated_date
    FROM DC_File f
    WHERE f.APPLICANT_NO =  s.beneficiary_id
    AND f.GRANT_TYPE = s.Grant_TYPE
    AND f.CHILD_ID_NO = s.CHILD_ID -- REMOVE FOR NON CHILD GRANT
    AND ROWNUM = 1
    and unq_File_NO not in (select capture_reference from dc_socpen)
)
WHERE s.Grant_TYPE = ''
and capture_reference is null;
commit;