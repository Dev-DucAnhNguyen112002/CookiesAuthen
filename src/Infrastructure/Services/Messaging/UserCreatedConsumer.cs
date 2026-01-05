using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CookiesAuthen.Application.Common.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CookiesAuthen.Infrastructure.Services.Messaging;
public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        // Lấy dữ liệu từ tin nhắn
        var message = context.Message;

        _logger.LogInformation("=================================================");
        _logger.LogInformation($"[RabbitMQ] Nhận được tin nhắn: User mới tạo {message.Email}");
        _logger.LogInformation("Đang giả lập gửi Email... (Mất 3s)");

        await Task.Delay(3000); // Giả vờ gửi mail lâu

        _logger.LogInformation($"[RabbitMQ] Đã gửi Email thành công cho {message.UserId}");
        _logger.LogInformation("=================================================");
    }
}
