update dc_socpen
set STATUS_CODE = 'INACTIVE'
where GRANT_TYPE = 'S'
AND APPLICATION_DATE < add_months( trunc(sysdate), -12*3 )