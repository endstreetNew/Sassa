using Sassa.eForms.Data;
using System.Collections.Generic;
using System.Linq;

namespace Sassa.eForms.Services
{
    public class EFormService
    {
        public List<eForm> eforms;
        public EFormService(Microsoft.Extensions.Configuration.IConfiguration Config)
        {

            eforms = new List<eForm>
            {
                new eForm { Id=1, Title = "Older Persons Grant", Blurp = "<p>Applicants must meet the following requirements:</p><ul><li>The applicant must be a South African citizen, permanent resident or refugee.</li><li>The applicant must reside in South Africa.</li><li>Must be older than 60 years.</li><li>The applicant must not be in receipt of another social grant for him or herself.</li><li>The applicant and spouse must comply with the means test.</li><li>The applicant must not be maintained or cared for in a State Institution.</li><li>The applicant must submit a 13 digit bar coded identity document.</li></ul>", Url = $"{Config["FormsURL"]}/presentation/lfserver/OldAgeGrantProcess" },
                new eForm { Id=2, Title = "Child Support Grant", Blurp = "<p>Applicants must meet the following requirements:</p><ul><li>The primary care giver must be a South African citizen, permanent resident or refugee;</li><li>Both the applicant and the child must reside in South Africa;</li><li>The child must be 18 years of age or younger; </li><li>Must provide a birth certificate for the child; </li><li>Must provide a 13 digit barcoded identity document or smart ID card for the applicant; </li><li>Applicant must be the primary care giver of the child/children concerned; </li><li>The applicant and spouse must meet the requirements of the means test; </li><li>Cannot apply for more than six non biological children. </li><li>Child can not be cared for in State institution. </li><li>It should be noted that one of the intentions of the child support grant is to ensure that children attend and complete schooling. It is therefore a requirement that a school attendance certificate be produced for children aged between 7 and 18 years. However, failure to produce this certificate or failure to attend school will not result in the refusal to pay their child support grant </li></ul>", Url = $"{Config["FormsURL"]}/presentation/lfserver/ChildSupportGrantProcess" },
                new eForm { Id=3, Title = "Foster Child Grant", Blurp = "<p>Applicants must meet the following requirements:</p><ul><li>The applicant and child must be resident in South Africa; </li><li>Must provide a court order indicating foster care status; </li><li>The foster parent must be a South African citizen, permanent resident or refugee. </li><li>Child must remain in the care of the foster parent (s). </li><li>Foster parent must provide a 13 digit barcoded identity document; the smart ID card. </li><li>Must provide a birth certificate for the foster child.</li><ul>", Url = $"{Config["FormsURL"]}/presentation/lfserver/FosterChildGrantProcess" }
            };
            if (Config["ValidEMailDomain"] != "*")
            {
                foreach (eForm form in eforms)
                {
                    form.Url += "_PreProd";
                }
            }
        }

        public eForm GeteForm(int id)
        {
            return eforms.Where(e => e.Id == id).FirstOrDefault();
        }
    }
}
