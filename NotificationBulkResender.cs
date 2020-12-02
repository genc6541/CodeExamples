using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Notification.Operation.Operation
{
    public class NotificationBulkResender
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly int PageNumber = 1;
        private static readonly int PageSize = 50;
        NotificationQueueViewRequest NotifyQueueViewRequest { get; set; }
        NotificationWebServiceRequestEntity UpdatingInfo { get; set; }

        public NotificationBulkResender(NotificationQueueViewRequest notificationQueueViewRequest, NotificationWebServiceRequestEntity updatingInfo) {
            NotifyQueueViewRequest = notificationQueueViewRequest;
            UpdatingInfo = updatingInfo;
        }

        public void Process() {
            int loopCount = 1;
            while (true) {
                try {
                    CommonUtilities.SetEsbContext();
                    if (loopCount == 1) {
                        NotificationOperation.InsertBulkSenderNotificationProcessingInfo(UpdatingInfo);
                    }
                    NotifyQueueViewRequest.PageNumber = PageNumber;
                    NotifyQueueViewRequest.PageSize = GetPageSize();
                    List<NotificationQueueView> result = NotificationOperation.GetNotificationQueueWithPaging(NotifyQueueViewRequest);
                    if (result.Count == 0) {
                        log.DebugFormat("No result found for sending notification NotificationBulkResender");
                        NotificationOperation.DeleteNotificationBulkProcessingInfo();
                        return;
                    }
                    else {
                        NotificationOperation.RemoveFromQueue(result, UpdatingInfo);
                    }
                    NotificationOperation.AddToQueue(result);
                    loopCount++;
                }
                catch (Exception ex) {
                    log.ErrorFormat("Exception in adding queue NotificationBulkResender {0}", ex);
                    NotificationOperation.DeleteNotificationBulkProcessingInfo();
                    break;
                }
            }
        }

        public static int GetPageSize() {
#pragma warning disable IDE0018 // Inline variable declaration. Uygulanırsa hata alıyor
            int pageSize;
#pragma warning restore IDE0018 // Inline variable declaration
            try {
                if (ForaConfiguration.TryGetSetting<int>("BulkNotificationSenderPageSize", out pageSize)) {
                    return pageSize;
                }
            }
            catch(Exception ex) {
                log.WarnFormat("NotificationBulkResender-BulkNotificationSenderPageSize configuration is not found!Exception:{0}", ex);
            }
            return PageSize;
        }

    }
}
