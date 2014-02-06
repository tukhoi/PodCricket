using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.Utilities.Extensions
{
    public static class WebClientExtensions
    {
        /// <summary>Downloads the resource with the specified URI as a string, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded string.</returns>
        public static Task<string> DownloadStringTaskAsync(this WebClient webClient, string address)
        {
            return DownloadStringTaskAsync(webClient, new Uri(address));
        }

        /// <summary>Downloads the resource with the specified URI as a string, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded string.</returns>
        public static Task<string> DownloadStringTaskAsync(this WebClient webClient, Uri address)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<string>(address);

            // Setup the callback event handler
            DownloadStringCompletedEventHandler handler = null;
            handler = (sender, e) => EAPCommon.HandleCompletion(tcs, e, () => e.Result, () => webClient.DownloadStringCompleted -= handler);
            webClient.DownloadStringCompleted += handler;

            // Start the async work
            try
            {
                webClient.DownloadStringAsync(address, tcs);
            }
            catch (Exception exc)
            {
                // If something goes wrong kicking off the async work,
                // unregister the callback and cancel the created task
                webClient.DownloadStringCompleted -= handler;
                tcs.TrySetException(exc);
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }
    }
}
