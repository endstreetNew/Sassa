    delete from inpayment;
    commit;
insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.Applicant_no,f.Grant_type,f.region_id,substr(f.user_firstname,1,50),f.user_lastname,null,
f.updated_date,SUBSTR(f.application_status,1,10),SUBSTR(f.brm_barcode,1,8),f.unq_file_no,f.file_number,null,'NONE',f.office_id
 from dc_file f
 where exists(select 1 from cust_payment p where p.id_number = f.applicant_no and p.grant_type = f.grant_type);
 commit;
 insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,f.region_id,f.name,f.surname,null,
null,substr(f.registry_type,1,10),NULL,NULL,f.file_number,null,'NONE',null
 from MIS_LIVELINK_TBL f
 where exists(select 1 from cust_payment p where p.id_number = f.id_number and p.grant_type = f.grant_type);
 commit;
 
  insert into inpayment
  (Applicant_no, Grant_type, Region_id, First_name, surname, child_id_no,
   trans_date,reg_type,brm_barcode,clm_no,MIS_file_no,ec_mis_file,oga_status,paypoint)
Select f.ID_number,f.Grant_type,'2',f.name,f.surname,null,
null,f.box_type,NULL,NULL,f.FORM_TYPE + f.FORM_NUMBER,null,'NONE',null
 from SS_APPLICATION f
 where exists(select 1 from cust_payment p where p.id_number = f.id_number and p.grant_type = f.grant_type);
 commit;