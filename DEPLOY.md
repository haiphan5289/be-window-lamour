# Lamour — Deployment Guide

## Cấu trúc thư mục trên máy Windows

```
D:\app-lamour\
├── start-lamour.bat              ← double-click để khởi động
├── LamourApi\
│   └── api-win\
│       ├── Lamour.Api.exe
│       └── appsettings.json      ← cấu hình DB connection string
└── LamourDesktop\
    └── desktop-win\
        ├── DesktopLamour.exe
        └── appsettings.json      ← cấu hình server URL
```

---

## Khởi động ứng dụng

Double-click `D:\app-lamour\start-lamour.bat`

Bat file sẽ:
1. Khởi động Lamour API (minimized, chạy nền)
2. Chờ 20 giây để API init xong
3. Mở DesktopLamour WPF

---

## Đổi server URL (khi deploy khác máy)

Mở `D:\app-lamour\LamourDesktop\desktop-win\appsettings.json` bằng Notepad:

```json
{
  "ServerUrl": "http://192.168.1.50:5282"
}
```

Thay IP thành IP của máy chạy BE. Không cần rebuild lại WPF.

---

## Prerequisites

- **PostgreSQL** phải đang chạy (Service `postgresql-x64-16`, auto-start với Windows)
- Không cần cài .NET runtime — cả API lẫn WPF đều là self-contained exe

---

## Publish từ Mac (khi có code mới)

### BE (ASP.NET Core → Windows)

```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
dotnet publish src/Lamour.Api \
  -r win-x64 \
  --self-contained true \
  -c Release \
  -o publish/api-win
```

Copy thư mục `publish/api-win/` lên `D:\app-lamour\LamourApi\api-win\` trên Windows.

### WPF (Windows, chạy trên UTM)

> ⚠️ Luôn đủ 3 lệnh theo đúng thứ tự **sync → publish → zip**. Thiếu `Compress-Archive` (hoặc chạy
> trước `dotnet publish`) thì `desktop-win-new.zip` không xuất hiện lại trên Mac, hoặc chứa nhầm
> build cũ — xem chi tiết ở mục "Quy trình update build chuẩn" bên dưới. **Không copy tay
> `publish\desktop-win\` sang máy đích** — luôn đi qua bước zip này.

```powershell
cd C:\projects\desktop-lamour
.\sync.ps1

dotnet publish src\DesktopLamour -r win-x64 --self-contained true -c Release -o publish\desktop-win

Compress-Archive -Path "C:\projects\desktop-lamour\publish\desktop-win\*" -DestinationPath "Z:\publish\desktop-win-new.zip" -Force
```

Zip xuất hiện lại trên Mac tại `desktop-lamour/publish/desktop-win-new.zip` (qua `Z:\` map) — giải nén rồi copy vào `D:\app-lamour\LamourDesktop\desktop-win\` trên máy đích.

---

## Các vấn đề đã fix

### 1. WPF hardcode IP Mac UTM
**Nguyên nhân:** `HomeServiceCollectionExtensions.cs` và `AuthenticationServiceCollectionExtensions.cs` hardcode `http://192.168.64.1:5282`.

**Fix:** Đọc URL từ `appsettings.json` qua `IConfiguration` trong `App.xaml.cs`, truyền vào DI qua `serverUrl` parameter.

### 2. `start-lamour.bat` sai path API
**Nguyên nhân:** Path `D:\app-lamour\LamourApi\Lamour.Api.exe` thiếu subfolder `api-win`.

**Fix:** Sửa thành `D:\app-lamour\LamourApi\api-win\Lamour.Api.exe`.

### 3. API không start được qua bat (login failed)
**Nguyên nhân:** `start /min exe` không set đúng working directory → API không đọc được config.

**Fix:** Thêm `/d "D:\app-lamour\LamourApi\api-win"` vào lệnh `start` để set working directory đúng.

```bat
start "Lamour API" /min /d "D:\app-lamour\LamourApi\api-win" "D:\app-lamour\LamourApi\api-win\Lamour.Api.exe"
```

---

## Test sau khi deploy

1. Double-click `start-lamour.bat`
2. Chờ ~20 giây
3. WPF mở tự động
4. Login với số điện thoại + mật khẩu
5. Mở Task Manager → Processes → kiểm tra có `Lamour.Api` và `DesktopLamour` trong Apps

---

## Khi update build (lần sau deploy nhanh hơn)

### Checklist các lỗi hay gặp

| Lỗi | Nguyên nhân | Fix |
|---|---|---|
| `password authentication failed for user "lamour"` | `appsettings.Production.json` có `Password=CHANGE_ME` chưa đổi | Sửa `D:\app-lamour\LamourApi\api-win\appsettings.Production.json` |
| Login failed trên WPF | ServerUrl sai IP | Sửa `D:\app-lamour\LamourDesktop\desktop-win\appsettings.json` thành `localhost` nếu cùng máy |
| WPF hiện build cũ | Copy nhầm file cũ từ Mac | Lấy file từ UTM `C:\projects\desktop-lamour\publish\desktop-win\` |
| WPF hiện code cũ dù publish trên UTM | Chạy `Compress-Archive` ngay sau `sync.ps1`, quên chạy `dotnet publish` ở giữa | Luôn theo đúng thứ tự: `sync.ps1` → `dotnet publish` → `Compress-Archive` |

### Quy trình update build chuẩn

**Bước 1 — Publish BE (Mac terminal):**
```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
dotnet publish src/Lamour.Api -r win-x64 --self-contained true -c Release -o publish/api-win
```

**Bước 2 — Sync + Publish + Zip WPF (UTM PowerShell):**
```powershell
cd C:\projects\desktop-lamour
.\sync.ps1

dotnet publish src\DesktopLamour -r win-x64 --self-contained true -c Release -o publish\desktop-win

Compress-Archive -Path "C:\projects\desktop-lamour\publish\desktop-win\*" -DestinationPath "Z:\publish\desktop-win-new.zip" -Force
```

> **⚠️ QUAN TRỌNG:** Phải chạy `.\sync.ps1` **trước** `dotnet publish`, và **không được bỏ qua bước publish**. `sync.ps1` chỉ copy source code (`src\`) từ Mac sang UTM — nó không tự build. Nếu zip/copy output ngay sau khi sync mà quên chạy `dotnet publish`, `Compress-Archive` sẽ nén **build cũ** còn nằm sẵn trong `publish\desktop-win\` từ lần trước, code mới vừa sync sẽ không được đưa vào exe. Thứ tự bắt buộc: **sync → publish → zip**.
>
> Zip xong sẽ xuất hiện trên Mac tại `desktop-lamour/publish/desktop-win-new.zip` (do `Z:\` map tới folder đó) — **không tự xuất hiện** nếu bỏ qua lệnh `Compress-Archive`, vì `publish\desktop-win\` là ổ `C:\` local của UTM, không sync ngược về Mac.

> **Lưu ý:** Nếu báo lỗi `The path 'Z:\publish' either does not exist...`, thư mục `publish` trong ổ `Z:\` chưa tồn tại (Compress-Archive không tự tạo thư mục cha). Tạo trước:
> ```powershell
> dir Z:\                      # kiểm tra ổ Z có mount không
> mkdir Z:\publish -Force      # tạo thư mục publish nếu chưa có
> ```
> rồi chạy lại lệnh Compress-Archive.

**Bước 3 — Dừng app trên máy đích:**
```powershell
Stop-Process -Name "Lamour.Api" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "DesktopLamour" -Force -ErrorAction SilentlyContinue
```

**Bước 4 — Copy file lên máy đích (dùng TeamViewer File Transfer):**
- BE: copy toàn bộ `publish/api-win/` → `D:\app-lamour\LamourApi\api-win\`
- WPF: copy `desktop-win-new.zip` → extract vào `D:\app-lamour\LamourDesktop\desktop-win\`

**Bước 5 — Kiểm tra 3 file config quan trọng sau mỗi lần copy:**

`D:\app-lamour\LamourApi\api-win\appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=lamour_db;Username=lamour;Password=lamour123"
  },
  "Jwt": { "Key": "supersecretkey_changeme_32chars!!" },
  "Urls": "http://0.0.0.0:5282"
}
```

`D:\app-lamour\LamourDesktop\desktop-win\appsettings.json`:
```json
{
  "ServerUrl": "http://localhost:5282"
}
```

> **⚠️ QUAN TRỌNG:** Mỗi lần `dotnet publish src/Lamour.Api` từ Mac, file `appsettings.Production.json` trong output LUÔN chứa `Password=CHANGE_ME` (giá trị placeholder trong source code, không phải mật khẩu thật). Publish mới **KHÔNG** giữ lại giá trị `lamour123` đã sửa lần trước.
>
> → Copy `publish/api-win/` sang máy đích **KHÔNG đè** file `appsettings.Production.json`, hoặc copy đè xong thì **sửa lại ngay** `Password=CHANGE_ME` → `Password=lamour123` trước khi chạy `start-lamour.bat`. Nếu quên bước này, `Lamour.Api` sẽ không connect được DB (không thấy process trong Task Manager hoặc WPF báo "Login failed" chung chung).

**Bước 6 — Chạy lại:**
```
Double-click D:\app-lamour\start-lamour.bat
```

### Lưu ý quan trọng

- **KHÔNG** lấy WPF publish từ Mac path `/Users/haiphan/.../publish/desktop-win/` — file đó cũ, publish WPF phải chạy trên UTM
- **appsettings.Production.json** của API luôn override `appsettings.json` — kiểm tra file này trước tiên khi gặp lỗi DB
- Password PostgreSQL user `lamour`: `lamour123`
- Nếu quên password `postgres`: dùng pgAdmin hoặc reset qua `pg_hba.conf`

### Credentials máy Windows (lưu để không quên)

| Thứ | Giá trị |
|---|---|
| PostgreSQL user | `lamour` |
| PostgreSQL password | `lamour123` |
| App login (phone) | `0901234567` |
| App login (password) | `123456` |
