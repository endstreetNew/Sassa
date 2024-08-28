using Sassa.BRM.Models;
using Sassa.Brm.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Sassa.Brm.Common.Helpers
{
    public static class BulkPrint
    {
        public static string Header()
        {
            StringBuilder sb = new StringBuilder();
            //Header
            sb.Append("<!DOCTYPE html>");
            sb.Append("<html lang=\"en\">");
            sb.Append("<head>");
            sb.Append("<meta charset=\"utf-8\" />");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />");
            sb.Append("<title> Sassa.BRM </title>");
            //sb.Append($"<base href=\"{baseurl}\" />");
            sb.Append("<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css\" rel=\"stylesheet\" integrity=\"sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC\" crossorigin=\"anonymous\">");
            //sb.Append("<link href=\"css/site.css\" rel=\"stylesheet\" />");
            //sb.Append("<link href=\"Sassa.BRM.styles.css\" rel=\"stylesheet\" />");
            sb.Append("<style>.centered{text-align:center}.right{text-align:right}.printme{width:800px}@media print{p{page-break-after:always}}qrcode{width:100px;height:100px}.rotate{-webkit-transform:rotate(90deg);-webkit-transform-origin:0 100%;-moz-transform:rotate(90deg);-moz-transform-origin:0 100%;-ms-transform:rotate(90deg);-ms-transform-origin:0 100%;-o-transform:rotate(90deg);-o-transform-origin:0 100%;transform:rotate(90deg);transform-origin:0 100%;zoom:1;position:absolute;left:110px;height:10px;width:200px}.chkboxLabel{display:inline;padding-left:5px;font-size:10px;font-weight:400}</style>");
            sb.Append("</head> ");
            sb.Append("<body> ");

            return sb.ToString();
        }
        public static string Footer()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
        public static string CreateFileCover(DcFile file, string GrantType, string RegionName, string baseurl, string LcType, List<RequiredDocsView> docs)
        {
            try
            {
                BarCodeService bcService = new BarCodeService();
                if(file.GrantType == "S" && string.IsNullOrEmpty(file.SrdNo))file.SrdNo = "Unknown";

            string QrCode = bcService.GetQrSvg(file.BrmBarcode, file.UnqFileNo, file.GrantType == "S" ? file.SrdNo : file.ApplicantNo, file.FullName, GrantType);
            string IdBarCode = bcService.GetBarCode(file.GrantType == "S" ? file.SrdNo : file.ApplicantNo);
            string ClmBarCode = bcService.GetBarCode(file.UnqFileNo);
            string BarCode = bcService.GetBarCode(file.BrmBarcode);
            //string imgeString = GetImageString(Path.GetFullPath("wwwroot\\images\\sassa_logoSmall.jpg"));
            List<string> _docspresent;
            if (string.IsNullOrEmpty(file.DocsPresent))
            {
                _docspresent = new List<string>();
            }
            else
            {
                _docspresent = file.DocsPresent.Split(';').ToList();
                _docspresent = _docspresent.Distinct().Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }

            StringBuilder sb = new StringBuilder();

                //Main Div
                sb.Append("<div class=\"printme row\">");
                sb.Append("<div class=\"col-12\">");

                sb.Append("<div class=\"row\">");
                sb.Append("<div class=\"col-4\">");
                sb.Append($"<img src=\"data:image/jpg; base64,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\" alt=\"Loading Image\">");
                //sb.Append("<img alt=\"\" src=\"images/sassa_logoSmall.jpg\" width=\"200\" />");
                sb.Append("</div>");
                //Middle
                sb.Append("<div class=\"col-4 centered\">");
                sb.Append("<label class=\"h2\">File Coversheet</label>");
                sb.Append("<div>BRM File Number:</div>");
                sb.Append($"<div><strong>{file.BrmBarcode}</strong></div>");
                sb.Append("<div>CLM Number:</div>");
                sb.Append($"<div><strong>{file.UnqFileNo}</strong></div>");
                sb.Append($"{QrCode}");
                sb.Append("</div>");
                //Right
                sb.Append("<div class=\"col-4 right\">");
                sb.Append($"<div>{DateTime.Now.ToShortDateString()}</div>");
                sb.Append("<div>");
                sb.Append($"<label class=\"chkboxLabel\" style=\"padding-right: 10px; font-size: 16px\">Approved - Main</label><input type=\"checkbox\" {(file.ApplicationStatus.Contains("MAIN") ? "checked" : "")} disabled /><br />");
                sb.Append($"<label class=\"chkboxLabel\" style=\"padding-right: 10px; font-size: 16px\">Rejected - Archive</label><input type=\"checkbox\" {(file.ApplicationStatus.Contains("ARCHIVE") ? "checked" : "")} disabled /><br />");
                sb.Append($"<label class=\"chkboxLabel\" style=\"padding-right: 10px; font-size: 16px\">Loose Correspondence</label><input type=\"checkbox\" {(file.ApplicationStatus.Contains("LC") ? "checked" : "")} disabled /><br />");
                sb.Append($"<label class=\"chkboxLabel\" style=\"padding-right: 10px; font-size: 16px\">Review</label><input type=\"checkbox\" {(file.TransType == 2 ? "checked" : "")} disabled /><br />");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</div>");
                //Middle section
                sb.Append("<div class=\"row\">");
                sb.Append("<div class=\"col-5\">");
                sb.Append($"<div>{file.FullName}</div>");
                sb.Append($"<div>{RegionName}</div>");
                sb.Append($"<div>Transaction Date: </div>");
                sb.Append($"<div>Date Last Reviewed: {(file.Lastreviewdate == null ? "" : ((DateTime)file.Lastreviewdate).ToString("yyyy/MM/dd"))}</div>");
                //if lctype is not null
                sb.Append($"<div>LC Type: {LcType}</div>");
                sb.Append("</div>");

                sb.Append("<div class=\"col-5\">");
                sb.Append($"<div>{(file.GrantType == "S" ? file.SrdNo : file.ApplicantNo)}</div>");
                sb.Append($"<div>{GrantType}</div>");
                sb.Append($"<div>{(file.TransDate == null ? "" : ((DateTime)file.TransDate).ToString("yyyy/MM/dd"))}</div>");
                sb.Append($"<div>Archive Year: {file.ArchiveYear}</div>");
                sb.Append("</div>");

                //Right Barcodes
                sb.Append("<div class=\"col-2\" style=\"position: relative;\">");
                sb.Append("<div class=\"centered rotate\" style=\"top:0;\">");
                sb.Append($"<div> {IdBarCode}</div>");
                sb.Append($"<div>{(file.GrantType == "S" ? file.SrdNo + "(Srd)" : file.ApplicantNo + "(Id)")}</div>");
                sb.Append("</div>");
                sb.Append("<div class=\"centered rotate\" style=\"top: 220px;\">");
                sb.Append($"<div>{ClmBarCode}</div>");
                sb.Append($"<div>{file.UnqFileNo}</div>");
                sb.Append("</div>");
                sb.Append("<div class=\"centered rotate\" style=\"top: 520px;\">");
                sb.Append($"<div>{BarCode}</div>");
                sb.Append($"<div>{file.BrmBarcode}</div>");
                sb.Append("</div>");
                sb.Append("</div>");

                sb.Append("</div>");
                //Docs
                sb.Append("<br />");
                sb.Append("<div class=\"row\">");
                sb.Append("<div class=\"col-10\">");
                foreach (var section in "General Particulars|Particulars of Income|Particulars of Assets".Split('|'))
                {
                    sb.Append($"<h5>{section}</h5>");
                    int OddCol = 0;
                    int EvenCol = 0;
                    sb.Append("<div class=\"row\">");
                    sb.Append("<div class=\"col\">");
                    foreach (var doc in docs.Where(d => d.DOC_SECTION == section))
                    {
                        if (OddCol % 2 == 0)
                        {
                            sb.Append($"<div><input type=\"checkbox\" {(_docspresent.Contains(doc.DOC_ID.ToString()) ? "checked" : "")} /><label class=\"chkboxLabel\">{doc.DOC_NAME}</label></div>");
                        }
                        OddCol = OddCol + 1;
                    }
                    sb.Append($"</div>");
                    sb.Append($"<div class=\"col\">");
                    foreach (var doc in docs.Where(d => d.DOC_SECTION == section))
                    {
                        if (EvenCol % 2 != 0)
                        {
                            sb.Append($"<div><input type=\"checkbox\" {(_docspresent.Contains(doc.DOC_ID.ToString()) ? "checked" : "")} /><label class=\"chkboxLabel\">{doc.DOC_NAME}</label></div>");
                        }
                        EvenCol = EvenCol + 1;
                    }
                    sb.Append("</div>");
                    sb.Append("</div>");
                    sb.Append("<br/>");
                }
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("<br />");

                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("<p></p>");
                //Footer
                //sb.Append("</body>");
                //sb.Append("</html>");
                var ss = sb.ToString();
                return ss;
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return $"Invalid Cover {file.BrmBarcode}";

        }
        public static string CreateBatchCover(List<DcFile> items, string officeName, decimal batchId)
        {
            BarCodeService bcService = new BarCodeService();
            string batchBarCode = bcService.GetBarCode(batchId.ToString());
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"printme pdf\" >");
            sb.Append("<div class=\"row\">");
            sb.Append("<div class=\"col-4\">");
            sb.Append("<div style=\"float: left; display: inline-block\">");
            sb.Append($"<img src=\"data:image/jpg; base64,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\" alt=\"Loading Image\">");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("<div class=\"col-4\">");
            sb.Append("<div style=\"display: inline-block; \">");
            sb.Append("<h3>Batch Inventory</h3>");
            sb.Append($"<h5>{officeName}</h5>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("<div class=\"col-4\">");
            sb.Append("<div style=\"float:right; \">");
            sb.Append($"<div style=\"float:right; \">{DateTime.Now.ToShortDateString()}</div>");
            sb.Append("<br />");
            sb.Append("<div style=\"float:right; min-width:200px; \">");
            sb.Append($"<div>{batchBarCode}</div>");
            sb.Append($"<div>{batchId}</div>");
            sb.Append("</div>");
            sb.Append("<br />");
            sb.Append("<br />");
            sb.Append("<br />");
            sb.Append("<p>");
            sb.Append($"No of Files:<span>{items.Count()}</span>");
            sb.Append("<br />");
            sb.Append("Batch No:<span>BatchId</span>");
            sb.Append("<br />");
            sb.Append("</p>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("<div class=\"row\">");
            sb.Append("<div class=\"col-12\">");
            sb.Append("<table class=\"table table-sm\" style=\"font-size:x-small;\">");
            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<th scope=\"col\">CLM Unique Code</th>");
            sb.Append("<th scope=\"col\">BRM File no</th>");
            sb.Append("<th scope=\"col\">ID no</th>");
            sb.Append("<th scope=\"col\">Name and Surname</th>");
            sb.Append("<th scope=\"col\">Grant Type</th>");
            sb.Append("<th scope=\"col\">Reg Type </th>");
            sb.Append("<th scope=\"col\">App Date </th>");
            sb.Append("<th scope=\"col\">Brm Barcode</th>");
            sb.Append("<th></th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");
            foreach (DcFile u in items)
            {
                sb.Append(CreateBatchItem(u));
            }
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            return sb.ToString();
        }
        private static string CreateBatchItem(DcFile u)
        {
            BarCodeService bcService = new BarCodeService();
            string qrCode = bcService.GetQrSvg(u.BrmBarcode, u.UnqFileNo, u.GrantType == "S" ? u.SrdNo : u.ApplicantNo, u.FullName, StaticDataService.GrantTypes![u.GrantType]);
            StringBuilder sb = new StringBuilder();
            sb.Append("<tr>");
            sb.Append($"<td>{u.UnqFileNo}</td>");
            sb.Append($"<td>{u.BrmBarcode}</td>");
            sb.Append($"<td>{u.ApplicantNo}</td>");
            sb.Append($"<td>{u.FullName}</td>");
            sb.Append($"<td>{u.GrantType}</td>");
            sb.Append($"<td>{u.RegType}</td>");
            sb.Append($"<td>{(u.TransDate == null ? "" : ((DateTime)u.TransDate).ToShortDateString())}</td>");
            sb.Append($"<td colspan=\"2\">{qrCode}</td>");
            sb.Append("</tr>");
            return sb.ToString();
        }

    }
}
