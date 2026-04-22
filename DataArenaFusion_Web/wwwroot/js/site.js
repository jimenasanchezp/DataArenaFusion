// site.js - Data Arena Fusion Global Scripts

// Mapa de tipos a extensiones de filtro
const fileAcceptMap = {
    'csv':  '.csv',
    'json': '.json',
    'xml':  '.xml',
    'txt':  '.txt,.csv'
};

function triggerFile(type) {
    console.log(`[ACTION] Triggering file upload for type: ${type}`);
    const uploader = document.getElementById('fileUploader');
    if (!uploader) return;
    uploader.accept = fileAcceptMap[type] || '*';
    uploader.value = '';
    uploader.click();
}

// Hacer triggerFile global
window.triggerFile = triggerFile;

document.addEventListener('DOMContentLoaded', () => {
    console.log("[INIT] Data Arena Fusion scripts loaded.");

    const uploader = document.getElementById('fileUploader');
    if (uploader) {
        uploader.addEventListener('change', async (e) => {
            const file = e.target.files[0];
            if (!file) return;

            console.log(`[UPLOAD] Selected file: ${file.name}`);
            const fd = new FormData();
            fd.append('file', file);

            const tbody = document.getElementById('dgvBody');
            if (tbody) {
                tbody.innerHTML = '<tr><td class="text-center p-4 text-primary" colspan="100%"><strong>⏳ Cargando datos...</strong></td></tr>';
            }
             
            try {
                const res = await fetch('/api/DataApi/Upload', {
                    method: 'POST',
                    body: fd
                });

                if (res.ok) {
                    console.log("[UPLOAD] Success. Refreshing grid...");
                    if (typeof window.loadDataGrid === 'function') {
                        await window.loadDataGrid();
                    } else {
                        window.location.reload();
                    }
                } else {
                    const err = await res.json().catch(() => ({ message: 'Error desconocido' }));
                    alert('Error al cargar el archivo: ' + err.message);
                }
            } catch (ex) {
                console.error("[UPLOAD ERROR]", ex);
                alert('ERROR FATAL: El servidor no responde.');
            }
        });
    }

    // Sidebar Listeners
    document.getElementById('btnLimpiar')?.addEventListener('click', async () => {
        console.log("[ACTION] Clear Data button pressed.");
        
        try {
            const res = await fetch('/api/DataApi/Limpiar', { method: 'POST' });
            if (res.ok) {
                console.log("[ACTION] Server data cleared successfully.");
                if (typeof window.loadDataGrid === 'function') {
                    console.log("[ACTION] Refreshing Data Grid...");
                    await window.loadDataGrid();
                } else {
                    console.log("[ACTION] Reloading page...");
                    window.location.reload();
                }
                alert("Datos limpiados correctamente.");
            } else {
                console.error("[ACTION] Server failed to clear data.");
            }
        } catch (e) {
            console.error("[ERROR] Clear operation failed", e);
        }
    });

    document.getElementById('btnApi')?.addEventListener('click', async () => {
        console.log("[ACTION] API Enrichment requested.");
        const btn = document.getElementById('btnApi');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> PROCESANDO...';
        btn.disabled = true;

        try {
            const res = await fetch('/api/DataApi/EnriquecerApi', { method: 'POST' });
            if (res.ok) {
                if (typeof window.loadDataGrid === 'function') await window.loadDataGrid();
                alert("¡Datos enriquecidos con éxito desde las APIs de internet!");
            } else {
                const err = await res.json().catch(() => ({ message: 'Error al procesar' }));
                alert("No se pudo enriquecer: " + err.message);
            }
        } catch (e) {
            console.error("[ERROR] API enrichment failed", e);
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    });

    // DB buttons placeholder alerts if not already using inline onclick
    const btnMaria = document.getElementById('btnMariaDB');
    if (btnMaria) {
        btnMaria.addEventListener('click', () => abrirModalConexion('MariaDb'));
    }
    const btnPostgre = document.getElementById('btnPostgre');
    if (btnPostgre) {
        btnPostgre.addEventListener('click', () => abrirModalConexion('PostgreSql'));
    }

    // Modal Handlers
    document.getElementById('btnTestConn')?.addEventListener('click', async () => {
        await realizarAccionConexion('TestConnection');
    });

    document.getElementById('btnAcceptConn')?.addEventListener('click', async () => {
        await realizarAccionConexion('Connect');
    });
});

function abrirModalConexion(provider) {
    console.log(`[DB] Opening connection modal for: ${provider}`);
    const modal = new bootstrap.Modal(document.getElementById('modalDbConnection'));
    
    document.getElementById('dbProvider').value = provider;
    document.getElementById('modalDbTitle').innerText = `Conexión ${provider}`;
    document.getElementById('modalDbSubtitle').innerText = `Configura la conexión para ${provider}`;
    document.getElementById('dbPort').value = (provider === 'MariaDb' ? 3306 : 5432);
    document.getElementById('dbHost').value = 'localhost';
    
    const status = document.getElementById('dbConnStatus');
    status.style.display = 'none';
    status.innerText = '';

    modal.show();
}

async function realizarAccionConexion(endpoint) {
    const provider = document.getElementById('dbProvider').value;
    const settings = {
        Provider: provider,
        Host: document.getElementById('dbHost').value,
        Port: parseInt(document.getElementById('dbPort').value),
        Database: document.getElementById('dbName').value,
        Username: document.getElementById('dbUser').value,
        Password: document.getElementById('dbPass').value,
        UseSsl: document.getElementById('dbSsl').checked
    };

    const status = document.getElementById('dbConnStatus');
    status.style.display = 'block';
    status.style.backgroundColor = '#f1f5f9';
    status.style.color = '#475569';
    status.innerText = endpoint === 'TestConnection' ? '⏳ Probando conexión...' : '⏳ Conectando...';

    try {
        const res = await fetch(`/api/DataApi/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(settings)
        });

        const result = await res.json();

        if (res.ok) {
            status.style.backgroundColor = '#dcfce7';
            status.style.color = '#166534';
            status.innerText = result.message || '¡Éxito!';
            
            if (endpoint === 'Connect') {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalDbConnection'));
                setTimeout(() => {
                    modal.hide();
                    if (typeof window.loadDataGrid === 'function') window.loadDataGrid();
                }, 1000);
            }
        } else {
            status.style.backgroundColor = '#fee2e2';
            status.style.color = '#991b1b';
            status.innerText = '❌ Error: ' + (result.message || 'Error desconocido');
        }
    } catch (e) {
        status.style.backgroundColor = '#fee2e2';
        status.style.color = '#991b1b';
        status.innerText = '❌ Error de red: ' + e.message;
    }
}
