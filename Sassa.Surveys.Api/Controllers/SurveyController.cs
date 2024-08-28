using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sassa.Surveys.Data;

namespace BlazorSurveys.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly SurveyContext _context;

        public SurveyController(SurveyContext context)
        {
            _context = context;
        }

        private static ConcurrentBag<Survey> surveys = new ConcurrentBag<Survey> {
          new Survey {
              Id = 1,
              Title = "Sassa online grants customer satisfaction survey.",
              Options = new List<SurveyOption>{
                  new SurveyOption{ Id = 1,OptionName = "How easy was it to complete your social grant application online?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 2,OptionName = "Could you complete the application in reasonable time?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 3,OptionName = "How likely is it that you would recommend SASSA’s online application to someone ?", optionType = OptionType.Rating, OptionValue = 3},
                  new SurveyOption{ Id = 4,OptionName = "To what extent were you satisfied with the online application process?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 5,OptionName = "Have you benefitted form the online application?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 6,OptionName = "Do you feel reassured that your application would be processed efficiently?", optionType = OptionType.Rating, OptionValue = 3},
                  new SurveyOption{ Id = 7,OptionName = "Do you feel that the documentation you submitted would be handled with confidentiality?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 8,OptionName = "Were you kept informed regarding progress and next step during your application?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 9,OptionName = "Was the online application form and annexures clear and easy to understand?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 10,OptionName = "How easy was it to upload supporting documents?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 11,OptionName = "Did we meet your expectations during the application process?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 12,OptionName = "Please tell us how can we improve the applications experience?", optionType = OptionType.Comment }
              }
          },
          new Survey {
              Id = 2,
              Title = "Sassa online grants internal survey.",
              Options = new List<SurveyOption>{
                  new SurveyOption{ Id = 1,OptionName = "Did the online application system reduce a long queue at local office?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 2,OptionName = "Did the online application lead to reduction of functions performed by officials at a local office?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 3,OptionName = "Time taken to complete attesting by Grant Administrator (screening and capturing of application)? ", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 4,OptionName = "Time taken to complete verification of application by a Senior Grant Administrator?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 5,OptionName = "Time taken to complete an application (attesting to approval?)", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 6,OptionName = "Are all officials able to complete the allocated cases for the day?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 7,OptionName = "How easy to communicate outcomes of application to the applicants?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 8,OptionName = "System accessible to clients?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 9,OptionName = "System user friendly to clients?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 10,OptionName = "Did clients receive feedback about outcomes in good time?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 11,OptionName = "Were outcomes easy to understand by clients?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 12,OptionName = "Were the payment options clear to clients?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 13,OptionName = "Does the office still use service points to bring social grants application services closer to communities?", optionType = OptionType.Rating, OptionValue = 3 },
                  new SurveyOption{ Id = 14,OptionName = "Suggestions to improve:", optionType = OptionType.Comment }
                }
          },
        };

        [HttpGet()]
        public IEnumerable<Survey> GetSurveys()
        {
            return surveys;
        }

        [HttpGet("{id}")]
        public ActionResult GetSurvey(int id)
        {
            var survey = surveys.SingleOrDefault(t => t.Id == id);
            if (survey == null) return NotFound();

            return new JsonResult(survey);
        }

        // Note an [ApiController] will automatically return a 400 response if any
        // of the data annotation valiadations defined in AddSurveyModel fails
        [HttpPut()]
        public async Task<Survey> AddSurvey([FromBody]AddSurveyModel addSurveyModel)
        {
            var survey = new Survey {
                Id = surveys.Max(x => x.Id) + 1,
                Title = addSurveyModel.Title
            };
            survey.OptionFromModel(addSurveyModel.Options);
            surveys.Add(survey);
            return survey;
        }


        [HttpGet("{id}/answers")]
        public ActionResult GetSurveyResults(int id)
        {
            List<SurveyResult> results = _context.SureyResult.Where(r => r.SurveyId == id).ToList();
            //var survey = surveys.SingleOrDefault(t => t.Id == id);
            //if (survey == null) return NotFound();

            return new JsonResult(results);
        }

        [HttpPost("answer")]
        public async Task<ActionResult> AnswerSurvey([FromBody]SurveyResult answer)
        {
            _context.SureyResult.Add(answer);
            await  _context.SaveChangesAsync();
            return new JsonResult(answer);
        }
    }
}