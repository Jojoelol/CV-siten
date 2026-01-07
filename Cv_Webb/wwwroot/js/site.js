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
                window.location.href = redirectUrl;
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
});