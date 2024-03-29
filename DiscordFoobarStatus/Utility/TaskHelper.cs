﻿using System;
using System.Threading.Tasks;

namespace DiscordFoobarStatus.Utility
{
    public static class TaskHelper
    {
        private static readonly Action<Task> DefaultErrorContinuation =
        t =>
        {
            try
            {
                t.Wait();
            }
            catch
            {
            }
        };

        public static void FireForget(Func<Task> action, Action<Exception>? handler = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var task = Task.Run(action);

            if (handler == null)
            {
                task.ContinueWith(
                    DefaultErrorContinuation,
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                task.ContinueWith(
                    t => handler(t.Exception!.GetBaseException()),
                    TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);
            }
        }
    }
}
