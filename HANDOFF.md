# BalloonPop — Teknik Devir Teslim Dokümanı

> Bu dosya, projeye yeni katılan bir AI ajanının (veya geliştiricinin) hiçbir ön bilgi olmadan
> çalışmaya devam edebilmesi için hazırlanmıştır. Tüm yollar, mimari kararlar, tuzaklar ve
> bilinen açık işler burada.

**Son güncelleme:** 2026-07-20
**Son commit:** `888b17b`

---

## 1. Proje Künyesi

| Alan | Değer |
|---|---|
| Oyun | **BalloonPop** — match-3 balon patlatma, mobil (portre) |
| Engine | **Unity 6000.4.7f1** (Unity 6.4) |
| Render Pipeline | **Built-in** (URP DEĞİL) |
| Proje yolu | `C:\Users\syste\Desktop\mobile1game` |
| Repo | `https://github.com/db1881/mobile1game` (public) |
| Branch | `main` |
| GitHub CLI hesabı | `emreozrepo` (`gh` ile auth'lu, `repo` scope) |
| Ortak geliştirici | Arkadaş: GitHub `db1881` (repo sahibi) |
| Build hedefi | **Android**, IL2CPP, **ARM64**, min SDK 25 |
| Release | `v0.1.0` → `Build/BalloonPop.apk` (39.3 MB, debug keystore) |

**Kritik:** Repo iki kişi tarafından ortak geliştiriliyor. Değişiklikten önce **daima `git pull`**,
sonra push. Geçmişte arkadaş 3 commit atıp sync sorunu yaşandı.

---

## 2. Klasör Yapısı

```
mobile1game/
├── Assets/
│   ├── Editor/          # 18 editor script (build, autosetup, sprite generator'lar)
│   ├── Fonts/           # Fredoka, Baloo2, PaytoneOne (TTF + SDF asset + material)
│   ├── Prefabs/         # 10 prefab
│   ├── Resources/
│   │   ├── LevelDatabase.asset
│   │   └── Levels/      # Level_01 … Level_100.asset
│   ├── Scenes/          # MainMenu.unity, Game.unity
│   ├── Scripts/         # 74 runtime script (aşağıda detay)
│   ├── Shaders/         # UIChromaKey.shader
│   ├── Sprites/         # 106 PNG
│   └── Video/           # logo.mp4, LogoRT.renderTexture
├── Packages/
│   ├── manifest.json
│   └── com.coplaydev.unity-mcp/   # MCP paketi REPO'YA GÖMÜLÜ (embedded)
├── ProjectSettings/
└── Build/               # .gitignore'da — APK buraya çıkar, commit EDİLMEZ
```

---

## 3. Sahneler ve UI Mimarisi

### 3.1 Sahneler
- **`Assets/Scenes/MainMenu.unity`** — ana menü + tüm pop-up'lar
- **`Assets/Scenes/Game.unity`** — oyun tahtası, HUD, booster'lar, pause/win/lose

### 3.2 UI nasıl oluşuyor (ÇOK ÖNEMLİ)
UI **elle Unity Editor'da tasarlanmadı**. İki editor script'i sahneleri **koddan üretiyor**:

- **`Assets/Editor/AutoSetup.cs`** — `[InitializeOnLoad]`, ilk açılışta çalışır,
  `EditorPrefs` marker'ı (`BalloonPop_AutoSetupDone_v51`) ile bir kez çalışır.
  Menü: `BalloonPop ▸ Run Full Auto-Setup` (marker'ı silip yeniden kurar).
- **`Assets/Editor/SceneUIBuilder.cs`** (~2800 satır) — tüm panelleri, butonları, HUD'u
  GameObject GameObject kurar.

**⚠️ UYARI:** `Run Full Auto-Setup` çalıştırılırsa **sahnedeki tüm manuel UI düzenlemeleri SİLİNİR
ve sıfırdan kurulur** — bu dokümanda anlatılan tüm görsel iyileştirmeler kaybolur.
Kalıcı değişiklik isteniyorsa `SceneUIBuilder.cs` içindeki üretim kodu da güncellenmelidir.
Şu ana kadar yapılan iyileştirmeler **sadece sahnede** (MainMenu.unity / Game.unity) duruyor,
builder koduna yansıtılmadı.

### 3.3 Sahne hiyerarşisi — MainMenu

```
Menu Canvas (CanvasScaler: ScaleWithScreenSize, ref 1080x1920, match 0.5)
├── MenuBg              (Image, sprite bg_menu)
├── LogoVideo           (RawImage → LogoRT, material LogoChroma)
├── Logo                (Image, KAPALI — video ile değiştirildi)
├── Mascot              (Image, KAPALI)
├── HeaderHUD           (kaynak kapsülleri — sol üst)
│   ├── CoinCard/Body(Image cap_coin) + Text
│   ├── HeartCard/Body(Image cap_heart) + Text
│   └── StarCard/Body(Image cap_star) + Text
├── PlayButtonWrap / LevelSelectButtonWrap / SettingsButtonWrap / QuitButtonWrap
│   └── <X>Button (Image btn_plate_*) → Label (TMP)
├── ShopButton / DailyButton / StatsButton / AchButton   (alt bar, tek-parça sprite + BannerLabel)
├── LevelSelectPanel / SettingsPanel / CoinShopPanel /
│   DailyRewardPanel / StatsPanel_Menu / AchievementListPanel / NoHeartsPanel
└── (LogoVideoPlayer sahne KÖKÜNDE, canvas dışında)
```

### 3.4 Sahne hiyerarşisi — Game

```
HUD Canvas
├── PauseButton
├── LevelPillWrap        (SAHİL • 1)
├── ScoreCardWrap / MovesCardWrap
├── GoalContainer        (HorizontalLayoutGroup, GoalItem prefab'ları runtime spawn)
├── StarProgressBar      (Track + 3 yıldız)
├── BoosterCard
│   ├── Hammer  / Shuffle / MovePack   (her biri 210x210 KARE)
│   │   └── Body(Image booster_*) + Label + Count
├── PausePanel / WinPanel / LosePanel / MysteryBoxPanel
└── ComboText
```

---

## 4. Runtime Script Haritası (`Assets/Scripts/`)

### Core
| Dosya | Görev |
|---|---|
| `Core/GameManager.cs` | Singleton, oyun durumu, `CurrentLevel` |
| `Core/LevelLoader.cs` | `LoadLevelByNumber(int)` — level yükleme |
| `Core/GameSceneBootstrap.cs` | `GetWorldName(worldIndex)` — dünya adı |
| `Core/LocalizationManager.cs` | `Lang {TR,EN}`, `Get(key)`, `Toggle()`, `OnLanguageChanged` |
| `Core/CameraFitter.cs` | Kamera ortografik fit |
| `Core/Singleton.cs`, `GameEvents.cs`, `PerformanceBootstrap.cs` | Altyapı |

### Data
| Dosya | Görev |
|---|---|
| `Data/BalloonType.cs` | `enum BalloonType {None,Red,Blue,Green,Yellow,Purple,Orange,Pink}`<br>`enum SpecialType {Normal,LineH,LineV,Bomb,Rainbow,Gold}` |
| `Data/LevelData.cs` | ScriptableObject: `LevelNumber`, `WorldIndex`, `Goals[]`, `Moves` |
| `Data/LevelDatabase.cs` | Tüm level'lar (`Resources/LevelDatabase.asset`) |

### Grid / Gameplay
| Dosya | Görev |
|---|---|
| `Grid/GridManager.cs` | Izgara, balon havuzu (`SetActive` ile) |
| `Grid/MatchFinder.cs` | Eşleşme tespiti |
| `Grid/Cell.cs` | Hücre |
| `Gameplay/Balloon.cs` | Balon görseli — **satır 135-136:** özel tipler sprite'ı tamamen değiştirir (overlay değil) |
| `Gameplay/GameplayController.cs` | Oyun akışı |
| `Gameplay/BoosterManager.cs` | Çekiç / Karıştır / +Hamle |
| `Gameplay/ScoreManager.cs`, `StatsTracker.cs`, `AchievementManager.cs` | Skor / istatistik / başarım |
| `Gameplay/SpecialBalloonResolver.cs` | Özel balon oluşturma |

### Save
| Dosya | Görev |
|---|---|
| `Save/SaveSystem.cs` | JSON save; `Language` (0=TR,1=EN), coin, kalp, yıldız |
| `Save/HeartSystem.cs` | Can sistemi + dolum sayacı |

### UI (kritik olanlar)
| Dosya | Görev / Not |
|---|---|
| `UI/GameHUD.cs` | HUD; `BuildGoals()` **runtime'da GoalItem prefab spawn eder** (satır ~77) |
| `UI/LevelButton.cs` | `Setup(number, unlocked, stars)`; alanlar: `numberText`→Number, `lockedIcon`→Lock, `starIcons[]`→[Star0,Star1,Star2] |
| `UI/LevelSelectUI.cs` | Level grid'i runtime doldurur (LevelButton prefab) |
| `UI/BoosterPanel.cs` | Booster butonları |
| `UI/CoinShopPanel.cs` / `DailyRewardPanel.cs` / `SettingsPanel.cs` / `StatsPanel.cs` / `AchievementListPanel.cs` | Pop-up mantıkları |
| `UI/WinPanel.cs` / `LosePanel.cs` / `PauseButtonHooks.cs` | Sonuç panelleri |
| `UI/LocalizedText.cs` | TMP'ye key ile çeviri basar (`Apply()`) |
| `UI/LanguageToggleHook.cs` | **satır 32:** dil butonu yazısını runtime'da set eder → `"TR / EN"` |
| `UI/TMPArcText.cs` | **BİZİM YAZDIĞIMIZ** — TMP mesh'ini parabolik arc ile büker (`arcHeight`), banner başlıkları için |
| `UI/PanelAnimator.cs` | Panel açılış animasyonu (**CanvasGroup.alpha** ile — ekran görüntüsü alırken sorun çıkarır) |

---

## 5. Yeni UI Asset Seti (2026-07 tasarım yenilemesi)

Tüm yeni görseller AI ile (Grok) üretilip Python (Pillow + scipy) ile kesildi.
Ortak dil: **krem/candy "Toon Blast" tarzı**, altın çerçeveli, banner başlıklı.

### Sprite envanteri (`Assets/Sprites/`)
| Sprite | Kullanım |
|---|---|
| `bg_menu.png` | Ana menü arka planı (1024×1536, **Sprite Mode: Multiple** — `bg_menu_0` alt-sprite'ı kullanılıyor) |
| `btn_plate_green/blue/purple/red.png` | Ana menü butonları (2000×640) |
| `btn_magaza/gunluk/istatistik/basarim.png` | Alt bar (512², ikon gömülü + kurdele bandı) |
| `panel_levelselect.png` | Seviye Seç çerçevesi |
| `panel_pause.png` | Pause + Günlük Ödül + İstatistik + Win/Lose ortak çerçevesi |
| `panel_pause_v2.png` | Yeni sade krem Pause paneli; üst başlık plakası entegre, kurdele yok |
| `panel_settings.png` | Ayarlar çerçevesi |
| `panel_shop.png` | Mağaza çerçevesi (656×850) |
| `leveltile_normal/done/locked.png` | Seviye tile'ları (256²) |
| `booster_hammer/shuffle/plus.png` | Booster tile'ları (256², kare) — Mağaza satırlarında da kullanılıyor |
| `pausebtn_green/blue/red.png` | Pause + Win/Lose pill butonları |
| `pausebtn_green/blue/red_v2.png` | Yeni Pause butonları; yazılar Unity TMP ile bindiriliyor |
| `settbtn_blue/green/orange.png` | Ayarlar pill'leri + Günlük Ödül "ÖDÜLÜ AL" |
| `slider_track/fill/handle.png` | Ayarlar slider'ları (**Pillow ile kodla çizildi**, 8x süpersample) |
| `cap_coin/heart/star.png` | Sol üst kaynak kapsülleri (768×~200) |
| `topbar_user.png` | Eski avatar+isim barı — **ARTIK KULLANILMIYOR** |

### Fontlar
| Font | Kullanım |
|---|---|
| `Fredoka SDF` | Genel UI (menü butonları, panel yazıları, HUD) |
| `Baloo2 SDF` | Seviye Seç başlığı |
| `PaytoneOne SDF` | Arkadaşın eklediği — ana menü fontu |

Hepsi Google Fonts (OFL), **dynamic atlas**, Türkçe karakterler (İ Ş Ğ Ç Ö Ü) destekli.

### Video logo
- `Assets/Video/logo.mp4` — 1280×720, **ping-pong** (289 kare, dikişsiz loop)
- Alfa YOK → `Assets/Shaders/UIChromaKey.shader` ile gri (#D3D3D3) runtime'da şeffaf yapılır
- Zincir: **VideoPlayer** (sahne kökünde) → **LogoRT.renderTexture** → **RawImage** (`LogoVideo`, material `LogoChroma`)
- `Packages/manifest.json`'a **`com.unity.modules.video`** eklendi (yoksa VideoPlayer play'de strip olur!)

---

## 6. BİLİNEN TUZAKLAR (bunları bilmeden vakit kaybedilir)

### 6.1 Unity / Editor
| Tuzak | Çözüm |
|---|---|
| **Editörü Bash `&` ile başlatma** — shell kapanınca Unity ölür | PowerShell `Start-Process` ile detached başlat |
| Temiz kapanmazsa **"Recovering Scene Backups"** ekranında takılır | `Desktop\mobile1game\Temp` klasörünü sil, yeniden aç |
| Play modunda `EditorSceneManager.MarkSceneDirty/SaveScene` **exception atar** | Önce `manage_editor stop`, sonra düzenle+kaydet |
| `Application.isPlaying` kontrolü yapmadan sahne düzenlemesi → değişiklik kaybolur | Her düzenleme başında `if(Application.isPlaying) return;` |
| TMP `outlineWidth` / `outlineColor` set etmek **edit modda NullReferenceException** atar | Bu iki alana dokunma; renk/pozisyon/autosize güvenli |
| Yeni PNG ekleyince beyaz kutu görünüyor | `TextureImporter.textureType = Sprite` **açıkça set et** — sadece `ImportAsset` yetmez |
| Play'e girince aktif sahne **Game**'e geçebilir (level yüklenince) | Düzenlemeden önce `SceneManager.GetActiveScene().name` kontrol et, gerekirse `OpenScene` |
| Panel'ler `CanvasGroup.alpha=0` ile kapalı gelir | Screenshot için `SetActive(true)` + `cg.alpha=1` |

### 6.2 MCP (Unity MCP bridge)
| Tuzak | Çözüm |
|---|---|
| **Bridge portu açılışlar arası değişir** (6402 → 6404 görüldü) | `C:\Users\syste\.unity-mcp\unity-mcp-status-*.json` içinden `unity_port` oku, `set_active_instance`'a ver |
| Köprü sık sık düşer | Port dinliyor mu kontrol et, `set_active_instance` ile yeniden bağlan |
| `execute_code` varsayılan derleyici **CodeDom (C# 6)** | `using` direktifi YOK, yerel fonksiyon YOK → **tam nitelikli isim** kullan (`UnityEditor.AssetDatabase.…`). `UnityEngine.Object.DestroyImmediate` (sadece `Object` belirsiz) |

### 6.3 Görsel/asset işleme (Python)
| Durum | Doğru yöntem |
|---|---|
| Grok çıktıları **JPEG** (alfa yok), zemin kareli/gri/siyah | Kenar-bağlantılı flood-fill ile zemin sil |
| **Beyaz outline'lı** sticker sprite | Zemin rengine düşük toleransla flood-fill (beyaz kenarlık korunur) |
| İçinde **gri/desatüre** eleman varsa (kilitli tile) | Doygunluk maskesi ELEMEZ → zemin-renk mesafesi kullan |
| Panel içindeki gömülü içeriği temizleme | **Satır-eşleşmeli** dolgu: her `y` için çevredeki kremi örnekle (düz renk dolgu "kare tepsi" görünümü yapar) |
| Nötr gri zemin vs sıcak krem panel | **Warmth maskesi**: `R - B > 30` |
| Çok parçalı sheet'i ayırma | `scipy.ndimage.label` → en büyük N bileşen → x/y merkezine göre sırala |

### 6.4 Layout
| Tuzak | Çözüm |
|---|---|
| **Kare sprite'ı dikdörtgen RectTransform'a** koymak → ezilme | Buton/container'ı sprite'ın en-boy oranına göre boyutlandır (booster'lar bu yüzden 210×210 kare yapıldı) |
| Farklı oranlı sprite'ları tek kutuda eşit bölmek → esneme | Her sprite için `W / aspect` ile ayrı yükseklik hesapla |
| Gömülü banner/slot'a yazı hizalama | Sprite'tan **piksel piksel ölç** (parlaklık/renk bandı tara), tahmin etme |

---

## 7. Build

```csharp
// Assets/Editor/AndroidBuildScript.cs
BalloonPop.EditorTools.AndroidBuildScript.BatchBuildAPK();
BalloonPop.EditorTools.AndroidBuildScript.BatchBuildAAB();
```
- Menü: `BalloonPop ▸ Build APK` veya `BalloonPop ▸ Build AAB for Play`
- Çıktılar: `Build/BalloonPop.apk`, `Build/BalloonPop.aab` (package `com.triogames.balloonpop`)
- IL2CPP + ARM64 + Release, min SDK 25, portre
- **Önce Android target'a geçiş gerekir** (Standalone'dayken build sessizce hiç başlamayabilir).
  `EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android)`
  → tüm asset'ler yeniden import edilir (dakikalar sürer) → sonra build.
- `Build/` **.gitignore'da**, APK GitHub **Release** olarak dağıtılıyor.
- AAB, kullanıcı profilindeki `C:\Users\syste\.android\balloon-pop-upload.keystore` ile imzalanır;
  şifreler repoya girmeyen `balloon-pop-upload-credentials.txt` dosyasından okunur. Her iki dosyayı
  güvenli biçimde yedekle; aynı upload anahtarı sonraki Play sürümleri için gereklidir.

---

## 8. Şu Anki Durum

### ✅ Tamamlanan ekranlar
Ana menü · Seviye Seç · Oyun HUD'u · Booster'lar · Pause · Ayarlar · Mağaza ·
Günlük Ödül · İstatistikler · Başarımlar · Win · Lose — hepsi yeni krem/candy asset setinde.

### ✅ Sol üst kaynak kapsülleri (tamamlandı / doğrulandı)
**Konum:** `MainMenu.unity` → `Menu Canvas / HeaderHUD`

Avatar+isim barı kaldırıldı; 3 kapsül (`cap_coin/heart/star`) sol üste alt alta dizildi ve
her biri kendi sprite oranıyla boyutlandırıldı (esneme yok). 2026-07-20'de Play modunda
1080×1920 Game View ile doğrulandı: tek haneli kalp/yıldız değerleri krem alanda ortalı,
okunaklı ve coin kapsülünden belirgin biçimde kısa.

```
CoinCard : 235 x 83    (Text anchor X 0.34–0.72, sağda + rozeti için yer)
HeartCard: 170 x 78    (Text anchor X 0.43–0.88; kısa, 1–2 hanelik alan)
StarCard : 170 x 78.5  (Text anchor X 0.43–0.88; kısa, 1–2 hanelik alan)
Y positions: 0 / -93 / -181; container anchoredPosition = (22, -22), pivot/anchor = sol üst
font autosize 16–34
```

Bu ölçüler `Candy UI Capsules _ Transparent.png` adlı yeni 2816×5920 transparan sheet'ten
oran korunarak kesilen `cap_coin/heart/star.png` assetleriyle Play modunda doğrulandı.

Ek olarak:
- Coin kapsülündeki gömülü yeşil `+` alanına şeffaf `Button` + `ButtonOpenTarget` eklendi;
  Play testinde `CoinShopPanel` açıldığı doğrulandı.
- Kalp tamken sayaç gizli kalır. Can eksikken kalp sayısı krem alanın soluna, `MM:SS` sayacı
  sağına yerleşir; `5 | 29:59` önizlemesi taşma ve çakışma olmadan doğrulandı.
- `SceneUIBuilder.cs` içindeki **HeaderHUD üretimi** mevcut kapsül sprite'ları, ölçüleri,
  tıklama alanı ve sayaç davranışıyla senkronlandı.

### ✅ Booster adet sayaçları (tamamlandı / doğrulandı)
`Game.unity` içindeki çekiç, karıştır ve +5 hamle kartlarının sağ üst saydam dairelerine
envanter adetleri yerleştirildi. `BoosterPanel.Refresh()` zaten `SaveSystem.Data` değerlerini
okuyordu; `Count` nesneleri kapalı eski `Badge` altından doğrudan `Body` altına taşındı.
Play testinde kayıt ve UI değerleri `1 / 1 / 1` olarak eşleşti. `SceneUIBuilder.cs` de yeni
kurulumlarda ekstra rozet çizmeden aynı sayaç yerleşimini üretecek şekilde güncellendi.

### ✅ Pause paneli v2 (tamamlandı / doğrulandı)
`balondor/Candy Pause Menu Panel.png` ve `Candy Game Buttons Set.png` kaynaklarındaki gömülü
dama arka planı temizlenerek `panel_pause_v2.png` ile üç `pausebtn_*_v2.png` sprite'ı üretildi.
`Game.unity` Pause paneli yeni oranlara göre hizalandı; başlık ve buton yazıları TMP olarak
korundu. 1080×1920 Play görünümünde panel, başlık ve üç buton görsel olarak doğrulandı.
`SceneUIBuilder.cs` yeni assetleri ve aynı yerleşimi yeniden üretecek şekilde senkronlandı.

### ✅ Ayarlar paneli v2 (tamamlandı / doğrulandı)
`MainMenu.unity` içindeki Ayarlar ekranı da `panel_pause_v2.png` tabanına geçirildi. Dil için
mavi, Kapat için yeşil v2 buton kullanılıyor; slider assetleri korundu. `TÜM SEVİYELERİ AÇ`
yazısı alt bölümde görünür bırakıldı ancak tıklanabilir bir kontrol değil; GM/geliştirme işlevi kodda tutuluyor.
Sağ üst kapatma işareti font uyumu için düz `X` yapıldı. Builder aynı düzenle senkronlandı.

### ✅ İkincil menü panelleri Candy UI v2 (tamamlandı / doğrulandı)
`MainMenu.unity` içindeki Mağaza, Günlük Ödül, İstatistikler ve Başarımlar panelleri
`panel_pause_v2.png` tabanına geçirildi. Mağaza satın alma satırları ve yeşil `AL` butonları,
Günlük Ödülün hediye ikonu ile yeşil alma butonu, istatistik satırları ve kaydırılabilir
başarım kartları yeni krem/altın görsel dile uyarlandı. Beş açma bağlantısı (alt bardaki dört
buton + coin kapsülündeki mağaza alanı) yeni panel örneklerine yönlendirildi. Dört ekran da
1080×1920 Play görünümünde doğrulandı; mevcut satın alma, ödül, sayaç ve başarım işlevleri korundu.
Mağaza ürün görselleri oyun HUD'uyla aynı güncel `booster_hammer`, `booster_shuffle` ve
`booster_plus` sprite'larını kullanıyor; eski `icon_*` görselleri mağazada kullanılmıyor.
`SceneUIBuilder.RebuildSecondaryMenuPanels()` yalnızca bu dört paneli güvenli biçimde yeniden
üretmek ve mevcut açma bağlantılarını taşımak için eklendi.

### ✅ Google Play Games (tamamlandı / yayınlandı)
Google Play Games Services `v2.1.0`, otomatik/manuel Google girişi, ana menüde giriş + liderlik
butonları ve her zaferde en yüksek bölüm skorunu gönderme akışı hazır. Play Games app ID
`867456826239`, `Highest Score` leaderboard ID `CgkI__bSw58ZEAIQAQ`; üretim ve debug Android
SHA-1 kimlik bilgileri Play Console'a bağlandı. Play Games değişiklikleri yayınlandı ve OAuth
uygulaması `In production` durumunda. Unity Android Setup kaynakları üretildi; Play Mode kontrolünde
servis `configured=True` ve menü bileşeni bağlı bulundu. IL2CPP/ARM64 debug APK derlemesi başarılı;
APK SHA-1 imzası Play Console'daki `Balloon Pop Debug` kimliğiyle eşleşiyor. Son kalan doğrulama,
bu APK üzerinde gerçek hesap seçme ve skor gönderme cihaz testidir. Ayrıca imzalı AAB
`0.1.0-internal-1` adıyla Dahili test kanalında yayınlandı; kanal etkin ve geliştirici Google
hesabı test davetini kabul etti. Katılım bağlantısı:
`https://play.google.com/apps/internaltest/4701107718413357996`. Ayrıntılar:
`GOOGLE_PLAY_GAMES_SETUP.md`.

2026-07-21'de liderlik butonunun Google Play Games'in harici ekranını açması kaldırıldı. Buton artık
`LeaderboardPanel` adlı oyun içi Candy UI panelini açıyor. `GooglePlayGamesService.LoadLeaderboard()`
Google Play Games'ten herkese açık, tüm zamanların ilk 10 skorunu `LoadScores` ile alıyor; oyuncu
adları `LoadUsers` ile tamamlanıyor. Panel sıra, oyuncu adı, skor ve kullanıcının kendi sırasını oyun
içinde gösteriyor; yenileme ve yeniden giriş akışları destekleniyor. Builder tarafındaki
`Rebuild Google Play Games Menu` komutu paneli ve tek satırlık runtime şablonunu da yeniden üretir.
Android APK derlemesi ve Unity Play Mode panel önizlemesi başarılıdır.

### 📋 Diğer açık işler
1. **10'lu avatar seti** — prompt hazırlandı, üretilmedi. Avatar barı kaldırıldığı için şu an gereksiz.
2. **`topbar_user.png`** — artık kullanılmıyor, silinebilir.
3. **`SceneUIBuilder.cs` genel senkronu** — HeaderHUD senkronlandı; diğer manuel görsel
   iyileştirmeler hâlâ denetlenmeli. `Run Full Auto-Setup` çalıştırmadan önce builder/sahne farkını kontrol et.

---

## 9. Çalışma Yöntemi Önerisi

Bu projede en çok vakit kaybettiren şey **tahminle konum ayarlamak** oldu. İşe yarayan yöntem:

1. **Ölç, tahmin etme.** Sprite'ın içindeki banner/slot/ikon konumunu Python ile piksel tarayıp
   normalize koordinata çevir, sonra Unity anchor'ına aynı sayıyı ver.
2. **Tek mockup, sabit ölçü.** Bir ekranın tüm parçalarını tek görselde ürettir; parça parça
   üretim tutarsızlık yaratıyor.
3. **Tek geçişte uygula, tek doğrulama.** Her mikro değişiklik için stop→uygula→play→screenshot
   turu atmak çok pahalı; değişiklikleri toplu uygulayıp bir kez doğrula.
4. Sahne değişikliği → **mutlaka edit modda** + `SaveOpenScenes()`.

---

## 10. Hızlı Komut Referansı

```bash
# Editörü detached başlat
powershell -NoProfile -Command "Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe' -ArgumentList '-projectPath','C:\Users\syste\Desktop\mobile1game'"

# MCP portunu öğren
cat C:/Users/syste/.unity-mcp/unity-mcp-status-*.json

# Git akışı (ortak repo!)
cd C:/Users/syste/Desktop/mobile1game
git pull origin main
# ... değişiklikler ...
git add -A && git commit -m "mesaj" && git push origin main

# APK'yı Release'e yükle
gh release create vX.Y.Z Build/BalloonPop.apk --repo db1881/mobile1game --title "..." --notes "..."
```
