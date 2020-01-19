using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Fan.Blog.IntegrationTests
{
    /// <summary>
    /// This test class demos how MediatR works.
    /// </summary>
    /// <remarks>
    /// It is based on a combination of both of these samples
    /// https://github.com/jbogard/MediatR/blob/master/test/MediatR.Tests/PublishTests.cs
    /// https://github.com/jbogard/MediatR/blob/master/samples/MediatR.Examples.AspNetCore/Program.cs
    /// </remarks>
    public class MediatRTest
    {
        public class Ping : INotification
        {
            public string Message { get; set; }
        }

        public class PongHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PongHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pong");
            }
        }

        public class PungHandler : INotificationHandler<Ping>
        {
            private readonly TextWriter _writer;

            public PungHandler(TextWriter writer)
            {
                _writer = writer;
            }

            public Task Handle(Ping notification, CancellationToken cancellationToken)
            {
                return _writer.WriteLineAsync(notification.Message + " Pung");
            }
        }

        [Fact]
        public async Task Should_resolve_main_handler()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            var services = new ServiceCollection();
            // 'MediatR.ServiceFactory' needed to activate 'MediatR.Mediator'
            services.AddScoped<ServiceFactory>(p => p.GetService);
            services.AddSingleton<TextWriter>(writer); // add writer as a singleton

            services.Scan(scan => scan
               .FromAssembliesOf(typeof(IMediator), typeof(Ping))
               .AddClasses()
               .AsImplementedInterfaces());

            var provider = services.BuildServiceProvider();
            var mediator = provider.GetRequiredService<IMediator>(); // depends on 'MediatR.ServiceFactory'

            // this will call the two handlers' Handle method
            await mediator.Publish(new Ping { Message = "Ping" });

            var result = builder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            Assert.Contains("Ping Pong", result);
            Assert.Contains("Ping Pung", result);
        }
    }
}
