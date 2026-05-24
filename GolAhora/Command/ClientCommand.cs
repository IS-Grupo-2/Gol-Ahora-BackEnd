using GolAhora.Models;
using AppContext = GolAhora.Data.AppContext;

namespace GolAhora.Command
{
    public class ClientCommand
    {
        private readonly AppContext _appContext;

        public ClientCommand(AppContext appContext)
        {
            _appContext = appContext;
        }

        public async Task AddClient(ClientProfile client)
        {
            _appContext.ClientProfiles.Add(client);
            await _appContext.SaveChangesAsync();
        }
    }
}
