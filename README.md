ASP.NET Core örnekleri ;

ApplicationHelperService.cs  -> Db'den applciaiton bilgilerini çeker bir servis.

CredentialService.cs -> Credential contoller'ın çağırdığı bazı servsilerden user bilgileri ve yetkilerini toplayan bir servis.

MenuService -> Uygulama menü listesini db'den çeken bir servis.


ASP.NET framework da yazılan Örnekler;

GsmStatusController.cs -> Gönderilen sms'lerin status'larını db'deki kayıtları çekerek daha sonra gsm opertörü servisinden  thread'lar ile sorgulayan servis.

NotificationBulkResender.cs -> Gönderilmeyen sms'leri daha sonra ekrandan toplu göndermeye yarayan bir servis.

NotificationDateTimeValidations.cs  -> Email ve sms 'lerin ekrandan tanımlanan saatler arasında gönderilp gönderilmeyecağine karar veren rule pattern ile yazılmış bir class.

PushSenderDataFactory.cs -> Telefonun işletim sistemine göre push gönderimi yapan bir servis.


React-Typescript Örnek kodlar ; 

reactTextBoxComponent.ts -> External bir js kütüphanesinin input elementinin ihtiyaca göre react ile yeniden gelişmiş bir component haline getirilmesi.

reactDrawerComponent.ts -> External bir js kütüphanesinin drawer elementinin react class component haline getirilmesi.

reactTypeScriptApplicationHelper.ts -> Applciation bilgilerini çeken servisn önyüz'den çağırımı.

reactTypeScriptCredentialService-> User credential bilgilerini çeken servisin önyüzden çağırımı.







