// site.js - Data Arena Fusion Global Scripts

const fileAcceptMap = {
    csv: '.csv',
    json: '.json',
    xml: '.xml',
    txt: '.txt,.csv'
};

function triggerFile(type) {
    console.log(`[ACTION] Triggering file upload for type: ${type}`);
    const uploader = document.getElementById('fileUploader');
    if (!uploader) return;
    uploader.accept = fileAcceptMap[type] || '*';
    uploader.value = '';
    uploader.click();
}

window.triggerFile = triggerFile;

document.addEventListener('DOMContentLoaded', () => {
    console.log('[INIT] Data Arena Fusion scripts loaded.');

    let _globalColumns = [];
    let _globalData = [];
    let _chartInstance = null;
    let _currentPage = 1;
    let _pageSize = 100;
    let _totalPages = 0;

    window.loadDataGrid = async function (page = 1) {
        _currentPage = page;
        const pageSizeControl = document.getElementById('cmbPageSize');
        const selectedSize = parseInt(pageSizeControl?.value || `${_pageSize}`, 10);
        _pageSize = Number.isFinite(selectedSize) ? selectedSize : 100;

        const res = await fetch(`/api/DataApi/GetData?page=${_currentPage}&pageSize=${_pageSize}`);
        if (!res.ok) return;

        const result = await res.json();
        _globalColumns = result.columns || [];
        _globalData = result.data || [];
        _totalPages = result.totalPages || 0;

        renderTable(_globalColumns, _globalData, result);
        poblarSelectorGrafica(_globalColumns, _globalData);

        if (_globalData.length > 0) {
            generarGrafica();
        } else {
            if (_chartInstance) {
                _chartInstance.destroy();
                _chartInstance = null;
            }

            const ph = document.getElementById('chartPlaceholder');
            if (ph) ph.style.display = '';

            const cv = document.getElementById('chartPrincipal');
            if (cv) cv.style.display = 'none';
        }

        actualizarPaginacion(result);
    };

    function renderTable(columns, data, meta = {}) {
        console.log('[DEBUG] Renderizando tabla con', columns.length, 'columnas y', data.length, 'filas.');

        const reg = document.getElementById('lblRegistros');
        if (reg) reg.innerText = meta.totalCount ?? data.length;

        const trHead = document.getElementById('dgvHeaders');
        if (!trHead) return;
        trHead.innerHTML = '';

        const cmb = document.getElementById('cmbFiltroColumna');
        if (cmb) cmb.innerHTML = '<option>Todas las columnas</option>';

        columns.forEach(col => {
            const th = document.createElement('th');
            th.innerText = col;
            th.style.whiteSpace = 'nowrap';
            th.style.minWidth = '120px';
            trHead.appendChild(th);

            if (cmb) {
                const opt = document.createElement('option');
                opt.value = col;
                opt.innerText = col;
                cmb.appendChild(opt);
            }
        });

        const tbody = document.getElementById('dgvBody');
        if (!tbody) return;
        tbody.innerHTML = '';

        if (data.length === 0) {
            tbody.innerHTML = '<tr><td class="text-center p-5 text-muted" colspan="100%">No hay datos cargados.</td></tr>';
            return;
        }

        const fragment = document.createDocumentFragment();
        for (const row of data) {
            const tr = document.createElement('tr');

            columns.forEach(col => {
                const td = document.createElement('td');
                const val = row[col] !== undefined && row[col] !== null ? row[col] : '';

                if (String(col).endsWith('(MXN)')) {
                    td.style.textAlign = 'right';
                    td.style.fontWeight = 'bold';
                    td.style.color = '#059669';
                }

                td.innerText = val;
                tr.appendChild(td);
            });

            fragment.appendChild(tr);
        }

        tbody.appendChild(fragment);
    }

    function actualizarPaginacion(meta = {}) {
        const totalPages = meta.totalPages || 0;
        const currentPage = meta.page || 1;
        const totalCount = meta.totalCount || 0;

        const info = document.getElementById('tblPageInfo');
        if (info) {
            info.innerText = totalCount > 0
                ? `Pagina ${currentPage} de ${totalPages} | ${totalCount} registros`
                : 'Sin datos';
        }

        const prevBtn = document.getElementById('btnPrevPage');
        const nextBtn = document.getElementById('btnNextPage');
        if (prevBtn) prevBtn.disabled = currentPage <= 1;
        if (nextBtn) nextBtn.disabled = currentPage >= totalPages || totalPages === 0;
    }

    function poblarSelectorGrafica(columns, data) {
        const cmbX = document.getElementById('cmbEjeX');
        const cmbY = document.getElementById('cmbEjeY');
        if (!cmbX || !cmbY) return;

        cmbX.innerHTML = '';
        cmbY.innerHTML = '';

        const numericas = columns.filter(col => {
            const val = data.find(r => r[col] !== '' && r[col] !== null)?.[col];
            return val !== undefined && !isNaN(parseFloat(String(val).replace(/[^0-9.-]/g, '')));
        });
        const texto = columns.filter(c => !numericas.includes(c));

        [...texto, ...numericas].forEach(col => {
            const o = document.createElement('option');
            o.value = col;
            o.innerText = col;
            cmbX.appendChild(o);
        });

        [...numericas, ...texto].forEach(col => {
            const o = document.createElement('option');
            o.value = col;
            o.innerText = col;
            cmbY.appendChild(o);
        });
    }

    function generarGrafica() {
        const ejeX = document.getElementById('cmbEjeX')?.value;
        const ejeY = document.getElementById('cmbEjeY')?.value;
        const tipo = document.getElementById('cmbTipoGrafica')?.value || 'bar';
        const placeholder = document.getElementById('chartPlaceholder');

        if (!_globalData.length || !ejeX || !ejeY) return;

        const isNumericY = _globalData.some(row => {
            const val = row[ejeY];
            return val !== undefined && val !== '' && val !== null && !isNaN(parseFloat(String(val).replace(/[^0-9.-]/g, '')));
        });

        const grupos = {};
        _globalData.forEach(row => {
            const labelKey = row[ejeX] ?? '(vacio)';

            if (isNumericY) {
                const raw = String(row[ejeY] ?? '0').replace(/[^0-9.-]/g, '');
                const num = parseFloat(raw) || 0;
                grupos[labelKey] = (grupos[labelKey] || 0) + num;
            } else {
                grupos[labelKey] = (grupos[labelKey] || 0) + 1;
            }
        });

        const entries = Object.entries(grupos)
            .sort((a, b) => b[1] - a[1])
            .slice(0, 20);

        const labels = entries.map(e => e[0]);
        const values = entries.map(e => parseFloat(e[1].toFixed(2)));
        const palette = labels.map((_, i) => tipo === 'bar' ? 'rgba(54, 162, 235, 0.8)' : `hsl(${(i * 37) % 360}, 70%, 55%)`);
        const borders = labels.map((_, i) => tipo === 'bar' ? 'rgb(54, 162, 235)' : `hsl(${(i * 37) % 360}, 70%, 40%)`);

        if (_chartInstance) {
            _chartInstance.destroy();
            _chartInstance = null;
        }

        const canvas = document.getElementById('chartPrincipal');
        if (!canvas) return;

        if (placeholder) placeholder.style.display = 'none';
        canvas.style.display = 'block';

        _chartInstance = new Chart(canvas, {
            type: tipo,
            data: {
                labels,
                datasets: [{
                    label: 'Datos',
                    data: values,
                    backgroundColor: palette,
                    borderColor: borders,
                    borderWidth: 1,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                        labels: { boxWidth: 20, font: { size: 12, weight: 'bold' } }
                    },
                    datalabels: {
                        anchor: 'end',
                        align: 'top',
                        formatter: (value) => value.toLocaleString(),
                        font: { weight: 'bold', size: 11 },
                        color: '#475569'
                    },
                    title: {
                        display: true,
                        text: `${tipo.charAt(0).toUpperCase() + tipo.slice(1)}: ${ejeX} vs ${ejeY}`,
                        font: { size: 15, weight: 'bold' },
                        color: '#111827'
                    }
                },
                scales: (tipo === 'bar' || tipo === 'line') ? {
                    x: { ticks: { maxRotation: 35, font: { size: 11 } } },
                    y: { beginAtZero: true }
                } : {}
            }
        });
    }

    async function accionAPI(accion) {
        const dgvBody = document.getElementById('dgvBody');
        if (dgvBody) dgvBody.style.opacity = '0.5';

        try {
            if (accion === 'Ordenar') {
                await fetch('/api/DataApi/Ordenar', { method: 'POST' });
            } else if (accion === 'Agrupar') {
                const col = document.getElementById('cmbFiltroColumna').value;
                if (col === 'Todas las columnas') {
                    alert('Selecciona una columna especifica para agrupar.');
                    return;
                }
                const res = await fetch('/api/DataApi/Agrupar?columna=' + encodeURIComponent(col), { method: 'POST' });
                if (res.ok) {
                    const agrupados = await res.json();
                    mostrarModalAgrupar(col, agrupados);
                    return;
                }
            } else if (accion === 'Duplicados') {
                const col = document.getElementById('cmbFiltroColumna').value;
                if (col === 'Todas las columnas') {
                    alert('Selecciona una columna especifica para buscar duplicados.');
                    return;
                }
                const res = await fetch('/api/DataApi/Duplicados?columna=' + encodeURIComponent(col), { method: 'POST' });
                if (res.ok) {
                    const duplicados = await res.json();
                    const rows = document.querySelectorAll('#dgvBody tr');
                    const colIdx = _globalColumns.indexOf(col);
                    rows.forEach(tr => {
                        const cellVal = tr.cells[colIdx]?.innerText ?? '';
                        tr.style.backgroundColor = duplicados.includes(cellVal) ? '#fee2e2' : '';
                    });
                    alert(`Se encontraron ${duplicados.length} valores duplicados en "${col}". Estan resaltados en rojo.`);
                    return;
                }
            }
            await window.loadDataGrid(1);
        } catch (error) {
            alert('Ocurrio un error de red: ' + error.message);
        } finally {
            if (dgvBody) dgvBody.style.opacity = '1';
        }
    }

    function mostrarModalAgrupar(columna, datos) {
        document.getElementById('modalAgruparTitle').innerText = `Resumen por ${columna}`;
        document.getElementById('thAgruparClave').innerText = columna;

        const tbody = document.getElementById('tbodyAgrupar');
        tbody.innerHTML = '';

        datos.forEach(item => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${item.clave ?? item.key ?? item.Key ?? ''}</td><td class="text-end">${item.valor ?? item.value ?? item.Value ?? ''}</td>`;
            tbody.appendChild(tr);
        });

        const modal = new bootstrap.Modal(document.getElementById('modalAgrupar'));
        modal.show();
    }

    document.getElementById('btnOrdenar')?.addEventListener('click', () => accionAPI('Ordenar'));
    document.getElementById('btnAgrupar')?.addEventListener('click', () => accionAPI('Agrupar'));
    document.getElementById('btnDuplicados')?.addEventListener('click', () => accionAPI('Duplicados'));
    document.getElementById('cmbPageSize')?.addEventListener('change', () => window.loadDataGrid(1));
    document.getElementById('btnPrevPage')?.addEventListener('click', () => {
        if (_currentPage > 1) window.loadDataGrid(_currentPage - 1);
    });
    document.getElementById('btnNextPage')?.addEventListener('click', () => {
        if (_totalPages > _currentPage) window.loadDataGrid(_currentPage + 1);
    });

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
                tbody.innerHTML = '<tr><td class="text-center p-4 text-primary" colspan="100%"><strong>Cargando datos...</strong></td></tr>';
            }

            try {
                const res = await fetch('/api/DataApi/Upload', {
                    method: 'POST',
                    body: fd
                });

                if (res.ok) {
                    console.log('[UPLOAD] Success. Refreshing grid...');
                    if (typeof window.loadDataGrid === 'function') {
                        await window.loadDataGrid(1);
                    } else {
                        window.location.reload();
                    }
                } else {
                    const err = await res.json().catch(() => ({ message: 'Error desconocido' }));
                    alert('Error al cargar el archivo: ' + err.message);
                }
            } catch (ex) {
                console.error('[UPLOAD ERROR]', ex);
                alert('ERROR FATAL: El servidor no responde.');
            }
        });
    }

    document.getElementById('btnLimpiar')?.addEventListener('click', async () => {
        console.log('[ACTION] Clear Data button pressed.');

        try {
            const res = await fetch('/api/DataApi/Limpiar', { method: 'POST' });
            if (res.ok) {
                console.log('[ACTION] Server data cleared successfully.');
                if (typeof window.loadDataGrid === 'function') {
                    await window.loadDataGrid(1);
                } else {
                    window.location.reload();
                }
                alert('Datos limpiados correctamente.');
            }
        } catch (e) {
            console.error('[ERROR] Clear operation failed', e);
        }
    });

    document.getElementById('btnApi')?.addEventListener('click', async () => {
        console.log('[ACTION] API Enrichment requested.');
        const btn = document.getElementById('btnApi');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> PROCESANDO...';
        btn.disabled = true;

        try {
            const res = await fetch('/api/DataApi/EnriquecerApi', { method: 'POST' });
            const result = await res.json().catch(() => ({}));

            if (res.ok) {
                if (typeof window.loadDataGrid === 'function') {
                    await window.loadDataGrid(1);
                }

                const columnas = Array.isArray(result.addedColumns) ? result.addedColumns : [];
                const warnings = Array.isArray(result.warnings) ? result.warnings : [];
                const extraColumnas = columnas.length > 0 ? `\nColumnas agregadas: ${columnas.join(', ')}` : '';
                const extraWarnings = warnings.length > 0 ? `\nAvisos: ${warnings.join(' | ')}` : '';

                alert((result.message || 'Datos enriquecidos con exito desde la API.') + extraColumnas + extraWarnings);
            } else {
                alert('No se pudo enriquecer: ' + (result.message || 'Error al procesar'));
            }
        } catch (e) {
            console.error('[ERROR] API enrichment failed', e);
            alert('No se pudo conectar con la API de enriquecimiento: ' + e.message);
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    });

    const btnMaria = document.getElementById('btnMariaDB');
    if (btnMaria) {
        btnMaria.addEventListener('click', () => abrirModalConexion('MariaDb'));
    }

    const btnPostgre = document.getElementById('btnPostgre');
    if (btnPostgre) {
        btnPostgre.addEventListener('click', () => abrirModalConexion('PostgreSql'));
    }

    document.getElementById('btnTestConn')?.addEventListener('click', async () => {
        await realizarAccionConexion('TestConnection');
    });

    document.getElementById('btnAcceptConn')?.addEventListener('click', async () => {
        await realizarAccionConexion('Connect');
    });

    window.loadDataGrid(1);
});

function abrirModalConexion(provider) {
    console.log(`[DB] Opening connection modal for: ${provider}`);
    const modal = new bootstrap.Modal(document.getElementById('modalDbConnection'));

    document.getElementById('dbProvider').value = provider;
    document.getElementById('modalDbTitle').innerText = `Conexion ${provider}`;
    document.getElementById('modalDbSubtitle').innerText = `Configura la conexion para ${provider}`;
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
    status.innerText = endpoint === 'TestConnection' ? 'Probando conexion...' : 'Conectando...';

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
            status.innerText = result.message || 'Exito';

            if (endpoint === 'Connect') {
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalDbConnection'));
                setTimeout(() => {
                    modal.hide();
                    if (typeof window.loadDataGrid === 'function') window.loadDataGrid(1);
                }, 1000);
            }
        } else {
            status.style.backgroundColor = '#fee2e2';
            status.style.color = '#991b1b';
            status.innerText = 'Error: ' + (result.message || 'Error desconocido');
        }
    } catch (e) {
        status.style.backgroundColor = '#fee2e2';
        status.style.color = '#991b1b';
        status.innerText = 'Error de red: ' + e.message;
    }
}
