using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using razor.Components.Models;

namespace Sassa.Surveys.Data
{
    public record Survey
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public List<SurveyOption> Options { get; init; } = new List<SurveyOption>();
        //public PieData PieData;

        //public void OptionFromModel(List<OptionModel> Cmodel)
        //{
        //    foreach (OptionModel model in Cmodel)
        //    {
        //        Options.Add(new SurveyOption { SurveyId = model.SurveyId, OptionName = model.OptionName, optionType = model.optionType });
        //    }
        //    PieData = new PieData(this,null);
        //}
    }

    public record SurveyOption
    {
        public int Id { get; init; }
        public int SurveyId { get; init; }
        public string OptionName { get; init; }
        public OptionType optionType { get; init; }
        public int OptionValue { get; set; }
        public string Comment { get; set; }
    }

    public record SurveyResult
    {
        [Key]
        public int Id { get; init; }
        public int SurveyId { get; init; }
        public int OptionId { get; init; }
        public int optionType { get; init; }
        public int OptionValue { get; set; }
        public string OComment { get; set; }
    }

    //public class PieData
    //{
    //    public List<PieRatingModel> Ratings;
    //    public List<PieOptionModel> Options;
    //    private Survey _survey;
    //    public PieData(Survey survey, List<SurveyResult> results)
    //    {
    //        _survey = survey;
             
    //        double total = results.Where(o => o.optionType == (int)OptionType.Rating).Count();
    //        if (total == 0) return;
    //        Ratings = new List<PieRatingModel> {
    //            new PieRatingModel{Name = "Very Dissatisfied",Color = "Red",Percent = results.Where(r => r.OptionValue == 0).Count()},
    //            new PieRatingModel{Name = "Dissatisfied",Color = "Orange",Percent = results.Where(r => r.OptionValue == 1).Count()},
    //            new PieRatingModel{Name = "Not Perfect",Color = "Yellow",Percent =  results.Where(r => r.OptionValue == 2).Count()},
    //            new PieRatingModel{Name = "OK",Color = "LightGreen",Percent = results.Where(r => r.OptionValue == 3).Count()},
    //            new PieRatingModel{Name = "Satisfied",Color = "Green",Percent = results.Where(r => r.OptionValue == 4).Count()},
    //            new PieRatingModel{Name = "Very Satisfied",Color = "DarkGreen",Percent = results.Where(r => r.OptionValue == 5).Count()},
    //            };
    //        //Calculate Rating %
    //        foreach (var rating in Ratings)
    //        {
    //            rating.Percent = rating.Percent * 100 / total;
    //        }
    //        //Calculate Option %
    //        Options = new List<PieOptionModel>();
    //        int Ototal = 0;
    //        foreach (var option in _survey.Options.Where(o => o.optionType == OptionType.Rating))
    //        {
    //            if (!results.Where(r => r.OptionId == option.Id && r.OptionValue < 3).Any()) continue;
    //            var count = results.Where(r => r.OptionId == option.Id && r.OptionValue < 3).Count();
    //            Options.Add( new PieOptionModel { Name = option.OptionName, Percent = count } ); /// _survey.Options.Where(o => o.optionType == OptionType.Rating).Count() 
    //            Ototal += count;
    //        }
    //        foreach (var option in Options)
    //        {
    //            option.Percent = System.Math.Round(option.Percent * 100 / Ototal, System.MidpointRounding.ToEven);
    //        }
    //     }
    //}
    public class PieRatingModel
    {
        public double Percent { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class PieOptionModel
    {
        public double Percent { get; set; }
        public string Name { get; set; }
    }

    public enum OptionType
    {
        Rating,
        Comment
    }
}
