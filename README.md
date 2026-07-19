# DJ Store — ASP.NET Core 8.0 (MVC + EF Core)

Đây là bản chuyển đổi từ mã nguồn tĩnh (HTML/JS/Tailwind + localStorage) trong `dj-store.zip`
sang một ứng dụng web server-side thực sự, chạy trên **.NET 8**, có cơ sở dữ liệu thật (SQLite),
đăng nhập/phân quyền thật (Cookie Authentication) và toàn bộ nghiệp vụ được xử lý ở backend
thay vì `localStorage`.

## 1. Yêu cầu môi trường

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) trở lên
- Không cần cài Node.js / npm — không có bước build frontend nào cả.

## 2. Chạy dự án

```bash
cd DjStoreWeb
dotnet restore
dotnet run
```

Lần chạy đầu tiên, ứng dụng sẽ:
1. Tự tạo file cơ sở dữ liệu SQLite tại `App_Data/djstore.db`.
2. Tự động seed (nạp) dữ liệu mẫu từ các file gốc trong `Data/Seed/*.json`
   (chính là `data/products.json`, `categories.json`, `users.json`, `orders.json` của bản HTML gốc).
3. Mật khẩu người dùng mẫu được băm lại bằng PBKDF2 (không lưu plaintext).

Mở trình duyệt tại địa chỉ được in ra trong terminal (mặc định `http://localhost:5080`).

## 3. Tài khoản demo

| Vai trò        | Email                  | Mật khẩu     |
|----------------|-------------------------|--------------|
| Quản trị viên  | admin@djstore.com       | admin123     |
| Nhân viên      | staff@djstore.com       | staff123     |
| Khách hàng     | customer@djstore.com    | customer123  |

Trang quản trị: `/Admin/Dashboard` (tự động chuyển hướng tới sau khi admin/nhân viên đăng nhập).

## 4. Kiến trúc dự án

```
DjStoreWeb/
├─ Controllers/          Home, Account, Cart, News (site công khai)
├─ Areas/Admin/          Toàn bộ khu vực quản trị (yêu cầu đăng nhập role admin/employee)
│  ├─ Controllers/       Dashboard, Products, Categories, Orders, Customers,
│  │                     Staffs, Vouchers, Warehouse, News
│  └─ Views/
├─ Models/                Product, Category, AppUser, Order, OrderItem, Voucher,
│                         NewsPost, InventoryLog
├─ Data/
│  ├─ AppDbContext.cs     EF Core DbContext (SQLite)
│  └─ Seed/               DTO + SeedRunner đọc JSON gốc để seed DB lần đầu
├─ Services/
│  ├─ CartService.cs      Giỏ hàng lưu theo Session (thay cho localStorage)
│  └─ PasswordHasher.cs   Băm mật khẩu PBKDF2 (không cần thư viện ngoài)
└─ Views/                 Giao diện công khai (Tailwind CDN, dark mode)
```

## 5. Tính năng đã triển khai (đối chiếu với bản HTML gốc)

- **Trang chủ / Cửa hàng**: danh mục, flash sale, sản phẩm nổi bật, lọc theo danh mục,
  tìm kiếm, sắp xếp giá/đánh giá, phân trang.
- **Chi tiết sản phẩm**: thông số kỹ thuật, sản phẩm liên quan, tồn kho thực tế.
- **Giỏ hàng & thanh toán**: giỏ hàng theo Session (server-side, không phụ thuộc localStorage),
  áp mã giảm giá qua AJAX, tạo đơn hàng thật trong CSDL và **trừ tồn kho tự động**.
- **Tài khoản**: đăng ký, đăng nhập, quên mật khẩu, cập nhật hồ sơ, lịch sử đơn hàng.
- **Tin tức / cẩm nang**: danh sách + chi tiết bài viết, đếm lượt xem tự động, lọc theo chuyên mục.
- **Quản trị (Admin)**:
  - Dashboard: doanh thu, đơn hàng, sản phẩm sắp hết hàng, biểu đồ doanh thu 7 ngày (Chart.js).
  - Quản lý sản phẩm & danh mục (CRUD đầy đủ).
  - Quản lý đơn hàng: xem chi tiết, cập nhật trạng thái.
  - **Quản lý kho hàng**: nhập kho (+), xuất kho (-), thiết lập số lượng trực tiếp kèm lý do,
    lưu lịch sử điều chỉnh, và **xuất báo cáo CSV (UTF-8 BOM)** giống bản gốc.
  - Quản lý khách hàng, nhân viên (chỉ admin), mã giảm giá (voucher), tin tức.
- **Phân quyền thật**: Cookie Authentication với 3 vai trò `admin`, `employee`, `customer`
  thay vì lưu trong `localStorage` như bản gốc.

## 6. Ghi chú kỹ thuật

- Cơ sở dữ liệu dùng SQLite (`Microsoft.EntityFrameworkCore.Sqlite`) để không cần cài đặt
  SQL Server / cấu hình phức tạp — chỉ cần `dotnet run`. Có thể đổi sang SQL Server bằng cách
  sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json` và
  `UseSqlite(...)` → `UseSqlServer(...)` trong `Program.cs`.
- Ứng dụng dùng `Database.EnsureCreated()` (không dùng migrations) để đơn giản hoá triển khai lần đầu.
  Nếu bạn muốn dùng EF Core Migrations về sau, hãy xoá `EnsureCreatedAsync()` trong
  `SeedRunner.RunAsync` và chạy `dotnet ef migrations add Init`.
- Ảnh sản phẩm vẫn dùng URL Unsplash gốc (không cần tải ảnh về máy).
