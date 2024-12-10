using CharacterCountingBot.Models;

namespace CharacterCountingBot.Services
{
    public interface IStorage
    {
        Session GetSession(long chatId);
    }
}
