﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IBusClient
    {
        Task Publish<T>(T model, CancellationToken cancellationToken = default);

        Task Send<T>(T model, CancellationToken cancellationToken = default);

        Task<R> SendAndGetReply<T, R>(T model, CancellationToken cancellationToken = default);
    }
}