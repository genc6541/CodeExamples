using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Operation.Operation.Mobile
{
    public static class GsmStatusController
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Setup(List<GsmStatusThreadInfo> gsmStatusThreadInfoList) {
            if (gsmStatusThreadInfoList == null || gsmStatusThreadInfoList.Count == 0) {
                log.ErrorFormat("GsmStatusSettings thread info null or empty , Error message");
                return;
            }
            try {
                CommonUtilities.SetEsbContext();
            }
            catch (Exception ex) {
                log.ErrorFormat("Error while creating new esb context :{0}", ex);
            }

            try {
                var gsmStatusThreadInfo = gsmStatusThreadInfoList.FirstOrDefault();
                if (!IsGsmStatusControllerActive()) {
                    return;
                }
                try {
                    gsmStatusThreadInfo = GetGsmStautsThreadInfo(gsmStatusThreadInfoList);
                }
                catch (Exception ex) {
                    log.ErrorFormat("GsmStatusSettings not found , Error message :{0}", ex);
                    return;
                }

                if (!gsmStatusThreadInfo.CheckGsmStatus) {
                    log.ErrorFormat("CheckGsmStatus is false in webconfig");
                    return;
                }

                if (gsmStatusThreadInfo.ThreadCount > 0 && gsmStatusThreadInfo.RecordCount > 0) {
                    log.InfoFormat("NotificationGsmStatusController-Setup-Starting Threads");

                    for (int i = 1; i <= gsmStatusThreadInfo.ThreadCount; i++) {
                        Task.Factory.StartNew(() => {
                            Process(gsmStatusThreadInfo.RecordCount);
                        });
                    }
                }
            }
            catch (Exception ex) {
                log.FatalFormat("Error In GsmStatusController setup Processing:ERROR MESSAGE : {0}", ex);
            }
        }
        static void Process(int takeCount) {
            while (true) {
                List<GsmStatusInfo> gsmGetStatusEntitiyList = null;
                List<List<GsmStatusInfo>> gsmGetStatusEntitiyGroupedList = new List<List<GsmStatusInfo>>();
                CommonUtilities.SetEsbContext();
                try {
                    try {
                        gsmGetStatusEntitiyList = NotificationOperation.GetGsmRecordsForStatusQuerying(takeCount);
                        if (gsmGetStatusEntitiyList.Count > 0) {
                            gsmGetStatusEntitiyGroupedList = gsmGetStatusEntitiyList.GroupBy(o => o.Operator).Select(grp => grp.ToList()).ToList();
                        }
                    }
                    catch (Exception ex) {
                        log.ErrorFormat("An arror occoured while selecting gsm records from db, Error : {0}", ex);
                    }

                    foreach (var gsmListItem in gsmGetStatusEntitiyGroupedList) {
                        try {
                            BuildSmsStatusRequestByOperator(gsmListItem);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("GsmStatusController process error : {0}", ex);
                            NotificationOperation.UpdateProcessingErrorLog(gsmListItem, ex);
                        }
                    }
                    if (gsmGetStatusEntitiyGroupedList.Count > 0) {
                        Thread.Sleep(100);
                    }
                    else {
                        var sleepTime = 3000;
                        string sleepTimeKey = Constants.GsmStatusControllerSleepTimeKey;
                        try {
                            sleepTime = ConfigurationHelper.GetConfiguration<int>(sleepTimeKey, sleepTime);
                        }
                        catch (Exception ex) {
                            log.ErrorFormat("{0} configuration deðeri bulunamadý.{1}", sleepTimeKey, ex);
                        }
                        Thread.Sleep(sleepTime);
                    }
                }
                catch (Exception outerException) {
                    log.ErrorFormat("GsmStatusController process error : {0}", outerException);
                }
            }
        }
        static bool IsGsmStatusControllerActive() {
            bool isGsmControllerActive = false;
            try {
                isGsmControllerActive = Services.Create<IConfigurationManager>().GetAppSetting<bool>(Constants.IsGsmControllerActive);
            }
            catch (Exception ex) {
                log.ErrorFormat("NotificationGsmStatusController-Error in getting configuration key:{0} , Error:{1}", Constants.IsGsmControllerActive, ex);
            }
            log.InfoFormat("NotificationGsmStatusController-GSM Controller Active flag is {0},WebConfig Key:{1}", isGsmControllerActive, Constants.IsGsmControllerActive);
            return isGsmControllerActive;
        }

        static GsmStatusThreadInfo GetGsmStautsThreadInfo(List<GsmStatusThreadInfo> gsmStatusThreadInfoList) {
            var gsmStatusThreadInfo = new GsmStatusThreadInfo();
            try {
                gsmStatusThreadInfo.CheckGsmStatus = gsmStatusThreadInfoList.FirstOrDefault().CheckGsmStatus;
                gsmStatusThreadInfo.ThreadCount = gsmStatusThreadInfoList.FirstOrDefault().ThreadCount;
                gsmStatusThreadInfo.RecordCount = gsmStatusThreadInfoList.FirstOrDefault().RecordCount;
            }
            catch (Exception ex) {
                log.ErrorFormat("NotificationGsmStatusController-Error in getting configuration key about thread info , Error:{0}", ex);
            }
            return gsmStatusThreadInfo;
        }

        public static SmsStatusResult BuildSmsStatusRequestByOperator(List<GsmStatusInfo> gsmGetStatusEntitiyList) {

            try {
                CommonUtilities.SetEsbContext();
            }
            catch (Exception ex) {
                log.ErrorFormat("Error while creating new esb context :{0}", ex);
            }

            if (gsmGetStatusEntitiyList == null)
                return null;

            var gsmOperator = gsmGetStatusEntitiyList.FirstOrDefault().Operator;
            GsmOperatorSender sender = new GsmOperatorSender(gsmOperator);
            SmsStatusRequestEntity smsGetStatusItem = new SmsStatusRequestEntity();
            foreach (var item in gsmGetStatusEntitiyList) {
                if (gsmOperator == GsmOperator.VODAFONE) {
                    smsGetStatusItem.MultipleStatusRequestKeys.Add(new MultipleStatusRequestKey {
                        MessageId = item.OperatorResponseId,
                        NotificationResponseId = item.NotificationResponseId,
                        SessionId = item.SessionId,
                        GsmStatusId = item.Id
                    });
                    smsGetStatusItem.SessionId = item.SessionId;
                    smsGetStatusItem.NotificationResponseId = item.NotificationResponseId;
                }
                else {
                    smsGetStatusItem.MessageIdList.Add(item.OperatorResponseId);
                    if (!string.IsNullOrEmpty(item.SessionId)) {
                        smsGetStatusItem.SessionId = item.SessionId;
                    }
                }
            }

            SmsStatusRequest smsStatusRequest = sender.BuildSmsStatusRequest(smsGetStatusItem);
            SmsStatusResult smsStatusResult = sender.GetSmsStatus(smsStatusRequest);

            if (smsStatusResult.GsmSmsStatusResultList == null || smsStatusResult.GsmSmsStatusResultList.Count == 0) {
                throw new Exception(string.Format("Gsm status result null or empty, gsm operator:{0}", gsmOperator));
            }

            foreach (var smsStatusResulItem in smsStatusResult.GsmSmsStatusResultList) {
                var gsmStatusFields = sender.SetGsmStatusFields(smsStatusResulItem, sender);
                GsmStatusInfo smallGsmEntitiy = GetInsertDateAndIdOfGsmItem(smsStatusResulItem.MessageId, smsStatusResulItem.SessionId, gsmOperator, gsmGetStatusEntitiyList);
                NotificationOperation.UpdateGsmSmsStatusResults(smsStatusResulItem, gsmOperator, smallGsmEntitiy, gsmStatusFields);
                if (gsmStatusFields.InternalStatus != GsmOperatorStatus.Completed) {  
                    DateTime nexCheckDate = sender.SetNextStatusCheckDateOfGsmSms(smallGsmEntitiy.InsertDate, gsmStatusFields.InternalStatus);
                    NotificationOperation.UpdateNexCheckDateOfGsmRecord(nexCheckDate, smsStatusResulItem.MessageId, smallGsmEntitiy);
                }
            }
            return smsStatusResult;
        }

        static GsmStatusInfo GetInsertDateAndIdOfGsmItem(string messageId, string sessionId, GsmOperator gsmOperator, List<GsmStatusInfo> gsmGetStatusEntitiyList) {
            GsmStatusInfo smallGsmEntitiy = new GsmStatusInfo();
            foreach (var item in gsmGetStatusEntitiyList) {
                if (gsmOperator == GsmOperator.VODAFONE) {
                    if ((messageId == item.OperatorResponseId && sessionId == item.SessionId)) {
                        smallGsmEntitiy = (GsmStatusInfo)item.Clone();
                        break;
                    }
                }
                else {
                    if (messageId == item.OperatorResponseId) {
                        smallGsmEntitiy = (GsmStatusInfo)item.Clone();
                        break;
                    }
                }
            }
            return smallGsmEntitiy;
        }

        [EsbScreenMethod("Gets gsm status of record")]
        public static GsmStatusInfo GetGsmSmsStatusInfo(long responseId, DateTime createDate) {

            GsmStatusInfo smallGsmStatusEntity = new GsmStatusInfo();
            SmsStatusResult smsStatusResult;
            int messageStatus;

            List<GsmStatusInfo> gsmGetStatusEntitiyList = NotificationOperation.GetGsmSmsStatusOfSingleRecord(responseId, createDate);

            if (gsmGetStatusEntitiyList.Count == 0) {
                throw new EsbBusinessException("RecordNotFound");
            }

            if (gsmGetStatusEntitiyList.FirstOrDefault().Status != GsmOperatorStatus.Completed) {

                smsStatusResult = BuildSmsStatusRequestByOperator(gsmGetStatusEntitiyList);
                messageStatus = smsStatusResult.GsmSmsStatusResultList.FirstOrDefault().MessageStatus;

                var gsmOperator = gsmGetStatusEntitiyList.FirstOrDefault().Operator;
                GsmOperatorSender sender = new GsmOperatorSender(gsmOperator);
                string statusExplanation = sender.GetStatusExplanationFromEnum(messageStatus);

                smallGsmStatusEntity.NotificationResponseId = responseId;
                smallGsmStatusEntity.GsmStatus = messageStatus.ToString();
                smallGsmStatusEntity.GsmStatusExplanation = statusExplanation;
                smallGsmStatusEntity.DeliveryTime = smsStatusResult.GsmSmsStatusResultList.FirstOrDefault().MessageDeliveryDate;
                smallGsmStatusEntity.Operator = gsmOperator;
            }
            else {
                smallGsmStatusEntity = gsmGetStatusEntitiyList.FirstOrDefault();
            }

            return smallGsmStatusEntity;
        }
    }
}
