using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Intertech.Surface.DataLayer;
using Intertech.Surface.Operation.Entities;
using Intertech.Surface.Operation.Services.ApplicationServices;
using Intertech.Surface.Operation;
using log4net;
using Newtonsoft.Json;
using Intertech.Surface.Operation.Tracing;

namespace Intertech.Surface.Operation.Operation {

    public class ApplicationHelperService : IApplicationHelperService {

        private readonly ILogAdapter log;
        public ApplicationHelperService(ILogAdapter log) {
            this.log = log;
        }

        public string GetScreenCodesWithApplicationInfo(ISurfaceUnitOfWork surfaceUnitOfWork) {

            var applications = surfaceUnitOfWork.SURF_ApplicationRepository.Table().ToList();

            var menus = surfaceUnitOfWork.SURF_MenuRepository.Table().ToList();

            Dictionary<string, List<string>> applicationWithScreenCodes = new Dictionary<string, List<string>>();

            foreach (var app in applications) {

                List<string> tempList = menus.Where(x => x.ApplicationCode == app.ApplicationCode).Select(y => y.ScreenCode).ToList();
                applicationWithScreenCodes.Add(app.ApplicationCode, tempList);
            }

            var appJson = JsonConvert.SerializeObject(applicationWithScreenCodes);

            return appJson;
        }

        public string GetSurfaceApplicationCode(ISurfaceUnitOfWork surfaceUnitOfWork, string gatewayEsbApplicationCode) {
            string applicationCode = surfaceUnitOfWork.SURF_ApplicationRepository.Table()
               .Where(a => a.EsbApplicationCode == gatewayEsbApplicationCode)
               .Select(a => a.ApplicationCode).FirstOrDefault();
            return applicationCode;
        }

        public List<Application> GetApplications(ISurfaceUnitOfWork surfaceUnitOfWork) {

            List<Application> queryApplications = new List<Application>();

            try {
                queryApplications = (from application in surfaceUnitOfWork.SURF_ApplicationRepository.Table()
                                     select new Application {
                                         Id = application.Id,
                                         ApplicationCode = application.ApplicationCode,
                                         RedirectKey = application.RedirectValue,
                                         RedirectValue = application.RedirectValue,
                                         UrlTag = application.UrlTag,
                                         LastUpdateDate = application.LastUpdateDate,
                                         LastUpdateUserCode = application.LastUpdateUserCode,
                                         RecordStatus = application.RecordStatus,
                                         GatewayUrl = application.GatewayUrl,
                                         ApplicationName = application.ApplicationCode,
                                         EsbApplicationCode = application.EsbApplicationCode,
                                         Port = application.Port
                                        
                                     }).ToList();

                log.InfoFormat("Applications get from SurfaceDb");
            }
            catch (Exception ex) {
                log.ErrorFormat("{0}-{1}-{2}", "ApplicationData was get from SurfaceDb", "MenuService-GetApplications", "Error: " + ex);
            }

            return queryApplications;

        }
    }
}
