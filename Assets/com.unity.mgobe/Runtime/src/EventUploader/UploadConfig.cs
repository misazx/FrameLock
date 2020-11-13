using System;
using UnityEngine;

namespace com.unity.mgobe.src.EventUploader
{
    [Serializable]
    public static class UploadConfig
    {
        private static int _reportInterval = 10000;
        private static bool _disableReport = false;
        private static bool _disableFrameReport = false;
        private static bool _disableReqReport = false;
        private static int _minReportSize = 10;
        
        public static int ReportInterval
        {
            get => _reportInterval;
            set => _reportInterval = value;
        }
        
        public static bool DisableReport
        {
            get => _disableReport;
            set => _disableReport = value;
        }
        
        public static bool DisableFrameReport
        {
            get => _disableFrameReport;
            set => _disableFrameReport = value;
        }
        
        public static bool DisableReqReport
        {
            get => _disableReqReport;
            set => _disableReqReport = value;
        }
        
        public static int MinReportSize
        {
            get => _minReportSize;
            set => _minReportSize = value;
        }
    }
}