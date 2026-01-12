// ==============================
// --- ALLMÄNNA FUNKTIONER ---
// ==============================

// Växlar mellan visning och redigering på profilsidan / projectdetails.
// Klarar både inline style="display:none" och class .u-hidden
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
        // vissa av dina vyer vill ha inline-flex, andra bara "block"
        saveBtn.style.display = 'inline-flex';
        saveBtn.classList?.remove('u-hidden');
    }
}


// Öppnar modalen för att gå med i ett projekt och fyller i ID/Namn (stöd för inline onclick)
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

function initAddProjectSuccessPopup() {
    const popupData = document.getElementById('popup-data');
    if (!popupData) return;

    const shouldShow = popupData.dataset.show === "true" || popupData.getAttribute("data-show") === "true";
    if (!shouldShow) return;

    const popup = document.getElementById('successPopup');
    const redirectUrl = popupData.dataset.url || popupData.getAttribute("data-url");

    if (!popup || !redirectUrl) return;

    // Visa popup (klarar både style och u-hidden)
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

    function clearResults() {
        resultsEl.innerHTML = "";
    }

    function setSelected(person) {
        receiverIdEl.value = person.id;
        searchInput.value = person.name;
        clearResults();
    }

    async function search(q) {
        const res = await fetch(`/Message/SearchPerson?q=${encodeURIComponent(q)}`, {
            headers: { "Accept": "application/json" }
        });
        if (!res.ok) return [];
        return await res.json();
    }

    function render(items) {
        clearResults();
        if (!items || items.length === 0) return;

        for (const p of items) {
            const btn = document.createElement('button');
            btn.type = "button";
            btn.className = "list-group-item list-group-item-action d-flex align-items-center gap-2";

            const img = document.createElement('img');
            img.alt = p.name;
            img.style.width = "32px";
            img.style.height = "32px";
            img.style.borderRadius = "50%";
            img.style.objectFit = "cover";
            img.src = p.imageUrl || "/images/profilePicture/defaultPicture.jpg";
            img.onerror = () => { img.src = "/images/profilePicture/defaultPicture.jpg"; };

            const span = document.createElement('span');
            span.textContent = p.name;

            btn.appendChild(img);
            btn.appendChild(span);

            btn.addEventListener('click', () => setSelected(p));
            resultsEl.appendChild(btn);
        }
    }

    searchInput.addEventListener('input', function () {
        const q = (searchInput.value || "").trim();
        receiverIdEl.value = "";
        clearTimeout(timer);

        if (q.length < 2) {
            clearResults();
            return;
        }

        timer = setTimeout(async () => {
            const items = await search(q);
            render(items);
        }, 200);
    });
}

function initMessagesPage() {
    const readModalEl = document.getElementById("readMessageModal");
    const deleteModalEl = document.getElementById("deleteMessageModal");
    const sendModalEl = document.getElementById("sendMessageModal");


    if (!readModalEl && !deleteModalEl && !sendModalEl) return;

    const readFromEl = document.getElementById('readMessageFrom');
    const readContentEl = document.getElementById('readMessageContent');
    const deleteIdEl = document.getElementById('deleteMessageId');
    const deleteInfoEl = document.getElementById("deleteMessageInfo");
    const replyBtn = document.getElementById('replyBtn');

    let lastOpenedMessage = { from: "", senderId: "", subject: "" };


    readModalEl.addEventListener("show.bs.modal", (event) => {
        const triggerEl = event.relatedTarget;
        if (!triggerEl) return;

        const row = triggerEl.closest('tr.message-row');
        if (!row) return;

        const from = row.getAttribute('data-from') || '';
        const senderId = row.getAttribute('data-sender-id') || '';
        const subject = row.getAttribute('data-subject') || '';

        const contentCell = row.querySelector('td.message-content');
        const content = contentCell ? contentCell.textContent.trim() : '';

        if (readFromEl) readFromEl.textContent = "Från: " + from;
        if (readContentEl) readContentEl.textContent = content;

        lastOpenedMessage = { from, senderId, subject };
    });


    if (deleteModalEl) {
        deleteModalEl.addEventListener("show.bs.modal", (event) => {
            const triggerBtn = event.relatedTarget;
            if (!triggerBtn) return;

            const id = triggerBtn.getAttribute("data-id") || "";
            const from = triggerBtn.getAttribute("data-from") || "";
            const subject = triggerBtn.getAttribute("data-subject") || "";

            if (deleteIdEl) deleteIdEl.value = id;
            if (deleteInfoEl) deleteInfoEl.textContent = `${from} – ${subject}`.trim();
        });
    }

    if (replyBtn) {
        replyBtn.addEventListener('click', function () {
            if (!lastOpenedMessage || !lastOpenedMessage.senderId) return;

            const receiverSearch = document.getElementById('receiverSearch');
            const receiverId = document.getElementById('receiverId');
            const subjectInput = document.getElementById('sendSubject');
            const receiverResults = document.getElementById('receiverResults');

            if (receiverSearch) receiverSearch.value = lastOpenedMessage.from || '';
            if (receiverId) receiverId.value = lastOpenedMessage.senderId || '';
            if (subjectInput) {
                const s = (lastOpenedMessage.subject || '').trim();
                subjectInput.value = s.toLowerCase().startsWith('re:') ? s : ('Re: ' + s);
            }
            if (receiverResults) receiverResults.innerHTML = "";

            if (readModalEl && window.bootstrap) {
                const readInstance = window.bootstrap.Modal.getInstance(readModalEl);
                if (readInstance) readInstance.hide();
            }
            if (sendModalEl && window.bootstrap) {
                const sendInstance = window.bootstrap.Modal.getOrCreateInstance(sendModalEl);
                sendInstance.show();
            }
        });
    }

    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.btn-mark-read');
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();
        e.stopImmediatePropagation();

        const messageId = btn.getAttribute('data-id');
        if (!messageId) return;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const res = await fetch('/Message/MarkRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token
            },
            body: `id=${encodeURIComponent(messageId)}`
        });

        if (!res.ok) return;

        const row = btn.closest('tr');
        row?.classList.remove('table-light');

        const badge = row?.querySelector('.message-badge-new');
        if (badge) {
            badge.textContent = 'Läst';
            badge.classList.remove('message-badge-new');
            badge.classList.add('message-badge-read');
        }

        btn.remove();
    }, true);
}



function openSendModalIfNeeded() {
    const shouldOpen = document.body?.dataset?.openSendModal === "true";
    if (!shouldOpen) return;

    const el = document.getElementById("sendMessageModal");
    if (!el || !window.bootstrap) return;

    (bootstrap.Modal.getInstance(el) || new bootstrap.Modal(el)).show();
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

    const showErr = (el, msg) => { if (el) { el.textContent = msg; el.style.display = 'block'; } };
    const clearErr = (el) => { if (el) { el.textContent = ''; el.style.display = 'none'; } };


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


function initModalCleanup() {
    function cleanup() {
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('padding-right');
        document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
    }

    ['sendMessageModal', 'readMessageModal', 'deleteMessageModal'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        el.addEventListener('hidden.bs.modal', cleanup);
    });
}

function initSendMessageModalPrefill() {
    const sendEl = document.getElementById('sendMessageModal');
    if (!sendEl || !window.bootstrap) return;

    // Prefill när modalen öppnas via en trigger med data-receiver-id / data-receiver-name
    sendEl.addEventListener('show.bs.modal', (event) => {
        const trigger = event.relatedTarget;
        if (!trigger) return;

        const receiverId = trigger.getAttribute('data-receiver-id');
        const receiverName = trigger.getAttribute('data-receiver-name') || '';

        const receiverIdEl = document.getElementById('receiverId');
        const receiverSearchEl = document.getElementById('receiverSearch');
        const subjectEl = document.getElementById('sendSubject');
        const resultsEl = document.getElementById('receiverResults');

        if (receiverIdEl && receiverId) receiverIdEl.value = receiverId;
        if (receiverSearchEl && receiverName) receiverSearchEl.value = receiverName;

        if (subjectEl) subjectEl.value = '';
        if (resultsEl) resultsEl.innerHTML = '';
    });
}

function initProfileImagePreview() {
    const imageInput = document.getElementById('imageInput');
    const previewContainer = document.getElementById('profile-image-preview');
    if (!imageInput || !previewContainer) return;

    imageInput.addEventListener('change', function (event) {
        const file = event.target.files && event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            previewContainer.innerHTML = `
                <img src="${e.target.result}"
                     class="img-thumbnail mb-2"
                     style="width: 150px; height: 150px; object-fit: cover; border-radius: 50%; border: 3px solid #002d5a;" />
            `;
        };
        reader.readAsDataURL(file);
    });
}

function initSaveSuccessRedirect() {
    const successPopup = document.getElementById('saveSuccessPopup');
    if (!successPopup) return;

    const redirectUrl = successPopup.getAttribute('data-redirect-url');
    if (!redirectUrl) return;

    setTimeout(() => {
        window.location.replace(redirectUrl);
    }, 3000);
}

function initDateRangeValidation() {
    const startDateInput = document.querySelector('input[name="StartDate"]');
    const endDateInput = document.querySelector('input[name="EndDate"]');
    if (!startDateInput || !endDateInput) return;

    startDateInput.addEventListener("change", function () {
        if (startDateInput.value) {
            endDateInput.min = startDateInput.value;
        }

        if (endDateInput.value && startDateInput.value && endDateInput.value < startDateInput.value) {
            endDateInput.value = "";
            alert("Slutdatumet har rensats eftersom det var före det nya startdatumet.");
        }
    });
}

function initSmartBackButton() {
    // Spara referrer om vi är på ProjectDetails
    if (window.location.pathname.includes("/Project/ProjectDetails")) {
        const referrer = document.referrer;

        if (referrer && !referrer.includes(window.location.pathname)) {
            sessionStorage.setItem("originalProjectSource", referrer);
        }
    }

    const smartBackBtn = document.getElementById("smartBackBtn");
    if (!smartBackBtn) return;

    smartBackBtn.addEventListener("click", function (e) {
        e.preventDefault();
        const source = sessionStorage.getItem("originalProjectSource");

        if (source) {
            window.location.href = source;
        } else {
            window.location.href = "/Project/AllProjects";
        }
    });
}

function initJoinProjectDelegation() {
    // Klick på knappar med data-join-project-id (utan dubbla handlers)
    document.addEventListener("click", (e) => {
        const btn = e.target.closest("[data-join-project-id]");
        if (!btn) return;

        const projectId = btn.getAttribute("data-join-project-id") || "";
        const projectName = btn.getAttribute("data-join-project-name") || "";

        openJoinRoleModal(projectId, projectName);
    });
}

function initEditButtonsDelegation() {
    // Stöd för både data-edit-field och data-enable-edit
    document.addEventListener("click", (e) => {
        const btn =
            e.target.closest("[data-edit-field]") ||
            e.target.closest("[data-enable-edit]");

        if (!btn) return;

        const field = btn.getAttribute("data-edit-field") || btn.getAttribute("data-enable-edit");
        if (!field) return;

        enableEdit(field);
    });
}

function initCvUpload() {
    const uploadBtn = document.querySelector('[data-action="cv-upload"]');
    const fileInput = document.getElementById("cvFileInput");
    const uploadForm = document.getElementById("cvUploadForm");
    if (!uploadBtn || !fileInput) return;

    uploadBtn.addEventListener("click", () => fileInput.click());

    fileInput.addEventListener("change", () => {
        if (uploadForm) uploadForm.submit();
    });
}

function initConfirmForms() {
    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (e) => {
            const msg = form.getAttribute("data-confirm") || "Är du säker?";
            if (!window.confirm(msg)) e.preventDefault();
        });
    });
}


document.addEventListener("DOMContentLoaded", function () {
    // Fånga upp alla knappar med klassen 'all-projects-join-btn'
    const joinButtons = document.querySelectorAll('.all-projects-join-btn');

    joinButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Hämta data från knappen
            const projectId = this.getAttribute('data-project-id');
            const projectName = this.getAttribute('data-project-name');

            // Fyll i modalens fält
            const idInput = document.getElementById('modalProjectId');
            const textDisplay = document.getElementById('modalProjectText');

            if (idInput && textDisplay) {
                idInput.value = projectId;
                textDisplay.innerText = "Gå med i: " + projectName;

                // Visa modalen (Bootstrap 5 syntax)
                const joinModal = new bootstrap.Modal(document.getElementById('joinRoleModal'));
                joinModal.show();
            }
        });
    });

    // Logik för att stänga success-popupen i ProjectDetails om den finns
    const successPopup = document.getElementById('saveSuccessPopup');
    if (successPopup) {
        setTimeout(() => {
            successPopup.style.display = 'none';
        }, 3000);
    }
});

document.addEventListener("DOMContentLoaded", function () {
    // Vi använder .cv-delete-form-wrap (som i din HTML)
    // Vi använder querySelectorAll för att fånga både knappen i sidebaren och i main content
    const deleteCvForms = document.querySelectorAll('.cv-delete-form-wrap');

    deleteCvForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!confirm("Är du säker på att du vill ta bort ditt CV permanent?")) {
                e.preventDefault(); // Avbryter inskickningen om användaren klickar "Avbryt"
            }
        });
    });
});


    document.addEventListener("DOMContentLoaded", function () {
        const cvInput = document.getElementById('cvFileInput');
        const cvForm = document.getElementById('cvUploadForm');

        if (cvInput && cvForm) {
            cvInput.addEventListener('change', function () {
                const file = this.files[0];
                if (file) {
                    const fileName = file.name;
                    const extension = fileName.split('.').pop().toLowerCase();

                    if (extension !== 'pdf') {
                        // Visa en snabb varning
                        alert("Felaktigt filformat! Du kan bara ladda upp PDF-filer.");
                        // Rensa inputen så att den felaktiga filen försvinner
                        this.value = "";
                    } else {
                        // Om det är en PDF, skicka formuläret som vanligt
                        cvForm.submit();
                    }
                }
            });
        }
    });

    // ==============================
    // --- DOMContentLoaded (EN gång) ---
    // ==============================

    document.addEventListener("DOMContentLoaded", function () {
        initAddProjectSuccessPopup();
        initScrollRestoreOnSearch();

        initReceiverSearch();
        initMessagesPage();
        initModalCleanup();

        openSendModalIfNeeded();
        initSendMessageModalPrefill();
        initSendMessageModalValidation();

        initProfileImagePreview();
        initSaveSuccessRedirect();

        initDateRangeValidation();
        initSmartBackButton();

        initJoinProjectDelegation();
        initEditButtonsDelegation();
        initCvUpload();
        initConfirmForms();

        initAllProjectsJoinButtons();
        initHideProjectDetailsSuccessPopup();
        initCvDeleteConfirm();
        initCvUploadPdfOnly();
    });
