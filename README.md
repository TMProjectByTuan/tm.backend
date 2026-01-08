# TM Project

Monorepo chứa cả Backend (.NET) và Frontend (Next.js) cho dự án TM.

## 📁 Cấu trúc Project

```
TM-PROJECT/
├── tm.backend/          # Backend API (.NET 8)
│   ├── tm.Api/          # API Layer
│   ├── tm.Application/  # Business Logic
│   ├── tm.Domain/       # Domain Models
│   └── tm.Infrastructure/ # Infrastructure (Database, etc.)
├── tm.frontend/         # Frontend (Next.js)
├── docker-compose.yml   # Docker configuration
└── .github/workflows/    # CI/CD workflows
```

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 20.x
- Docker Desktop (cho SQL Server)
- Git

### Backend Setup

Xem chi tiết trong [tm.backend/README.md](tm.backend/README.md)

### Frontend Setup

```bash
cd tm.frontend
npm install
npm run dev
```

## 🔄 Git & GitHub

### Setup Git Remotes

Project này là monorepo, có thể push lên 1 trong 2 repos:

**Option 1: Push lên repo backend (khuyến nghị cho monorepo)**
```bash
git remote add origin git@github.com:TMProjectByTuan/tm.backend.git
```

**Option 2: Push lên repo frontend**
```bash
git remote add origin git@github.com:TMProjectByTuan/tm.frontend.git
```

**Option 3: Push lên cả 2 repos (nếu muốn đồng bộ)**
```bash
git remote add backend git@github.com:TMProjectByTuan/tm.backend.git
git remote add frontend git@github.com:TMProjectByTuan/tm.frontend.git
```

### Push Code

```bash
# Lần đầu tiên
git add .
git commit -m "Initial commit"
git branch -M main
git push -u origin main

# Các lần sau
git add .
git commit -m "Your commit message"
git push
```

## 🔧 CI/CD

Project có 3 workflows CI/CD:

1. **Backend CI** (`.github/workflows/backend-ci.yml`)
   - Chạy khi có thay đổi trong `tm.backend/`
   - Build và test backend

2. **Frontend CI** (`.github/workflows/frontend-ci.yml`)
   - Chạy khi có thay đổi trong `tm.frontend/`
   - Lint và build frontend

3. **Full CI** (`.github/workflows/full-ci.yml`)
   - Chạy cả backend và frontend
   - Trigger khi push vào `main` hoặc `develop`

## 📝 Development Workflow

1. Tạo branch mới: `git checkout -b feature/your-feature`
2. Code và commit: `git commit -m "Add feature"`
3. Push và tạo PR: `git push origin feature/your-feature`
4. CI/CD sẽ tự động chạy khi có PR

## 🐳 Docker

Xem chi tiết trong [tm.backend/README.md](tm.backend/README.md) phần Docker Commands.

## 📚 Documentation

- [Backend Documentation](tm.backend/README.md)
- [Frontend Documentation](tm.frontend/README.md)

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

[Your License Here]

