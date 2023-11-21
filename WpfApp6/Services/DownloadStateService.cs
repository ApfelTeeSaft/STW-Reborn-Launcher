using System;

namespace WpfApp6.Services
{
    public class DownloadStateService
    {
        private int _downloadProgress;
        private bool _isDownloadInProgress;
        private bool _isCancelButtonVisible;

        public event Action<int> ProgressChanged;

        public int DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                _downloadProgress = value;
                ProgressChanged?.Invoke(value);
            }
        }

        public bool IsDownloadInProgress
        {
            get => _isDownloadInProgress;
            set => _isDownloadInProgress = value;
        }

        public bool IsCancelButtonVisible
        {
            get => _isCancelButtonVisible;
            set => _isCancelButtonVisible = value;
        }

        public void ResetProgress()
        {
            _downloadProgress = 0;
            ProgressChanged?.Invoke(0);
            _isDownloadInProgress = false;
            _isCancelButtonVisible = false;
        }
    }
}