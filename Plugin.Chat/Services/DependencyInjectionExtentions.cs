using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Chat.Services;
using Plugin.Chat.Providers;

namespace Sassa.Brm.Common.Helpers
{
    public static class DependencyInjectionExtentions
    {
        public static IServiceCollection AddChatService(this IServiceCollection services)
        {
            services.AddSingleton<IUserStateProvider, UserStateProvider>();

            services.AddScoped<IConnectedClientService, InMemoryConnectedClientService>();
            services.AddScoped<ClientCircuitHandler>();
            services.AddScoped<CircuitHandler>(ctx => ctx.GetService<ClientCircuitHandler>());

            var channel = System.Threading.Channels.Channel.CreateBounded<Message>(100);

            services.AddSingleton<IMessagesPublisher>(ctx =>
            {
                return new MessagesPublisher(channel.Writer);
            });

            services.AddSingleton<IMessagesConsumer>(ctx =>
            {
                return new MessagesConsumer(channel.Reader);
            });

            services.AddHostedService<MessagesConsumerWorker>();

            services.AddSingleton<IChatService, ChatService>();

            return services;
        }
    }
}
