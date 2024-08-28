INSERT INTO DC_SOCPENPG (ADABAS_ISN_ARCHIVE, BENEFICIARY_ID, CHILD_ID, NAME, SURNAME,  GRANT_TYPE, REGION_ID, APPLICATION_DATE, APPROVAL_DATE, STATUS_DATE, STATUS_CODE, UNIQUE_ID,PAYPOINT) 
SELECT 
    A.ADABAS_ISN AS ADABAS_ISN_ARCHIVE,
    LPAD(A.PENSION_NO,13,0) AS BENEFICIARY_ID,
    C.ID_NO AS CHILD_ID,
    B.NAME_EXT,
    B.SURNAME_EXT,
    A.GRANT_TYPE,
    D.REGION_CODE AS REGION_ID,
    NVL(A.APPLICATION_DATE,C.APPLICATION_DATE) AS APPLICATION_DATE,
    C.APPROVAL_DATE,
    C.STATUS_DATE,
    CASE
        WHEN C.STATUS_CODE = '1' OR (A.PRIM_STATUS IN ('B','A','9') AND A.SEC_STATUS IN ('2')) THEN 'ACTIVE'
        ELSE 'INACTIVE'
    END AS STATUS_CODE,
    NULL AS UNIQUE_ID,
    B.secondary_paypoint as paypoint  
FROM sassa_archive.socpen_personal_grants_archive A
JOIN sassa_archive.SOCPEN_PERSONAL_ARCHIVE B ON  A.PENSION_NO = B.PENSION_NO
LEFT JOIN sassa_archive.SOCPEN_P12_CHILDREN_ARCHIVE C ON A.PENSION_NO = c.pension_no AND A.GRANT_TYPE = C.GRANT_TYPE
LEFT JOIN sassa.cust_rescodes D ON b.secondary_paypoint = d.res_code;

commit;

INSERT INTO DC_SOCPENPG (ADABAS_ISN_MAIN, BENEFICIARY_ID, CHILD_ID, NAME, SURNAME,  GRANT_TYPE, REGION_ID, APPLICATION_DATE, APPROVAL_DATE, STATUS_DATE, STATUS_CODE, UNIQUE_ID,PAYPOINT) 
SELECT 
    A.ADABAS_ISN AS ADABAS_ISN_MAIN,
    LPAD(A.PENSION_NO,13,0) AS BENEFICIARY_ID,
    C.ID_NO AS CHILD_ID,
    B.NAME_EXT,
    B.SURNAME_EXT,
    A.GRANT_TYPE,
    D.REGION_CODE AS REGION_ID,
    NVL(A.APPLICATION_DATE,C.APPLICATION_DATE) AS APPLICATION_DATE,
    C.APPROVAL_DATE,
    C.STATUS_DATE,
    CASE
        WHEN C.STATUS_CODE = '1' OR (A.PRIM_STATUS IN ('B','A','9') AND A.SEC_STATUS IN ('2')) THEN 'ACTIVE'
        ELSE 'INACTIVE'
    END AS STATUS_CODE,
    NULL AS UNIQUE_ID,
    B.secondary_paypoint as paypoint  
FROM sassa.socpen_personal_grants A
JOIN SOCPEN_PERSONAL B ON  A.PENSION_NO = B.PENSION_NO
LEFT JOIN SOCPEN_P12_CHILDREN C ON A.PENSION_NO = c.pension_no AND A.GRANT_TYPE = C.GRANT_TYPE
LEFT JOIN sassa.cust_rescodes D ON b.secondary_paypoint = d.res_code;

commit;

WHERE A.Application_date >= to_date('{bookmark.ToString("dd/MMM/yyyy")}') 
AND not exists (SELECT E.ADABAS_ISN_MAIN FROM DC_SOCPEN E WHERE E.ADABAS_ISN_MAIN = A.ADABAS_ISN)

select count(*) from dc_socpenPG --40330597
where status_code ='ACTIVE' --22886591

select count(*) from dc_socpen --23660800
where status_code ='ACTIVE' --16185932

select count(*) from dc_file where office_id is null --357

update dc_file f set (office_id)
=(select office_id from dc_local_office k
where k.region_id = f.region_id and k.office_type ='RMC')
where office_id is null


DELETE dc_file f where applicant_no is null 
and not exists (select filefolder_code from tdw_file_location where filefolder_code = f.brm_barcode);
commit;

select count(*) from dc_file f where applicant_no is null --201043
and exists (select filefolder_code from tdw_file_location where filefolder_code = f.brm_barcode);


DELETE 
select * from dc_file f where 
exists(
SELECT 
    brm_barcode
FROM 
    dc_file ff
    where ff.brm_barcode = f.brm_barcode
GROUP BY 
    brm_barcode
HAVING COUNT(*) > 1)
and application_status = 'DESTROY';
commit;


update dc_file
set applicant_no


UPDATE
  dc_file
SET
  application_status = REPLACE( application_status, 'ACTIVE', 'MAIN' );
  
  commit;
  
  UPDATE
  dc_file
SET
  application_status = REPLACE( application_status, 'INACTIVE', 'ARCHIVE' );
  
  commit;
 
 UPDATE
  dc_batch
SET
  reg_type = REPLACE(  reg_type, 'ACTIVE', 'MAIN' );
  
  commit;
  
  UPDATE
  dc_batch
SET
  reg_type = REPLACE(  reg_type, 'INACTIVE', 'ARCHIVE' );
  
  commit; 
  
  
  select * from dc_batch
  
  select distinct REG_TYPE from dc_batch
  
  select * from dc_socpen where beneficiary_id ='7508040710084'
  


UPDATE DC_SOCPEN d SET (NAME, SURNAME) 
= (SELECT 
    A.NAME_EXT,
    A.SURNAME_EXT
FROM sassa.socpen_personal A
where  A.PENSION_NO = d.BENEFICIARY_ID);
commit;


select * from sassa.socpen_personal
select * from mis_livelink_file where id_number = '8810020550087'

select * from mis_livelink_tbl

select * from ss_application where id_number = '8810020550087'


SELECT count(*)
--w.id_number
from WC_exc w
join dc_destruction_list_temp d on w.pension_no = d.pension_no
where d.pension_no is null

--Total 46199
--Not in destruction list 38963
--In Destruction list

Update wc_exc w
SET Socpen_Status = 'ACTIVE'
where exists (SELECT STATUS_CODE from dc_SOCPENPG t where w.pension_no = t.beneficiary_id and status_code = 'ACTIVE' )

selec
update wc_exc set Pension_no = to_char(id_number)

commit;

select * from wc_exc where close_reason = 'DESTROY' and SOCPEN_STATUS = 'ACTIVE'


SELECT count(*) from tdw_file_location t
join wc_exc w on w.pension_no = t.description


Select * from sassa.socpen_dow_applications_chec01 where id_no = '6407220276084'
select * from sassa.socpen_personal_grants where pension_no = '6407220276084'
Select Count(*) from sassa.socpen_dow_applications_chec01 where application_no is null --26908
Select Count(*) from sassa_archive.socpen_dow_appl_chec01_archive where application_date is null --127


select * from dc_file where tdw_boxno = 'TDW001'
select * from dc_file where brm_barcode = 'BRM00002'

Select Count(*) from sassa.socpen_personal_grants where application_date is null --1209642


Select Count(*) from sassa.socpen_dow_applications_chec01 where application_no is null AND application_date is null --19962


select DISTINCT DI.DOC_NO_IN AS IdString
                            from SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01 DAC
                            LEFT JOIN SASSA.SOCPEN_DOCUMENTS_IN DI ON DI.ADABAS_ISN = DAC.ADABAS_ISN
                            LEFT JOIN SASSA.SOCPEN_DOC_REL_IN DRI ON DRI.ADABAS_ISN = DI.ADABAS_ISN AND DRI.DPS_PE_SEQ = DI.DPS_PE_SEQ
                            where ID_NO = '7803060780087'
                            and GRANT_TYPE = 'S'
                            and DPS_MU_SEQ = '001'
                            and DOC_REL_IN = 'Y' 
                            AND APPLICATION_DATE = '09/MAR/09'
                            
                            
                            
update DC_SOCPEN
set DC_SOCPEN.Application_date = C.APPLICATION_DATE
from  DC_SOCPEN
JOIN SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01 C on DC_SOCPEN.ADABAS_ISN_MAIN = C.adabas_isn;
commit;          

UPDATE 
(SELECT DC_SOCPEN.APPLICATION_DATE as OLD, SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01.APPLICATION_DATE as NEW
 FROM DC_SOCPEN
 INNER JOIN SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01
 ON DC_SOCPEN.ADABAS_ISN_MAIN = SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01.ADABAS_ISN
 WHERE SASSA.SOCPEN_DOW_APPLICATIONS_CHEC01.APPLICATION_DATE is not null
) t
SET t.OLD = t.NEW;
commit;


update DC_SOCPEN B  
set B.APPLICATION_DATE 
= (SELECT A.APPLICATION_DATE FROM VW_MAX_SRD_TYPE A 
WHERE B.SRD_NO = A.SRD_NO);
commit;


                                AND A.Application_date >= to_date('{bookmark.ToString("dd/MMM/yyyy")}')";
                                
                                
                                7INSERT INTO DC_SOCPEN (UNIQUE_ID, ADABAS_ISN_SRD,SRD_NO,BENEFICIARY_ID,NAME,SURNAME,GENDER,GENDER_DESC,GRANT_TYPE,REGION_ID,
                            APPLICATION_DATE,APPROVAL_DATE,STATUS_CODE,PAYPOINT)
                            SELECT
                                A.UNIQUE_ID,
                                A.ADABAS_ISN,
                                A.SRD_NO,
                                A.BENEFICIARY_ID,
                                A.NAME,
                                A.SURNAME,
                                A.GENDER,
                                A.GENDER_DESC,
                                'S' AS GRANT_TYPE,
                                A.PROVINCE AS REGION_ID,
                                A.APPLICATION_DATE,
                                A.APPROVAL_DATE,
                                A.STATUS_CODE,
                                A.PAYPOINT
                            FROM
                                VW_MAX_SRD_TYPE A
                            WHERE
                                NOT EXISTS (SELECT B.SRD_NO FROM DC_SOCPEN B WHERE B.SRD_NO = A.SRD_NO)
                                AND A.Application_date >= to_date('{bookmark.ToString("dd/MMM/yyyy")}')";
                                
                                
                                
update 
(select DC_SOCPEN.APPLICATION_DATE as OLD, VW_MAX_SRD_TYPE.Application_date AS NEW
 FROM DC_SOCPEN
 INNER JOIN VW_MAX_SRD_TYPE ON VW_MAX_SRD_TYPE.SRD_NO = DC_SOCPEN.SRD_NO
where DC_SOCPEN.srd_no is not null) t
SET t.OLD = t.NEW;
commit;       


---bulk update from tdw data for kamo
select
f.filefolder_altcode AS UNQ_FILE_NO,
Office_ID,
Region_ID,
f.description as Applicant_no,
Grant_type,
'0' as TRANS_Type,
UPDATED_DATE,
USER_FIRSTNAME,
USER_LASTNAME,
'MAIN' AS APPLICATION_STATUS,
BRM_BARCODE,
f.Container_code as TDW_BOXNO
select count(*) from tdw_file_location f
left join sassa.socpen_dow_applications_chec01 c on f.description = c.id_no and f.Grant_type = c.grant_type
where Not exists (Select d.brm_barcode from dc_file d where d.brm_barcode = f.filefolder_CODE )
and LENgth( f.filefolder_code) = 8 
and LENgth( f.filefolder_altcode) = 12
and c.adabas_isn is not null


update DC_SOCPEN B  
set B.APPLICATION_DATE 
= (SELECT A.APPLICATION_DATE FROM VW_MAX_SRD_TYPE A 
WHERE B.SRD_NO = A.SRD_NO and  A.APPLICATION_DATE  >= to_date('01/JAN/2022') and A.APPLICATION_DATE  <=  to_date('05/Jan/2022'))
where b.srd_no is not null;
commit;


select * from dc_file where tdw_boxno = 'BOX002233'
select * from dc_file where brm_barcode = 'CJ792878'

 select * from dc_file where tdw_boxno = '3618286'
  select * from dc_file where batch_no = '481691'