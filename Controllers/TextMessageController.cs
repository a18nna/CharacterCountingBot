using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using CharacterCountingBot.Services;
using CharacterCountingBot.Configuration;


namespace CharacterCountingBot.Controllers
{
    public class TextMessageController
    {
        private readonly ITelegramBotClient _telegramClient;
        private readonly IStorage _memoryStorage;

        public TextMessageController(AppSettings appSettings, ITelegramBotClient telegramClient, IStorage memoryStorage)
        {
            _appSettings = appSettings;
            _telegramClient = telegramClient;
            _memoryStorage = memoryStorage;
        }

        public async Task Handle(Message message, CancellationToken ct)
        {
            // Получаем текущую сессию пользователя
            var userSession = _memoryStorage.GetSession(message.From.Id);

            // Если действие выбрано, обрабатываем сообщение
            if (!string.IsNullOrEmpty(userSession.ChoiceAction))
            {
                switch (userSession.ChoiceAction)
                {
                    case "count":
                        await HandleCountCharsAsync(message.Chat.Id, message.Text);
                        break;
                    case "sum":
                        await HandleSumNumbersAsync(message.Chat.Id, message.Text);
                        break;
                    default:
                        await _telegramClient.SendTextMessageAsync(message.Chat.Id, "Неизвестное действие.", cancellationToken: ct);
                        break;
                }

                // Сбрасываем выбранное действие после обработки
                userSession.ChoiceAction = null;
                return;
            }

            // Если действие не выбрано, обрабатываем стандартные команды
            switch (message.Text)
            {
                case "/start":
                    var buttons = new List<InlineKeyboardButton[]>();
                    buttons.Add(new[]
                    {
                InlineKeyboardButton.WithCallbackData($"Подсчет количества символов в сообщении", $"count"),
                InlineKeyboardButton.WithCallbackData($"Подсчет суммы чисел", $"sum")
            });
                    await _telegramClient.SendTextMessageAsync(message.Chat.Id, $"<b>  Я умею считать количество символов в сообщении.</b> {Environment.NewLine}" +
                         $"{Environment.NewLine}А еще могу посчитать сумму чисел, отправленных в сообщении.{Environment.NewLine}", cancellationToken: ct, parseMode: ParseMode.Html, replyMarkup: new InlineKeyboardMarkup(buttons));
                    break;

                default:
                    await _telegramClient.SendTextMessageAsync(message.Chat.Id, "Не удалось выполнить действие.", cancellationToken: ct);
                    break;
            }
        }

        public async Task HandleCountCharsAsync(long chatId, string text)
        {
            int charCount = text?.Length ?? 0;
            await _telegramClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Количество символов в сообщении: {charCount}"
            );
        }

        public async Task HandleSumNumbersAsync(long chatId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                await _telegramClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Сообщение пустое. Пожалуйста, отправьте числа через пробел."
                );
                return;
            }

            var numbers = text.Split(' ')
                              .Select(x => double.TryParse(x, out var number) ? number : (double?)null)
                              .Where(x => x.HasValue)
                              .Select(x => x.Value)
                              .ToList();

            if (numbers.Count == 0)
            {
                await _telegramClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Не найдено чисел. Пожалуйста, отправьте числа через пробел."
                );
                return;
            }

            double sum = numbers.Sum();
            await _telegramClient.SendTextMessageAsync(
                chatId: chatId,
                text: $"Сумма чисел: {sum}"
            );
        }
    }
}
