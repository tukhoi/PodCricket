using PodCricket.Utilities.Helpers;
using PodCricket.Utilities.Helpers.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodCricket.ApplicationServices
{
    public class AppConfig
    {
        public static SerializationType DEFAULT_SERIALIZATION_TYPE = SerializationType.JsonSerialization;
        public static string SUBSCRIBED_PODS_FILE = "map.store";
        public static string SETTING_FILE = "setting.store";

        public static string CURRENT_PLAYING_STREAM_CONFIG_KEY = "CurrentPlayingStream";
        public static string LAST_KNOWN_POSITION_CONFIG_KEY = "LastKnownStreamPostion";

        public static string[] STREAM_SUPPORTED_TYPES = { "audio/x-m4a", "audio/mpeg", "audio/mp3", "audio/x-mp3" };
        public static string TEMP_DOWNLOAD_ROOT = "shared/transfers/";
        public static string PODCAST_DIRECTORY = "shared/media/";

        public static string PRO_VERSION = "Pro Version";

        public static bool Loaded = false;

        #region User configurable

        public string GetConfigKey(BooleanConfigName name)
        { 
            switch(name){
                case BooleanConfigName.SubscribedPodsAsGrid:
                    return "SubscribePodsAsGrid";
                case BooleanConfigName.QueueStreamToPlayListAfterDownloading:
                    return "QueueStreamToPlayListAfterDownloading";
                case BooleanConfigName.PlayStreamInApp:
                    return "PlayStreamInApp";
                case BooleanConfigName.AutoResumeStream:
                    return "AutoResumeStream";
                case BooleanConfigName.AutoRemoveCompletedDownload:
                    return "AutoRemoveCompletedDownload";
                case BooleanConfigName.JustDownloadWithWifi:
                    return "JustDownloadWithWifi";
                case BooleanConfigName.AutoBackup:
                    return "AutoBackup";
                case BooleanConfigName.KeepScreenOn:
                    return "KeepScreenOn";
            }

            return string.Empty;
        }

        public enum BooleanConfigName
        { 
            SubscribedPodsAsGrid,
            QueueStreamToPlayListAfterDownloading,
            PlayStreamInApp,
            AutoResumeStream,
            AutoRemoveCompletedDownload,
            JustDownloadWithWifi,
            AutoBackup,
            KeepScreenOn
        }

        public bool SubscribedPodsAsGrid
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.SubscribedPodsAsGrid), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.SubscribedPodsAsGrid), value);
            }
        }

        public bool QueueStreamToPlayListAfterDownloading {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.QueueStreamToPlayListAfterDownloading), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.QueueStreamToPlayListAfterDownloading), value);
            }
        }

        public bool PlayStreamInApp {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.PlayStreamInApp), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.PlayStreamInApp), value);
            }
        }

        public bool AutoResumeStream
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.AutoResumeStream), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.AutoResumeStream), value);
            }
        }

        public bool AutoRemoveCompletedDownload 
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.AutoRemoveCompletedDownload), false);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.AutoRemoveCompletedDownload), value);
            }
        }

        public bool JustDownloadWithWifi 
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.JustDownloadWithWifi), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.JustDownloadWithWifi), value);
            }
        }

        public bool AutoBackup 
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.AutoBackup), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.AutoBackup), value);
            }
        }

        public bool KeepScreenOn
        {
            get
            {
                return GetConfig(GetConfigKey(BooleanConfigName.KeepScreenOn), true);
            }
            set
            {
                SetConfig(GetConfigKey(BooleanConfigName.KeepScreenOn), value);
            }
        }

        private bool GetConfig(string key, bool defaultValue)
        {
            string should;
            if (StorageHelper.LoadConfig(key, out should))
                return bool.Parse(should);
            return defaultValue;
        }

        private void SetConfig(string key, bool value)
        {
            StorageHelper.SaveConfig(key, value.ToString());
        }

        #endregion

        private static AppConfig _instance;

        public static AppConfig Instance()
        {
            if (_instance == null)
                _instance = new AppConfig();

            return _instance;
        }
    }
}
