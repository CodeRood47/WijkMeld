
using Microsoft.EntityFrameworkCore;
using WijkMeld.API.Data;
using WijkMeld.API.Entities;



namespace WijkMeld.API.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly WijkMeldContext _context;

        public UserRepository(WijkMeldContext context) 
        { 
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if(user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _context.Users.Include(u => u.Incidents).ToListAsync();
        

        public async Task<User?> GetByIdAsync(Guid id)
            => await _context.Users.Include(u => u.Incidents)
            .FirstOrDefaultAsync(u => u.Id == id);
     

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
