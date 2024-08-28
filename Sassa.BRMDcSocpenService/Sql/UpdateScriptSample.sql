--all but child grants brm barcode cause duplicate error
Update DC_SOCPEN s SET (CAPTURE_Reference,capture_date,brm_barcode)  
= (
    SELECT f.UNQ_FILE_NO,f.Updated_date,f.brm_barcode
    FROM DC_FILE f
    WHERE f.APPLICANT_NO =  s.beneficiary_id
    AND instr('C59', f.Grant_TYPE) = 0
    AND f.grant_type = s.Grant_TYPE
    AND ROWNUM = 1
)
where s.capture_reference is null
AND instr('C59', s.Grant_TYPE) = 0
and s.STATUS_CODE = 'ACTIVE';
commit;

--child grants brm barcode cause duplicate error
Update DC_SOCPEN s 
SET (CAPTURE_Reference,capture_date,brm_barcode)  
= (
SELECT f.UNQ_FILE_NO,f.Updated_date,f.brm_barcode
FROM DC_FILE f
WHERE f.APPLICANT_NO =  s.beneficiary_id
AND instr('C59', f.Grant_TYPE) > 0 
AND f.CHILD_ID_NO = s.CHILD_ID
AND ROWNUM = 1
)
where s.capture_reference is null
AND instr('C59', s.Grant_TYPE) > 0
and s.STATUS_CODE = 'ACTIVE';
commit;


--WORKED ON 2019-10-30 brm barcode cause duplicate error
Update DC_SOCPEN s SET (CAPTURE_Reference,capture_date)  
= (
    SELECT f.UNQ_FILE_NO,f.Updated_date
    FROM DC_FILE f
    WHERE f.APPLICANT_NO =  s.beneficiary_id
    AND instr('C59', f.Grant_TYPE) = 0
    AND f.grant_type = s.Grant_TYPE
    AND ROWNUM = 1
)
where s.capture_reference is null
AND instr('C59', s.Grant_TYPE) = 0
and s.STATUS_CODE = 'ACTIVE';
commit;

Update DC_SOCPEN s SET (brm_barcode)  
= (
    SELECT f.brm_barcode
    FROM DC_FILE f
    WHERE f.UNQ_FILE_NO =  s.capture_reference
    AND instr('C59', f.Grant_TYPE) = 0
    AND f.grant_type = s.Grant_TYPE
    AND ROWNUM = 1
)
where s.brm_barcode is null
commit;