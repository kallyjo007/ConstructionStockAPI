document.addEventListener('DOMContentLoaded', async () => {
    const dateInput = document.getElementById('reportDate');
    const today = new Date().toISOString().split('T')[0];
    dateInput.value = today;

    await loadReport(today);
    await updateAlertBadge();

    dateInput.addEventListener('change', (e) => loadReport(e.target.value));
    document.getElementById('exportPdfBtn').addEventListener('click', exportPdf);
});

async function loadReport(date) {
    try {
        const response = await apiFetch(`/reports/daily?date=${date}`);
        if (response.success) {
            renderReport(response.data);
        }
    } catch (error) {
        console.error('Failed to load report:', error);
    }
}

function renderReport(data) {
    document.getElementById('totalIn').textContent = data.totalInQuantity;
    document.getElementById('totalOut').textContent = data.totalOutQuantity;
    document.getElementById('txCount').textContent = data.transactions.length;

    const tbody = document.getElementById('reportTableBody');
    tbody.innerHTML = '';

    if (data.transactions.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" class="text-center">No transactions for this date</td></tr>';
        return;
    }

    data.transactions.forEach(tx => {
        const tr = document.createElement('tr');
        const typeClass = tx.transactionType === 'IN' ? 'badge-success' : 'badge-warning';
        const time = new Date(tx.transactionDate).toLocaleTimeString('en-GB', {
            hour: '2-digit',
            minute: '2-digit'
        });

        tr.innerHTML = `
            <td>${time}</td>
            <td><span class="badge ${typeClass}">${tx.transactionType}</span></td>
            <td>${tx.itemName}</td>
            <td>${tx.quantity}</td>
            <td>${tx.recordedBy}</td>
        `;
        tbody.appendChild(tr);
    });
}

async function exportPdf() {
    const date = document.getElementById('reportDate').value;
    const token = localStorage.getItem('token');
    
    try {
        const response = await fetch(`http://localhost:5287/api/reports/daily/export?date=${date}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error('Export failed');

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `daily-report-${date}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
    } catch (error) {
        alert('Failed to export PDF: ' + error.message);
    }
}

async function updateAlertBadge() {
    try {
        const response = await apiFetch('/alerts');
        if (response.success && response.data.length > 0) {
            const badge = document.getElementById('alertCount');
            badge.textContent = response.data.length;
            badge.style.display = 'inline-flex';
        }
    } catch (error) {}
}
