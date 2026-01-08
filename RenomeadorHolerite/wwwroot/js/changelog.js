// wwwroot/js/changelog.js

const appVersions = [
    {
        version: "v5.0 - The Modern Era (Atual)",
        color: "text-primary",
        items: [
            "✨ <b>Redesign Completo:</b> Migração para Bootstrap 5 e ícones FontAwesome.",
            "🔧 <b>Arquitetura:</b> Separação total entre HTML, CSS e JavaScript.",
            "🛡️ <b>Estabilidade:</b> Correção de vazamento de código na tela.",
            "🚀 <b>Performance:</b> Carregamento otimizado de modais."
        ]
    },
    {
        version: "v4.5 - Security & Logs",
        color: "text-secondary",
        items: [
            "✂️ <b>Limitação de Caracteres:</b> Nomes cortados automaticamente em 30 chars.",
            "🌙 <b>Dark Mode:</b> Tema escuro integrado para descanso visual.",
            "📟 <b>System Logs:</b> Visualizador de terminal em tempo real.",
            "🔍 <b>Modo RAW:</b> Debug para inspecionar o texto cru do PDF."
        ]
    },
    {
        version: "v4.0 - Retro & Features",
        color: "text-secondary",
        items: [
            "🏖️ <b>Recibo de Férias:</b> Adicionado suporte a terceiro tipo de documento.",
            "💾 <b>Visual Windows 98:</b> Tema nostálgico (descontinuado).",
            "⚡ <b>Upgrade .NET 10:</b> Atualização do Core do sistema."
        ]
    },
    {
        version: "v3.0 - Intelligence",
        color: "text-secondary",
        items: [
            "🧠 <b>Regex Avançado:</b> Melhoria na captura de nomes compostos.",
            "🧹 <b>Limpeza de Texto:</b> Remoção de 'Matrícula', 'Cód', etc.",
            "📈 <b>Social Credit:</b> Easter egg de sucesso."
        ]
    },
    {
        version: "v2.0 - Dockerization",
        color: "text-secondary",
        items: [
            "🐳 <b>Docker:</b> Criação do Dockerfile e docker-compose.",
            "🏦 <b>Comprovantes:</b> Suporte adicionado para comprovantes bancários.",
            "📦 <b>ZIP Download:</b> Agrupamento automático dos arquivos."
        ]
    },
    {
        version: "v1.0 - MVP",
        color: "text-secondary",
        items: [
            "🚀 <b>Holerite Only:</b> Leitura básica de PDFs.",
            "📄 <b>Extração Simples:</b> Identificação do nome via posição."
        ]
    }
];

function renderChangelog() {
    const container = document.getElementById('changelogBody');
    if (!container || typeof appVersions === 'undefined') return;

    let htmlContent = '';

    appVersions.forEach(ver => {
        htmlContent += `<h6 class="fw-bold ${ver.color} mt-3">${ver.version}</h6>`;
        htmlContent += `<ul class="list-group list-group-flush small mb-2">`;
        ver.items.forEach(item => {
            htmlContent += `<li class="list-group-item bg-transparent">${item}</li>`;
        });
        htmlContent += `</ul>`;
    });

    // É AQUI QUE O CHECKBOX É CRIADO
    htmlContent += `
        <div class="alert alert-light border mt-4 text-center small bg-opacity-10">
            <div class="form-check d-inline-block">
                <input class="form-check-input" type="checkbox" id="dontShowChangelog">
                <label class="form-check-label text-muted" for="dontShowChangelog">
                    Não mostrar novidades automaticamente
                </label>
            </div>
        </div>
    `;

    container.innerHTML = htmlContent;
}