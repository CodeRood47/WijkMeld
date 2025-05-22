using WijkMeld.API.Entities;
using WijkMeld.API.Entities.Enums;

namespace WijkMeld.API.Data
{
    public static class Seed
    {
        public static void Initialize(WijkMeldContext context)
        {
            if (context.Users.Any()) 
            {
                return;
            
            }



            var users = new List<User>
            {
                new User {Id = Guid.NewGuid(), UserName = "A", Email="yoyo@gemail.com", PasswordHash = "&hdhkauKKK$", Incidents = null, Role= UserRole.USER}
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
