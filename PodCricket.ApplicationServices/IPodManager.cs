using PodCricket.Utilities.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices
{
    public interface IPodManager
    {
        bool Loaded { get; set; }

        #region Subscribe/Unsubscribe

        Task<AppResult<bool>> SubscribePodAsync(Guid podId);
        Task<AppResult<bool>> UnSubscribePodAsync(Guid podId);

        #endregion

        #region Get Pod/Stream

        AppResult<Pod> GetPod(Guid id);
        AppResult<PodList> GetSubscribedPod();
        AppResult<PodList> GetSearchedPod(string terms);
        Task<AppResult<bool>> GetStreamList(Pod pod);
        AppResult<Stream> FindStream(string streamId);

        #endregion

        #region Downloading Stream

        AppResult<bool> QueueToDownload(Stream stream, bool retry = false);
        Task<AppResult<bool>> MarkDownloadFinished(Stream stream);
        AppResult<bool> MarkDownloadFailed(Stream stream);
        AppResult<bool> MarkDownloading(Stream stream);
        AppResult<bool> DeleteDownloadedStream(Stream stream);

        #endregion

        #region PlayStream

        AppResult<IList<Stream>> GetPlayList();
        AppResult<bool> QueueToPlay(Stream stream);
        AppResult<bool> RemoveFromPlay(Stream stream);

        AppResult<KeyValuePair<string, string>> GetCurrentPlayingStream();
        AppResult<bool> SaveCurrentPlayingStream(KeyValuePair<string, string> currentPlaying);

        #endregion

        #region Search Pod

        Task<AppResult<PodList>> SearchPod(string terms);
        Task<AppResult<PodList>> SearchPod(string terms, bool refresh);

        #endregion

        #region Persistent

        Task<bool> SavePodMapAsync();
        Task<bool> LoadPodMapAsync();
        bool LoadPodMap();
        bool SavePodMap();
        Task<bool> SaveSettings();
        Task<bool> LoadSettings();

        #endregion
    }
}
