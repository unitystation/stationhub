using System.Collections.Generic;

namespace UnitystationLauncher.Models{

    public class DownloadManager
    {
        private List<Download> downloads = new List<Download>();

        IEnumerable<Download> Downloads => downloads;

        public void AddDownload(Download download)
        {
            downloads.Add(download);
        }
    }
}