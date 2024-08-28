using Microsoft.EntityFrameworkCore;

namespace Sassa.Surveys.Data
{
    public class SurveyContext : DbContext
    {
        public SurveyContext(DbContextOptions<SurveyContext> options)
            : base(options)
        {
        }

        public DbSet<SurveyResult> SurveyResult { get; set; }

    }
}