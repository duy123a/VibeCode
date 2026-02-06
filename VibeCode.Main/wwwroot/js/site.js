const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const sidebarClose = document.getElementById('sidebarClose');
const sidebarOverlay = document.getElementById('sidebarOverlay');
const topNavbar = document.querySelector('.top-navbar');

function closeSidebar() {
    sidebar.classList.remove('show');
}

function openSidebar() {
    sidebar.classList.add('show');
}

if (sidebarToggle) {
    sidebarToggle.addEventListener('click', function () {
        if (window.innerWidth >= 992) {
            sidebar.classList.toggle('collapsed');
            if (topNavbar) {
                topNavbar.classList.toggle('sidebar-collapsed');
            }
        } else {
            sidebar.classList.toggle('show');
        }
    });
}

if (sidebarClose) {
    sidebarClose.addEventListener('click', closeSidebar);
}

if (sidebarOverlay) {
    sidebarOverlay.addEventListener('click', closeSidebar);
}

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeSidebar();
    }
});

const userDropdown = document.getElementById('userDropdown');
if (userDropdown) {
    userDropdown.addEventListener('show.bs.dropdown', function () {
        this.classList.add('arrow-up');
    });
    userDropdown.addEventListener('hidden.bs.dropdown', function () {
        this.classList.remove('arrow-up');
    });
}

