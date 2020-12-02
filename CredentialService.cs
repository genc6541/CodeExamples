using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace Credential.Gateway.Aggregator.Services {
    /// <summary>
    /// ImsCredentialService
    /// </summary>
    public class CredentialService : ICredentialService {

        private readonly IUserAuthorityService userAuthorityService;
        private readonly ILogger log;
        private readonly IMenuCacheService menuCacheService;
        private readonly IMenuService menuService;

        public CredentialService(IUserAuthorityService userAuthorityService, ILogger log, IMenuCacheService menuCacheService, IMenuService menuService) {
            this.userAuthorityService = userAuthorityService;
            this.log = log;
            this.menuCacheService = menuCacheService;
            this.menuService = menuService;
        }

        public CredentialResponse GetCredentials(CredentialRequest credentialRequest) {

            if (credentialRequest == null) {
                log.ErrorFormat("{0}{1}", "CredentialService-GetCredentials", "CredentialRequest is null!");
                return new CredentialResponse {
                    HasError = true,
                    ErrorDetail = "CredentialRequest is null!"
                };
            }
            else if (string.IsNullOrEmpty(credentialRequest.UserCode)) {
                log.ErrorFormat("{0}{1}", "CredentialService-GetCredentials", "UserCode is empty!");
                return new CredentialResponse {
                    HasError = true,
                    ErrorDetail = "UserCode is empty!"
                };
            }
            else {

                try {
                    DateTime dateTimeStart = DateTime.Now;
                    MenuResponse result = menuService.GetMenus().Result; //menuCacheService.GetCachedMenu(credentialRequest.UserCode);
                    log.InfoFormat("{0}-{1}", "GetCachedMenuTimer", (DateTime.Now - dateTimeStart).TotalMilliseconds);

                    if (result == null) {
                        log.ErrorFormat("CredentialService-GetCredentials GetCachedMenu result is null Menu bilgilerine ulaşılamadı!");
                        return new CredentialResponse {
                            HasError = true,
                            ErrorDetail = "Menu bilgilerine ulaşılamadı!"
                        };
                    }
                    else if (result.Applications == null) {
                        log.ErrorFormat("CredentialService-GetCredentials  Applications is null Uygulama bilgilerine ulaşılamadı!");

                        return new CredentialResponse {
                            HasError = true,
                            ErrorDetail = "Uygulama bilgilerine ulaşılamadı!"
                        };
                    }
                    else {
                        var applications = JsonConvert.DeserializeObject(result.Applications);

                        ImsCredentialRequest imsCredentialRequest = new ImsCredentialRequest {
                            Applications = applications,
                            UserCode = credentialRequest.UserCode
                        };

                        User user = userAuthorityService.GetUserCredentials(imsCredentialRequest);

                        if (user == null) {
                            log.ErrorFormat("{0}{1}{2}", "CredentialService-GetCredentials", "Applications : " + result.Applications, "UserCode: " + credentialRequest.UserCode);
                            return new CredentialResponse {
                                HasError = true,
                                ErrorDetail = "Kullanıcı bilgileri SSO uygulamasından alınamadı!"
                            };
                        }
                        else {
                            CredentialResponse credentialResponse = new CredentialResponse {
                                MenuTree = GetMenuTree(user, result),
                                ApplicationCredentials = GetApplicationCredentials(user),
                                Applications = GetApplicationsMultiLanguageValues(applications, user),
                                DysUrl = TryGetUrl(Constants.DysUrl),
                                DashBoardApiUrl = TryGetUrl(Constants.DashBoardApiUrl),
                                IsProduction = GetEnvironmentInfo(),
                                ProductionControlFlag = GetProductionControlFlag(),
                                IsTablet = false , //TabletDetectorHelper.IsTabletModeEnabled(HttpContext.Current.Request),
                                ChannelDate = GetChannelDate(credentialRequest.ChannelCode)
                            };
                            return credentialResponse;
                        }
                    }
                }
                catch (Exception ex) {
                    log.ErrorFormat("CredentialService Error : {0}-{1}", ex, "CredentialService-GetCredentials");
                    return new CredentialResponse {
                        HasError = true,
                        ErrorDetail = string.Format("{0} Message: {1} InnerException: {2} StackTrace: {3}", "Kullanıcı yetkileri alınamadı: Detay:", ex.Message, ex.InnerException,ex.StackTrace)
                    };
                }
            }
        }

        private string GetApplicationCredentials(User user) {
            user.UserPrinters = GetUserPrinters(user.UserCode, user.UserPrinters);
            return JsonConvert.SerializeObject(user);
        }

        private string GetMenuTree(User user, MenuResponse menuResponse) {
            List<dynamic> menus = GetMultiLanguagesForMenuItems(menuResponse);
            List<string> allAuthorizedScreens = GetAllAuthorizedScreensFromUser(user);
            List<dynamic> authorizedMenus = menus.Where(m => allAuthorizedScreens.Any(s => m.Id.ToString().Equals(s))).ToList();
            return JsonConvert.SerializeObject(authorizedMenus);
        }

        private static List<string> GetAllAuthorizedScreensFromUser(User user) {
            return user.UserAuthorityInfo.AsParallel().Where(x => x.AuthorizedScreens != null && x.AuthorizedScreens.Count > 0).ToList().SelectMany(i => i.AuthorizedScreens).ToList();
        }

        private List<UserPrinter> GetUserPrinters(string userCode, List<UserPrinter> userPrinters) {
            try {
                var userPrinterInfo = Esb.App.Sso.GetUserPrinters(new UserSearchCriteria { UserCode = userCode });
                return userPrinterInfo.UserPrinters;
            }
            catch (Exception ex) {
                log.ErrorFormat("{0}{1}{2}{3}", ex, "Error getting printer info from ims", "userCode: " + userCode, "CredentialService-GetCredentials");
            }

            return userPrinters;
        }

        private List<dynamic> GetMultiLanguagesForMenuItems(MenuResponse result) {
            List<dynamic> menus = new List<dynamic>();
            try {
                if (!string.IsNullOrEmpty(result.Menus)) {
                    dynamic menuJson = JsonConvert.DeserializeObject(result.Menus);
                    if (menuJson != null) {
                        foreach (var menuItem in menuJson) {
                            if (menuItem["Text"] != null && menuItem["Text"].Value != null) {
                                menuItem["Text"] = SM.GetString(menuItem["Text"].Value);
                            }
                            if (menuItem["TranCode"] != null && menuItem["TranCode"].Value != null) {
                                menuItem["TranCode"] = SM.GetString(menuItem["TranCode"].Value);
                            }
                            menus.Add(menuItem);
                        }
                    }
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("{0}-{1}-{2}", "Error while getting ML for menus !", "CredentialService-GetMultiLanguagesForApplications", ex);

            }

            return menus;
        }

        public string GetApplicationsMultiLanguageValues(dynamic applications, User user) {
            try {
                if (applications.Count > 0) {
                    foreach (var item in applications) {
                        string applicationName = string.Empty;
                        applicationName = SM.GetStringWithLanguage(user.LanguageInfo.LanguageCode, item.ApplicationCode.Value);
                        item.ApplicationName = applicationName;
                    }
                }
            }
            catch (Exception ex) {
                log.WarnFormat("{0}-{1}", "Error on getting ApplicationNames from EsbML", ex);
            }

            return JsonConvert.SerializeObject(applications);
        }

        private string TryGetUrl(string key) {
            try {
                return ServiceLocator.Create<Shell.GatewayCore.Configuration.IConfigurationService>().GetConfiguration<string>(key);
            }
            catch (Exception ex) {
                log.DebugFormat("{0}{1}{2}{3}", "Error getting dysUrl, Error: ", ex, "CredentialService", key);
            }
            return string.Empty;
        }

        private bool GetProductionControlFlag() {
            try {
                return ServiceLocator.Create<Shell.GatewayCore.Configuration.IConfigurationService>().TryGetSetting(Constants.ProductionControlFlag, false);
            }
            catch (Exception ex) {
                log.DebugFormat("{0}-{1}-{2}-{3}", "Error getting GetProductionControlFlag, Error: ", ex, "CredentialService", "ProductionControlFlag");
            }
            return false;
        }

        private bool GetEnvironmentInfo() {
            bool isProduction = false;
            try {
                isProduction = ServiceLocator.Create<IConfigurationService>().IsProduction;
            }
            catch (Exception ex) {
                log.WarnFormat("Error getting isProduction config, Error: {0}-{1}", ex, "CredentialService-GetEnvironmentInfo");
            }

            return isProduction;
        }


        private DateTime GetChannelDate(string channelCode) {

            DateTime channelDate = new DateTime();
            channelDate = DateTime.Now;
            try {
                if (!string.IsNullOrEmpty(channelCode)) {
                    channelDate = ChannelCache.Instance[channelCode].Today;
                }
                else {
                    channelDate = ChannelCache.Instance["SUBE"].Today;
                }
            }
            catch (Exception ex) {
                log.WarnFormat("Error getting channelDate from esb, Error: {0}{1}", ex, "CredentialService-GetChannelDate");
            }

            return channelDate;
        }


    }
}
