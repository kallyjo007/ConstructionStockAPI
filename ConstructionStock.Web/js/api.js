// Use same origin API when frontend is served by the backend app.
// Falls back to absolute dev URL if needed via environment in future.
let API_BASE_URL = window.location.origin + '/api';

// If running on Live Server (port 5500), point to the dotnet API port (5287)
if (window.location.port === '5500') {
    API_BASE_URL = 'http://localhost:5287/api';
}

async function apiFetch(endpoint, method = 'GET', body = null) {
    const token = localStorage.getItem('token');
    
    const headers = {
        'Content-Type': 'application/json'
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const options = {
        method,
        headers
    };

    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const url = `${API_BASE_URL}${endpoint}`;
        console.debug('API Request:', method, url, options);
        const response = await fetch(url, options);
        
        if (response.status === 401) {
            // Unauthorized - clear storage and redirect to login
            localStorage.clear();
            window.location.href = 'index.html';
            return;
        }

        const result = await response.json();
        
        if (!response.ok) {
            throw new Error(result.message || 'Something went wrong');
        }

        return result;
    } catch (error) {
        // More explicit diagnostics for network/CORS/dev-certificate issues
        if (typeof navigator !== 'undefined' && !navigator.onLine) {
            console.error('API Error: offline', error);
            throw new Error('No network connection');
        }

        console.error('API Error:', {
            name: error.name,
            message: error.message,
            stack: error.stack
        });

        // TypeError from fetch often means network, CORS, or mixed-content issues in browsers
        if (error instanceof TypeError) {
            throw new Error('Network error or request blocked (CORS / certificate / mixed-content). See browser console Network tab for details.');
        }

        throw error;
    }
}
