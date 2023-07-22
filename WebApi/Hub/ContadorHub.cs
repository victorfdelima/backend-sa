using System.Threading.Tasks;
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
            await Clients.All.SendAsync("ReceiveMessage", message);
        } 

        public async Task SendText(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var rabbitMqClient = new RabbitMqClient(_hubContext);
            rabbitMqClient.Connect();
            rabbitMqClient.SendMessageToRabbitMq("minha-fila", content);

            bool isValidMessage = IsValidMessage(content);
            await Clients.All.SendAsync("ReceiveMessage", content, isValidMessage);
        }

        private bool IsValidMessage(string content)
        {
            int lowerCaseCount = content.Count(char.IsLower);
            int upperCaseCount = content.Count(char.IsUpper);
            int repeatedCharsCount = content.GroupBy(c => c).Count(g => g.Count() >= 4);
            int specialCharCount = content.Count(c => !char.IsLetterOrDigit(c));

            return content.Length >= 15 &&
                   content.Length <= 20 &&
                   lowerCaseCount >= 2 && upperCaseCount >= 5 &&
                   repeatedCharsCount >= 4 && specialCharCount >= 2;
        }
    }
}
