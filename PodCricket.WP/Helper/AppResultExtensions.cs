using PodCricket.ApplicationServices;
using PodCricket.WP.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.WP.Helper
{
    public static class AppResultExtensions
    {
        public static string ErrorMessage<T>(this AppResult<T> appResult)
        {
            var message = string.Empty;
            switch (appResult.Error)
            { 
                case ErrorCode.None:
                    message = string.Empty;
                    break;
                case ErrorCode.PodSubscribed:
                    message = AppResources.ErrPodSubscribed;
                    break;
                case ErrorCode.CouldNotFindPod:
                    message = AppResources.ErrCouldNotFindPod;
                    break;
                case ErrorCode.GetPodError:
                    message = AppResources.ErrGetPodError;
                    break;
                case ErrorCode.NoPodSubscribed:
                    message = AppResources.ErrNoPodSubscribed;
                    break;
                case ErrorCode.PodPassedInNull:
                    message = AppResources.ErrPodPassedInNull;
                    break;
                case ErrorCode.NoOfflineSearchedPodFound:
                    message = AppResources.ErrNoOfflineSearchedPodFound;
                    break;
                case ErrorCode.NoOnlineSearchedPodFound:
                    message = AppResources.ErrNoOnlineSearchedPodFound;
                    break;
                case ErrorCode.StreamAlreadyInDownloading:
                    message = AppResources.ErrStreamAlreadyInDownloading;
                    break;
                case ErrorCode.NoStreamDownloadingFound:
                    message = AppResources.ErrNoStreamDownloadingFound;
                    break;
                case ErrorCode.CouldNotSaveDownloadedStream:
                    message = AppResources.ErrCouldNotSaveDownloadedStream;
                    break;
                case ErrorCode.StreamAlreadyInPlayingList:
                    message = AppResources.ErrStreamAlreadyInPlayingList;
                    break;
                case ErrorCode.NoPlayingStreamFound:
                    message = AppResources.ErrNoPlayingStreamFound;
                    break;
                case ErrorCode.CouldNotFindStream:
                    message = AppResources.ErrCouldNotFindStream;
                    break;
                case ErrorCode.NoStreamDownloadUri:
                    message = AppResources.ErrNoStreamDownloadUri;
                    break;
                case ErrorCode.CouldNotDeleteStream:
                    message = AppResources.ErrCouldNotDeleteStream;
                    break;
                case ErrorCode.NetworkNotAvailable:
                    message = AppResources.ErrNetworkNotAvailable;
                    break;
                case ErrorCode.LicenseRequiredForVideo:
                    message = AppResources.ErrLicenseRequiredForVideo;
                    break;
                default:
                    message = AppResources.ErrGenericError;
                    break;
            }

            return message;
        }
    }
}
