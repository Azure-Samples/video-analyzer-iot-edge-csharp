// -----------------------------------------------------------------------
//  <copyright company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Tools.JwtTokenIssuer
{
    /// <summary>
    /// JWT Token issuer.
    /// </summary>
    public class Program : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly string _keyType = "RSA";

        private int _exitCode;

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static async Task<int> Main(string[] args)
        {
            try
            {
                await Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddCommandLine(args))
                    .ConfigureAppConfiguration((hostingContext, config) => config.AddEnvironmentVariables())
                    .ConfigureServices((hostContext, services) => services.AddHostedService<Program>())
                    .UseConsoleLifetime(o => o.SuppressStatusMessages = true)
                    .RunConsoleAsync(o => o.SuppressStatusMessages = true);

                return 0;
            }
            catch (OperationCanceledException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Program"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="appLifetime">Application lifetime provider.</param>
        public Program(
            IConfiguration configuration,
            IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                try
                {
                    var audience = _configuration["audience"] ?? "https://videoanalyzer.azure.net";
                    var issuer = _configuration["issuer"] ?? "https://contoso.com";
                    var expiration = DateTime.Parse(_configuration["expiration"] ?? "2100-01-01T00:00:00.000Z", CultureInfo.InvariantCulture);

                    var certificatePath = _configuration["certificatePath"];
                    var certificatePassword = _configuration["certificatePassword"];

                    // Load the certificate
                    using var certificate = string.IsNullOrWhiteSpace(certificatePath)
                        ? CreateSelfSigned()
                        : new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.Exportable);

                    // Create a token
                    var token = CreateToken(audience, issuer, expiration, certificate);

                    // Print Key Information
                    var rsaParameters = certificate.GetRSAPublicKey().ExportParameters(false);

                    var keyInfo = new KeyInfo
                    {
                        KeyId = token.SigningCredentials.Kid,
                        KeyType = _keyType,
                        KeyAlgorithm = token.SignatureAlgorithm,
                        Modulus = Convert.ToBase64String(rsaParameters.Modulus),
                        Exponent = Convert.ToBase64String(rsaParameters.Exponent),
                    };

                    // Print
                    WriteMessage("Issuer: ", issuer, ConsoleColor.Cyan);
                    WriteMessage("Audience: ", audience, ConsoleColor.Cyan);

                    WriteKeyInfo(keyInfo);

                    WriteMessage("Token: ", _tokenHandler.WriteToken(token), ConsoleColor.Cyan);

                    // Wait for user input
                    Console.WriteLine("Press ESC to stop");
                    while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
                    {
                        Thread.Sleep(100);
                    }
                    Console.WriteLine("Exiting...");
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    WriteMessage("Error", e.Message, ConsoleColor.Red);

                    WriteMessage(
                        "Usage",
                        $"{Assembly.GetExecutingAssembly().GetName().Name} [--audience=<audience>] [--issuer=<issuer>] [--expiration=<expiration>] [--certificatePath=<filepath> --certificatePassword=<password>]",
                        ConsoleColor.Cyan);

                    _exitCode = -1;
                }
                finally
                {
                    _appLifetime.StopApplication();
                }
            });

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Environment.ExitCode = _exitCode;

            return Task.CompletedTask;
        }

        private static X509Certificate2 CreateSelfSigned()
        {
            const string certificateName = "AzureVideoAnalyzer SelfSigned";

            WriteMessage(
                "Warning",
#pragma warning disable SA1118 // Parameter should not span multiple lines
                $"This is auto-generate a self-signed certificate. It is highly recommended that you do not use an auto-generated certificate.{Environment.NewLine}" +
                $"If you continue to use the auto-generated certificate you will be required to update the Video Analyzer's access policy every time you generate a new token {Environment.NewLine}" +
                $"to reflect the new certificate's Issuer, Audience, Key Type, Algorithm, Key ID, RSA Key Module, and the RSA Key Exponent.", ConsoleColor.Yellow);
#pragma warning restore SA1118 // Parameter should not span multiple lines

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName(Environment.MachineName);

            var distinguishedName = new X500DistinguishedName($"CN={certificateName}");

            using var rsa = RSA.Create(2048);

            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.DataEncipherment |
                    X509KeyUsageFlags.KeyEncipherment |
                    X509KeyUsageFlags.DigitalSignature,
                    false));

            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") },
                    false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            return request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
        }

        private static void WriteKeyInfo(KeyInfo keyinfo)
        {
            WriteMessage("Key Type: ", keyinfo.KeyType ?? "Key type not available", ConsoleColor.Cyan);
            WriteMessage("Algorithm: ", keyinfo.KeyAlgorithm ?? "Key algorithm not available", ConsoleColor.Cyan);
            WriteMessage("Key Id: ", keyinfo.KeyId ?? "Key Id not available", ConsoleColor.Cyan);
            WriteMessage("RSA Key Modulus (n): ", keyinfo.Modulus ?? "Key modulus not available", ConsoleColor.Cyan);
            WriteMessage("RSA Key Exponent (e): ", keyinfo.Exponent ?? "Key exponent not available", ConsoleColor.Cyan);
        }

        private static void WriteMessage(string header, string message, ConsoleColor headerColor)
        {
            var currentColor = Console.ForegroundColor;

            Console.ForegroundColor = headerColor;

            try
            {
                Console.Write($"{header}: ");
            }
            finally
            {
                Console.ForegroundColor = currentColor;
            }

            Console.WriteLine(message);
            Console.WriteLine();
        }

        private JwtSecurityToken CreateToken(
            string audience,
            string issuer,
            DateTime expiration,
            X509Certificate2 certificate)
        {
            var keySingingCredentials = new X509SigningCredentials(certificate);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = audience,
                Issuer = issuer,
                Expires = expiration,
                SigningCredentials = keySingingCredentials,
                NotBefore = DateTime.UtcNow - TimeSpan.FromMinutes(5),
            };

            return (JwtSecurityToken)_tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}
