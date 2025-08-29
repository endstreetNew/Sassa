
--START CLEAN
TRUNCATE table inpaymentmonthly

--ADD ARCHIVE RECORDS TODO LEFT JOIN to add missing socpen_personal records
INSERT INTO INPAYMENTMONTLY (APPLICANT_NO,FIRST_NAME,SURNAME, GRANT_TYPE, REGION_ID)
Select ID_NUMBER AS APPLICANT_NO,p.NAME,p.SURNAME, GRANT_TYPE,NVL(r.REGION_code,0)
FROM DC_PAYMENT d
inner join sassa_ARCHIVE.socpen_personal_ARCHIVE p on p.pension_no = d.id_number
left join sassa.cust_rescodes r on r.Res_code = p.Secondary_paypoint

--ADD MAIN RECORDS TODO LEFT JOIN to add missing socpen_personal records
INSERT INTO INPAYMENTMONTHLY (APPLICANT_NO,FIRST_NAME,SURNAME, GRANT_TYPE, REGION_ID)
Select ID_NUMBER AS APPLICANT_NO,p.NAME,p.SURNAME, GRANT_TYPE,NVL(r.REGION_code,0)
FROM DC_PAYMENT d
inner join sassa.socpen_personal p on p.pension_no = d.id_number
left join sassa.cust_rescodes r on r.Res_code = p.Secondary_paypoint

--REPORT MISSING PERSONAL FOR KAMO
--select * from dc_payment p 
--where not exists(select * from sassa.socpen_personal where pension_no = p.id_number)

UPDATE INPAYMENTMONTHLY s
SET ECMIS = 1
WHERE EXISTS (
SELECT ID_NUMBER
    FROM HK_ECMIS h
    WHERE h.id_number = s.applicant_no
);
commit;