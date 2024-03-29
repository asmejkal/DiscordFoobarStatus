﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordFoobarStatus.Utility
{
    public static class SemaphoreSlimExtensions
    {
        public class LockScope : IDisposable
        {
            private SemaphoreSlim Semaphore { get; }

            public LockScope(SemaphoreSlim semaphore)
            {
                Semaphore = semaphore;
            }

            public void Dispose() => Semaphore.Release();
        }

        public static IDisposable Claim(this SemaphoreSlim s)
        {
            s.Wait();
            return new LockScope(s);
        }

        public static async Task<IDisposable> ClaimAsync(this SemaphoreSlim s)
        {
            await s.WaitAsync();
            return new LockScope(s);
        }

        public static async Task<IDisposable> ClaimAsync(this SemaphoreSlim s, CancellationToken ct)
        {
            await s.WaitAsync(ct);
            return new LockScope(s);
        }
    }
}
