using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices
{
    public enum ErrorCode
    {
        None = 0,
        PodSubscribed=1,
        CouldNotFindPod,
        GetPodError,
        NoPodSubscribed,
        PodPassedInNull,
        NoOfflineSearchedPodFound,
        NoOnlineSearchedPodFound,
        StreamAlreadyInDownloading,
        NoStreamDownloadingFound,
        CouldNotSaveDownloadedStream,
        //CouldNotQueueDownloading,
        StreamAlreadyInPlayingList,
        NoPlayingStreamFound,
        NoStreamInPlayList,
        CouldNotFindStream,
        NoStreamDownloadUri,
        CouldNotDeleteStream,
        NetworkNotAvailable,
        LicenseRequiredForVideo
    }
}
