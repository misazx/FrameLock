using System;
using System.Collections.Generic;
using com.unity.mgobe.src.Util;
using UnityEngine;

namespace com.unity.mgobe.src.EventUploader {
    public static class AnimateUtil {
        private static long _lastTime = 0;
        public static long frameRate = 0;

        private static readonly Action<long> Callback = (timestamp) => {
            if (Math.Abs (AnimateUtil._lastTime) < 0.00001) {
                AnimateUtil._lastTime = timestamp;
                return;
            }

            var now = timestamp;
            var delta = now - AnimateUtil._lastTime;
            var frameRate = Convert.ToInt64 (1000 / (delta + 0.000001));

            AnimateUtil.frameRate = frameRate;
            AnimateUtil._lastTime = now;

            StatCallbacks.onRenderFrameRate?.Invoke (delta);
        };
        public static void Run (long timestamp) {
            Callback?.Invoke (timestamp);

        }
    }
    public class EventUpload {
        private static bool _isInited = false;
        private static readonly BeaconSdk Beacon = new BeaconSdk ();

        private static List<BaseEvent> _queue = new List<BaseEvent> ();
        private static int _validSeq = 0;

        public static void StartEventUpload () {
            var timer = new Timer ();
            timer.SetTimer (() => {
                if (!_isInited) return;
                if (_queue.Count == 0) return;
                PushEvent<ReqEventParam> (_queue);
                _queue.Clear ();
            }, UploadConfig.ReportInterval);
            AnimateUtil.Run (0);
        }

        public static void Init (string openId, string playerId) {
            
            Debugger.Log("EventUploader");

            Beacon.Init ();
            Util.SetOpenId (openId);

            _isInited = true;

            StartEventUpload();
        }

        // 上报接口调用结果
        public static void PushRequestEvent (ReqEventParam param) {
            if (!_isInited) {
                return;
            }

            if (UploadConfig.DisableReqReport)
            {
                return;
            }
            
            AddEventToQueue (param, Events.Request);
        }

        // 上报心跳时延
        public static void PushPingEvent (PingEventParam param) {
            if (!_isInited) {
                return;
            }
            StatCallbacks.onPingTime?.Invoke (param.Time);
            AddEventToQueue (param, Events.Ping);
        }

        // 上报帧广播间隔
        public static void PushFrameRateEvent (long deltaTime) {
            if (!_isInited) {
                return;
            }

            if (UploadConfig.DisableFrameReport)
            {
                return;
            }

            var param = new FrameRateEventParam {
                FrRt = AnimateUtil.frameRate,
                ReFt = deltaTime,
            };
            AddEventToQueue (param, Events.FrameRate);
        }

        private static void AddEventToQueue (BaseEventParam param, string eventName) {
            param.Sv = SdKonfig.Version;
            param.Pi = Player.Id;
            param.Gi = GameInfo.GameId;
            param.Sc = 9;
            _queue.Add (new BaseEvent (param, eventName));
        }

        private static void PushEvent<T> (IEnumerable<BaseEvent> events, bool force = false, Action callback = null) {
            if (!force && !SdkStatus.IsInited ()) return;

            if (UploadConfig.DisableReport)
            {
                return;
            }
            
            BeaconSdk.OnEvents (events, null);
        }
    }
}