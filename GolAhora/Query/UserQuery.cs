using GolAhora.Models;
using Microsoft.EntityFrameworkCore;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Query
{
    public class UserQuery
    {
        private readonly AppContext _appContext;

        public UserQuery(AppContext appContext)
        {
            _appContext = appContext;
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _appContext.Users
                .AsNoTracking()
                .Include(u => u.clientProfile)
                .Include(u => u.personalClubProfile)
                    .ThenInclude(pcp => pcp.adminProfile)
                .Include(u => u.personalClubProfile)
                    .ThenInclude(pcp => pcp.employeeProfile)
                .Include(u => u.personalClubProfile)
                    .ThenInclude(pcp => pcp.professorProfile)
                .ToListAsync();
        }

        public async Task<ClientProfile?> GetClientProfile(int idUSer)
        {
            return await _appContext.ClientProfiles
                .FirstOrDefaultAsync(cp => cp.idUser == idUSer);
        }

        public async Task<bool> ClientHasReservations(int idClient)
        {
            return await _appContext.Reservations
                .AnyAsync(r => r.idClient == idClient);
        }
    }
}
