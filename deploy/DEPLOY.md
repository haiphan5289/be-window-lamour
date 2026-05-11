# Hướng dẫn Deploy Lamour lên Windows Server

## Tổng quan kiến trúc

```
[Windows Server]                    [Máy client Windows]
  ├─ PostgreSQL (port 5432)           └─ Lamour Desktop App (.exe)
  └─ Lamour API  (port 5282)              └─ kết nối tới http://<SERVER_IP>:5282
```

---

## Bước 1 — Cài đặt PostgreSQL trên Windows Server

1. Tải PostgreSQL cho Windows: https://www.postgresql.org/download/windows/
2. Cài đặt với user mặc định `postgres`
3. Sau khi cài xong, mở **psql** hoặc **pgAdmin** và chạy script setup:

```bash
psql -U postgres -f postgresql-setup.sql
```

> Script này tạo user `lamour` và database `lamour_db`.
> **Nhớ đổi** `CHANGE_ME_STRONG_PASSWORD` trong file thành mật khẩu thực.

---

## Bước 2 — Publish BE API

Chạy trên **máy developer** (có .NET SDK):

```bat
publish-be.bat
```

Output: `publish\be\` — copy toàn bộ thư mục này lên Windows Server vào `C:\lamour\be\`

---

## Bước 3 — Cấu hình BE trên Windows Server

Mở file `C:\lamour\be\appsettings.Production.json` và sửa:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=lamour_db;Username=lamour;Password=<mật_khẩu_ở_bước_1>"
  },
  "Jwt": {
    "Key": "supersecretkey_changeme_32chars!!"
  },
  "Urls": "http://0.0.0.0:5282"
}
```

> ⚠️ Đổi `Jwt.Key` thành một chuỗi bí mật khác nếu muốn bảo mật hơn.

---

## Bước 4 — Chạy EF Migration trên Windows Server

Trên máy developer, chạy migration vào DB của Windows Server:

```bash
# Đổi connection string tạm thời trong appsettings.json trỏ tới server
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update \
  --project src/Lamour.Infrastructure \
  --startup-project src/Lamour.Api
```

Hoặc copy và chạy migration trực tiếp trên Windows Server:
```bat
cd C:\lamour\be
set ASPNETCORE_ENVIRONMENT=Production
Lamour.Api.exe --migrate
```

---

## Bước 5 — Đăng ký BE như Windows Service

Chạy **với quyền Administrator** trên Windows Server:

```bat
install-service.bat
```

BE sẽ tự khởi động khi Windows Server boot. Kiểm tra:

```bat
sc query LamourApi
```

---

## Bước 6 — Mở firewall cho port 5282

Trên Windows Server, chạy **với quyền Administrator**:

```powershell
netsh advfirewall firewall add rule `
  name="Lamour API" `
  dir=in action=allow protocol=TCP localport=5282
```

Kiểm tra BE đã chạy: mở browser trên máy khác → `http://<SERVER_IP>:5282/api/v1/employees`

---

## Bước 7 — Đổi IP trong WPF client

> Chỉ cần làm khi IP server thay đổi.

File cần sửa:
```
desktop-lamour/src/DesktopLamour/Features/HomePage/HomeServiceCollectionExtensions.cs
```

Tìm tất cả (10 chỗ):
```
http://192.168.64.1:5282
```

Thay bằng:
```
http://<SERVER_IP>:5282
```

---

## Bước 8 — Build WPF installer

Chạy trên **máy developer Windows**:

```bat
cd desktop-lamour
deploy\publish-wpf.bat
```

Sau đó:
1. Tải Inno Setup: https://jrsoftware.org/isdl.php
2. Mở `deploy\installer.iss` bằng Inno Setup
3. Nhấn **Build > Compile** (Ctrl+F9)
4. File `deploy\output\LamourSetup-1.0.0.exe` sẽ được tạo

---

## Bước 9 — Cài WPF trên máy client

Chạy `LamourSetup-1.0.0.exe` trên từng máy Windows client.

Installer sẽ:
- Cài app vào `C:\Program Files\Lamour\`
- Tạo shortcut trên Desktop (tuỳ chọn)
- Thêm vào Add/Remove Programs để có thể gỡ cài đặt

---

## Troubleshooting

| Lỗi | Nguyên nhân | Fix |
|---|---|---|
| WPF không kết nối được BE | Sai IP hoặc port | Kiểm tra `HomeServiceCollectionExtensions.cs` + firewall |
| BE lỗi 500 khi khởi động | Sai connection string | Kiểm tra `appsettings.Production.json` |
| Login thất bại (401) | JWT Key không khớp | Đảm bảo `Jwt.Key` giống nhau trong config |
| `role "lamour" does not exist` | Chưa chạy `postgresql-setup.sql` | Chạy lại Bước 1 |
| Port 5282 không truy cập được | Firewall chặn | Chạy lại Bước 6 |
