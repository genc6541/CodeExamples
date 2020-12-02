using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace Surface.Operation.Operation {
    public class MenuService : IMenuService {

        private readonly IApplicationHelperService applicationHelperService;
        private readonly ILogAdapter log;
        private static Guid menuGuid = Guid.NewGuid();

        public MenuService(IApplicationHelperService applicationHelperService, ILogAdapter log) {
            this.applicationHelperService = applicationHelperService;
            this.log = log;
        }

        public MenuResponse GetMenuAndApplications(ISurfaceUnitOfWork surfaceUnitOfWork) {
            List<dynamic> menus = GetMenus(surfaceUnitOfWork);
            List<Application> applications = applicationHelperService.GetApplications(surfaceUnitOfWork);

            MenuResponse menuResponse = new MenuResponse {
                Applications = applications == null ? null : JsonConvert.SerializeObject(applications),
                Menus = menus == null ? null : JsonConvert.SerializeObject(menus)
            };

            return menuResponse;
        }

        public List<dynamic> GetMenus(ISurfaceUnitOfWork surfaceUnitOfWork) {
            string tenantCode = EsbContextOperation.GetTenantCode();
            string status = "fail";
            DateTime startDateTime = DateTime.Now;
            try {
                log.InfoFormat("{0}-{1}", "GetMenusStart", menuGuid);
                List<dynamic> menus = new List<object>();
                var results = (from menuItem in surfaceUnitOfWork.SURF_MenuRepository.Table()
                               join menuHierarcy in surfaceUnitOfWork.SURF_MenuHierarcyRepository.Table() on menuItem.Id equals menuHierarcy.MenuId
                               join application in surfaceUnitOfWork.SURF_ApplicationRepository.Table() on menuItem.ApplicationCode equals application.ApplicationCode
                               where menuItem.RecordStatus == true && application.RecordStatus == true && menuItem.TenantCode == tenantCode
                               select new {
                                   menuItem.Id,
                                   Text = menuItem.StringKey,
                                   menuItem.ScreenCode,
                                   ParentId = menuHierarcy.ParentMenuId,
                                   Icon = menuItem.IconCode,
                                   menuItem.TenantCode,
                                   Application = application.ApplicationCode,
                                   application.GatewayUrl,
                                   menuItem.IsVisible,
                                   menuHierarcy.MenuOrder,
                                   menuItem.TranCode
                               }).ToList();

                foreach (var item in results) {
                    menus.Add(item);
                }

                TimeSpan timeSpan = DateTime.Now - startDateTime;
                status = "ok";
                log.InfoFormat("{0}-{1}-{2}-{3}", "GetMenusSuccess", menuGuid, timeSpan.TotalMilliseconds, status);
                return menus;
            }
            catch (Exception ex) {
                log.ErrorFormat("{0}-{1}-{2}", "GetMenus", menuGuid, ex);
                throw;
            }
            finally {
                TimeSpan timeSpan = DateTime.Now - startDateTime;
                log.InfoFormat("{0}-{1}-{2}-{3}", "GetMenusFinally", menuGuid, timeSpan, status);
            }
        }
    }
}
