namespace Sassa.BRM.Services
{

    public class TimedService : IHostedService, IDisposable
    {
        //private int executionCount = 0;
        //private readonly ILogger<TimedService> _logger;
        JsonFileUtils _fu;

        public GlobalVars Globals = new GlobalVars();

        DateTime bookmark;

        RawSqlService _raw;

        private Timer? schedule = null;

        public string? SassaTeamsUrl { get; set; }

        public string fileName;
        private string sqlPath;

        //private PeriodicTimer schedule = null;
        //private Timer _timerTDW = null!;
        //private Timer _timerBRM = null!;
        //private Timer _timerLO = null!;
        //private Timer _timerProgress = null!;

        public TimedService(IWebHostEnvironment env, JsonFileUtils fu, RawSqlService raw)
        {
            fileName = Path.Combine(env.ContentRootPath, "bookmarks") + "\\bookMarks.json";
            sqlPath = Path.Combine(env.ContentRootPath, "sql");
            _raw = raw;
            _fu = fu;
            Globals = _fu.ReadJson<TimedService.GlobalVars>(fileName)!;
            Globals.Status = true;
            fu.WriteJson(Globals, fileName);
        }

        public void Schedule(DateTime scheduleTarget)
        {
            Globals.NextRefreshDate = scheduleTarget;
            TimeSpan delayTime = Globals.AHRefreshDate() - DateTime.Now;
            Globals.Progress = $"Waiting schedule {Globals.NextRefreshDate}";
            _fu.WriteJson(Globals, fileName);
            schedule = new Timer(SyncSOCPEN, null, delayTime, TimeSpan.FromHours(24));
        }
        public async Task Start()
        {
            if (schedule == null)//First run
            {
                if (Globals.NextRefreshDate > DateTime.Now)
                {
                    Schedule(Globals.NextRefreshDate);
                }
                else
                {
                    Schedule(DateTime.Now + TimeSpan.FromHours(24));
                }
            }
            else
            {
                if (Globals.NextRefreshDate < DateTime.Now)//Run if due
                {
                    await Task.Run(() => SyncSOCPEN(null));
                }
            }
            return;
        }

        public void Run()
        {
            SyncSOCPEN(null);
        }
        /// <summary>
        /// Autostart
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken stoppingToken)
        {
            return Start();
        }



        private async void SyncSOCPEN(object? state)
        {
            if (!Globals.Status) return;
            Globals.Status = false;
            string sql = "";
            Globals.LastRunDate = DateTime.Now;
            Globals.Progress = "Syncing.";
            _fu.WriteJson(Globals, fileName);
            bookmark = _raw.GetBookMark("Select max(Capture_date) from dc_socpen");
            bookmark = bookmark.AddDays(-14); //Start two weeks ago.
            try
            {
                //New SRD's
                sql = File.ReadAllText(sqlPath + "\\AddSrds.sql");

                Globals.Progress = "Insert new SRDs.";
                _fu.WriteJson(Globals, fileName);
                await _raw.ExecuteNonQuery(sql);

                sql = File.ReadAllText(sqlPath + "\\AddMainGrants.sql");

                Globals.Progress = "Insert new Main Grants.";
                _fu.WriteJson(Globals, fileName);
                await _raw.ExecuteNonQuery(sql);

                sql = File.ReadAllText(sqlPath + "\\AddChildGrants.sql");

                Globals.Progress = "Insert new Child Grants.";
                _fu.WriteJson(Globals, fileName);
                await _raw.ExecuteNonQuery(sql);

                sql = File.ReadAllText(sqlPath + "\\RetireSrds.sql");
                Globals.Progress = "Retire Srds.";
                _fu.WriteJson(Globals, fileName);
                await _raw.ExecuteNonQuery(sql);


                //sql = $@"UPDATE
                //    (SELECT DC_SOCPEN.LOCALOFFICE_ID as OLD, DC_FILE.OFFICE_ID as NEW
                //        FROM DC_SOCPEN
                //        INNER JOIN DC_FILE
                //        ON DC_SOCPEN.capture_reference = DC_FILE.unq_file_no
                //    ) t
                //    SET t.OLD = t.NEW";
                //Globals.Progress = "Local Office from dc file.";
                //await _raw.ExecuteNonQuery(sql);


                //sql = $@"UPDATE
                //    (SELECT DC_SOCPEN.BRM_BARCODE as OLD, DC_FILE.BRM_BARCODE as NEW
                //        FROM DC_SOCPEN
                //        INNER JOIN DC_FILE
                //        ON DC_SOCPEN.capture_reference = DC_FILE.unq_file_no
                //    ) t
                //    SET t.OLD = t.NEW";
                //Globals.Progress = "BarCode from dc file.";
                //await _raw.ExecuteNonQuery(sql);

                Globals.Status = true;//Continue running after scheduled run
                Globals.Progress = "Done.";
            }
            catch (Exception ex)
            {
                Globals.Progress = $"{Globals.Progress} : {ex.Message}";
                //_fu.WriteJson(Globals, fileName);
            }
            finally
            {
                Globals.NextRefreshDate = DateTime.Now.AddDays(1);
                _fu.WriteJson(Globals, fileName);
            }
        }

        private async Task SyncBRM(object? state)
        {
            try
            {
                string sql = $@"update DC_SOCPEN f set CAPTURE_REFERENCE =
                (
                        select b.unq_file_no from dc_file b
                        where b.APPLICANT_NO = f.Beneficiary_id
                        and rownum = 1
                        AND (b.GRANT_TYPE = f.GRANT_TYPE or b.GRANT_TYPE = '3' AND f.GRANT_TYPE = '0')
                        and f.APPlication_DATE <= b.updated_date
                        and b.updated_date is not null
                        and f.CAPTURE_DATE is null
                        and brm_barcode is not null
                        and not exists(Select capture_reference from DC_SOCPEN dc where dc.capture_reference = b.unq_file_no)
                ) 
                where CAPTURE_DATE is null
                and localOffice_id is not null
                AND Application_date >= to_date('{bookmark.ToString("dd/MMM/yyyy")}')";
                Globals.Progress = "Capture ref from dc file.";
                await _raw.ExecuteNonQuery(sql);

            }
            catch (Exception ex)
            {
                Globals.Progress = $"{Globals.Progress} : {ex.Message}";
            }

        }

        //todo: batch this update monthly
        //for now manual run on TDW data receipt
        private async void SyncTDW(object? state)
        {
            try
            {
                //22/Mar/2017
                int startyear = 2022;
                int startmonth = 1;
                int startday = 1;


                for (int year = startyear; year <= 2023; year++)
                {
                    for (int month = startmonth; month <= 12; month++)
                    {
                        for (int day = startday; day <= DateTime.DaysInMonth(year, month); day++)
                        {
                            Globals.Progress = new DateTime(year, month, day).ToString("dd/MMM/yyyy");
                            string sql = "update DC_SOCPEN f set BRM_BARCODE = ( " +
                            " select filefolder_code from tdw_file_location b" +
                            " where b.Description = f.Beneficiary_id" +
                            " and rownum = 1" +
                            " AND (b.GRANT_TYPE = f.GRANT_TYPE OR b.GRANT_TYPE = '3' AND f.GRANT_TYPE = '0')" +
                            " and f.CAPTURE_REFERENCE is null" +
                            " and b.filefolder_code is not null" +
                            //" and not exists(Select BRM_BARCODE from DC_SOCPEN dc where dc.BRM_BARCODE = b.filefolder_code)" +
                            ") " +
                            $" where f.BRM_BARCODE is null and Application_date >= to_date('{new DateTime(year, month, day).ToString("dd/MMM/yyyy")}') and Application_date <= to_date('{new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("dd/MMM/yyyy")}')";
                            await _raw.ExecuteNonQuery(sql);

                            sql = $"UPDATE dc_SOCPEN dc SET dc.TDW_REC = Application_date  where dc.BRM_BARCODE is not null and dc.Capture_reference is null AND Application_date >= to_date('{new DateTime(year, month, 1).ToString("dd/MMM/yyyy")}') and Application_date <= to_date('{new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("dd/MMM/yyyy")}')";
                            await _raw.ExecuteNonQuery(sql);

                        }
                        startday = 1;

                    }
                    startmonth = 1;
                }
                Globals.Progress = "Done";
            }
            catch (Exception ex)
            {
                Globals.Progress = $"{Globals.Progress} : {ex.Message}";
            }

        }
        private void NewCodeForPersonal()
        {
            //            INSERT INTO DC_SOCPENPG(ADABAS_ISN_ARCHIVE, BENEFICIARY_ID, CHILD_ID, NAME, SURNAME, GRANT_TYPE, REGION_ID, APPLICATION_DATE, APPROVAL_DATE, STATUS_DATE, STATUS_CODE, UNIQUE_ID, PAYPOINT)
            //SELECT
            //    A.ADABAS_ISN AS ADABAS_ISN_ARCHIVE,
            //    LPAD(A.PENSION_NO, 13, 0) AS BENEFICIARY_ID,
            //    C.ID_NO AS CHILD_ID,
            //    B.NAME_EXT,
            //    B.SURNAME_EXT,
            //    A.GRANT_TYPE,
            //    D.REGION_CODE AS REGION_ID,
            //    NVL(A.APPLICATION_DATE, C.APPLICATION_DATE) AS APPLICATION_DATE,
            //    C.APPROVAL_DATE,
            //    C.STATUS_DATE,
            //    CASE
            //        WHEN C.STATUS_CODE = '1' OR(A.PRIM_STATUS IN('B', 'A', '9') AND A.SEC_STATUS IN('2')) THEN 'ACTIVE'
            //        ELSE 'INACTIVE'
            //    END AS STATUS_CODE,
            //    NULL AS UNIQUE_ID,
            //    B.secondary_paypoint as paypoint
            //FROM sassa_archive.socpen_personal_grants_archive A
            //JOIN sassa_archive.SOCPEN_PERSONAL_ARCHIVE B ON A.PENSION_NO = B.PENSION_NO
            //LEFT JOIN sassa_archive.SOCPEN_P12_CHILDREN_ARCHIVE C ON A.PENSION_NO = c.pension_no AND A.GRANT_TYPE = C.GRANT_TYPE
            //LEFT JOIN sassa.cust_rescodes D ON b.secondary_paypoint = d.res_code;

            //            commit;

            //            INSERT INTO DC_SOCPENPG(ADABAS_ISN_MAIN, BENEFICIARY_ID, CHILD_ID, NAME, SURNAME, GRANT_TYPE, REGION_ID, APPLICATION_DATE, APPROVAL_DATE, STATUS_DATE, STATUS_CODE, UNIQUE_ID, PAYPOINT)
            //SELECT
            //    A.ADABAS_ISN AS ADABAS_ISN_MAIN,
            //    LPAD(A.PENSION_NO, 13, 0) AS BENEFICIARY_ID,
            //    C.ID_NO AS CHILD_ID,
            //    B.NAME_EXT,
            //    B.SURNAME_EXT,
            //    A.GRANT_TYPE,
            //    D.REGION_CODE AS REGION_ID,
            //    NVL(A.APPLICATION_DATE, C.APPLICATION_DATE) AS APPLICATION_DATE,
            //    C.APPROVAL_DATE,
            //    C.STATUS_DATE,
            //    CASE
            //        WHEN C.STATUS_CODE = '1' OR(A.PRIM_STATUS IN('B', 'A', '9') AND A.SEC_STATUS IN('2')) THEN 'ACTIVE'
            //        ELSE 'INACTIVE'
            //    END AS STATUS_CODE,
            //    NULL AS UNIQUE_ID,
            //    B.secondary_paypoint as paypoint
            //FROM sassa.socpen_personal_grants A
            //JOIN SOCPEN_PERSONAL B ON  A.PENSION_NO = B.PENSION_NO
            //LEFT JOIN SOCPEN_P12_CHILDREN C ON A.PENSION_NO = c.pension_no AND A.GRANT_TYPE = C.GRANT_TYPE
            //LEFT JOIN sassa.cust_rescodes D ON b.secondary_paypoint = d.res_code;

            //            commit;
        }
        /// <summary>
        /// This SQL needs to run if we receive updates for MIS or ss_application
        /// </summary>
        private void MISSQL()
        {
            //            update DC_SOCPEN f set MIS_FILES =
            //(select File_number from mis_livelink_file b
            //where b.ID_NUMBER = f.Beneficiary_id
            //and rownum = 1
            //AND(b.GRANT_TYPE = f.GRANT_TYPE OR b.GRANT_TYPE = '3' AND f.GRANT_TYPE = '0')
            //and f.CAPTURE_REFERENCE is null)
            //where f.MIS_FILES is null and f.BRM_BARCODE is null and Application_date >= to_date('30/Mar/2016')  and Application_date<to_date('01/Jan/2019');
            //            commit;

            //            update DC_SOCPEN f set MIS_FILES =
            //            (select 'SSARecord' from SS_APPLICATION b
            //            where b.ID_NUMBER = f.Beneficiary_id
            //            and rownum = 1
            //AND(b.GRANT_TYPE = f.GRANT_TYPE OR b.GRANT_TYPE = '3' AND f.GRANT_TYPE = '0')
            //and f.CAPTURE_REFERENCE is null)
            //where f.MIS_FILES is null and f.BRM_BARCODE is null;
            //            commit;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            //_logger.LogInformation("Timed Hosted Service is stopping.");
            schedule?.Change(Timeout.Infinite, 0);
            //_timerTDW?.Change(Timeout.Infinite, 0);
            //_timerBRM?.Change(Timeout.Infinite, 0);
            //_timerLO?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            schedule?.Dispose();
            //_timerTDW?.Dispose();
            //_timerBRM?.Dispose();
            //_timerLO?.Dispose();

        }

        public class GlobalVars
        {
            public DateTime? LastRunDate { get; set; }
            public bool Status { get; set; }
            public string? Progress { get; set; }
            public DateTime NextRefreshDate { get; set; } = System.DateTime.Now;
            public string TimeString
            {
                get
                {
                    return NextRefreshTime.ToString("HH:mm");
                }
                set
                {
                    var hhmm = value.Split(":");

                    NextRefreshTime = new TimeOnly(int.Parse(hhmm[0]), int.Parse(hhmm[1]));
                }
            }
            public TimeOnly NextRefreshTime = TimeOnly.FromDateTime(DateTime.Now);

            public DateTime AHRefreshDate()
            {
                if (NextRefreshDate.Hour > 8 && NextRefreshDate.Hour < 17) //Cant run in business hours.
                {
                    NextRefreshDate = NextRefreshDate.AddHours(19 - NextRefreshDate.Hour);
                }
                return NextRefreshDate;
            }

        }
    }
}