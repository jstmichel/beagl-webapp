/**
 * Beagl interop helpers for Blazor Server.
 */
window.beagl = {
    /**
     * Attempts to copy text to the clipboard.
     * Returns true on success, false when the browser blocks the operation.
     * @param {string} text - The text to copy.
     * @returns {Promise<boolean>}
     */
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(text);
                return true;
            }

            const textarea = document.createElement("textarea");
            textarea.value = text;
            textarea.style.position = "fixed";
            textarea.style.left = "-9999px";
            document.body.appendChild(textarea);
            textarea.select();
            const ok = document.execCommand("copy");
            document.body.removeChild(textarea);
            return ok;
        } catch {
            return false;
        }
    },

    /**
     * Selects all text inside an input element.
     * @param {HTMLInputElement} element - The input element.
     */
    selectAll: function (element) {
        if (element && element.select) {
            element.select();
        }
    }
};
