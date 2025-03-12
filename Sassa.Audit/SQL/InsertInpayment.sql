insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.Applicant_no,f.Grant_type,f.region_id,f.user_firstname,f.user_lastname,null,
f.updated_date,f.application_status,f.brm_barcode,f.unq_file_no,f.file_number,null,'NONE',f.office_id
 from dc_file f
 where exists(select 1 from cust_payment p where p.id_number = f.applicant_no and p.grant_type = f.grant_type)

 --Update MIS
  insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,f.region_id,f.name,f.surname,null,
null,substr(f.registry_type,1,10),NULL,NULL,f.file_number,null,'NONE',null
 from MIS_LIVELINK_TBL f
 where exists(select 1 from cust_payment p where p.id_number = f.id_number and p.grant_type = f.grant_type)

 -----
 UPDATE inpayment t1
   SET (MIS_FILE_NO) = (SELECT t2.FILE_NUMBER
                         FROM MIS_LIVELINK_TBL t2
                        WHERE t1.APPLICANT_NO = t2.id_NUMBER and t1.Grant_Type = t2.GRANT_TYPE and ROWNUM =1)
 WHERE EXISTS (
    SELECT 1
      FROM MIS_LIVELINK_TBL t2
    WHERE t1.APPLICANT_NO = t2.id_NUMBER and t1.Grant_Type = t2.GRANT_TYPE)

    --update ecmis
    UPDATE inpayment t1
   SET (MIS_FILE_NO) = (SELECT t2.FORM_TYPE + t2.FORM_NUMBER
                         FROM SS_APPLICATION t2
                        WHERE t1.APPLICANT_NO = t2.id_NUMBER and t1.Grant_Type = t2.GRANT_TYPE and ROWNUM =1)
 WHERE EXISTS (
    SELECT 1
      FROM SS_APPLICATION t2
    WHERE t1.APPLICANT_NO = t2.id_NUMBER and t1.Grant_Type = t2.GRANT_TYPE)


    ---current script
    
insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.Applicant_no,f.Grant_type,f.region_id,f.user_firstname,f.user_lastname,null,
f.updated_date,f.application_status,SUBSTR(f.brm_barcode,1,8),f.unq_file_no,f.file_number,null,'NONE',f.office_id
 from dc_file f
 where exists(select 1 from cust_payment p where p.id_number = f.applicant_no and p.grant_type = f.grant_type)
 
 insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,f.region_id,f.name,f.surname,null,
null,substr(f.registry_type,1,10),NULL,NULL,f.file_number,null,'NONE',null
 from MIS_LIVELINK_TBL f
 where exists(select 1 from cust_payment p where p.id_number = f.id_number and p.grant_type = f.grant_type)
 
  insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,'2',f.name,f.surname,null,
null,f.box_type,NULL,NULL,f.FORM_TYPE + f.FORM_NUMBER,null,'NONE',null
 from SS_APPLICATION f
 where exists(select 1 from cust_payment p where p.id_number = f.id_number and p.grant_type = f.grant_type)
 
 
 
 
   insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,0,homing_acc_name,homing_acc_name,null,
null,null,NULL,NULL,null,null,'NONE',null
 from cust_payment f
 where not exists(select 1 from inpayment p where p.applicant_no = f.id_number and p.grant_type = f.grant_type)