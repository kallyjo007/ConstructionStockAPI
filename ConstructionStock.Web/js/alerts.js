document.addEventListener('DOMContentLoaded', async () => {
    await loadAlerts();
    // Auto-refresh every 60 seconds
    setInterval(loadAlerts, 60000);
});

async function loadAlerts() {
    try {
        const response = await apiFetch('/alerts');
        if (response.success) {
            renderAlerts(response.data);
            updateBadge(response.data.length);
        }
    } catch (error) {
        console.error('Failed to load alerts:', error);
    }
}

function renderAlerts(alerts) {
    const tbody = document.getElementById('alertsTableBody');
    const container = document.getElementById('alertsTableContainer');
    const noAlerts = document.getElementById('noAlertsMessage');
    
    tbody.innerHTML = '';

    if (alerts.length === 0) {
        container.style.display = 'none';
        noAlerts.style.display = 'block';
        return;
    }

    container.style.display = 'block';
    noAlerts.style.display = 'none';

    alerts.forEach(alert => {
        const tr = document.createElement('tr');
        const date = new Date(alert.alertDate).toLocaleString('en-GB', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        tr.innerHTML = `
            <td>${date}</td>
            <td><strong>${alert.itemName}</strong></td>
            <td>${alert.quantityAtAlert}</td>
            <td>${alert.minimumQuantity}</td>
            <td>
                <button onclick="resolveAlert(${alert.alertId})" class="btn btn-primary" style="padding: 0.25rem 0.75rem; font-size: 0.875rem;">Resolve</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function resolveAlert(id) {
    try {
        const response = await apiFetch(`/alerts/${id}/resolve`, 'PUT');
        if (response.success) {
            await loadAlerts();
        }
    } catch (error) {
        alert('Failed to resolve alert: ' + error.message);
    }
}

function updateBadge(count) {
    const badge = document.getElementById('alertCount');
    if (count > 0) {
        badge.textContent = count;
        badge.style.display = 'inline-flex';
    } else {
        badge.style.display = 'none';
    }
}
