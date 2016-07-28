﻿using Echo.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.Rpc;
using Rabbit.Rpc.Exceptions;
using Rabbit.Rpc.ProxyGenerator;
using Rabbit.Transport.Simple;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WeChatDistribution.RpcAbstractions;

namespace Echo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection
                .AddLogging()
                .AddClient()
                .UseSharedFileRouteManager(@"d:\routes.txt")
                .UseSimpleTransport();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            serviceProvider.GetRequiredService<ILoggerFactory>()
                .AddConsole((c, l) => true);

            var serviceProxyGenerater = serviceProvider.GetRequiredService<IServiceProxyGenerater>();
            var serviceProxyFactory = serviceProvider.GetRequiredService<IServiceProxyFactory>();
            var services = serviceProxyGenerater.GenerateProxys(new[] { typeof(IUserService), typeof(IMessageHandler), typeof(IMessageStoreService) }).ToArray();

            //创建IUserService的代理。
            var userService = serviceProxyFactory.CreateProxy<IUserService>(services.Single(typeof(IUserService).GetTypeInfo().IsAssignableFrom));
            var messageHandler = serviceProxyFactory.CreateProxy<IMessageHandler>(services.Single(typeof(IMessageHandler).GetTypeInfo().IsAssignableFrom));
            var messageStoreService = serviceProxyFactory.CreateProxy<IMessageStoreService>(services.Single(typeof(IMessageStoreService).GetTypeInfo().IsAssignableFrom));

            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            while (true)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        Console.WriteLine("start");
                        await userService.Exists(1);
                        userService.Try();
                        /*                        var model = new WeChatMessageModel
                                                {
                                                    Content = "test",
                                                    Id = Guid.NewGuid().ToString(),
                                                    ShellName = "xwyh"
                                                };
                                                await messageStoreService.StoreAsync(model);
                                                userService.HandleAsync(model);*/
                        Console.WriteLine("end");
                    }).Wait();
                    /*                        Console.WriteLine($"userService.GetUserName:{await userService.GetUserName(1)}");
                                                                    Console.WriteLine($"userService.GetUserId:{await userService.GetUserId("rabbit")}");
                                                                    Console.WriteLine(
                                                                        $"userService.GetUserLastSignInTime:{await userService.GetUserLastSignInTime(1)}");
                                                                    Console.WriteLine($"userService.Exists:{await userService.Exists(1)}");
                                                                    var user = await userService.GetUser(1);
                                                                    Console.WriteLine($"userService.GetUser:name={user.Name},age={user.Age}");
                                                                    Console.WriteLine($"userService.Update:{await userService.Update(1, user)}");
                                                                    Console.WriteLine($"userService.GetDictionary:{(await userService.GetDictionary())["key"]}");
                                                                    await userService.TryThrowException();*/
                }
                catch (RpcRemoteException remoteException)
                {
                    logger.LogError(remoteException.Message);
                }
                catch
                {
                }
                Console.ReadLine();
            }
        }
    }
}