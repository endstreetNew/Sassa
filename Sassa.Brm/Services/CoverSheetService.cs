using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Models;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using System.Text;

namespace Sassa.BRM.Services
{
    public class CoverSheetService(IDbContextFactory<ModelContext> _contextFactory, StaticService _staticService,  SessionService _sessionService)
    {
        private UserSession _userSession = _sessionService.session;

        public string getCoverHtml(string unqFileNo)
        {
            using var context = _contextFactory.CreateDbContext();
            var file = context.DcFiles.FirstOrDefault(x => x.UnqFileNo == unqFileNo);
            if (file == null) throw new Exception($"File {unqFileNo} not found.");
            List<RequiredDocsView> docs = _staticService.GetGrantDocuments(file.GrantType);
            string lcType = "";
            if (file.Lctype != null && file.Lctype > 0)
            {
                lcType = _staticService.GetLcType((decimal)file.Lctype);
            }
            StringBuilder sb = new StringBuilder();
            
            sb.Append(BulkPrint.Header());
            sb.Append(BulkPrint.Body(file, _staticService.GetGrantType(file.GrantType), _staticService.GetRegion(file.RegionId), lcType, docs));
            sb.Append(BulkPrint.Footer());
            
            return sb.ToString();
        }
    }
}
