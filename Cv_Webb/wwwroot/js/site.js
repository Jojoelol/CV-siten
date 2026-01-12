// ==========================================
// --- ROBUST INITIALISERINGSMOTOR ---
// ==========================================

// Denna variabel behövs för att komma ihåg vilken profil man skickar meddelande från
let __lastSendMessageTrigger = null;

/**
 * Hjälpfunktion för att köra init-funktioner säkert.
 * Förhindrar att ett fel i en funktion stoppar resten av skriptet.
 */
function safeInit(initFn, name) {
    try {
        if (typeof initFn === 'function') {
            initFn();
        }
    } catch (e) {
        console.error(`Fel vid laddning av ${name}:`, e);
    }
}

// ==============================
// --- ALLMÄNNA FUNKTIONER ---
// ==============================

// Växlar mellan visning och redigering på profilsidan / projectdetails.
function enableEdit(sectionName) {
    const container = document.getElementById('section-' + sectionName);
    if (!container) return;

    const displayMode = container.querySelector('.display-mode');
    const editMode = container.querySelector('.edit-mode');
    const editBtn = container.querySelector('.edit-btn');
    const saveBtn = container.querySelector('.save-btn');

    if (displayMode) {
        displayMode.style.display = 'none';
        displayMode.classList?.add('u-hidden');
    }

    if (editMode) {
        editMode.style.display = 'block';
        editMode.classList?.remove('u-hidden');
        if (typeof editMode.focus === 'function') editMode.focus();
    }

    if (editBtn) {
        editBtn.style.display = 'none';
        editBtn.classList?.add('u-hidden');
    }

    if (saveBtn) {
        saveBtn.style.display = 'inline-flex';
        saveBtn.classList?.remove('u-hidden');
    }
}

function showJoinPopup(projectId, projectName) {
    openJoinRoleModal(projectId, projectName);
}

function openJoinRoleModal(projectId, projectName) {
    const idField = document.getElementById('modalProjectId');
    const textField = document.getElementById('modalProjectText');
    const modalElement = document.getElementById('joinRoleModal');

    if (!idField || !textField || !modalElement || !window.bootstrap) return;

    idField.value = projectId ?? "";
    textField.innerText = projectName ? ("Gå med i: " + projectName) : "Gå med i projekt";

    const myModal = window.bootstrap.Modal.getOrCreateInstance(modalElement);
    myModal.show();
}

// ==============================
// --- INIT-FUNKTIONER ---
// ==============================

// Popup för när man sparar profiländringar (SKICKAR VIDARE ELLER DÖLJER)
function initSaveSuccessRedirect() {
    const successPopup = document.getElementById('saveSuccessPopup');
    if (!successPopup) return;

    const redirectUrl = successPopup.getAttribute('data-redirect-url');

    setTimeout(() => {
        if (redirectUrl) {
            window.location.replace(redirectUrl);
        } else {
            successPopup.style.display = 'none';
        }
    }, 3000);
}

// Popup för när man skapat ett projekt
function initAddProjectSuccessPopup() {
    const popupData = document.getElementById('popup-data');
    if (!popupData) return;

    const shouldShow = popupData.dataset.show === "true" || popupData.getAttribute("data-show") === "true";
    if (!shouldShow) return;

    const popup = document.getElementById('successPopup');
    const redirectUrl = popupData.dataset.url || popupData.getAttribute("data-url");

    if (!popup || !redirectUrl) return;

    popup.style.display = 'flex';
    popup.classList?.remove('u-hidden');

    setTimeout(() => {
        window.location.replace(redirectUrl);
    }, 2000);
}

function initScrollRestoreOnSearch() {
    const searchForm = document.querySelector(".content-toolbar");
    if (!searchForm) return;

    searchForm.addEventListener("submit", () => {
        sessionStorage.setItem("scrollPos", window.scrollY);
    });

    const scrollPos = sessionStorage.getItem("scrollPos");
    if (scrollPos) {
        window.scrollTo(0, Number(scrollPos));
        sessionStorage.removeItem("scrollPos");
    }
}

function initReceiverSearch() {
    const searchInput = document.getElementById('receiverSearch');
    const resultsEl = document.getElementById('receiverResults');
    const receiverIdEl = document.getElementById('receiverId');
    if (!searchInput || !resultsEl || !receiverIdEl) return;

    let timer = null;

    function clearResults() { resultsEl.innerHTML = ""; }

    function setSelected(person) {
        receiverIdEl.value = person.id;
        searchInput.value = person.name;
        clearResults();
    }

    async function search(q) {
        const res = await fetch(`/Message/SearchPerson?q=${encodeURIComponent(q)}`, {
            headers: { "Accept": "application/json" }
        });
        return res.ok ? await res.json() : [];
    }

    function render(items) {
        clearResults();
        if (!items || items.length === 0) return;
        items.forEach(p => {
            const btn = document.createElement('button');
            btn.type = "button";
            btn.className = "list-group-item list-group-item-action d-flex align-items-center gap-2";
            btn.innerHTML = `<img src="${p.imageUrl || '/images/profilePicture/defaultPicture.jpg'}" style="width:32px;height:32px;border-radius:50%;object-fit:cover" onerror="this.src='/images/profilePicture/defaultPicture.jpg'"><span>${p.name}</span>`;
            btn.addEventListener('click', () => setSelected(p));
            resultsEl.appendChild(btn);
        });
    }

    searchInput.addEventListener('input', function () {
        const q = (this.value || "").trim();
        receiverIdEl.value = "";
        clearTimeout(timer);
        if (q.length < 2) return clearResults();
        timer = setTimeout(async () => render(await search(q)), 200);
    });
}

function initMessagesPage() {
    const readModalEl = document.getElementById("readMessageModal");
    const deleteModalEl = document.getElementById("deleteMessageModal");
    const sendModalEl = document.getElementById("sendMessageModal");

    if (!readModalEl && !deleteModalEl) return;

    const readFromEl = document.getElementById('readMessageFrom');
    const readContentEl = document.getElementById('readMessageContent');
    const deleteIdEl = document.getElementById('deleteMessageId');
    const deleteInfoEl = document.getElementById("deleteMessageInfo");
    const replyBtn = document.getElementById('replyBtn');

    let lastOpenedMessage = { from: "", senderId: "", subject: "" };

    if (readModalEl) {
        readModalEl.addEventListener("show.bs.modal", (event) => {
            const triggerEl = event.relatedTarget;
            const row = triggerEl?.closest('tr.message-row');
            if (!row) return;

            const from = row.getAttribute('data-from') || '';
            const senderId = row.getAttribute('data-sender-id');
            const subject = row.getAttribute('data-subject') || '';
            const content = row.querySelector('td.message-content')?.textContent.trim() || '';

            // --- LOGIK FÖR SVARA-KNAPPEN ---
            if (replyBtn) {
                // Vi kollar om senderId har ett riktigt värde
                const hasSender = senderId && senderId !== "" && senderId !== "null" && senderId !== "0";
                // Vi dubbelkollar också om namnet innehåller "(extern)"
                const isExternalText = from.includes("(extern)");

                if (hasSender && !isExternalText) {
                    replyBtn.style.display = 'inline-flex'; // Visa för inloggade
                } else {
                    replyBtn.style.display = 'none'; // Dölj för externa
                }
            }

            if (readFromEl) readFromEl.textContent = "Från: " + from;
            if (readContentEl) readContentEl.textContent = content;

            lastOpenedMessage = { from, senderId, subject };
        });
    }

    if (deleteModalEl) {
        deleteModalEl.addEventListener("show.bs.modal", (event) => {
            const btn = event.relatedTarget;
            if (!btn) return;
            if (deleteIdEl) deleteIdEl.value = btn.getAttribute("data-id") || "";
            if (deleteInfoEl) deleteInfoEl.textContent = `${btn.getAttribute("data-from")} – ${btn.getAttribute("data-subject")}`.trim();
        });
    }

    if (replyBtn) {
        replyBtn.addEventListener('click', function () {
            if (!lastOpenedMessage.senderId) return;

            const rSearch = document.getElementById('receiverSearch');
            const rId = document.getElementById('receiverId');
            const rSubject = document.getElementById('sendSubject');

            if (rSearch) rSearch.value = lastOpenedMessage.from;
            if (rId) rId.value = lastOpenedMessage.senderId;
            if (rSubject) {
                const s = lastOpenedMessage.subject;
                rSubject.value = s.toLowerCase().startsWith('re:') ? s : 'Re: ' + s;
            }

            bootstrap.Modal.getInstance(readModalEl)?.hide();
            bootstrap.Modal.getOrCreateInstance(sendModalEl).show();
        });
    }

    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-mark-read');
        if (!btn) return;

        const messageId = btn.getAttribute('data-id');
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const res = await fetch('/Message/MarkRead', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token },
            body: `id=${encodeURIComponent(messageId)}`
        });

        if (res.ok) {
            const row = btn.closest('tr');
            row?.classList.remove('table-light');
            const badge = row?.querySelector('.message-badge-new');
            if (badge) {
                badge.textContent = 'Läst';
                badge.classList.replace('message-badge-new', 'message-badge-read');
            }
            btn.remove();
        }
    }, true);
}

function initSendMessageModalValidation() {
    const modal = document.getElementById('sendMessageModal');
    if (!modal) return;

    const form = modal.querySelector('#sendMessageForm') || modal.querySelector('form');
    const submitBtn = modal.querySelector('#sendMessageSubmitBtn');
    if (!form || !submitBtn) return;

    const receiverId = modal.querySelector('#receiverId');
    const receiverSearch = modal.querySelector('#receiverSearch');
    const senderName = modal.querySelector('#senderName'); 
    const subject = modal.querySelector('#sendSubject');
    const content = modal.querySelector('#sendContent');

    const receiverErr = modal.querySelector('#receiverClientError');
    const senderErr = modal.querySelector('#senderNameClientError');
    const subjectErr = modal.querySelector('#subjectClientError');
    const contentErr = modal.querySelector('#contentClientError');

    const showErr = (el, msg) => {
        if (!el) return;
        el.textContent = msg;
        el.style.display = 'block';
    };

    const clearErr = (el) => {
        if (!el) return;
        el.textContent = '';
        el.style.display = 'none';
    };

    receiverSearch?.addEventListener('input', () => clearErr(receiverErr));
    senderName?.addEventListener('input', () => clearErr(senderErr));
    subject?.addEventListener('input', () => clearErr(subjectErr));
    content?.addEventListener('input', () => clearErr(contentErr));

    submitBtn.addEventListener('click', () => {
        let ok = true;

        clearErr(receiverErr);
        clearErr(senderErr);
        clearErr(subjectErr);
        clearErr(contentErr);

        if (!receiverId?.value?.trim()) {
            ok = false;
            showErr(receiverErr, "Välj en mottagare i listan.");
        }

        if (senderName && senderName.value.trim().length < 2) {
            ok = false;
            showErr(senderErr, "Ange ditt namn (minst 2 tecken).");
        }

        if (!subject?.value?.trim()) {
            ok = false;
            showErr(subjectErr, "Ange ett ämne.");
        }

        if (!content?.value?.trim()) {
            ok = false;
            showErr(contentErr, "Skriv ett meddelande.");
        }

        if (!ok) {
            if (senderErr?.style.display === 'block') senderName?.focus();
            else if (receiverErr?.style.display === 'block') receiverSearch?.focus();
            else if (subjectErr?.style.display === 'block') subject?.focus();
            else if (contentErr?.style.display === 'block') content?.focus();
            return;
        }

        if (form.requestSubmit) form.requestSubmit();
        else form.submit();
    });
}

function initProfileImagePreview() {
    const input = document.getElementById('imageInput');
    const container = document.getElementById('profile-image-preview');
    if (!input || !container) return;

    input.addEventListener('change', function (e) {

        // ❗ Om valideringen markerat filen som ogiltig → gör INGENTING
        if (this.dataset.invalidFile === "true") return;

        const file = e.target.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (event) => {
            container.innerHTML = `
                <img src="${event.target.result}"
                     class="img-thumbnail mb-2"
                     style="width:150px;height:150px;object-fit:cover;border-radius:50%;border:3px solid #002d5a;">
            `;
        };
        reader.readAsDataURL(file);
    });
}

function initImageFileTypeValidation() {
    const input = document.getElementById('imageInput');
    const errorEl = document.getElementById('imageFileError');
    if (!input || !errorEl) return;

    input.addEventListener('change', function () {
        const file = this.files?.[0];

        // Rensa tidigare status
        this.dataset.invalidFile = "false";

        if (!file) {
            errorEl.style.display = "none";
            errorEl.textContent = "";
            return;
        }

        const allowed = ['jpg', 'jpeg', 'png'];
        const ext = file.name.split('.').pop().toLowerCase();

        if (!allowed.includes(ext)) {
            errorEl.textContent = "Endast .jpg och .png är tillåtna bildformat.";
            errorEl.style.display = "block";

            // Markera som ogiltig
            this.dataset.invalidFile = "true";

            // Rensa filen
            this.value = "";
        } else {
            errorEl.style.display = "none";
            errorEl.textContent = "";
        }
    });
}

function initProjectImagePreview() {
    const input = document.getElementById('projectImageInput');
    const preview = document.getElementById('projectImagePreview');
    if (!input || !preview) return;

    input.addEventListener('change', function (e) {
        const file = e.target.files?.[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = (ev) => {
                preview.src = ev.target.result;
                preview.style.display = 'block';
            };
            reader.readAsDataURL(file);
        } else {
            preview.style.display = 'none';
        }
    });
}

function initCvUploadPdfOnly() {
    const cvInput = document.getElementById('cvFileInput');
    const cvForm = document.getElementById('cvUploadForm');
    if (!cvInput || !cvForm) return;

    cvInput.addEventListener('change', function () {
        const file = this.files[0];
        if (!file) return;
        const ext = file.name.split('.').pop().toLowerCase();
        if (ext !== 'pdf') {
            alert("Felaktigt filformat! Du kan bara ladda upp PDF-filer.");
            this.value = "";
        } else {
            cvForm.submit();
        }
    });
}

function initDateRangeValidation() {
    const start = document.querySelector('input[name="StartDate"]');
    const end = document.querySelector('input[name="EndDate"]');

    // Om vi inte hittar fälten, gör ingenting
    if (!start || !end) return;

    // Funktion som sätter reglerna
    const updateConstraints = () => {
        if (start.value) {
            // 1. Sätt min-datum på slutdatumet till startdatumet
            end.min = start.value;

            // 2. Om slutdatum redan är valt och är FÖRE startdatum -> Rensa
            if (end.value && end.value < start.value) {
                end.value = "";
                // Valfritt: Visa en varning bara om användaren faktiskt interagerar
                // alert("Slutdatumet har rensats eftersom det var före startdatumet.");
            }
        } else {
            // Om inget startdatum finns, ta bort begränsningen
            end.removeAttribute("min");
        }
    };

    // Kör direkt vid laddning (Fixar buggen vid omladdning/redigering)
    updateConstraints();

    // Kör när användaren ändrar startdatum
    start.addEventListener("change", updateConstraints);

    // Kör när användaren klickar på/fokuserar slutdatum (Extra säkerhet)
    end.addEventListener("focus", updateConstraints);
}

function initSmartBackButton() {
    if (window.location.pathname.includes("/Project/ProjectDetails")) {
        const ref = document.referrer;
        if (ref && !ref.includes(window.location.pathname)) {
            sessionStorage.setItem("originalProjectSource", ref);
        }
    }

    const btn = document.getElementById("smartBackBtn");
    if (btn) {
        btn.addEventListener("click", (e) => {
            e.preventDefault();
            const source = sessionStorage.getItem("originalProjectSource");
            window.location.href = source || "/Project/AllProjects";
        });
    }
}

function initSendMessagePrefillFromProfile() {
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('[data-bs-target="#sendMessageModal"]');
        if (btn) __lastSendMessageTrigger = btn;
    }, true);

    const sendEl = document.getElementById('sendMessageModal');
    if (!sendEl) return;

    sendEl.addEventListener('show.bs.modal', function (event) {
        const trigger = event.relatedTarget || __lastSendMessageTrigger || document.activeElement;
        if (!trigger) return;

        const receiverId = trigger.getAttribute?.('data-receiver-id');
        if (!receiverId) return;

        const receiverName = trigger.getAttribute?.('data-receiver-name') || '';
        const rIdEl = sendEl.querySelector('#receiverId');
        const rSearchEl = sendEl.querySelector('#receiverSearch');

        if (rIdEl) rIdEl.value = receiverId;
        if (rSearchEl) rSearchEl.value = receiverName;
    });
}

function initModalCleanup() {
    function cleanup() {
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('padding-right');
        document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
    }
    ['sendMessageModal', 'readMessageModal', 'deleteMessageModal'].forEach(id => {
        document.getElementById(id)?.addEventListener('hidden.bs.modal', cleanup);
    });
}

// ==========================================
// --- MASTER DOM LOAD (Kör allt säkert) ---
// ==========================================

document.addEventListener("DOMContentLoaded", function () {
    // Grundläggande logik & Popups
    safeInit(initAddProjectSuccessPopup, "Add Project Popup");
    safeInit(initSaveSuccessRedirect, "Save Success Redirect");
    safeInit(initScrollRestoreOnSearch, "Scroll Restore");
    safeInit(initReceiverSearch, "Receiver Search");

    // Meddelandehantering
    safeInit(initMessagesPage, "Messages Logic");
    safeInit(initSendMessagePrefillFromProfile, "Send Message Prefill");
    safeInit(initSendMessageModalValidation, "Message Validation");
    safeInit(initModalCleanup, "Modal Cleanup");

    // Förhandsgranskningar
    safeInit(initImageFileTypeValidation, "Image File Type Validation");
    safeInit(initProfileImagePreview, "Profile Preview");
    safeInit(initProjectImagePreview, "Project Preview");

    // Projekt & CV & Övrigt
    safeInit(initCvUploadPdfOnly, "CV PDF Check");
    safeInit(initDateRangeValidation, "Date Validation");
    safeInit(initSmartBackButton, "Smart Back Button");

    // Event Delegation för dynamiska knappar
    document.addEventListener("click", (e) => {
        const joinBtn = e.target.closest('.all-projects-join-btn');
        if (joinBtn) {
            openJoinRoleModal(joinBtn.getAttribute('data-project-id'), joinBtn.getAttribute('data-project-name'));
        }

        const editBtn = e.target.closest('[data-edit-field]') || e.target.closest('[data-enable-edit]');
        if (editBtn) {
            enableEdit(editBtn.getAttribute('data-edit-field') || editBtn.getAttribute('data-enable-edit'));
        }
    });

    // CV Delete Confirm
    document.querySelectorAll('.cv-delete-form-wrap').forEach(form => {
        form.addEventListener('submit', (e) => {
            if (!confirm("Är du säker på att du vill ta bort ditt CV permanent?")) e.preventDefault();
        });
    });
});