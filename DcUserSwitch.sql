insert into dc_user (ad_user,dc_local_office_id,dc_fsp_id,FirstName, settings)
select r.username,r.office_id,r.fsp_id,r.username,r.settings
  from (select t.username,t.office_id,t.fsp_id,supervisor || ';csv' as settings,
               row_number () over (partition by username order by null) rn
          from dc_office_kuaf_link t) r
 where rn = 1