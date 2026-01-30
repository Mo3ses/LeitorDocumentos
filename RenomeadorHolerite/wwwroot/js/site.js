// Variáveis Globais
let globalResults = [];

document.addEventListener('DOMContentLoaded', function() {
    // 1. Renderizar Changelog (se a função existir)
    if (typeof renderChangelog === 'function') {
        renderChangelog();
    }

    // 2. Verificar se deve abrir o Changelog automaticamente
    const hide = localStorage.getItem('hideChangelog_v5.5');
    // Se não estiver oculto E tivermos dados de versão, abre o modal
    if (!hide && typeof appVersions !== 'undefined') {
        openModal('changelogModal');
    }
});

// --- SISTEMA DE MODAL MANUAL (Zero Bootstrap) ---
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if(modal) {
        modal.style.display = 'block'; // Mostra a janela
    }
    
    // Se for o modal de logs, já carrega os dados
    if(modalId === 'logsModal') {
        fetchLogs();
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if(modal) {
        modal.style.display = 'none'; // Esconde a janela
    }
}

// Atalhos para os botões do HTML chamarem
function openLogsModal() {
    openModal('logsModal');
}

function closeChangelog() {
    const checkbox = document.getElementById('dontShowChangelog');
    if (checkbox && checkbox.checked) {
        localStorage.setItem('hideChangelog_v5.5', 'true');
    }
    closeModal('changelogModal');
}
// ------------------------------------------------

function updateCount() {
    const input = document.getElementById('fileInput');
    const status = document.getElementById('fileStatus');
    
    if (input && input.files.length > 0) {
        status.innerText = `🖌️ ${input.files.length} arquivo(s) na paleta!`;
        status.style.color = "#00bcd4";
    } else if (status) {
        status.innerText = "";
    }
}

async function fetchLogs() {
    const screen = document.getElementById('consoleOutput');
    if(!screen) return;
    
    screen.innerHTML = "Conectando ao servidor...";
    
    try {
        const res = await fetch('/logs');
        const logs = await res.json();
        screen.innerHTML = logs.map(l => `<div>> ${l}</div>`).join('') + "<div class='blink'>_</div>";
        screen.scrollTop = screen.scrollHeight;
    } catch (err) {
        screen.innerHTML = `<span style="color:red">Erro: ${err.message}</span>`;
    }
}

function showRawDebug(index) {
    const item = globalResults[index];
    const text = item ? item.debug : "Vazio.";
    
    const win = window.open("", "Debug", "width=800,height=600");
    win.document.write(`
        <html>
        <body style="background:#fff; font-family:'Courier New'; padding:20px; border: 5px solid black;">
            <h3>RAW DATA (Modo Paint)</h3>
            <hr style="border: 2px solid black;">
            <pre>${text.replace(/</g, "&lt;")}</pre>
        </body>
        </html>
    `);
}

// --- SUBMIT DO FORMULÁRIO ---
const form = document.getElementById('uploadForm');
if (form) {
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const files = document.getElementById('fileInput').files;
        
        if (files.length === 0) {
            alert("Ei! Selecione um arquivo primeiro!");
            return;
        }

        const btnSubmit = document.getElementById('btnSubmit');
        const btnText = document.getElementById('btnText');
        const btnSpinner = document.getElementById('btnSpinner');
        const resultArea = document.getElementById('resultArea');
        const tableBody = document.getElementById('tableBody');
        
        // Efeitos visuais de carregamento
        btnSubmit.disabled = true;
        btnText.innerText = "Pintando...";
        btnSpinner.classList.remove('d-none');
        if(resultArea) resultArea.classList.add('d-none');

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
                // Classes CSS do nosso estilo Paint
                const tagClass = isSuccess ? "status-tag ok" : "status-tag err"; 
                
                const row = `
                    <tr>
                        <td>${item.original}</td>
                        <td style="font-weight:bold; color:#00f;">${item.novo}</td>
                        <td><span class="${tagClass}">${item.status}</span></td>
                        <td style="text-align:center">
                            <button type="button" class="paint-btn secondary" style="padding:2px 8px; font-size:0.8rem;" onclick="showRawDebug(${index})">
                                🔍
                            </button>
                        </td>
                    </tr>
                `;
                tableBody.insertAdjacentHTML('beforeend', row);
            });

            if(resultArea) resultArea.classList.remove('d-none');
            
            // Download
            const link = document.createElement("a");
            link.href = `data:application/zip;base64,${data.arquivoZip}`;
            link.download = "Obras_de_Arte.zip";
            link.click();

        } catch (err) {
            alert("Deu ruim: " + err.message);
        } finally {
            btnSubmit.disabled = false;
            btnText.innerText = "✨ PROCESSAR MÁGICA ✨";
            btnSpinner.classList.add('d-none');
        }
    });
}