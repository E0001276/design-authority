document.querySelectorAll("[data-column-toggle]").forEach((toggle) => {
    const column = toggle.dataset.columnToggle;
    const applyVisibility = () => {
        document.querySelectorAll(`[data-column="${column}"]`).forEach((cell) => {
            cell.hidden = !toggle.checked;
        });
    };
    toggle.addEventListener("change", applyVisibility);
    applyVisibility();
});

const tramiteSelect = document.querySelector("#TramiteId");
const substageSelect = document.querySelector("[data-substage-select]");
if (tramiteSelect && substageSelect) {
    const filterSubstages = () => {
        const tramiteId = tramiteSelect.value;
        let selectedIsAvailable = false;
        substageSelect.querySelectorAll("option[data-tramite-id]").forEach((option) => {
            const available = option.dataset.tramiteId === tramiteId;
            option.hidden = !available;
            option.disabled = !available;
            selectedIsAvailable ||= available && option.selected;
        });
        if (!selectedIsAvailable) substageSelect.value = "0";
    };
    tramiteSelect.addEventListener("change", filterSubstages);
    filterSubstages();
}
