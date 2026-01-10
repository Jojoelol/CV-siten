// --- ALLMÄNNA FUNKTIONER ---

// Växlar mellan visning och redigering på profilsidan
function enableEdit(sectionName) {
    const container = document.getElementById('section-' + sectionName);
    if (container) {
        const displayMode = container.querySelector('.display-mode');
        const editMode = container.querySelector('.edit-mode');
        const editBtn = container.querySelector('.edit-btn');
        const saveBtn = container.querySelector('.save-btn');

        if (displayMode && editMode) {
            displayMode.style.display = 'none';
            editMode.style.display = 'block';
            editMode.focus();
        }
        if (editBtn && saveBtn) {
            editBtn.style.display = 'none';
            saveBtn.style.display = 'inline-flex';
        }
    }
}

// --- HÄNDELSER VID SIDLADDNING ---

document.addEventListener("DOMContentLoaded", function () {

    // 1. Logik för Pop-up vid sparat projekt
    const popupData = document.getElementById('popup-data');
    if (popupData && popupData.dataset.show === "true") {
        const popup = document.getElementById('successPopup');
        const redirectUrl = popupData.dataset.url;

        if (popup) {
            popup.style.display = 'flex';
            setTimeout(function () {
                window.location.replace(redirectUrl);
            }, 2000);
        }
    }

    // 2. Hantera scroll-position vid sökning
    const searchForm = document.querySelector(".content-toolbar");
    if (searchForm) {
        searchForm.addEventListener("submit", function () {
            sessionStorage.setItem("scrollPos", window.scrollY);
        });
    }

    // Återställ scroll-position om den finns sparad
    const scrollPos = sessionStorage.getItem("scrollPos");
    if (scrollPos) {
        window.scrollTo(0, scrollPos);
        sessionStorage.removeItem("scrollPos");
    }

    initReceiverSearch();
    initMessagesPage();
    initModalCleanup();

});
// --- MESSAGES PAGE ---
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
    const readFromEl = document.getElementById('readMessageFrom');
    const readContentEl = document.getElementById('readMessageContent');
    const deleteIdEl = document.getElementById('deleteMessageId');
    const replyBtn = document.getElementById('replyBtn');

    // Om sidan inte ens har message-table/modaler, gör inget
    if (!readFromEl && !deleteIdEl && !replyBtn) return;

    let lastOpenedMessage = null;

    document.addEventListener('click', function (e) {

        // DELETE-knapp (bara fyll hidden input)
        const deleteBtn = e.target.closest('.btn-delete-message');
        if (deleteBtn) {
            e.stopPropagation();
            if (deleteIdEl) deleteIdEl.value = deleteBtn.getAttribute('data-id') || '';
            return;
        }

        // Klick på en rad -> fyll READ-modalen
        const row = e.target.closest('.message-row');
        if (row) {
            const from = row.getAttribute('data-from') || '';
            const senderId = row.getAttribute('data-sender-id') || '';
            const subject = row.getAttribute('data-subject') || '';
            const content = row.querySelector('.message-content')?.textContent?.trim() || '';

            if (readFromEl) readFromEl.textContent = "Från: " + from;
            if (readContentEl) readContentEl.textContent = content;

            lastOpenedMessage = { from, senderId, subject };
            return;
        }
    });

    // Reply-knapp: fyll send-modal och växla modaler
    replyBtn?.addEventListener('click', function () {
        if (!lastOpenedMessage) return;

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

        const readEl = document.getElementById('readMessageModal');
        const sendEl = document.getElementById('sendMessageModal');

        if (readEl && window.bootstrap) {
            (bootstrap.Modal.getInstance(readEl) || new bootstrap.Modal(readEl)).hide();
        }
        if (sendEl && window.bootstrap) {
            (bootstrap.Modal.getInstance(sendEl) || new bootstrap.Modal(sendEl)).show();
        }
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
    document.addEventListener('click', function (e) {
    const btn = e.target.closest('[data-bs-target="#sendMessageModal"][data-receiver-id]');
    if (!btn) return;

    const receiverId = btn.getAttribute('data-receiver-id');
    const receiverName = btn.getAttribute('data-receiver-name') || '';

    const receiverIdEl = document.getElementById('receiverId');
    const receiverSearchEl = document.getElementById('receiverSearch');
    const subjectEl = document.getElementById('sendSubject');
    const resultsEl = document.getElementById('receiverResults');

    if (receiverIdEl) receiverIdEl.value = receiverId;
    if (receiverSearchEl) receiverSearchEl.value = receiverName;

    if (subjectEl) subjectEl.value = '';

    if (resultsEl) resultsEl.innerHTML = '';
});

//VISA PROFILBILD LIVE
document.addEventListener("DOMContentLoaded", function () {

    // --- 1. Förhandsvisning av profilbild ---
    const imageInput = document.getElementById('imageInput');
    const previewContainer = document.getElementById('profile-image-preview');

    if (imageInput && previewContainer) {
        imageInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    previewContainer.innerHTML = `
                        <img src="${e.target.result}" 
                             class="img-thumbnail mb-2" 
                             style="width: 150px; height: 150px; object-fit: cover; border-radius: 50%; border: 3px solid #002d5a;" />
                    `;
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // --- 2. Automatisk omdirigering efter sparande ---
    const successPopup = document.getElementById('saveSuccessPopup');
    if (successPopup) {
        // Hämta URL:en från data-attributet vi skapade i HTML
        const redirectUrl = successPopup.getAttribute('data-redirect-url');

        if (redirectUrl) {
            setTimeout(function () {
                window.location.replace(redirectUrl);
            }, 3000);
        }
    }
});

//GÖRA SLUTDATUM EFTER STARTDATUM GRÅA OSV
document.addEventListener("DOMContentLoaded", function () {
    const startDateInput = document.querySelector('input[name="StartDate"]');
    const endDateInput = document.querySelector('input[name="EndDate"]');

    startDateInput.addEventListener("change", function () {
        // Sätt 'min'-attributet på slutdatum till det valda startdatumet
        if (startDateInput.value) {
            endDateInput.min = startDateInput.value;
        }

        // Om användaren redan valt ett slutdatum som nu är ogiltigt, rensa det
        if (endDateInput.value && endDateInput.value < startDateInput.value) {
            endDateInput.value = "";
            alert("Slutdatumet har rensats eftersom det var före det nya startdatumet.");
        }
    });
});

//TILLBAKA KNAPP I PROJECTDETAILS, VETA VAR MAN KOM IFRÅN (PROFILE/ALLPROJECTS)
document.addEventListener("DOMContentLoaded", function () {
    // Kontrollera om vi är på en projektdetaljsida
    if (window.location.pathname.includes("/Project/ProjectDetails")) {
        const currentUrl = window.location.href;
        const referrer = document.referrer;

        // Om vi kommer från en annan sida (inte oss själva), spara den som vår "hembas"
        if (referrer && !referrer.includes(window.location.pathname)) {
            sessionStorage.setItem("originalProjectSource", referrer);
        }
    }

    // Fix för tillbaka-knappen
    const smartBackBtn = document.getElementById("smartBackBtn");
    if (smartBackBtn) {
        smartBackBtn.addEventListener("click", function (e) {
            e.preventDefault();
            const source = sessionStorage.getItem("originalProjectSource");

            if (source) {
                window.location.href = source;
            } else {
                // Om ingen källa finns sparad (t.ex. man skrev in URL direkt), 
                // gå till AllProjects som standard
                window.location.href = "/Project/AllProjects";
            }
        });
    }
});


// Öppnar modalen för att gå med i ett projekt och fyller i ID/Namn
function showJoinPopup(projectId, projectName) {
    const idField = document.getElementById('modalProjectId');
    const textField = document.getElementById('modalProjectText');
    const modalElement = document.getElementById('joinRoleModal');

    if (idField && textField && modalElement) {
        idField.value = projectId;
        textField.innerText = "Gå med i: " + projectName;

        // Skapar och visar modalen med Bootstrap 5-logik
        const myModal = new bootstrap.Modal(modalElement);
        myModal.show();
    }
}



//PROFILE, KANSKE INTE BEHÖVS


function enableEdit(fieldName) {
    const section = document.getElementById(`section-${fieldName}`);
    if (!section) return;

    const display = section.querySelector(".display-mode");
    const edit = section.querySelector(".edit-mode");
    const editBtn = section.querySelector(".edit-btn");
    const saveBtn = section.querySelector(".save-btn");

    if (display) display.classList.add("u-hidden");
    if (edit) edit.classList.remove("u-hidden");
    if (editBtn) editBtn.classList.add("u-hidden");
    if (saveBtn) saveBtn.classList.remove("u-hidden");

    // Sätt fokus i textarea om den finns
    if (edit && typeof edit.focus === "function") edit.focus();
}

document.addEventListener("DOMContentLoaded", () => {
    // 1) Koppla "Redigera"-knappar utan inline onclick
    document.querySelectorAll("[data-edit-field]").forEach((btn) => {
        btn.addEventListener("click", () => enableEdit(btn.dataset.editField));
    });

    // 2) CV-upload utan inline onclick/onchange
    const uploadBtn = document.querySelector('[data-action="cv-upload"]');
    const fileInput = document.getElementById("cvFileInput");
    const uploadForm = document.getElementById("cvUploadForm");

    if (uploadBtn && fileInput) {
        uploadBtn.addEventListener("click", () => fileInput.click());

        fileInput.addEventListener("change", () => {
            if (uploadForm) uploadForm.submit();
        });
    }

    // 3) Confirm dialogs utan inline onsubmit
    document.querySelectorAll("form[data-confirm]").forEach((form) => {
        form.addEventListener("submit", (e) => {
            const msg = form.getAttribute("data-confirm") || "Är du säker?";
            if (!window.confirm(msg)) e.preventDefault();
        });
    });

    // 4) (Valfritt) Bootstrap modal: om din SendMessageModal har inputs för receiver.
    // Den här delen är "safe" – den gör inget om fälten inte finns.
    const modalEl = document.getElementById("sendMessageModal");
    if (modalEl && window.bootstrap) {
        modalEl.addEventListener("show.bs.modal", (event) => {
            const trigger = event.relatedTarget;
            if (!trigger) return;

            const receiverId = trigger.getAttribute("data-receiver-id");
            const receiverName = trigger.getAttribute("data-receiver-name");

            // Försök hitta vanliga fältnamn i modalens form
            const idInput =
                modalEl.querySelector('input[name="ReceiverId"]') ||
                modalEl.querySelector("#ReceiverId");

            const nameEl =
                modalEl.querySelector('[data-role="receiver-name"]') ||
                modalEl.querySelector("#ReceiverName");

            if (idInput && receiverId) idInput.value = receiverId;
            if (nameEl && receiverName) nameEl.textContent = receiverName;
        });
    }
});


// ====== Messages: modal-logik (Read / Reply / Delete) ====== KANSKE ÖVERFLÖDIG
document.addEventListener("DOMContentLoaded", () => {
    // --- Läs meddelande modal ---
    const readModalEl = document.getElementById("readMessageModal");
    const readTitleEl = document.getElementById("readMessageFrom");
    const readContentEl = document.getElementById("readMessageContent");
    const replyBtn = document.getElementById("replyBtn");

    // Fält i "Skicka meddelande"-modalen (för reply)
    const sendModalEl = document.getElementById("sendMessageModal");
    const receiverIdInput = document.getElementById("receiverId");
    const receiverSelectedEl = document.getElementById("receiverSelected");
    const sendSubjectInput = document.getElementById("sendSubject");

    // Håll info om senaste öppnade meddelande så "Svara" vet vem/ämne
    let lastMessage = { senderId: "", from: "", subject: "" };

    if (readModalEl) {
        readModalEl.addEventListener("show.bs.modal", (event) => {
            const triggerRow = event.relatedTarget; // <tr>
            if (!triggerRow) return;

            const from = triggerRow.getAttribute("data-from") || "";
            const subject = triggerRow.getAttribute("data-subject") || "";
            const senderId = triggerRow.getAttribute("data-sender-id") || "";

            const contentCell = triggerRow.querySelector(".message-content");
            const content = contentCell ? contentCell.textContent.trim() : "";

            lastMessage = { senderId, from, subject };

            if (readTitleEl) {
                // Visas som: "Namn – Ämne"
                const title = subject ? `${from} – ${subject}` : from;
                readTitleEl.textContent = title;
            }

            if (readContentEl) {
                readContentEl.textContent = content;
            }
        });
    }

    // --- Svara-knapp: fyller "Skicka meddelande"-modalen ---
    if (replyBtn && sendModalEl && window.bootstrap) {
        replyBtn.addEventListener("click", () => {
            // Stäng read-modal
            const readInstance = window.bootstrap.Modal.getInstance(readModalEl);
            if (readInstance) readInstance.hide();

            // Förifyll mottagare + ämne
            if (receiverIdInput && lastMessage.senderId) receiverIdInput.value = lastMessage.senderId;
            if (receiverSelectedEl) receiverSelectedEl.textContent = lastMessage.from ? `Till: ${lastMessage.from}` : "";

            if (sendSubjectInput) {
                const s = lastMessage.subject || "";
                sendSubjectInput.value = s.toLowerCase().startsWith("re:") ? s : `Re: ${s}`.trim();
            }

            // Öppna send-modal
            const sendInstance = window.bootstrap.Modal.getOrCreateInstance(sendModalEl);
            sendInstance.show();
        });
    }

    // --- Ta bort modal: fyller id + info ---
    const deleteModalEl = document.getElementById("deleteMessageModal");
    const deleteInfoEl = document.getElementById("deleteMessageInfo");
    const deleteIdInput = document.getElementById("deleteMessageId");

    if (deleteModalEl) {
        deleteModalEl.addEventListener("show.bs.modal", (event) => {
            const triggerBtn = event.relatedTarget; // delete-knappen
            if (!triggerBtn) return;

            const id = triggerBtn.getAttribute("data-id") || "";
            const from = triggerBtn.getAttribute("data-from") || "";
            const subject = triggerBtn.getAttribute("data-subject") || "";

            if (deleteIdInput) deleteIdInput.value = id;
            if (deleteInfoEl) deleteInfoEl.textContent = `${from} – ${subject}`.trim();
        });
    }
});


// ====== EditAccount: redirect efter lyckad sparning (3s) ======
document.addEventListener("DOMContentLoaded", () => {
    const popup = document.getElementById("saveSuccessPopup");
    if (!popup) return;

    const redirectUrl = popup.getAttribute("data-redirect-url");
    if (!redirectUrl) return;

    setTimeout(() => {
        window.location.href = redirectUrl;
    }, 3000);
});


// ====== AddProject: success popup + redirect ======
document.addEventListener("DOMContentLoaded", () => {
    const popupData = document.getElementById("popup-data");
    const popup = document.getElementById("successPopup");

    if (!popupData || !popup) return;

    const shouldShow = popupData.getAttribute("data-show") === "true";
    const redirectUrl = popupData.getAttribute("data-url");

    if (!shouldShow || !redirectUrl) return;

    // Visa popup
    popup.classList.remove("u-hidden");

    // Redirect efter 3 sekunder (känns rimligt och matchar din andra vy-stil)
    setTimeout(() => {
        window.location.href = redirectUrl;
    }, 3000);
});


// ====== AllProjects: "Gå med" -> öppna modal och fyll projektinfo ======
document.addEventListener("DOMContentLoaded", () => {
    const modalEl = document.getElementById("joinRoleModal");
    const projectTextEl = document.getElementById("modalProjectText");
    const projectIdInput = document.getElementById("modalProjectId");

    if (!modalEl || !window.bootstrap) return;

    document.querySelectorAll("[data-join-project-id]").forEach((btn) => {
        btn.addEventListener("click", () => {
            const projectId = btn.getAttribute("data-join-project-id");
            const projectName = btn.getAttribute("data-join-project-name") || "";

            if (projectTextEl) projectTextEl.textContent = projectName ? `Projekt: ${projectName}` : "";
            if (projectIdInput) projectIdInput.value = projectId || "";

            const modal = window.bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
        });
    });
});

