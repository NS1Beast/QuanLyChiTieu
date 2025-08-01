// Thêm các using cần thiết ở đầu file
using Microsoft.EntityFrameworkCore;
using QuanLyChiTieu.Data;
using QuanLyChiTieu.Services; // Thay QuanLyChiTieu bằng tên dự án của bạn

var builder = WebApplication.CreateBuilder(args);

// --- 1. CẤU HÌNH CÁC DỊCH VỤ (SERVICE REGISTRATION) ---

// Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext với chuỗi kết nối
builder.Services.AddDbContext<DataBase_DoAnContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký các dịch vụ MVC
builder.Services.AddControllersWithViews();
//Đăng ký dịch vụ nhắc nhở email
builder.Services.AddHostedService<EmailReminderService>();
// Đăng ký dịch vụ tùy chỉnh (ví dụ: EmailService)
builder.Services.AddTransient<IEmailService, SmtpEmailService>();

// Cấu hình dịch vụ Authentication (Xác thực) bằng Cookie
builder.Services.AddAuthentication("MyCookieAuth").AddCookie("MyCookieAuth", options =>
{
    options.Cookie.Name = "MyCookieAuth";
    options.LoginPath = "/Account/Login"; // Trang sẽ chuyển đến nếu chưa đăng nhập
    options.AccessDeniedPath = "/Account/AccessDenied"; // Trang hiển thị khi không có quyền
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Thời gian cookie hết hạn
});

// --- 2. XÂY DỰNG ỨNG DỤNG ---
// Tất cả các dịch vụ phải được đăng ký TRƯỚC dòng này
var app = builder.Build();


// --- 3. CẤU HÌNH HTTP REQUEST PIPELINE (MIDDLEWARE) ---
// Thứ tự của các middleware rất quan trọng

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Bật tính năng định tuyến (routing)
app.UseRouting();

// Bật tính năng xác thực (Authentication). Phải đứng trước Authorization.
app.UseAuthentication();

// Bật tính năng phân quyền (Authorization)
app.UseAuthorization();


// Định nghĩa các route cho controller
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// --- 4. CHẠY ỨNG DỤNG ---
app.Run();