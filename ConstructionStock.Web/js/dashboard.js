document.addEventListener('DOMContentLoaded', async () => {
    updateUserInfo();
    await loadDashboardData();
    // Auto-refresh every 60 seconds
    setInterval(loadDashboardData, 60000);
});

function updateUserInfo() {
    const user = getCurrentUser();
    document.getElementById('siteNameHeader').textContent = user.siteName + ' Dashboard';
    document.getElementById('userNameHeader').textContent = 'Welcome back, ' + user.fullName;
    document.getElementById('siteLocation').textContent = user.siteName;
    document.getElementById('currentDate').textContent = new Date().toLocaleDateString('en-GB', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

async function loadDashboardData() {
    try {
        // Load Stock Levels
        const stockResponse = await apiFetch('/reports/stock-summary');
        if (stockResponse.success) {
            renderStockTable(stockResponse.data);
            updateStats(stockResponse.data);
        }

        // Load Alerts for the badge
        const alertsResponse = await apiFetch('/alerts');
        if (alertsResponse.success) {
            const count = alertsResponse.data.length;
            const badge = document.getElementById('alertCount');
            const statsActiveAlerts = document.getElementById('activeAlerts');
            
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = 'inline-flex';
                statsActiveAlerts.textContent = count;
            } else {
                badge.style.display = 'none';
                statsActiveAlerts.textContent = '0';
            }
        }
    } catch (error) {
        console.error('Failed to load dashboard data:', error);
    }
}

function renderStockTable(items) {
    const tbody = document.getElementById('stockTableBody');
    tbody.innerHTML = '';

    items.forEach(item => {
        const tr = document.createElement('tr');
        const statusClass = item.stockStatus === 'OK' ? 'badge-success' : 'badge-danger';
        
        tr.innerHTML = `
            <td>${item.itemName}</td>
            <td>${item.unit}</td>
            <td>${item.currentQuantity}</td>
            <td>${item.minimumQuantity}</td>
            <td><span class="badge ${statusClass}">${item.stockStatus}</span></td>
        `;
        tbody.appendChild(tr);
    });
}

function updateStats(items) {
    document.getElementById('totalItems').textContent = items.length;
    const lowStock = items.filter(i => i.stockStatus === 'LOW').length;
    document.getElementById('lowStockCount').textContent = lowStock;
}
