document.addEventListener('DOMContentLoaded', async () => {
    updateUserInfo();
    await initializeForm();
    await loadRecentTransactions();

    // Toggle Supplier field based on Transaction Type
    document.querySelectorAll('input[name="transactionType"]').forEach(radio => {
        radio.addEventListener('change', (e) => {
            const supplierGroup = document.getElementById('supplierGroup');
            supplierGroup.style.display = e.target.value === 'IN' ? 'block' : 'none';
            const supplierSelect = document.getElementById('supplierId');
            if (e.target.value === 'OUT') {
                supplierSelect.value = '';
                supplierSelect.removeAttribute('required');
            } else {
                supplierSelect.setAttribute('required', 'required');
            }
        });
    });

    // Handle Form Submission
    document.getElementById('recordForm').addEventListener('submit', handleRecordSubmit);
});

function updateUserInfo() {
    const user = getCurrentUser();
    document.getElementById('siteNameHeader').textContent = user.siteName;
    document.getElementById('userNameHeader').textContent = 'Welcome back, ' + user.fullName;
}

async function initializeForm() {
    try {
        // Load items for dropdown
        const itemsResponse = await apiFetch('/transactions/items');
        if (itemsResponse.success) {
            const itemSelect = document.getElementById('itemId');
            itemsResponse.data.forEach(item => {
                const opt = document.createElement('option');
                opt.value = item.itemId;
                opt.textContent = `${item.itemName} (${item.unit})`;
                itemSelect.appendChild(opt);
            });
        }

        // Load suppliers for dropdown
        const suppliersResponse = await apiFetch('/transactions/suppliers');
        if (suppliersResponse.success) {
            const supplierSelect = document.getElementById('supplierId');
            suppliersResponse.data.forEach(s => {
                const opt = document.createElement('option');
                opt.value = s.supplierId;
                opt.textContent = s.supplierName;
                supplierSelect.appendChild(opt);
            });
        }
    } catch (error) {
        console.error('Initialization failed:', error);
    }
}

async function handleRecordSubmit(e) {
    e.preventDefault();
    const msgDiv = document.getElementById('formMessage');
    msgDiv.style.display = 'none';

    const data = {
        itemId: parseInt(document.getElementById('itemId').value),
        quantity: parseInt(document.getElementById('quantity').value),
        transactionType: document.querySelector('input[name="transactionType"]:checked').value,
        supplierId: document.getElementById('supplierId').value ? parseInt(document.getElementById('supplierId').value) : null,
        remarks: document.getElementById('remarks').value
    };

    try {
        const response = await apiFetch('/transactions/record', 'POST', data);
        if (response.success) {
            msgDiv.textContent = 'Record saved successfully!';
            msgDiv.className = 'badge-success';
            msgDiv.style.display = 'block';
            msgDiv.style.padding = '10px';
            msgDiv.style.textAlign = 'center';
            
            document.getElementById('recordForm').reset();
            // Default radio back to IN
            document.querySelector('input[name="transactionType"][value="IN"]').checked = true;
            document.getElementById('supplierGroup').style.display = 'block';
            
            await loadRecentTransactions();
        }
    } catch (error) {
        msgDiv.textContent = error.message;
        msgDiv.className = 'badge-danger';
        msgDiv.style.display = 'block';
        msgDiv.style.padding = '10px';
        msgDiv.style.textAlign = 'center';
    }
}

async function loadRecentTransactions() {
    try {
        const response = await apiFetch('/transactions/log');
        if (response.success) {
            const tbody = document.getElementById('recentTableBody');
            tbody.innerHTML = '';
            
            // Show only first 5
            const recent = response.data.slice(0, 5);
            recent.forEach(tx => {
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
                    <td>${tx.remarks || ''}</td>
                `;
                tbody.appendChild(tr);
            });
        }
    } catch (error) {
        console.error('Failed to load recent transactions:', error);
    }
}
