const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const sidebarClose = document.getElementById('sidebarClose');
const sidebarOverlay = document.getElementById('sidebarOverlay');

function closeSidebar() {
    if (sidebar) {
        sidebar.classList.remove('show');
    }
}

function openSidebar() {
    if (sidebar) {
        sidebar.classList.add('show');
    }
}

if (sidebarToggle) {
    sidebarToggle.addEventListener('click', function () {
        if (sidebar) {
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

