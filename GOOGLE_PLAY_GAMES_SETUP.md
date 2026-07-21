# Google Play Games yapılandırması

Google Play Games Services kurulumu 21 Temmuz 2026 tarihinde tamamlandı. Projede eklenti
`v2.1.0`; otomatik/manuel Google girişi, ana menü giriş ve liderlik düğmeleri, bölüm
zaferlerinde skor gönderimi ve yerel en iyi skorun girişten sonra senkronlanması hazırdır.

## Canlı yapılandırma

- Paket adı: `com.triogames.balloonpop`
- Google Cloud projesi: `balloon-pop-triogames`
- Play Games app ID: `867456826239`
- Liderlik tablosu: `Highest Score`
- Liderlik tablosu ID: `CgkI__bSw58ZEAIQAQ`
- OAuth yayın durumu: `In production`
- Play Games yayın durumu: yayınlandı; bekleyen değişiklik yok
- Dahili test kanalı: etkin, sürüm `0.1.0-internal-1` (version code `1`)
- Katılım bağlantısı: `https://play.google.com/apps/internaltest/4701107718413357996`
- Test hesabı: geliştirici Google hesabı (`Balloon Pop Dahili Testçiler` listesi)

## Android kimlik bilgileri

- Üretim istemcisi: `Balloon Pop Play Games`
  - SHA-1: `B2:56:69:92:8B:E6:31:06:2E:00:2C:72:80:77:77:BD:EA:99:49:94`
  - OAuth client ID: `867456826239-g5f01q6emoafnudqe46uicm179gu7l1n.apps.googleusercontent.com`
  - Yeni Play yüklemeleri için varsayılan kimlik bilgisidir.
- Debug istemcisi: `Balloon Pop Debug`
  - SHA-1: `CB:B3:F3:DC:4C:AB:C1:37:A2:6E:47:DF:40:F4:96:1D:F0:35:4F:B3`
  - OAuth client ID: `867456826239-lhoq04uqp911fddpmgeas0v373io0a70.apps.googleusercontent.com`
- Play upload anahtarı:
  - SHA-1: `DA:6C:18:22:A0:DC:A4:0F:0E:9B:8D:45:6F:A9:F5:F8:80:D9:26:FE`
  - Keystore: `C:\Users\syste\.android\balloon-pop-upload.keystore`
  - Şifre dosyası: `C:\Users\syste\.android\balloon-pop-upload-credentials.txt`

Upload keystore ve şifre dosyası repoya dahil değildir. İleride yeni sürüm yükleyebilmek için bu
iki dosyanın güvenli bir yedeği mutlaka saklanmalıdır. Google Play'in dağıtılan APK'ları imzaladığı
uygulama imzalama sertifikasının SHA-1 değeri `B2:56:69:92:8B:E6:31:06:2E:00:2C:72:80:77:77:BD:EA:99:49:94`
ve üretim OAuth kimlik bilgisiyle eşleşmektedir.

OAuth client ID değerleri gizli anahtar değildir; Android uygulaması çalışma zamanında app ID ve
liderlik tablosu ID değerlerini kullanır. Temel giriş ve leaderboard için web client ID gerekmez.

## Unity dosyaları

- `Assets/GPGSIds.cs`: Google kaynak sabitleri
- `Assets/Plugins/Android/GooglePlayGamesManifest.androidlib/AndroidManifest.xml`: Play Games app ID metadata
- `Assets/Resources/GooglePlayGamesConfig.asset`: `CgkI__bSw58ZEAIQAQ`
- `ProjectSettings/GooglePlayGameSettings.txt`: Android setup kaydı
- `Assets/Editor/AndroidBuildScript.cs`: APK ve Play için imzalı AAB üretimi

Unity Play Mode doğrulamasında servis yapılandırılmış (`configured=True`) ve ana menüdeki
`GooglePlayGamesMenuUI` bulunur durumda doğrulandı. Gerçek hesap seçme ve Google'ın yerel
liderlik ekranı yalnızca eşleşen SHA-1 ile imzalanmış Android cihaz derlemesinde açılır.

21 Temmuz 2026 Android IL2CPP/ARM64 doğrulama derlemesi başarıyla tamamlandı:
`Build/BalloonPop.apk` (45.008.292 bayt). APK imzası `Android Debug`; SHA-1 değeri
`CB:B3:F3:DC:4C:AB:C1:37:A2:6E:47:DF:40:F4:96:1D:F0:35:4F:B3` ve Play Console'daki
`Balloon Pop Debug` kimlik bilgisiyle eşleşiyor.

İmzalı Play App Bundle da başarıyla üretildi: `Build/BalloonPop.aab` (44.940.946 bayt).
Play Console paketi kabul etti ve `0.1.0-internal-1` sürümü Dahili test kanalında yayınlandı.

## Uygulama davranışı

- Oyun açılışında sessiz otomatik kimlik doğrulama denenir.
- Oyuncu **Google ile Giriş** düğmesine basarsa manuel hesap seçme/giriş akışı açılır.
- Her bölüm zaferinde o bölümün skoru gönderilir; Google oyuncunun en yüksek değerini tutar.
- Başarılı girişten sonra cihazdaki kayıtlı en iyi bölüm skoru buluta senkronlanır.
- **Liderlik** düğmesi Google Play Games'in yerel `Highest Score` ekranını açar.

## Cihaz testi

1. Telefonda `Balloon Pop Dahili Testçiler` listesine eklenen Google hesabıyla oturum aç.
2. `https://play.google.com/apps/internaltest/4701107718413357996?authuser=1` bağlantısını aç.
3. Test davetini kabul et; birkaç dakikalık yayın yayılımından sonra **Download test app** ile
   Play Store sayfasını açıp oyunu yükle.
4. Oyunda Google ile giriş yap, bir bölüm kazan ve skorun `Highest Score` tablosuna
   gönderildiğini kontrol et.

Yeni bir Play sürümü için Unity menüsündeki **BalloonPop > Build AAB for Play** kullanılabilir.
Komut satırı girişi: `BalloonPop.EditorTools.AndroidBuildScript.BatchBuildAAB()`.
