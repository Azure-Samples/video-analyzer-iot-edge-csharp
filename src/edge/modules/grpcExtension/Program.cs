// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Grpc.Core;
using Microsoft.Azure.Media.LiveVideoAnalytics.Extensibility.Grpc.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcExtension
{
    public sealed class Program : IHostedService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private Server _grpcServer;
        private static async Task<int> Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddJsonFile("AppConfig.json"))
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddCommandLine(args))
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
                    .ConfigureLogging((hostingContext, logger) => {
                        logger.ClearProviders();
                        logger.AddConsole();
                    })
                    .ConfigureServices((hostContext, services) => services.AddHostedService<Program>())
                    .RunConsoleAsync();

                return 0;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
        }

        public Program(IConfiguration configuration, ILogger<Program> logger)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var grpcBinding = _configuration["grpcBinding"];
            var batchSize = string.IsNullOrWhiteSpace(_configuration["batchSize"]) ? 1 : int.Parse(_configuration["batchSize"]);

            if (string.IsNullOrWhiteSpace(grpcBinding))
            {
                Console.WriteLine($"ERROR: Invalid arguments.");
                Console.WriteLine($"USAGE: {Assembly.GetEntryAssembly().GetName()} --grpcBinding <GRPC_BINDING>");
                Environment.Exit(-1);
            }

            // Initializes the gRPC server
            Console.WriteLine($"Creating gRPC server on {grpcBinding}");

            var grpcUrl = new Uri(grpcBinding);
            var grpcPort = grpcUrl.Port;
            var grpcHost = grpcUrl.Host;

            var moduleExtension = new PipelineExtensionService(_logger, batchSize);
            _grpcServer = new Server(new[]
            {
                // Allow for large message transfers (hi-resolution images can go beyond 4MB default gRPC limit)
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue),
                new ChannelOption(ChannelOptions.MaxConcurrentStreams, 1)
            })
            {
                Services = { MediaGraphExtension.BindService(moduleExtension) },
                Ports = { new ServerPort(grpcHost, grpcPort, ServerCredentials.Insecure) },
            };
            _grpcServer.Start();

            Console.WriteLine("Inference server is listening. Hit <CTRL+C> to exit.");

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _grpcServer?.ShutdownAsync();
        }
    }
}