﻿using UserIdentity.Services.Abstract;
using UserIdentity.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserIdentity.WebUI.Infrastructure.Services
{
    public class MessageServices : IEmailSender
    {
        private readonly IEmailService _emailService;

        public MessageServices(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return _emailService.SendMailAsync(new List<EmailRecipient>
            {
                new EmailRecipient { Name = string.Empty, EmailAddress = email }
            }, subject, message);
        }
    }
}