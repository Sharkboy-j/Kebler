using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using TinyIpc.Messaging;

namespace Kebler.SI
{
    public static class SingleInstance<TApplication>
        where TApplication : Application, ISingle
    {
        private const string channelNameSufflix = ":KeblerInstanceIPCChannel";
        private static Mutex singleMutex;
        private static TinyMessageBus messageBus;

        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            var CommandLineArgs = Environment.GetCommandLineArgs();
            var applicationIdentifier = uniqueName + Environment.UserName;
            var channelName = $"{applicationIdentifier}{channelNameSufflix}";
            singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);

            if (firstInstance)
                CreateRemoteService(channelName);
            else
                SignalFirstInstance(channelName, CommandLineArgs);

            return firstInstance;
        }

        private static void SignalFirstInstance(string channelName, IList<string> commandLineArgs)
        {
            new TinyMessageBus(channelName).PublishAsync(commandLineArgs.Serialize());
        }

        private static void CreateRemoteService(string channelName)
        {
            messageBus = new TinyMessageBus(channelName);
            messageBus.MessageReceived += MessageBus_MessageReceived;
        }

        private static void MessageBus_MessageReceived(object sender, TinyMessageReceivedEventArgs e)
        {
            var app = Application.Current as TApplication;
            var args = e.Message.Deserialize<string[]>();
            app?.OnInstanceInvoked(args);
        }

        public static void Cleanup()
        {
            if (messageBus != null)
            {
                messageBus.Dispose();
                messageBus = null;
            }

            if (singleMutex != null)
            {
                singleMutex.Close();
                singleMutex = null;
            }
        }
    }
}