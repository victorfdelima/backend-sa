using Microsoft.AspNetCore.SignalR;
using WebApi.RabbitMQ;

namespace WebApi.Hub
{
    public class ContadorHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IHubContext<ContadorHub> _hubContext;
        private readonly ILogger<ContadorHub> _logger;

        public ContadorHub(IHubContext<ContadorHub> hubContext, ILogger<ContadorHub> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task ReceiveMessage(string message)
        {
            var isValidMessage = IsValidMessage(message);
            await Clients.All.SendAsync("ReceiveMessage", new { content = message, isValid = isValidMessage });
        }

        public async Task SendText(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var rabbitMqClient = new RabbitMqClient(_hubContext);
            rabbitMqClient.Connect();
            rabbitMqClient.SendMessageToRabbitMq("minha-fila", content);

            bool isValidMessage = IsValidMessage(content);
            await Clients.All.SendAsync("ReceiveMessage", new { content, isValid = isValidMessage });
        }

        private bool IsValidMessage(string content)
        {
            var listadevalidacoes = new List<string>();

            int lowerCaseCount = content.Count(char.IsLower);
            int upperCaseCount = content.Count(char.IsUpper);
            int repeatedCharsCount = content.GroupBy(c => c).Count(g => g.Count() >= 4);
            int specialCharCount = content.Count(c => !char.IsLetterOrDigit(c));
            if (content.Length < 15 || content.Length > 20) listadevalidacoes.Add("Tamanho inválido");

            if (lowerCaseCount < 2) listadevalidacoes.Add("Faltando caracteres mínusculos");

            if (upperCaseCount < 5) listadevalidacoes.Add("Faltando caracteres maiúsculos");

            if (repeatedCharsCount != 1) listadevalidacoes.Add("Faltando caracteres repetidos");

            if (specialCharCount < 2) listadevalidacoes.Add("Faltando caracteres especiais");

            return listadevalidacoes.Count() == 0;
        }
    }
}
