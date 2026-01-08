# Hướng dẫn Setup GitHub và CI/CD

## 📋 Bước 1: Kiểm tra SSH Key

Đảm bảo bạn đã có SSH key và đã add vào GitHub:

```bash
# Kiểm tra SSH key
ls -al ~/.ssh

# Nếu chưa có, tạo mới
ssh-keygen -t ed25519 -C "your_email@example.com"

# Copy public key để add vào GitHub
cat ~/.ssh/id_ed25519.pub
```

Sau đó:
1. Vào GitHub → Settings → SSH and GPG keys
2. Click "New SSH key"
3. Paste public key vào
4. Save

## 🔗 Bước 2: Setup Git Remotes

Bạn có 2 repos trên GitHub:
- `TMProjectByTuan/tm.backend`
- `TMProjectByTuan/tm.frontend`

Vì đây là monorepo (cả backend và frontend trong 1 repo), bạn có 3 options:

### Option 1: Push lên repo backend (Khuyến nghị)

```bash
git remote add origin git@github.com:TMProjectByTuan/tm.backend.git
```

### Option 2: Push lên repo frontend

```bash
git remote add origin git@github.com:TMProjectByTuan/tm.frontend.git
```

### Option 3: Push lên cả 2 repos (đồng bộ)

```bash
git remote add backend git@github.com:TMProjectByTuan/tm.backend.git
git remote add frontend git@github.com:TMProjectByTuan/tm.frontend.git
```

**Lưu ý:** Nếu chọn Option 3, bạn sẽ cần push vào cả 2 remotes:
```bash
git push backend main
git push frontend main
```

## 📤 Bước 3: Push Code lên GitHub

### Lần đầu tiên:

```bash
# Add tất cả files
git add .

# Commit
git commit -m "Initial commit: Setup monorepo with backend and frontend"

# Đổi tên branch thành main (nếu đang ở master)
git branch -M main

# Push lên GitHub
git push -u origin main
```

### Các lần sau:

```bash
git add .
git commit -m "Your commit message"
git push
```

## ✅ Bước 4: Kiểm tra CI/CD

Sau khi push code:

1. Vào GitHub repository
2. Click tab **Actions**
3. Bạn sẽ thấy workflows đang chạy:
   - **Backend CI** - chạy khi có thay đổi trong `tm.backend/`
   - **Frontend CI** - chạy khi có thay đổi trong `tm.frontend/`
   - **Full CI** - chạy khi push vào `main` hoặc `develop`

4. Click vào workflow để xem chi tiết

## 🔍 Kiểm tra Git Status

```bash
# Xem remotes đã setup
git remote -v

# Xem status
git status

# Xem branches
git branch -a
```

## 🐛 Troubleshooting

### Lỗi: "Permission denied (publickey)"

- Kiểm tra SSH key đã add vào GitHub chưa
- Test SSH connection:
  ```bash
  ssh -T git@github.com
  ```

### Lỗi: "remote origin already exists"

- Xem remotes hiện tại:
  ```bash
  git remote -v
  ```
- Xóa remote cũ:
  ```bash
  git remote remove origin
  ```
- Thêm lại:
  ```bash
  git remote add origin git@github.com:TMProjectByTuan/tm.backend.git
  ```

### Lỗi: "failed to push some refs"

- Pull code mới nhất trước:
  ```bash
  git pull origin main --rebase
  ```
- Sau đó push lại:
  ```bash
  git push
  ```

## 📝 Workflow đề xuất

1. **Tạo branch mới cho feature:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Code và commit:**
   ```bash
   git add .
   git commit -m "Add: your feature description"
   ```

3. **Push branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

4. **Tạo Pull Request trên GitHub:**
   - Vào repository trên GitHub
   - Click "Compare & pull request"
   - CI/CD sẽ tự động chạy khi tạo PR

5. **Sau khi merge PR:**
   ```bash
   git checkout main
   git pull origin main
   ```

## 🎯 Best Practices

- ✅ Commit thường xuyên với message rõ ràng
- ✅ Tạo branch riêng cho mỗi feature
- ✅ Kiểm tra CI/CD pass trước khi merge
- ✅ Review code trước khi merge vào main
- ❌ Không commit secrets/passwords
- ❌ Không commit file `.env` hoặc `appsettings.Production.json`

