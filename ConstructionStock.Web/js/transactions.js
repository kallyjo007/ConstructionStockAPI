document.addEventListener('DOMContentLoaded', async () => {
    await loadTransactions();
    await updateAlertBadge();
});

async function loadTransactions() {
    try {
        const response = await apiFetch('/transactions/log');
        if (response.success) {
            renderTransactionTable(response.data);
        }
    } catch (error) {
        console.error('Failed to load transactions:', error);
    }
}

function renderTransactionTable(transactions) {
    const tbody = document.getElementById('transactionTableBody');
    tbody.innerHTML = '';

    transactions.forEach(tx => {
        const tr = document.createElement('tr');
        const typeClass = tx.transactionType === 'IN' ? 'badge-success' : 'badge-warning';
        const date = new Date(tx.transactionDate).toLocaleString('en-GB', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        tr.innerHTML = `
            <td>${date}</td>
            <td><span class="badge ${typeClass}">${tx.transactionType}</span></td>
            <td>${tx.itemName}</td>
            <td>${tx.quantity}</td>
            <td>${tx.recordedBy}</td>
            <td>${tx.supplierName || '-'}</td>
            <td>${tx.remarks || ''}</td>
            <td>
                ${tx.transactionType === 'IN' && !tx.isApproved ? `<button class="btn btn-sm btn-primary" onclick="approveTransaction(${tx.transactionId})">Approve</button>` : (tx.isApproved ? `Approved${tx.approvedBy ? ' by ' + tx.approvedBy : ''}` : '')}
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function approveTransaction(id) {
    if (!confirm('Are you sure you want to approve this stock IN transaction?')) return;
    
    try {
        const response = await apiFetch(`/transactions/${id}/approve`, 'PUT');
        if (response.success) {
            await loadTransactions();
            updateAlertBadge();
        }
    } catch (error) {
        alert(error.message);
    }
}

async function updateAlertBadge() {
    try {
        const response = await apiFetch('/alerts');
        if (response.success && response.data.length > 0) {
            const badge = document.getElementById('alertCount');
            if (badge) {
                badge.textContent = response.data.length;
                badge.style.display = 'inline-flex';
            }
        }
    } catch (error) {}
}
