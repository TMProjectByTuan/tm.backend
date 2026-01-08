# TM Backend - Hướng dẫn phát triển

## 📋 Mục lục

- [Cấu trúc Project](#cấu-trúc-project)
- [Tạo Entity mới](#tạo-entity-mới)
- [Tạo Migration](#tạo-migration)
- [Update Database](#update-database)
- [Docker Commands](#docker-commands)
- [Xem Swagger](#xem-swagger)
- [Các lệnh hữu ích khác](#các-lệnh-hữu-ích-khác)

---

## 🏗️ Cấu trúc Project

```
tm.backend/
├── tm.Api/              # API Layer - Controllers, Endpoints
├── tm.Application/      # Business Logic Layer
├── tm.Domain/           # Domain Layer - Entities, Interfaces
│   └── Entities/        # Các Entity models
└── tm.Infrastructure/   # Infrastructure Layer - Database, External Services
    ├── Persistence/     # DbContext, Configurations
    └── Migrations/       # EF Core Migrations
```

---

## 🆕 Tạo Entity mới

### Bước 1: Tạo Entity trong `tm.Domain/Entities/`

Tạo file mới trong thư mục `tm.Domain/Entities/`, ví dụ: `Product.cs`

```csharp
using tm.Domain.Entities;

namespace tm.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
```

**Lưu ý:** Entity phải kế thừa từ `BaseEntity` để có các thuộc tính:
- `Id` (Guid)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime?)

### Bước 2: Thêm DbSet vào ApplicationDbContext

Mở file `tm.Infrastructure/Persistence/ApplicationDbContext.cs` và thêm:

```csharp
public DbSet<Product> Products { get; set; }
```

**Ví dụ đầy đủ:**

```csharp
using Microsoft.EntityFrameworkCore;
using tm.Application.Common.Interfaces;
using tm.Domain.Entities;

namespace tm.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Thêm DbSet cho Entity mới
    public DbSet<Product> Products { get; set; }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### Bước 3: (Tùy chọn) Tạo Entity Configuration

Nếu cần cấu hình chi tiết cho Entity (indexes, constraints, relationships), tạo file trong `tm.Infrastructure/Persistence/Configurations/`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using tm.Domain.Entities;

namespace tm.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");
            
        builder.HasIndex(p => p.Name);
    }
}
```

---

## 🔄 Tạo Migration

Sau khi tạo Entity và thêm DbSet, tạo migration để áp dụng thay đổi vào database:

### Lệnh tạo Migration

```bash
# Từ thư mục gốc project (D:\TM-PROJECT)
dotnet ef migrations add <TênMigration> --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

**Ví dụ:**

```bash
dotnet ef migrations add AddProductEntity --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

### Xóa Migration gần nhất (nếu cần sửa)

```bash
dotnet ef migrations remove --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

### Xem danh sách Migrations

```bash
dotnet ef migrations list --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

---

## 🗄️ Update Database

### Cách 1: Update Database trực tiếp (không qua Docker)

```bash
dotnet ef database update --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

### Cách 2: Update Database qua Docker

#### Bước 1: Đảm bảo SQL Server container đang chạy

```bash
docker-compose ps
```

Nếu container chưa chạy:

```bash
docker-compose up -d sqlserver
```

#### Bước 2: Chạy migration

```bash
dotnet ef database update --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

**Lưu ý:** Connection string trong `appsettings.json` phải trỏ đến `localhost`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TMProjectDb;User=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Xem Migration đã được apply

```bash
dotnet ef migrations list --project tm.backend\tm.Infrastructure --startup-project tm.backend\tm.Api
```

---

## 🐳 Docker Commands

### Kiểm tra trạng thái containers

```bash
docker-compose ps
```

### Khởi động SQL Server container

```bash
docker-compose up -d sqlserver
```

### Khởi động tất cả services (SQL Server + API)

```bash
docker-compose up -d
```

### Dừng SQL Server container

```bash
docker-compose stop sqlserver
```

### Dừng tất cả containers

```bash
docker-compose stop
```

### Dừng và xóa containers

```bash
docker-compose down
```

### Xem logs của SQL Server container

```bash
docker-compose logs sqlserver
```

### Xem logs real-time

```bash
docker-compose logs -f sqlserver
```

### Xóa tất cả (containers + volumes)

⚠️ **Cảnh báo:** Lệnh này sẽ xóa cả database data!

```bash
docker-compose down -v
```

### Rebuild và khởi động lại

```bash
docker-compose up -d --build
```

---

## 📚 Xem Swagger

### Cách 1: Chạy ứng dụng và mở trình duyệt

```bash
cd tm.backend\tm.Api
dotnet run
```

Sau đó mở trình duyệt và truy cập:
- **HTTP:** http://localhost:5290/swagger
- **HTTPS:** https://localhost:7243/swagger

### Cách 2: Sử dụng launchSettings.json

Trong Visual Studio hoặc VS Code, chọn profile **http** hoặc **https** và nhấn F5. Swagger sẽ tự động mở.

### Cách 3: Chạy qua Docker

```bash
docker-compose up -d
```

Sau đó truy cập: http://localhost:8080/swagger

---

## 🛠️ Các lệnh hữu ích khác

### Cài đặt dotnet-ef tool (nếu chưa có)

```bash
dotnet tool install --global dotnet-ef
```

### Cập nhật dotnet-ef tool

```bash
dotnet tool update --global dotnet-ef
```

### Build project

```bash
dotnet build
```

### Chạy project

```bash
cd tm.backend\tm.Api
dotnet run
```

### Restore packages

```bash
dotnet restore
```

### Clean build artifacts

```bash
dotnet clean
```

### Xem thông tin về migrations trong database

Kết nối SQL Server và chạy query:

```sql
SELECT * FROM [TMProjectDb].[dbo].[__EFMigrationsHistory]
ORDER BY MigrationId DESC;
```

---

## 📝 Lưu ý quan trọng

1. **Connection String:** 
   - Development: Sử dụng `localhost` trong `appsettings.json`
   - Docker: Sử dụng `sqlserver` (tên service) trong `docker-compose.yml`

2. **Password SQL Server:**
   - Docker container: `Your_strong_password123!` (theo docker-compose.yml)
   - Local SQL Server: `123` (theo appsettings.json hiện tại)

3. **Migrations:**
   - Luôn tạo migration sau khi thay đổi Entity hoặc DbContext
   - Không chỉnh sửa migration đã được apply vào database
   - Nếu cần sửa, tạo migration mới hoặc xóa migration chưa apply

4. **Docker:**
   - Đảm bảo Docker Desktop đang chạy trước khi sử dụng docker-compose
   - Data được lưu trong Docker volume `tm_sql_data`

---

## 🆘 Troubleshooting

### Lỗi: "Login failed for user 'sa'"

- Kiểm tra password trong `appsettings.json` có đúng không
- Kiểm tra SQL Server container có đang chạy không: `docker-compose ps`
- Thử restart container: `docker-compose restart sqlserver`

### Lỗi: "dotnet-ef does not exist"

- Cài đặt tool: `dotnet tool install --global dotnet-ef`
- Đảm bảo `tm.Api.csproj` có reference đến `Microsoft.EntityFrameworkCore.Design`

### Lỗi: "Cannot connect to SQL Server"

- Kiểm tra Docker Desktop đang chạy
- Kiểm tra port 1433 có bị chiếm không
- Kiểm tra connection string trong `appsettings.json`

---

## 🔄 Workflow Development

### Quy trình phát triển thông thường

Sau khi code xong một chức năng, bạn **KHÔNG CẦN** build API qua Docker ngay lập tức. Quy trình đề xuất:

#### 1. Development (Phát triển hàng ngày)

```bash
# Chỉ cần chạy trực tiếp
cd tm.backend\tm.Api
dotnet run
```

**Hoặc** nhấn **F5** trong Visual Studio/VS Code để chạy với hot reload.

**Lưu ý:** 
- SQL Server có thể chạy qua Docker (`docker-compose up -d sqlserver`)
- API chạy trực tiếp trên máy, không cần Docker
- Nhanh hơn, dễ debug hơn

#### 2. Khi nào cần build qua Docker?

Build API qua Docker chỉ cần trong các trường hợp sau:

**✅ Test môi trường production-like:**
```bash
docker-compose up -d --build
```

**✅ Test tích hợp với các services khác:**
- Khi cần test với nhiều containers cùng lúc
- Khi test networking giữa các services

**✅ Trước khi commit/deploy:**
- Đảm bảo code chạy được trong môi trường containerized
- Test Dockerfile có đúng không

**✅ CI/CD Pipeline:**
- Tự động build và test trong pipeline

#### 3. So sánh Development vs Docker

| Aspect | Development (dotnet run) | Docker |
|--------|-------------------------|--------|
| **Tốc độ** | ⚡ Nhanh (hot reload) | 🐌 Chậm hơn (phải rebuild) |
| **Debug** | ✅ Dễ debug | ❌ Khó debug hơn |
| **Môi trường** | Local machine | Containerized |
| **Khi nào dùng** | Hàng ngày | Test production, deploy |

### Quy trình đề xuất

```
1. Code chức năng mới
   ↓
2. Chạy trực tiếp: dotnet run (hoặc F5)
   ↓
3. Test chức năng với Swagger
   ↓
4. Fix bugs nếu có (lặp lại bước 2-3)
   ↓
5. Commit code
   ↓
6. (Tùy chọn) Build Docker để test production-like
   ↓
7. Push code
```

### Lưu ý quan trọng

- **SQL Server:** Có thể chạy qua Docker ngay cả khi API chạy trực tiếp
- **Hot Reload:** `dotnet run` hỗ trợ hot reload, code thay đổi sẽ tự động reload
- **Docker chỉ cho SQL Server:** Trong development, chỉ cần Docker cho SQL Server là đủ
- **API Docker:** Chỉ build khi cần test production environment

---

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Docker Desktop đang chạy
2. SQL Server container đang chạy (`docker-compose ps`)
3. Connection string đúng trong `appsettings.json`
4. dotnet-ef tool đã được cài đặt

