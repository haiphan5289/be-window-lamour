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
