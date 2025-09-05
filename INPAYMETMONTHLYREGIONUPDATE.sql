--UPDATE inpaymentmonthly region
MERGE INTO inpaymentmonthly i
USING (
  SELECT p.pension_no AS applicant_no,
         MAX(c.region_code) AS region_code
  FROM sassa.socpen_personal p
  JOIN sassa.cust_rescodes c
    ON c.res_code = p.secondary_paypoint
  GROUP BY p.pension_no
) src
ON (i.applicant_no = src.applicant_no)
WHEN MATCHED THEN
  UPDATE SET i.region_id = src.region_code
  WHERE NVL(i.region_id, '¤') <> src.region_code;