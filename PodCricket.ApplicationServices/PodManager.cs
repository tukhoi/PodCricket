using PodCricket.ApplicationServices.Parser;
using PodCricket.ApplicationServices.Search;
using PodCricket.Utilities.Helpers;
using PodCricket.Utilities.Helpers.Serialization;
using PodCricket.Utilities.Tasks;
using PodCricket.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Windows.Storage;
using Microsoft.Phone.BackgroundTransfer;
using PodCricket.Utilities.Toolkit;
using PodCricket.ApplicationServices.Helper;
using PodCricket.Utilities.AppLicense;

namespace PodCricket.ApplicationServices
{
    public sealed class PodManager : IPodManager
    {
        private PodDictionary _podMap;
        private StreamList _playQueue;

        private SerializationHelperManager _serializationManager;
        private static PodManager _instance;

        public bool Loaded { get; set; }

        public PodManager()
        {
            Initialize();
        }

        public static IPodManager Instance()
        {
            if (_instance == null)
                _instance = new PodManager();

            return _instance;
        }

        #region Subscribe/UnSubscribe

        public async Task<AppResult<bool>> SubscribePodAsync(Guid podId)
        {
            if (_podMap.ContainsKey(podId) && _podMap[podId].Subscribed)
                return new AppResult<bool>(false, ErrorCode.PodSubscribed);

            if (!_podMap.ContainsKey(podId))
                return new AppResult<bool>(false, ErrorCode.CouldNotFindPod);

            var pod = _podMap[podId];
            pod.Subscribed = true;

            await SavePodMapAsync();

            return new AppResult<bool>(true);
        }

        public async Task<AppResult<bool>> UnSubscribePodAsync(Guid podId)
        {
            if (!_podMap.ContainsKey(podId))
                return new AppResult<bool>(false, ErrorCode.CouldNotFindPod);

            var pod = _podMap[podId];
            pod.Subscribed = false;

            await SavePodMapAsync();

            return new AppResult<bool>(true);
        }

        #endregion

        #region Get Pod/Stream

        public AppResult<Pod> GetPod(Guid id)
        {
            if (!_podMap.ContainsKey(id))
                return new AppResult<Pod>(ErrorCode.CouldNotFindPod);

            var pod = _podMap[id];
            if (pod != null)
                return new AppResult<Pod>(pod);

            return new AppResult<Pod>(ErrorCode.GetPodError);
        }

        public AppResult<PodList> GetSubscribedPod()
        {
            var subscribedPods = _podMap.Values.Where(p => p.Subscribed).ToList<Pod>();

            if (subscribedPods == null)
                return new AppResult<PodList>(ErrorCode.GetPodError);
            if (subscribedPods.Count == 0)
                return new AppResult<PodList>(ErrorCode.NoPodSubscribed);

            return new AppResult<PodList>(new PodList(subscribedPods));
        }

        public AppResult<PodList> GetSearchedPod(string terms)
        {
            var searchedPods = _podMap.Values.Where(p => !p.Subscribed && p.SearchTerms.Equals(terms)).ToList<Pod>();

            if (searchedPods == null)
                return new AppResult<PodList>(ErrorCode.GetPodError);
            if (searchedPods.Count == 0)
                return new AppResult<PodList>(ErrorCode.NoPodSubscribed);

            return new AppResult<PodList>(new PodList(searchedPods));
        }

        public async Task<AppResult<bool>> GetStreamList(Pod pod)
        {
            if (pod == null) return new AppResult<bool>(ErrorCode.PodPassedInNull);
            if (!ConnectivityHelper.NetworkAvailable()) return new AppResult<bool>(ErrorCode.NetworkNotAvailable);

            var persistentPod = _podMap[pod.Id];
            if (persistentPod == null) return new AppResult<bool>(ErrorCode.PodPassedInNull);

            try
            {
                string data = await WebTasks.GetRawResult(persistentPod.FeedUrl);

                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                MemoryStream stream = new MemoryStream(byteArray);

                var reader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(reader);

                bool licenseRequired = false;
                persistentPod.StreamList = new StreamList().GetFrom(feed.Items, persistentPod, ref licenseRequired);
                persistentPod.IsVideo = licenseRequired;
                await SavePodMapAsync();

                if (licenseRequired && !LicenseHelper.Purchased(AppConfig.PRO_VERSION))
                    return new AppResult<bool>(true, ErrorCode.LicenseRequiredForVideo);

                return new AppResult<bool>(true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public AppResult<Stream> FindStream(string streamId)
        {
            Stream stream = null;

            foreach (var pod in _podMap.Values)
            {
                stream = pod.StreamList.FirstOrDefault(s => s.Id.Equals(streamId));
                if (stream != null) break;
            }

            if (stream == null)
                return new AppResult<Stream>(ErrorCode.CouldNotFindStream);
            return new AppResult<Stream>(stream);
        }

        #endregion

        #region Downloading Stream

        public AppResult<bool> QueueToDownload(Stream stream, bool retry = false)
        {
            if (stream == null) return new AppResult<bool>(ErrorCode.CouldNotFindStream);
            if (stream.IsVideo && !LicenseHelper.Purchased(AppConfig.PRO_VERSION))
                return new AppResult<bool>(ErrorCode.LicenseRequiredForVideo);
            if (stream.DownloadUri == null) return new AppResult<bool>(ErrorCode.NoStreamDownloadUri);

            if (retry)
            {
                var requested = BackgroundTransferService.Requests.FirstOrDefault(r => r.RequestUri.ToString().Equals(stream.DownloadUri.ToString()));
                if (requested != null)
                    BackgroundTransferService.Remove(requested);
            }

            BackgroundTransferRequest request = new BackgroundTransferRequest(stream.DownloadUri, new Uri(StreamHelper.GetLocalDownloadPath(stream.DownloadUri), UriKind.Relative))
            {
                TransferPreferences = AppConfig.Instance().JustDownloadWithWifi ? TransferPreferences.AllowBattery : TransferPreferences.AllowCellularAndBattery,
                Tag = stream.Id
            };

            request.TransferStatusChanged += request_TransferStatusChanged;

            try
            {
                BackgroundTransferService.Add(request);

                var pod = _podMap[stream.PodId];
                if (pod == null) return new AppResult<bool>(ErrorCode.CouldNotSaveDownloadedStream);

                var persistentStream = pod.StreamList.FirstOrDefault(s => s.Id == stream.Id);
                if (persistentStream == null) new AppResult<bool>(ErrorCode.CouldNotSaveDownloadedStream);

                if (!pod.Subscribed)
                    pod.Subscribed = true;

                persistentStream.DownloadState = DownloadState.Downloading;
                Task.Run(() => SavePodMapAsync());
            }
            catch (Exception ex) // An exception is thrown if this transfer is already requested.
            {
                return new AppResult<bool>(ErrorCode.StreamAlreadyInDownloading);
            }

            return new AppResult<bool>(true);
        }

        public async Task<AppResult<bool>> MarkDownloadFinished(Stream stream)
        {
            if (stream == null)
                return new AppResult<bool>(ErrorCode.NoStreamDownloadingFound);

            var moved = await StorageHelper.MoveFile(StreamHelper.GetLocalDownloadPath(stream.DownloadUri), StreamHelper.GetPodcastPath(stream.DownloadUri));

            if (!moved)
                return new AppResult<bool>(ErrorCode.CouldNotSaveDownloadedStream);

            var pod = _podMap[stream.PodId];
            if (pod == null) return new AppResult<bool>(ErrorCode.CouldNotSaveDownloadedStream);

            var persistentStream = pod.StreamList.FirstOrDefault(s => s.Id == stream.Id);
            if (persistentStream == null) new AppResult<bool>(ErrorCode.CouldNotSaveDownloadedStream);

            persistentStream.DownloadState = DownloadState.Downloaded;
            persistentStream.FinishedDownloadDate = DateTime.Now;

            await SavePodMapAsync();
            return new AppResult<bool>(true);
        }

        public AppResult<bool> MarkDownloadFailed(Stream stream)
        {
            if (stream == null)
                return new AppResult<bool>(ErrorCode.NoStreamDownloadingFound);

            stream.DownloadState = DownloadState.Failed;

            Task.Run(() => SavePodMapAsync());
            return new AppResult<bool>(true);
        }

        public AppResult<bool> MarkDownloading(Stream stream)
        {
            if (stream == null)
                return new AppResult<bool>(ErrorCode.NoStreamDownloadingFound);

            stream.DownloadState = DownloadState.Downloading;

            Task.Run(() => SavePodMapAsync());
            return new AppResult<bool>(true);
        }

        public AppResult<bool> DeleteDownloadedStream(Stream stream) 
        {
            if (stream == null)
                return new AppResult<bool>(ErrorCode.CouldNotFindStream);

            var deleted = StorageHelper.DeleteDownloadedStream(StreamHelper.GetPodcastPath(stream.DownloadUri));
            if (deleted)
            {
                var pod = _podMap.Values.FirstOrDefault(p => p.Id == stream.PodId);
                if (pod == null) return new AppResult<bool>(ErrorCode.CouldNotFindPod);

                var persistentStream = pod.StreamList.FirstOrDefault(s => s.Id == stream.Id);
                if (persistentStream == null) return new AppResult<bool>(ErrorCode.CouldNotFindStream);

                persistentStream.DownloadState = DownloadState.None;
                Task.Run(() => SavePodMapAsync());
                return new AppResult<bool>(true);
            }

            return new AppResult<bool>(ErrorCode.CouldNotDeleteStream);
        }

        #endregion

        #region PlayStream

        public AppResult<IList<Stream>> GetPlayList()
        {
            if (_playQueue.Count == 0)
                return new AppResult<IList<Stream>>(ErrorCode.NoStreamInPlayList);

            return new AppResult<IList<Stream>>(_playQueue.ToList<Stream>());
        }

        public AppResult<bool> QueueToPlay(Stream stream)
        {
            if (_playQueue.Contains(stream))
                return new AppResult<bool>(ErrorCode.StreamAlreadyInPlayingList);
            if (stream.IsVideo && !LicenseHelper.Purchased(AppConfig.PRO_VERSION))
                return new AppResult<bool>(ErrorCode.LicenseRequiredForVideo);

            _playQueue.Add(stream);
            return new AppResult<bool>(true);
        }

        public AppResult<bool> RemoveFromPlay(Stream stream) 
        {
            var playingStream = _playQueue.FirstOrDefault(s => s.Id == stream.Id);
            if (playingStream == null)
                return new AppResult<bool>(ErrorCode.NoPlayingStreamFound);

            _playQueue.Remove(playingStream);
            return new AppResult<bool>(true);
        }

        public AppResult<KeyValuePair<string, string>> GetCurrentPlayingStream()
        {
            string currentPlayingStream = string.Empty;
            string lastKnownPosition = string.Empty;
            StorageHelper.LoadConfig(AppConfig.CURRENT_PLAYING_STREAM_CONFIG_KEY, out currentPlayingStream);
            if (!string.IsNullOrEmpty(currentPlayingStream))
                StorageHelper.LoadConfig(AppConfig.LAST_KNOWN_POSITION_CONFIG_KEY, out lastKnownPosition);

            return new AppResult<KeyValuePair<string,string>>(new KeyValuePair<string, string>(currentPlayingStream, lastKnownPosition));
        }

        public AppResult<bool> SaveCurrentPlayingStream(KeyValuePair<string, string> currentPlaying)
        {
            if (currentPlaying.Key == null) return new AppResult<bool>(ErrorCode.NoPlayingStreamFound);

            StorageHelper.SaveConfig(AppConfig.CURRENT_PLAYING_STREAM_CONFIG_KEY, currentPlaying.Key);
            StorageHelper.SaveConfig(AppConfig.LAST_KNOWN_POSITION_CONFIG_KEY, currentPlaying.Value);

            return new AppResult<bool>(true);
        }

        #endregion

        #region Search Pod

        public async Task<AppResult<PodList>> SearchPod(string terms)
        {
            return await SearchPod(terms, true);
        }

        public async Task<AppResult<PodList>> SearchPod(string terms, bool refresh)
        {
            if (!refresh)
            {
                var searchedPods = _podMap.Values.Where(p => p.SearchTerms.Equals(terms)).ToList();
                if (searchedPods.Count > 0)
                    return new AppResult<PodList>(new PodList(searchedPods));
                return new AppResult<PodList>(ErrorCode.NoOfflineSearchedPodFound);
            }

            var parser = new TunesParser();
            var pods = await parser.GetFeed(terms);

            if (pods == null || pods.Count == 0)
                return new AppResult<PodList>(ErrorCode.NoOnlineSearchedPodFound);

            //Update back to _podMap
            pods.ForEach(p => {

                p.SearchTerms = terms.Trim();
                var persistentPod = _podMap.Values.FirstOrDefault(pod => pod.Name.Equals(p.Name));

                if (persistentPod == null)
                {
                    p.Id = Guid.NewGuid();
                    p.Subscribed = false;

                    _podMap.Add(p.Id, p);
                }
                else
                {
                    p.Id = persistentPod.Id;
                    p.Subscribed = persistentPod.Subscribed;
                }
            });

            return new AppResult<PodList>(pods);
        }

        #endregion

        #region Persistent

        public async Task<bool> SavePodMapAsync()
        {
            if (_podMap.Count == 0)
                return false;

            //var streams = BackgroundTransferService.Requests.Select(r => r.Tag.ToString()).ToList();

            //var filteredPodMap = _podMap.CloneToSave(streams);
            //if (filteredPodMap == null || filteredPodMap.Count == 0)
            //    return false;

            var subscribedPods = _podMap.Values.Where(p => p.Subscribed).ToList();
            var copied = new PodDictionary().GetFrom(subscribedPods);

            return await UpdateSerializedCopyAsync(copied, AppConfig.SUBSCRIBED_PODS_FILE);
        }

        public async Task<bool> LoadPodMapAsync()
        {
            //if (_podMap != null && _podMap.Count > 0) return true;
            //if (PodDictionary.Loaded) return true;
            if (this.Loaded) return true;

            var map = await ReadSerializedCopyAsync<PodDictionary>(AppConfig.SUBSCRIBED_PODS_FILE);
            if (map != null){
                _podMap.UpdateFrom(map);
                this.Loaded = true;
                //PodDictionary.Loaded = true;
                return true;
            }
            return false;
        }

        public bool SavePodMap()
        {
            if (_podMap.Count == 0)
                return false;

            var subscribedPods = _podMap.Values.Where(p => p.Subscribed).ToList();
            var copied = new PodDictionary().GetFrom(subscribedPods);

            return UpdateSerializeCopy(copied, AppConfig.SUBSCRIBED_PODS_FILE, AppConfig.Instance().AutoBackup);
        }

        public bool LoadPodMap()
        {
            var podMap = ReadSerializeCopy<PodDictionary>(AppConfig.SUBSCRIBED_PODS_FILE, AppConfig.Instance().AutoBackup);

            if (podMap == null)
            {
                System.Threading.Tasks.Task.Factory.StartNew(() => StorageHelper.DeleteFile(AppConfig.SUBSCRIBED_PODS_FILE));
                return false;
            }

            _podMap.UpdateFrom(podMap);
            return true;
         
        }

        public async Task<bool> SaveSettings()
        {
            return await UpdateSerializedCopyAsync(AppConfig.Instance(), AppConfig.SETTING_FILE);
        }

        public async Task<bool> LoadSettings()
        {
            if (AppConfig.Loaded) return true;

            var persistentSetting = await ReadSerializedCopyAsync<AppConfig>(AppConfig.SETTING_FILE);
            if (persistentSetting != null)
            {
                AppConfig.Instance().AutoRemoveCompletedDownload = persistentSetting.AutoRemoveCompletedDownload;
                AppConfig.Instance().JustDownloadWithWifi = persistentSetting.JustDownloadWithWifi;
                AppConfig.Instance().PlayStreamInApp = persistentSetting.PlayStreamInApp;
                AppConfig.Instance().QueueStreamToPlayListAfterDownloading = persistentSetting.QueueStreamToPlayListAfterDownloading;

                AppConfig.Loaded = true;
                return true;
            }

            return false;
        }

        #endregion

        #region Private

        void request_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            if (e.Request.TransferStatus == TransferStatus.Completed)
            {
                var streamResult = FindStream(e.Request.Tag);
                if (!streamResult.HasError)
                {
                    var stream = streamResult.Target;
                    Task.Run(() => MarkDownloadFinished(stream));

                    if (AppConfig.Instance().QueueStreamToPlayListAfterDownloading)
                    {
                        var queued = _playQueue.FirstOrDefault(s => s.Id == stream.Id);
                        if (queued != null)
                            _playQueue.Remove(queued);
                        this.QueueToPlay(stream);
                    }
                }

                if (AppConfig.Instance().AutoRemoveCompletedDownload)
                {
                    var request = BackgroundTransferService.Find(e.Request.RequestId);
                    if (request != null)
                        BackgroundTransferService.Remove(request);
                }
            }
        }

        private void Initialize()
        {
            _podMap = new PodDictionary();
            _playQueue = new StreamList();
            Loaded = false;

            _serializationManager = new SerializationHelperManager();
        }

        private async Task<bool> UpdateSerializedCopyAsync(object obj, string fileName)
        {
            try {
                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                var localStream = await StorageHelper.OpenStreamForWriteAsync(fileName, AppConfig.Instance().AutoBackup);

                return await serializationHelper.SerializeAsync(localStream, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool UpdateSerializeCopy(object obj, string fileName, bool createBackup = true)
        {
            try
            {
                var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
                var localStream = StorageHelper.GetFileStream(fileName);

                var serialized = serializationHelper.Serialize(localStream, obj);
                if (serialized && createBackup)
                {
                    StorageHelper.CopyFile(fileName, fileName + ".bak");
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        private async Task<T> ReadSerializedCopyAsync<T>(string fileName) 
            where T:class
        {
            var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
            var localStream = await StorageHelper.OpenStreamForReadAsync(fileName, AppConfig.Instance().AutoBackup);

            //There's no map file. User hasn't registered any pod
            if (localStream == null) return default(T);

            T graph = default(T);
            try
            {
                graph = await serializationHelper.DeserializeAsync<T>(localStream);
            }
            catch (Exception ex)
            {
            }

            if (graph == default(T))
                Task.Run(() => StorageHelper.DeleteAsync(AppConfig.SUBSCRIBED_PODS_FILE, AppConfig.Instance().AutoBackup));

            return graph;
        }

        private T ReadSerializeCopy<T>(string fileName, bool tryBackup = true)
            where T : class
        {
            var stream = StorageHelper.GetFileStream(fileName);
            if (stream == null && tryBackup)
                stream = StorageHelper.GetFileStream(fileName + ".bak");

            if (stream == null) return default(T);

            var serializationHelper = _serializationManager.GetSerializationHelper(AppConfig.DEFAULT_SERIALIZATION_TYPE);
            T graph = default(T);
            try
            {
                graph = serializationHelper.Deserialize<T>(stream);
            }
            catch (Exception ex)
            {
            }

            return graph;
        }

        private void TransferCanceled(object sender, BackgroundTransferEventArgs e)
        {
            var monitor = sender as DownloadMonitor;
            if (monitor == null) return;

            var stream = monitor.Tag as PodCricket.ApplicationServices.Stream;
            if (stream == null) return;

            UnSubscribeMonitor(monitor);

            this.MarkDownloadFailed(stream);
        }

        private void SubscribeMonitor(DownloadMonitor monitor)
        {
            UnSubscribeMonitor(monitor);
            monitor.Failed += TransferCanceled;
        }

        private void UnSubscribeMonitor(DownloadMonitor monitor)
        {
            monitor.Failed -= TransferCanceled;
        }

        #endregion
    }
}
