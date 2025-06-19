create table LC_MISMATCH
AS
select unq_file_no,applicant_no,user_firstname, user_lastname, grant_type,updated_date,region_id,office_id from dc_file 
where lctype is null
and application_status ike 'LC-%';
commit;

insert into LC_MISMATCH
select unq_file_no,applicant_no,user_firstname, user_lastname, grant_type,updated_date,region_id,office_id from dc_file 
where lctype is not null
and application_status not like 'LC-%';
commit;

UPdate dc_file
set application_status = 'LC-' || application_status
where lctype is not null
and application_status not like 'LC-%';
commit;

update dc_file
SET application_status = REPLACE(application_status, 'LC-', '')
where lctype is null
and application_status like 'LC-%';
commit;