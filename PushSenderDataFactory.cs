using System;
using System.Reflection;
using log4net;

namespace Notification.Utilities {


    // Interface
    public interface ISelectPushSenderType {
        PushMessage PreparePushMessage(NotificationQueueRequest queueRequest, RecipientAddress pushId);
    }

    //Class
#pragma warning disable S1118 // Utility classes should not have public constructors
    class PushSenderDataFactory {
#pragma warning restore S1118 // Utility classes should not have public constructors

        static public ISelectPushSenderType CreateSelectedObject(NotificationQueueRequest queueRequest) {
            ISelectPushSenderType selectedObj = null;
            if (queueRequest.PushPlatformType == PlatformType.Android) {
                selectedObj = new AndroidPushMessage();
            }
            if (queueRequest.PushPlatformType == PlatformType.IOS || queueRequest.PushPlatformType == PlatformType.Other) {
                selectedObj = new IosPushMessage();
            }
            return selectedObj;
        }
    }

    public class AndroidPushMessage : ISelectPushSenderType {
        private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PushMessage PreparePushMessage(NotificationQueueRequest queueRequest, RecipientAddress pushId) {

            int timeToLive = ConfigurationHelper.GetConfiguration<int>(Constants.PushFireBaseTimeToLive, Constants.PushFireBaseTimeToLiveDefault);

            PushMessage pushRequest = new PushMessage
            {
                to = pushId?.Adress, //"eRArK-0TwkM:APA91bH7APab_SVh2NeOqvlC0IQu9jcutjEIzCHwu8HATlLI_5mSdLIC6eDkH3WUlcklG-g9nlwtiIn2DGr5XJt1j4cx-Yl7fVXeNsh__LH6iJ8FcsQRcxveMwp7HOYejH5RJq6nq73rM-w_v3dwqmZpgvCBGaiDEg"; //for a topic to": "/topics/foo-bar"
                mutable_content = true
            };
            PushExtraData data = new PushExtraData
            {
                tranCode = queueRequest.TransactionCode,
                pushResponseId = queueRequest.ResponseID,
                badge = queueRequest.PushBadge
            };

            string subject = queueRequest.Subject;
            string content = queueRequest.Content;

            try {
                content = CommonOperation.EncodeJavaUnicodes(queueRequest.Content);
                subject = CommonOperation.EncodeJavaUnicodes(queueRequest.Subject);
            }
            catch (Exception ex) {
                log.WarnFormat("PreparePushMessage-EncodeJavaUnicodes Error :{0} ; Content:{1}, Subject:{2}, TransactionId:{3}, Customer : {4}", ex, queueRequest.Content, queueRequest.Subject, queueRequest.TransactionId, queueRequest.CustomerNumber);
            }
            data.title = subject;
            data.body = content;

            data.sound = "default";
            var credentials = PushSenderHelper.GetRsaCredentials();
            data.pushRegisterId = PushSenderHelper.EncryptPushRegisterId(queueRequest.PushRegisterId.ToString(), credentials.Password);

            data.tranData = JsonConvert.DeserializeObject(queueRequest.TranData);
            pushRequest.time_to_live = timeToLive;
            pushRequest.data = data;

            return pushRequest;
        }
    }

    public class IosPushMessage : ISelectPushSenderType {
        private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PushMessage PreparePushMessage(NotificationQueueRequest queueRequest, RecipientAddress pushId) {

            int timeToLive = ConfigurationHelper.GetConfiguration<int>(Constants.PushFireBaseTimeToLive, Constants.PushFireBaseTimeToLiveDefault);

            PushMessage pushRequest = new PushMessage
            {
                to = pushId?.Adress //"eRArK-0TwkM:APA91bH7APab_SVh2NeOqvlC0IQu9jcutjEIzCHwu8HATlLI_5mSdLIC6eDkH3WUlcklG-g9nlwtiIn2DGr5XJt1j4cx-Yl7fVXeNsh__LH6iJ8FcsQRcxveMwp7HOYejH5RJq6nq73rM-w_v3dwqmZpgvCBGaiDEg"; //for a topic to": "/topics/foo-bar"
            };
            PushMessageData notification = new PushMessageData();

            string subject = queueRequest.Subject;
            string content = queueRequest.Content;           

            try {
                content = CommonOperation.EncodeJavaUnicodes(queueRequest.Content);
                subject = CommonOperation.EncodeJavaUnicodes(queueRequest.Subject);
            }
            catch (Exception ex) {
                log.WarnFormat("PreparePushMessage-EncodeJavaUnicodes Error :{0} ; Content:{1}, Subject:{2}, TransactionId:{3}, Customer : {4}", ex, queueRequest.Content, queueRequest.Subject, queueRequest.TransactionId, queueRequest.CustomerNumber);
            }
            notification.title = subject;
            notification.body = content; 

            notification.sound = "default";
            notification.badge = queueRequest.PushBadge;
            pushRequest.notification = notification;
            pushRequest.mutable_content = true;
            PushExtraData data = new PushExtraData
            {
                tranCode = queueRequest.TransactionCode,
                pushResponseId = queueRequest.ResponseID
            };

            var credentials = PushSenderHelper.GetRsaCredentials();
            data.pushRegisterId = PushSenderHelper.EncryptPushRegisterId(queueRequest.PushRegisterId.ToString(), credentials.Password);

            data.tranData = JsonConvert.DeserializeObject(queueRequest.TranData);
            pushRequest.time_to_live = timeToLive;
            pushRequest.data = data;

            return pushRequest;
        }
    }
}
