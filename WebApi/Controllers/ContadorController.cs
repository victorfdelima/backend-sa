using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApi.Hub;
using WebApi.RabbitMQ;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContadorController : ControllerBase
    {
        private readonly IHubContext<ContadorHub> _hubContext;
        private readonly RabbitMqClient _rabbitMqClient;
        private static readonly List<string> _messages = new List<string>();

        public ContadorController(IHubContext<ContadorHub> hubContext)
        {
            _hubContext = hubContext;
            _rabbitMqClient = new RabbitMqClient(_hubContext);
        }

        [HttpPost("SendText")]
        public IActionResult SendText([FromBody] string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest("O conteúdo não pode estar vazio.");

            _rabbitMqClient.Connect();
            _rabbitMqClient.SendMessageToRabbitMq("minha-fila", content);

            return Ok();
        }

        [HttpGet("GetMessages")]
        public IActionResult GetMessages()
        {
            return Ok(_messages);
        }
    }
}
