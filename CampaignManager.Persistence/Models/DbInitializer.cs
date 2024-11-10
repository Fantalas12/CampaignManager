using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Persistence.Models
{

    // This class is used to initialize the database with some data.
    public class DbInitializer
    {

        public static void Initialize(CampaignManagerDbContext context)
        {

            // Create / update database based on migration classes.
            context.Database.Migrate();


            // There is no real data yet, so no need to seed anything.
            /*
            if (context.Lists.Any())
            {
                return;
            }
            */
        }
    }
}
