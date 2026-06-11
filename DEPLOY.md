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

```bat
cd C:\projects\desktop-lamour
dotnet publish src\DesktopLamour -r win-x64 --self-contained true -c Release -o publish\desktop-win
```

Copy thư mục `publish\desktop-win\` lên `D:\app-lamour\LamourDesktop\desktop-win\`.

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

### Quy trình update build chuẩn

**Bước 1 — Publish BE (Mac terminal):**
```bash
cd /Users/hai.phan/Desktop/haiphan/be-window-lamour
dotnet publish src/Lamour.Api -r win-x64 --self-contained true -c Release -o publish/api-win
```

**Bước 2 — Publish WPF (UTM PowerShell):**
```powershell
cd C:\projects\desktop-lamour
dotnet publish src\DesktopLamour -r win-x64 --self-contained true -c Release -o publish\desktop-win
```

**Bước 3 — Zip WPF từ UTM ra Mac (UTM PowerShell):**
```powershell
Compress-Archive -Path "C:\projects\desktop-lamour\publish\desktop-win\*" `
  -DestinationPath "Z:\publish\desktop-win-new.zip" -Force
```

**Bước 4 — Dừng app trên máy đích:**
```powershell
Stop-Process -Name "Lamour.Api" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "DesktopLamour" -Force -ErrorAction SilentlyContinue
```

**Bước 5 — Copy file lên máy đích (dùng TeamViewer File Transfer):**
- BE: copy toàn bộ `publish/api-win/` → `D:\app-lamour\LamourApi\api-win\`
- WPF: copy `desktop-win-new.zip` → extract vào `D:\app-lamour\LamourDesktop\desktop-win\`

**Bước 6 — Kiểm tra 3 file config quan trọng sau mỗi lần copy:**

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

**Bước 7 — Chạy lại:**
```
Double-click D:\app-lamour\start-lamour.bat
```

### Lưu ý quan trọng

- **KHÔNG** lấy WPF publish từ Mac path `/Users/hai.phan/.../publish/desktop-win/` — file đó cũ, publish WPF phải chạy trên UTM
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
