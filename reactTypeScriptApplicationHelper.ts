import { ApplicationInfo, ApplicationCredential } from "surface-helper/src/model/view/context";
import { CredentialResponse } from "./../model/dto/autogenerated/CredentialResponse";

export class ApplicationHelper {

    static getApplications(result: CredentialResponse, context: any): ApplicationInfo[] {
        let applications: ApplicationInfo[] = [];
        let applicationCredentials: ApplicationCredential[] = ApplicationHelper.getCredentials(result, context);
        if (result.Applications) {
            let applicationsData = JSON.parse(result.Applications);
            applicationsData.forEach((app: any) => {
                let applicationInfo = {} as ApplicationInfo;
                applicationInfo.ApplicationCode = app.ApplicationCode;
                applicationInfo.GateWayUrl = app.GatewayUrl;
                applicationInfo.ApplicationName = app.ApplicationName;
                applicationInfo.Port = app.Port;
                if (applicationInfo.ApplicationCode && applicationCredentials) {
                    let tempArray = applicationCredentials.find(i => i.key === applicationInfo.ApplicationCode);
                    if (tempArray) {
                        applicationInfo.ApplicationCredentials = tempArray.value;
                    }
                }
                applications.push(applicationInfo);
            });
        }
        return applications;
    }

    static getCredentials = (credentialResponse: CredentialResponse, context: any) => {

        let applicationCredentials: ApplicationCredential[] = [];
        if (credentialResponse.ApplicationCredentials) {
            let userAuthority = JSON.parse(credentialResponse.ApplicationCredentials);
            context.UserAuthority = userAuthority;

            if (userAuthority && userAuthority.UserAuthorityInfo) {
                userAuthority.UserAuthorityInfo.forEach((item: any) => {
                    if (item.ApplicationAuthorityValues.Credentials) {
                        let credentialObject: any = item.ApplicationAuthorityValues.Credentials;
                        applicationCredentials.push({
                            key: item.ApplicationCode,
                            value: credentialObject
                        });
                    }
                });
            }
        }
        return applicationCredentials;
    }
}
