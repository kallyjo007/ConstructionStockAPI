document.addEventListener('DOMContentLoaded', async () => {
    await loadItems();
    updateAlertBadge();
});

async function loadItems() {
    try {
        const response = await apiFetch('/items');
        if (response.success) {
            renderItems(response.data);
        }
    } catch (error) {
        console.error('Failed to load items:', error);
    }
}

function renderItems(items) {
    const tbody = document.getElementById('itemsTableBody');
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
            <td>
                <button onclick="editItem(${JSON.stringify(item).replace(/"/g, '&quot;')})" class="btn btn-secondary" style="padding: 0.25rem 0.5rem; font-size: 0.75rem;">Edit</button>
                <button onclick="deleteItem(${item.itemId})" class="btn btn-danger" style="padding: 0.25rem 0.5rem; font-size: 0.75rem;">Delete</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

function openModal(item = null) {
    const modal = document.getElementById('itemModal');
    const form = document.getElementById('itemForm');
    const qtyGroup = document.getElementById('qtyGroup');
    
    form.reset();
    modal.style.display = 'flex';

    if (item) {
        document.getElementById('modalTitle').textContent = 'Edit Item';
        document.getElementById('editItemId').value = item.itemId;
        document.getElementById('itemName').value = item.itemName;
        document.getElementById('unit').value = item.unit;
        document.getElementById('minimumQuantity').value = item.minimumQuantity;
        qtyGroup.style.display = 'none'; // Don't allow direct editing of quantity here
    } else {
        document.getElementById('modalTitle').textContent = 'Add New Item';
        document.getElementById('editItemId').value = '';
        qtyGroup.style.display = 'block';
    }
}

function closeModal() {
    document.getElementById('itemModal').style.display = 'none';
}

document.getElementById('itemForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('editItemId').value;
    
    const data = {
        itemName: document.getElementById('itemName').value,
        unit: document.getElementById('unit').value,
        minimumQuantity: parseInt(document.getElementById('minimumQuantity').value)
    };

    try {
        let response;
        if (id) {
            response = await apiFetch(`/items/${id}`, 'PUT', data);
        } else {
            data.currentQuantity = parseInt(document.getElementById('currentQuantity').value || 0);
            response = await apiFetch('/items', 'POST', data);
        }

        if (response.success) {
            closeModal();
            await loadItems();
        }
    } catch (error) {
        alert(error.message);
    }
});

async function editItem(item) {
    openModal(item);
}

async function deleteItem(id) {
    if (confirm('Are you sure you want to delete this item? This is a soft delete.')) {
        try {
            const response = await apiFetch(`/items/${id}`, 'DELETE');
            if (response.success) {
                await loadItems();
            }
        } catch (error) {
            alert(error.message);
        }
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
