
using PodCricket.Utilities.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices
{
    public class AppResult<T> : BaseResult<T, ErrorCode>
    {
        public AppResult(T target, ErrorCode error) : base(target, error)
        {
        }

        public AppResult(T target) : base(target, ErrorCode.None)
        {
        }

        public AppResult(ErrorCode error) : base(error)
        {
        }

        public bool HasError { get {
            return Error != ErrorCode.None;
        } }

        public string ErrorMessage { get {
            return GetErrorMessage(Error);
        } }

        public string GetErrorMessage(ErrorCode error)
        {
            var message = string.Empty;
            switch (error)
            { 
                case ErrorCode.None:
                    message = string.Empty;
                    break;
                case ErrorCode.PodSubscribed:
                    message = "This podcast is already subscribed";
                    break;
                case ErrorCode.CouldNotFindPod:
                    message = "Podcast could not be found";
                    break;
                case ErrorCode.GetPodError:
                    message = "There's an error while get this podcast";
                    break;
                case ErrorCode.NoPodSubscribed:
                    message = "No such podcast subscribed";
                    break;
                case ErrorCode.PodPassedInNull:
                    message = "Podcast was not found";
                    break;
                case ErrorCode.NoOfflineSearchedPodFound:
                    message = "This podcast was not searched before";
                    break;
                case ErrorCode.NoOnlineSearchedPodFound:
                    message = "Could not search for this podcast";
                    break;
                case ErrorCode.StreamAlreadyInDownloading:
                    message = "You already requested to download this post";
                    break;
                case ErrorCode.NoStreamDownloadingFound:
                    message = "No such post is downloading";
                    break;
                case ErrorCode.CouldNotSaveDownloadedStream:
                    message = "Post downloaded but couldn't be saved";
                    break;
                case ErrorCode.StreamAlreadyInPlayingList:
                    message = "This post already queued in play list";
                    break;
                case ErrorCode.NoPlayingStreamFound:
                    message = "There's no such post in play list";
                    break;
                case ErrorCode.CouldNotFindStream:
                    message = "Post not found";
                    break;
                case ErrorCode.NoStreamDownloadUri:
                    message = "No download link found";
                    break;
                case ErrorCode.CouldNotDeleteStream:
                    message = "Post not deleted";
                    break;
                case ErrorCode.NetworkNotAvailable:
                    message = "Network not found";
                    break;
                default:
                    message = "There's an error occured";
                    break;
            }

            return message;
        }
    }


}
