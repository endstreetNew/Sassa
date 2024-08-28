using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sassa.HostedService.Services
{
    public class TimedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedService> _logger;
        private Timer _timerTDW = null!;
        private Timer _timerBRM = null!;
        private Timer _timerLO = null!;

        public TimedService(ILogger<TimedService> logger)
        {
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _timerTDW = new Timer(SyncTDW, null, TimeSpan.Zero,TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private void SyncTDW(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

//            _logger.LogInformation(
//                "--tdw
//update socpen_personal_grants s set TDW_reference = (select filefolder_code from tdw_file_location t where s.pension_no = t.description and s.grant_type = t.grant_type and  rownum = 1)
//where tdw_Reference is null and brm_reference is null and lo_reference is null;
//            commit;

        }

        private void SyncBRM(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);


            //INSERT INTO DC_BRM_GRANTS(BRM_BARCODE, APPLICANT_NO, GRANT_TYPE, UPDATED_DATE)
            //SELECT BRM_BARCODE, APPLICANT_NO, GRANT_TYPE, UPDATED_DATE FROM DC_FILE f
            //WHERE NOT EXISTS(SELECT* FROM DC_BRM_GRANTS g WHERE g.APPLICANT_NO = f.APPLICANT_NO and g.GRANT_TYPE = f.GRANT_TYPE)
            //AND f.BRM_BARCODE is not null and f.Applicant_no is not null
            //ORDER BY f.UPDATED_DATE ASC;
            //commit;

            //UPDATE SASSA.SOCPEN_PERSONAL_GRANTS S
            //SET(BRM_Reference, File_DATE) = (
            //select BRM_BARCODE, UPDATED_DATE
            //from DC_BRM_GRANTS G
            //where S.PENSION_NO = G.APPLICANT_NO AND S.GRANT_TYPE = G.GRANT_TYPE and Rownum = 1)
            //WHERE S.BRM_Reference is null;
            //COMMIT;
        }

        private void SyncLO(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);

            //CREATE GLOBAL TEMPORARY TABLE LO_GRANTS on commit preserve rows as select Reference,IDENTIFICATION_PASS,grant_type from CUST_EXPORTED_SOCPEN @LOPROD;
            //update socpen_personal_grants s set s.LO_reference = (select Reference from LO_GRANTS f where s.pension_no = f.IDENTIFICATION_PASS and s.grant_type = f.grant_type and rownum = 1)
            //where s.tdw_Reference is null and s.brm_reference is null and s.lo_reference is null;
            //truncate table lo_grants;
            //drop table lo_grants;
            //commit;
        }
        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timerTDW?.Change(Timeout.Infinite, 0);
            _timerBRM?.Change(Timeout.Infinite, 0);
            _timerLO?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timerTDW?.Dispose();
            _timerBRM?.Dispose();
            _timerLO?.Dispose();

        }
    }
}
