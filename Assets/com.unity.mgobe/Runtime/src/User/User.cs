using System;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;

namespace com.unity.mgobe.src.User {
    public class LoginPara {
        public LoginPara () {
            this.GameId = GameInfo.GameId;
            this.OpenId = GameInfo.OpenId;
        }

        public string GameId { get; }

        public string OpenId { get; }
    }
    public class User : BaseNetUtil {
        public User (BstCallbacks bstCallbacks) : base (bstCallbacks) {

        }

        private static string CreateSignature (string key, string gameId, string openId, ulong timestamp, ulong nonce) {
            var str = $"game_id={gameId}&nonce={nonce}&open_id={openId}&timestamp={timestamp}";

            var hmac = new HMACSHA1 (Encoding.ASCII.GetBytes (key));
            var hashBytes = hmac.ComputeHash (Encoding.ASCII.GetBytes (str));

            var retStr = Convert.ToBase64String (hashBytes);
            return retStr;
        }

        ////////////////////////////////////// 请求 ////////////////////////////////////
        public string Login (LoginPara para, string secretKey, Signature signature, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdLoginReq;
            ulong timestamp;
            ulong nonce;
            string sign;

            if (signature != null) {
                timestamp = signature.Timestamp;
                nonce = signature.Nonce;
                sign = signature.Sign;
            } else {
                timestamp = SdkUtil.GetCurrentTimeSeconds();
                var gRand = new Random ();
                var buffer = new byte[sizeof (UInt32)];
                gRand.NextBytes (buffer);
                nonce = BitConverter.ToUInt32 (buffer, 0);
                sign = CreateSignature (secretKey, para.GameId, para.OpenId, timestamp, nonce);
            }

            var loginReq = new LoginReq {
                GameId = para.GameId,
                OpenId = para.OpenId,
                Sign = sign,
                Timestamp = timestamp,
                Nonce = nonce,
                Platform = 0,
                Channel = 0,
                DeviceId = para.OpenId + "_" + SdkUtil.deviceId,
                Mac = "",
                Imei = ""
            };

            UserStatus.SetStatus (UserStatus.StatusType.Logining);
            var response = new NetResponseCallback (LoginResponse);
            var seq = Send (loginReq, subcmd, response, callback);
            
            Debugger.Log ("Login_Para {0} {1}", loginReq, seq);
            
            return seq;
        }

        public string Logout (LogoutReq para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdLogoutReq;
            var reponse = new NetResponseCallback (LogoutResponse);
            var seq = this.Send (para, subcmd, reponse, callback);
            Debugger.Log ("Logout_Para {0} {1}", para, seq);

            return seq;
        }

        public string ChangeUserState (ChangeCustomPlayerStatusReq para, Action<ResponseEvent> callback) {
            const int subcmd = (int) ProtoCmd.ECmdChangePlayerStateReq;
            var reponse = new NetResponseCallback (ChangeUserStateResponse);
            var seq = this.Send (para, subcmd, reponse, callback);
            Debugger.Log ("ChangeUserState_Para {0} {1}", para, seq);
            return seq;
        }

        ////////////////////////////////////// 响应 ////////////////////////////////////
        private void LoginResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            if (send) {
                UserStatus.SetStatus (UserStatus.StatusType.Logout);
            }

            var rsp = (LoginRsp) res.Body;
            var eve = new ResponseEvent (res.RspWrap1.ErrCode, res.RspWrap1.ErrMsg, res.RspWrap1.Seq, rsp);
            
            Debugger.Log ("LoginResponse {0}", eve);
            
            NetClient.HandleSuccess (eve.Code, () => {
                if (eve.Code == ErrCode.EcOk) {
                    RequestHeader.AuthKey = rsp.Token;
                    RequestHeader.PlayerId = rsp.PlayerId;
                    var messageData = rsp;

                    // 更新状态
                    UserStatus.SetStatus (UserStatus.StatusType.Login);

                    // 设置 PlayerInfo
                    if (string.IsNullOrEmpty (GamePlayerInfo.GetInfo ().Id)) {
                        GamePlayerInfo.SetInfo (messageData.PlayerId);
                    }
                }
            });
            UserStatus.SetErrCode (eve.Code, eve.Msg);
            callback?.Invoke (eve);
        }

        private static void LogoutResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var wrap1 = res.RspWrap1;
            var eve = new ResponseEvent (wrap1.ErrCode, wrap1.ErrMsg, wrap1.Seq, null);
            
            Debugger.Log ("LogoutResponse {0}", eve);

            void HandleSuccess () {
                RequestHeader.AuthKey = null;
                RequestHeader.PlayerId = null;

                UserStatus.SetStatus (UserStatus.StatusType.Logout);

                var playerInfo = new PlayerInfo { Id = null };
                GamePlayerInfo.SetInfo (playerInfo);
            }

            NetClient.HandleSuccess (eve.Code, HandleSuccess);
            callback?.Invoke (eve);
        }

        private void ChangeUserStateResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var wrap1 = res.RspWrap1;
            var rsp = (ChangeCustomPlayerStatusRsp)res.Body;
            var eve = new ResponseEvent (wrap1.ErrCode, wrap1.ErrMsg, wrap1.Seq, rsp);
            
            Debugger.Log ("ChangeUserStateResponse {0}", eve);

            callback?.Invoke (eve);
        }
    }
}