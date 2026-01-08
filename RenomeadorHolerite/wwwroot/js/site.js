// Variáveis Globais
let globalResults = [];
let logsModalInstance;
let changelogModalInstance;

document.addEventListener('DOMContentLoaded', function () {
    // 1. Aplicar Tema Salvo
    const savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    // 2. Renderizar Changelog (Se o arquivo changelog.js carregou corretamente)
    if (typeof renderChangelog === 'function') {
        renderChangelog();
    } else {
        console.error("Erro: changelog.js não foi carregado corretamente.");
    }

    // 3. Inicializar Modais
    const logsEl = document.getElementById('logsModal');
    if (logsEl) logsModalInstance = new bootstrap.Modal(logsEl);

    const changeEl = document.getElementById('changelogModal');
    if (changeEl) changelogModalInstance = new bootstrap.Modal(changeEl);

    // 4. Mostrar Changelog se necessário
    const hide = localStorage.getItem('hideChangelog_v5.0');
    if (!hide && changelogModalInstance && typeof appVersions !== 'undefined') {
        changelogModalInstance.show();
    }
});

// --- LÓGICA DE TEMA (DARK MODE) ---
function toggleTheme() {
    const html = document.documentElement;
    const current = html.getAttribute('data-bs-theme');
    const next = current === 'dark' ? 'light' : 'dark';
    applyTheme(next);
    localStorage.setItem('theme', next);
}

function applyTheme(theme) {
    document.documentElement.setAttribute('data-bs-theme', theme);
    const icon = document.getElementById('themeIcon');
    if (icon) {
        icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
    }
}

// --- FUNÇÕES DE INTERFACE ---
function closeChangelog() {
    // PROTEÇÃO CONTRA ERRO DE NULL
    const checkbox = document.getElementById('dontShowChangelog');
    if (checkbox && checkbox.checked) {
        localStorage.setItem('hideChangelog_v5.0', 'true');
    }
    if (changelogModalInstance) changelogModalInstance.hide();
}

function updateCount() {
    const input = document.getElementById('fileInput');
    const status = document.getElementById('fileStatus');
    if (input && input.files.length > 0) {
        status.innerHTML = `<i class="fa-solid fa-check me-1"></i> ${input.files.length} arquivo(s) selecionado(s)`;
        status.className = "text-success fw-bold mt-2";
    } else if (status) {
        status.innerHTML = "";
    }
}

// --- LOGS E DEBUG ---
function openLogsModal() {
    if (logsModalInstance) logsModalInstance.show();
    fetchLogs();
}

async function fetchLogs() {
    const screen = document.getElementById('consoleOutput');
    if (!screen) return;
    screen.innerHTML = "<span class='text-secondary'>Conectando...</span>";
    try {
        const res = await fetch('/logs');
        const logs = await res.json();
        screen.innerHTML = logs.map(l => `<div>> ${l}</div>`).join('') + "<div class='mt-2 text-secondary blink'>_</div>";
        screen.scrollTop = screen.scrollHeight;
    } catch (err) {
        screen.innerHTML = `<span class="text-danger">Erro de conexão: ${err.message}</span>`;
    }
}

function showRawDebug(index) {
    const item = globalResults[index];
    const text = item ? item.debug : "Sem dados de debug.";
    const win = window.open("", "Debug", "width=800,height=600");
    win.document.write(`<html><head><title>Debug RAW</title></head><body style="background:#1e1e1e; color:#d4d4d4; font-family:monospace; padding:20px;"><h3>Conteúdo Extraído</h3><hr style="border-color:#444"><pre style="white-space: pre-wrap;">${text.replace(/</g, "&lt;")}</pre></body></html>`);
}

// --- ENVIO DO FORMULÁRIO ---
const form = document.getElementById('uploadForm');
if (form) {
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const files = document.getElementById('fileInput').files;

        if (files.length === 0) {
            alert("Selecione pelo menos um arquivo PDF.");
            return;
        }

        const btnSubmit = document.getElementById('btnSubmit');
        const btnText = document.getElementById('btnText');
        const btnSpinner = document.getElementById('btnSpinner');
        const resultArea = document.getElementById('resultArea');
        const tableBody = document.getElementById('tableBody');

        btnSubmit.disabled = true;
        btnText.innerText = "Processando...";
        btnSpinner.classList.remove('d-none');
        if (resultArea) resultArea.classList.add('d-none');

        const formData = new FormData();
        const tipoDoc = document.querySelector('input[name="tipoDoc"]:checked').value;
        formData.append('tipoDoc', tipoDoc);

        for (let i = 0; i < files.length; i++) formData.append('files', files[i]);

        try {
            const response = await fetch('/upload', { method: 'POST', body: formData });
            if (!response.ok) throw new Error("Erro no servidor");

            const data = await response.json();
            globalResults = data.lista;

            tableBody.innerHTML = "";
            data.lista.forEach((item, index) => {
                const isSuccess = !item.status.includes("Falha");
                const badgeClass = isSuccess ? "bg-success" : "bg-warning text-dark";
                const iconClass = isSuccess ? "fa-check" : "fa-triangle-exclamation";
                const row = `<tr><td class="ps-4 text-muted small">${item.original}</td><td class="fw-bold text-primary">${item.novo}</td><td><span class="badge ${badgeClass} status-badge"><i class="fa-solid ${iconClass} me-1"></i>${item.status}</span></td><td class="text-center"><button type="button" class="btn btn-outline-secondary btn-sm" onclick="showRawDebug(${index})" title="Ver Texto Bruto"><i class="fa-solid fa-magnifying-glass"></i></button></td></tr>`;
                tableBody.insertAdjacentHTML('beforeend', row);
            });

            if (resultArea) resultArea.classList.remove('d-none');

            const link = document.createElement("a");
            link.href = `data:application/zip;base64,${data.arquivoZip}`;
            link.download = "Documentos_Processados.zip";
            link.click();

        } catch (err) {
            alert("Ocorreu um erro: " + err.message);
        } finally {
            btnSubmit.disabled = false;
            btnText.innerText = "Processar Arquivos";
            btnSpinner.classList.add('d-none');
        }
    });
}