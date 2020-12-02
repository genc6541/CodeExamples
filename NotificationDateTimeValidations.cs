using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;

namespace Notification.Operation {

    public class RequestDateControlRule : INotificationValidateRule {

        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            if (!NotificationDateValidator.IsRequestDateNullOrEmpty(requestDate)) {
                if (requestDate.AddMinutes(notificationDefinition.SendEndDuration).IsLessThan(now)) {
                    startTime = DateTime.MinValue;
                    endTime = DateTime.MinValue;
                    isValid = false;
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }
    }
    public class NotificationOptionsRule : INotificationValidateRule {
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            if (!NotificationDateValidator.IsNotificationOptionsNullOrEmpty(notificationOptions) && !notificationOptions.ControlWithNotificationSchedule) {
                startTime = notificationOptions.SendStartTime;
                endTime = notificationOptions.SendEndTime;
                isValid = now.IsLessThanOrEquals(endTime);
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }
    }
    public class NotificationDefinitonValidTimeRule : INotificationValidateRule {
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            if (!NotificationDateValidator.IsNotificationDateDefinitionEmpty(notificationDefinition)) {
                startTime = notificationDefinition.SendStartTime;
                endTime = notificationDefinition.SendEndTime;
                isValid = now.IsLessThanOrEquals(endTime);
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }
    }
    public class StartTimeLessThanEndTimeAndBetweenStartAndEndTime : INotificationValidateRule {

        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;

            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    if (scheduleItem.StartTime < scheduleItem.EndTime) {
                        var tempEndDate = new DateTime(lastNow.Year, lastNow.Month, lastNow.Day, scheduleItem.EndTime.Hours, scheduleItem.EndTime.Minutes, scheduleItem.EndTime.Seconds);
                        var minEndTime = new DateTime[] { now.AddMinutes(notificationDefinition.SendEndDuration), tempEndDate }.Min();
                        if (lastNowTime.IsBetween(scheduleItem.StartTime, scheduleItem.EndTime) && scheduleItem.Days.Contains((int)lastNow.DayOfWeek)) {
                            startTime = lastNow;
                            endTime = minEndTime;
                            isValid = true;
                            break;
                        }
                    }
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }

    }
    public class StartTimeLessThanEndTimeAndOutOfStartAndEndTime : INotificationValidateRule {

        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            //lastNow degerini kullaniyoruz. DateTime.Now'a eklenen sure ile gun bilgisi degisebilir
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;
            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    if (scheduleItem.StartTime < scheduleItem.EndTime) {
                        var residualMinutes = scheduleItem.StartTime.Subtract(now.TimeOfDay).TotalMinutes;
                        residualMinutes = Math.Abs(residualMinutes);

                        if (residualMinutes < notificationDefinition.SendEndDuration && scheduleItem.Days.Contains((int)lastNow.DayOfWeek)) {
                            startTime = lastNow.AddMinutes(residualMinutes);
                            endTime = lastNow.AddMinutes(notificationDefinition.SendEndDuration);
                            isValid = true;
                            break;
                        }
                    }
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }

    }
    public class StartTimeEqualEndTime : INotificationValidateRule {
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    if (scheduleItem.StartTime == scheduleItem.EndTime && scheduleItem.Days.Contains((int)lastNow.DayOfWeek)) {
                        startTime = lastNow;
                        endTime = startTime.AddMinutes(notificationDefinition.SendEndDuration);
                        isValid = true;
                        break;
                    }
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }
    }
    public class StartTimeGreaterThanEndTime : INotificationValidateRule {
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            bool isValid = false;
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;

            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    if (scheduleItem.StartTime > scheduleItem.EndTime) {
                        var tempEndDate = new DateTime(lastNow.Year, lastNow.Month, lastNow.Day, scheduleItem.EndTime.Hours, scheduleItem.EndTime.Minutes, scheduleItem.EndTime.Seconds);
                        tempEndDate = tempEndDate.AddDays(1);
                        if ((lastNowTime.IsGreaterThanOrEquals(scheduleItem.StartTime) || lastNowTime.IsLessThanOrEquals(scheduleItem.EndTime)) && scheduleItem.Days.Contains((int)lastNow.DayOfWeek)) {
                            startTime = lastNow;
                            endTime = new DateTime[] { lastNow.AddMinutes(notificationDefinition.SendEndDuration), tempEndDate }.Min();
                            isValid = true;
                            break;
                        }

                    }
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);

        }

    }
    public class ControlScheduleDays : INotificationValidateRule {   //Takvim günlerini kontrol et o gün yoksa ilk geçerli günde gönder

        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();

            NotificationDateValidationInfo validDateInfo = new NotificationDateValidationInfo();
            validDateInfo.SendStartTime = startTime;
            validDateInfo.SendEndTime = endTime;
            validDateInfo.isValid = isValid;

            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            int today = (int)lastNow.DayOfWeek;
            List<int> combinedScheduleDays = new List<int>();
            int scheduleLoopCount = 0;

            if (NotificationDateValidator.GetSchedulingWaitingStatus(notificationDefinition)) {
                if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                    foreach (var scheduleItem in notificationDefinition.Schedules) {
                        scheduleLoopCount++;
                        if (notificationDefinition.Schedules.Count == 1) {
                            if (!scheduleItem.Days.Contains(today)) {  // 1,2,3,4,5,6,0 
                                validDateInfo = SetStartAndEndTime(scheduleItem.Days.ToList(), scheduleItem, today, lastNow, startTime, endTime, isValid, notificationDefinition);
                            }
                        }
                        else {
                            if (notificationDefinition.Schedules.Count > 1) {
                                if (!scheduleItem.Days.Contains(today)) {
                                    combinedScheduleDays = combinedScheduleDays.Union(scheduleItem.Days.ToList()).ToList();
                                    if (scheduleLoopCount == notificationDefinition.Schedules.Count) {
                                        validDateInfo = SetStartAndEndTime(combinedScheduleDays, scheduleItem, today, lastNow, startTime, endTime, isValid, notificationDefinition);
                                    }
                                }
                                else {
                                    break;// O gün takvimde varsa bu class'dan çýk yoluna devam et, burayla iþin yok
                                }

                            }
                        }
                    }

                }
            }


            return validDateInfo;

        }
        public NotificationDateValidationInfo SetStartAndEndTime(List<int> days, NotificationSchedule scheduleItem, int today, DateTime lastNow, DateTime startTime, DateTime endTime, bool isValid, NotificationDefinition notificationDefinition) {
            NotificationDateValidationInfo validDateInfo = new NotificationDateValidationInfo();
            var lastNowTime = lastNow.TimeOfDay;

            int daysOfWeek = 7;
            int counter = 0;
            int day = 0;
            while (day < daysOfWeek) {
                if (today < 6) {
                    today++;
                }
                else {
                    today = 0;
                }
                counter++;//Saysýn ki arada kaç gün var bilelim
                if (days.Contains(today)) {
                    lastNow = lastNow.AddDays(counter);
                    lastNowTime = lastNow.TimeOfDay;
                    startTime = lastNow.Date + scheduleItem.StartTime;
                    endTime = startTime.AddMinutes(notificationDefinition.SendEndDuration);
                    isValid = true;
                    break;
                }
                day++;
            }

            if (isValid) {
                validDateInfo.SendStartTime = startTime;
                validDateInfo.SendEndTime = endTime;
                validDateInfo.isValid = isValid;
            }

            return validDateInfo;

        }


    }
    public class HandleRequestOutOfScheduleTime : INotificationValidateRule {
        //Takvim saatleri dýþýnda öncesinde ve sonrasýnda gelen istekleri yönet
        public NotificationDateValidationInfo DecideNotificationTimeValidation(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate) {
            bool isValid = false;
            DateTime startTime = new DateTime();
            DateTime endTime = new DateTime();
            //lastNow degerini kullaniyoruz. DateTime.Now'a eklenen sure ile gun bilgisi degisebilir
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;
            TimeSpan midNight = new TimeSpan(23, 59, 59);
            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                if (NotificationDateValidator.GetSchedulingWaitingStatus(notificationDefinition)) {
                    foreach (var scheduleItem in notificationDefinition.Schedules) {
                        if (!lastNowTime.IsBetween(scheduleItem.StartTime, scheduleItem.EndTime)) {

                            //16:00-21:00 takvimi için ör istek saati : 15:00
                            //08:00-17:00 takvimi için ör istek saati : 07:00
                            if (lastNowTime < scheduleItem.StartTime) {
                                var newDate = new DateTime(now.Year, now.Month, now.Day, 00, 00, 00);
                                startTime = newDate + scheduleItem.StartTime;
                                endTime = startTime.AddMinutes(notificationDefinition.SendEndDuration);
                                isValid = true;
                                break;
                            }


                            if (lastNowTime > scheduleItem.EndTime) {

                                //16:00-21:00 takvimi için ör istek saati : 22:00
                                //08:00-17:00 takvimi için ör istek saati : 18:00
                                if (lastNowTime.IsBetween(scheduleItem.EndTime, midNight)) {
                                    var newDate = new DateTime(now.Year, now.Month, now.Day, 00, 00, 00);
                                    newDate = newDate.AddDays(1);
                                    startTime = newDate + scheduleItem.StartTime;
                                    endTime = startTime.AddMinutes(notificationDefinition.SendEndDuration);
                                    isValid = true;
                                    break;

                                }


                                //16:00-21:00 takvimi için ör istek saati : 00:00 'dan sonra 16:00'a kadar. Ör: 00:40 veya 02:00
                                //08:00-17:00 takvimi için ör istek saati : 00:00 'dan sonra 08:00'a kadar. Ör: 00:40 veya 02:00
                                if (lastNowTime.IsLessThanOrEquals(midNight)) {
                                    var newDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, 00, 00, 00);
                                    startTime = newDate + scheduleItem.StartTime;
                                    endTime = startTime.AddMinutes(notificationDefinition.SendEndDuration);
                                    isValid = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return NotificationDateValidator.SetValidDateInfos(startTime, endTime, isValid);
        }

    }
    public static class NotificationDateValidator {

        private static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static List<INotificationValidateRule> Rules = new List<INotificationValidateRule>();
        static NotificationDateValidator() {
            Rules.Add(new RequestDateControlRule());
            Rules.Add(new NotificationOptionsRule());
            Rules.Add(new NotificationDefinitonValidTimeRule());
            Rules.Add(new ControlScheduleDays());
            Rules.Add(new StartTimeLessThanEndTimeAndBetweenStartAndEndTime());
            Rules.Add(new StartTimeLessThanEndTimeAndOutOfStartAndEndTime());
            Rules.Add(new StartTimeEqualEndTime());
            Rules.Add(new StartTimeGreaterThanEndTime());
            Rules.Add(new HandleRequestOutOfScheduleTime());
        }

        public static bool CheckNotificationValidOptions(NotificationDefinition notificationDefinition, NotificationOptions notificationOptions, DateTime now, DateTime requestDate, out NotificationOptions validOptions) {
            var isValid = false;
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;


            NotificationDateValidationInfo DateValidInfo = new NotificationDateValidationInfo();
            foreach (var rule in Rules) {
                DateValidInfo = rule.DecideNotificationTimeValidation(notificationDefinition, notificationOptions, now, requestDate);
                if (DateValidInfo.isValid || IsRequestDateLessThanNow(notificationDefinition, requestDate, now)) {
                    break;
                }
            }

            validOptions = new NotificationOptions() {
                SendStartTime = DateTime.MinValue,
                SendEndTime = DateTime.MinValue
            };

            //Control if decision starttime expire options endTime
            if (!NotificationDateValidator.IsNotificationOptionsNullOrEmpty(notificationOptions) && notificationOptions.ControlWithNotificationSchedule) {
                if (!DateValidInfo.SendStartTime.IsLessThan(notificationOptions.SendEndTime) && DateValidInfo.SendStartTime != DateTime.MinValue) {
                    return isValid;
                }
                if (DateValidInfo.isValid) {
                    validOptions.SendEndTime = notificationOptions.SendEndTime;
                }
            }

            validOptions.SendStartTime = DateValidInfo.SendStartTime;
            if (validOptions.SendEndTime == DateTime.MinValue) {
                validOptions.SendEndTime = DateValidInfo.SendEndTime;
            }

            isValid = DateValidInfo.isValid;

            //validOptions.SendEndTime = ControlProcessTimeDayOut(validOptions, isValid, notificationDefinition, now);

            return isValid;
        }

        public static bool GetSchedulingWaitingStatus(NotificationDefinition notificationDefinition) {

            bool isWaitingForSchedulingActive = ConfigurationHelper.GetConfiguration<bool>(Constants.IsWaitingForSchedulingActiveKey, true);
            if (!isWaitingForSchedulingActive)
                return false;
            if (notificationDefinition.IsContinueSendingOtherDay) {
                return true;
            }
            return false;
        }

        public static bool IsExistSchedule(NotificationDefinition notificationDefinition) {

            return notificationDefinition.Schedules.Count > 0;
        }

        public static NotificationDateValidationInfo SetValidDateInfos(DateTime startTime, DateTime endTime, bool isValid) {

            NotificationDateValidationInfo DateValidInfo = new NotificationDateValidationInfo();
            DateValidInfo.SendStartTime = startTime;
            DateValidInfo.SendEndTime = endTime;
            DateValidInfo.isValid = isValid;

            return DateValidInfo;
        }

        public static DateTime ControlProcessTimeDayOut(NotificationOptions timeOptions, bool isValid, NotificationDefinition notificationDefinition, DateTime now) {

            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    if (isValid) {
                        if (timeOptions.SendEndTime.Day != lastNow.Day && !scheduleItem.Days.Contains((int)timeOptions.SendEndTime.DayOfWeek)) {
                            //endTime hesabi yapilirken bir sonraki gune gecildi
                            //ve bir sonraki gun valid gunlerden degil ise bir onceki gunun 23:59'u end time'dir.
                            timeOptions.SendEndTime = new DateTime(lastNow.Year, lastNow.Month, lastNow.Day, 23, 59, 59);
                        }
                    }
                }
            }

            return timeOptions.SendEndTime;
        }

        public static bool IsRequestDateNullOrEmpty(DateTime requestDate) {

            return requestDate == null || requestDate == DateTime.MinValue;
        }

        public static bool IsRequestDateLessThanNow(NotificationDefinition notificationDefinition, DateTime requestDate, DateTime now) {

            return requestDate.AddMinutes(notificationDefinition.SendEndDuration).IsLessThan(now);
        }

        public static bool IsNotificationOptionsNullOrEmpty(NotificationOptions notificationOptions) {

            return notificationOptions == null || (notificationOptions.SendStartTime == DateTime.MinValue && notificationOptions.SendEndTime == DateTime.MinValue);
        }

        public static bool IsNotificationDateDefinitionEmpty(NotificationDefinition notificationDefinition) {

            return notificationDefinition.SendStartTime == DateTime.MinValue && notificationDefinition.SendEndTime == DateTime.MinValue;
        }

        public static bool IsOutOfScheduleRequest(NotificationDefinition notificationDefinition, DateTime requestDate, DateTime now) {
            var lastNow = now.AddMinutes(notificationDefinition.SendStartDuration);
            var lastNowTime = lastNow.TimeOfDay;
            int today = (int)lastNow.DayOfWeek;

            bool result = true;

            if (NotificationDateValidator.IsExistSchedule(notificationDefinition)) {
                foreach (var scheduleItem in notificationDefinition.Schedules) {
                    bool isDayExistInSchedule = false;
                    bool isNowValidScheduletTime = false;

                    if (scheduleItem.StartTime == scheduleItem.EndTime) {
                        isNowValidScheduletTime = true;
                    }

                    if (scheduleItem.StartTime < scheduleItem.EndTime) {
                        if (lastNowTime.IsBetween(scheduleItem.StartTime, scheduleItem.EndTime)) {
                            isNowValidScheduletTime = true;
                        }
                    }

                    if (scheduleItem.StartTime > scheduleItem.EndTime) {
                        if (!lastNowTime.IsBetween(scheduleItem.EndTime, scheduleItem.StartTime)) {
                            isNowValidScheduletTime = true;
                        }
                    }

                    if (scheduleItem.Days.Contains(today)) {
                        isDayExistInSchedule = true;
                    }

                    if (isNowValidScheduletTime && isDayExistInSchedule) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
    }
}
