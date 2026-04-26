#!/bin/bash
P=$(jq -r '.prompt // ""')
BE="/Users/hai.phan/Desktop/haiphan/be-window-lamour/src/Lamour.Application/Features"
APP="/Users/hai.phan/Desktop/haiphan/desktop-lamour/src/DesktopLamour/Features/HomePage"
DOCS=""

check() {
  local pattern="$1" be_feat="$2" app_feat="$3"
  if echo "$P" | grep -qiE "$pattern"; then
    [ -n "$be_feat" ] && [ -d "$BE/$be_feat" ] && DOCS="$DOCS\n  BE: $BE/$be_feat/"
    [ -n "$app_feat" ] && [ -d "$APP/$app_feat" ] && DOCS="$DOCS\n  App: $APP/$app_feat/"
  fi
}

check "employee|nhan vien|nhân viên" "Employees/docs" "Employees/docs"
check "customer|khach hang|khách hàng" "Customers/docs" "Customers/docs"
check "product|san pham|sản phẩm" "Products/docs" "ProductList/docs"
check "supplier|nha cung cap|nhà cung cấp" "Suppliers/docs" "Suppliers/docs"
check "auth|login|dang nhap|đăng nhập" "Auth" ""
check "home|tong quan|tổng quan" "" "Home/docs"

[ -n "$DOCS" ] && printf "Reminder: read feature docs before coding. Relevant docs found:%b\n" "$DOCS"
exit 0
