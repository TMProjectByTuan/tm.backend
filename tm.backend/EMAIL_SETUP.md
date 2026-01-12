# Email Configuration Guide

## Cấu hình Email Service

Hệ thống sử dụng MailKit để gửi email qua SMTP. Bạn cần cấu hình thông tin SMTP trong `appsettings.json`.

### Cấu hình cho Gmail

1. Mở file `tm.Api/appsettings.json`

2. Cập nhật phần `EmailSettings`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "TM Project Management"
  }
}
```

### Tạo App Password cho Gmail

1. Đăng nhập vào tài khoản Google của bạn
2. Truy cập: https://myaccount.google.com/apppasswords
3. Chọn "Mail" và "Other (Custom name)"
4. Nhập tên: "TM Project"
5. Google sẽ tạo một mật khẩu 16 ký tự
6. Copy mật khẩu này và dán vào `SmtpPassword` trong `appsettings.json`

**Lưu ý:** Bạn cần bật 2-Step Verification trước khi có thể tạo App Password.

### Cấu hình cho các SMTP server khác

#### Outlook/Hotmail
```json
{
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": "587"
}
```

#### Yahoo Mail
```json
{
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": "587"
}
```

#### Custom SMTP Server
Chỉ cần thay đổi `SmtpServer` và `SmtpPort` theo cấu hình của server của bạn.

## Email được gửi tự động

1. **Email chào mừng**: Gửi khi user đăng ký tài khoản mới
2. **Email mời thành viên**: Gửi khi leader mời member vào project

## Testing

Sau khi cấu hình, bạn có thể test bằng cách:
1. Đăng ký một tài khoản mới - sẽ nhận được email chào mừng
2. Tạo project và mời thành viên - thành viên sẽ nhận được email mời

## Troubleshooting

- **Lỗi "Authentication failed"**: Kiểm tra lại username và password (App Password cho Gmail)
- **Lỗi "Connection timeout"**: Kiểm tra firewall và port 587 có bị chặn không
- **Email không đến**: Kiểm tra spam folder, hoặc thử với email khác

