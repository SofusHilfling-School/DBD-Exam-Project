﻿using lib.Mail;
using lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lib.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);

    }
}
