(function() {
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');
    const currentPage = window.location.pathname.split('/').pop();

    // If not logged in and not on index.html, redirect to login
    if (!token && currentPage !== 'index.html' && currentPage !== '') {
        window.location.href = 'index.html';
        return;
    }

    // If logged in and on index.html, redirect to appropriate dashboard
    if (token && (currentPage === 'index.html' || currentPage === '')) {
        if (role === 'StockManager') {
            window.location.href = 'dashboard.html';
        } else if (role === 'Storekeeper') {
            window.location.href = 'storekeeper.html';
        }
        return;
    }

    // Role-based protection
    const managerPages = ['dashboard.html', 'transactions.html', 'reports.html', 'suppliers.html', 'users.html'];
    const storekeeperPages = ['storekeeper.html', 'my-transactions.html'];

    if (role === 'Storekeeper' && managerPages.includes(currentPage)) {
        window.location.href = 'storekeeper.html';
    }

    if (role === 'StockManager' && storekeeperPages.includes(currentPage)) {
        window.location.href = 'dashboard.html';
    }
})();
