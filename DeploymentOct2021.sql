update dc_batch b
set b.no_of_files = (select count(*) from dc_file whe batch_no = b.batch_no);
commit;

UPDATE dc_file_request
Set Status = 'Requested'
where Status = 'TDWSent';
commit;
UPDATE dc_file_request
Set Status = 'Received'
where Status = 'Scanning';
commit;
UPDATE dc_file_request
Set Status = 'Compliant'
where Status = 'Closed';
commit;