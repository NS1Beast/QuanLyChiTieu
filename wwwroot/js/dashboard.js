// Hàm này sẽ được thực thi ngay khi trang được tải
document.addEventListener('DOMContentLoaded', function () {

    /* ===== 1. LOGIC CHO CHẾ ĐỘ SÁNG/TỐI (THEME TOGGLER) ===== */
    const themeToggler = document.getElementById('theme-toggler');
    const body = document.body;

    if (themeToggler) {
        const themeIcon = themeToggler.querySelector('i');

        // Hàm cập nhật icon dựa trên theme
        function updateIcon(theme) {
            if (theme === 'dark-mode') {
                themeIcon.classList.remove('bi-moon-stars-fill');
                themeIcon.classList.add('bi-sun-fill');
            } else {
                themeIcon.classList.remove('bi-sun-fill');
                themeIcon.classList.add('bi-moon-stars-fill');
            }
        }

        // Áp dụng theme đã lưu khi tải trang
        const currentTheme = localStorage.getItem('theme');
        if (currentTheme) {
            body.classList.add(currentTheme);
            updateIcon(currentTheme);
        }

        // Xử lý sự kiện click vào nút chuyển theme
        themeToggler.addEventListener('click', function () {
            body.classList.toggle('dark-mode');

            let theme = body.classList.contains('dark-mode') ? 'dark-mode' : 'light-mode';

            localStorage.setItem('theme', theme);
            updateIcon(theme);
        });
    }


    /* ===== 2. LOGIC ĐÁNH DẤU LINK ĐANG ACTIVE TRÊN SIDEBAR ===== */
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.sidebar .nav-link');

    navLinks.forEach(link => {
        const linkPath = new URL(link.href).pathname;

        // Active link nếu URL hiện tại bắt đầu bằng đường dẫn của link
        // Ví dụ: URL là /ChiTieu/Edit/5, link là /ChiTieu -> vẫn active
        if (currentPath.toLowerCase().startsWith(linkPath.toLowerCase()) && linkPath !== "/") {
            link.classList.add('active');
        } else if ((currentPath === "/" || currentPath.toLowerCase() === "/home/index") && linkPath.toLowerCase() === "/dashboard/index") {
            // Trường hợp đặc biệt cho trang chủ/dashboard
            link.classList.add('active');
        }
    });


    /* ===== 3. LOGIC MỚI CHO NÚT BẬT/TẮT SIDEBAR TRÊN MOBILE ===== */
    const sidebar = document.querySelector('.sidebar');
    const sidebarToggler = document.querySelector('.sidebar-toggler');

    if (sidebar && sidebarToggler) {
        // Xử lý sự kiện click vào nút toggler
        sidebarToggler.addEventListener('click', function (event) {
            event.stopPropagation(); // Ngăn sự kiện click lan ra ngoài
            sidebar.classList.toggle('active');
        });

        // Tự động đóng sidebar khi click ra ngoài trên mobile
        document.addEventListener('click', function (event) {
            if (window.innerWidth <= 992) {
                const isClickInsideSidebar = sidebar.contains(event.target);
                const isClickOnToggler = sidebarToggler.contains(event.target);

                if (!isClickInsideSidebar && !isClickOnToggler && sidebar.classList.contains('active')) {
                    sidebar.classList.remove('active');
                }
            }
        });
    }
});