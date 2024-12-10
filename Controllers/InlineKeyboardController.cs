using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CharacterCountingBot.Services;

namespace CharacterCountingBot.Controllers
{
    public class InlineKeyboardController
    {
        private readonly IStorage _memoryStorage;
        private readonly ITelegramBotClient _telegramClient;

        public InlineKeyboardController(ITelegramBotClient telegramClient, IStorage memoryStorage)
        {
            _telegramClient = telegramClient;
            _memoryStorage = memoryStorage;
        }

        public async Task Handle(CallbackQuery? callbackQuery, CancellationToken ct)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var messageText = callbackQuery.Message.Text;

            if (callbackQuery?.Data == null)
                return;

            // Обновление пользовательской сессии новыми данными
            _memoryStorage.GetSession(callbackQuery.From.Id).ChoiceAction = callbackQuery.Data;

            string choise = callbackQuery.Data switch
            {
                "count" => "Подсчет количества символов",
                "sum" => "Подсчет суммы чисел",
                _ => String.Empty
            };

            await _telegramClient.SendTextMessageAsync(callbackQuery.From.Id,
                $"<b>Действие - {choise}.{Environment.NewLine}</b>" +
                $"{Environment.NewLine}Введите сообщение для подсчета.", cancellationToken: ct, parseMode: ParseMode.Html);
        }
    }
}
