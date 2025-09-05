CREATE GLOBAL TEMPORARY TABLE tmp_src_region
(
    applicant_no VARCHAR2(13),
    region_id    VARCHAR2(2)
)
ON COMMIT PRESERVE ROWS;

TRUNCATE TABLE tmp_src_region;

INSERT INTO tmp_src_region (applicant_no, region_id)
SELECT p.applicant_no,
       MAX(p.region_id) AS region_id
FROM   dc_file p
JOIN   dc_local_office o 
       ON o.office_id = p.office_id
WHERE  o.office_type = 'RMC'
and length(p.Applicant_no) = 13
and exists (select 1 from inpaymentmonthly i where i.applicant_no = p.applicant_no)
GROUP  BY p.applicant_no;

UPDATE inpaymentmonthly i
SET    i.region_id = (
           SELECT t.region_id
           FROM   tmp_src_region t
           WHERE  t.applicant_no = i.applicant_no
       )
WHERE EXISTS (
           SELECT 1
           FROM   tmp_src_region t
           WHERE  t.applicant_no = i.applicant_no
           AND    NVL(i.region_id, '¤') <> t.region_id
       );