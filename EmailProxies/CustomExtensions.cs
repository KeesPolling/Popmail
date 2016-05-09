using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PopMail.EmailProxies
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Task extension to timeout after a number of milliseconds 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task">This is an extension method of Task</param>
        /// <param name="timeout">timeout time in milliseconds</param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int timeout)
        {
            var timeoutCancellationTokenSource = new CancellationTokenSource();

            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }
    }
}
