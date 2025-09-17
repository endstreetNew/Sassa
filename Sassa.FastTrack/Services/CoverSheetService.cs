using Microsoft.EntityFrameworkCore;
using Sassa.Brm.Common.Helpers;
using Sassa.Brm.Common.Services;
using Sassa.BRM.Models;
using Sassa.Services;
using System.Text;

namespace Sassa.BRM.Services
{
    public class CoverSheetService(IDbContextFactory<ModelContext> _contextFactory, StaticService _staticService)
    {
        public void AddCoverSheetToFile(string unqFileNo, string fileName, string targetFileName)
        {
            try
            {
                using var context = _contextFactory.CreateDbContext();
                var file = context.DcFiles.FirstOrDefault(x => x.UnqFileNo == unqFileNo);
                if (file == null) throw new Exception($"File {unqFileNo} not found.(BRM)");
                string coverSHtml = getCoverHtml(file);
                PdfService.AddFileToCover(coverSHtml, fileName, targetFileName);
                File.Delete(fileName);
            }
            catch
            {
                throw new Exception("Error adding Coversheet. (BRM) Retry");
            }
        }
        public string getCoverHtml(DcFile file)
        {

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
