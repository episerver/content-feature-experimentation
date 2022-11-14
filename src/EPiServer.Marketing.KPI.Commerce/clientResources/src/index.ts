import axios from "axios";

const commerceSettingsButton = document.getElementsByClassName("mt-commerce-settings-submit")[0];
commerceSettingsButton?.addEventListener("click", saveCommerceSettings);

const commerceSettingsCancel = document.getElementsByClassName("mt-commerce-settings-cancel")[0];
commerceSettingsCancel?.addEventListener("click", cancelCommerceSettings);

var PreferredMarketOrg = document.getElementById("PreferredMarket").value;

function cancelCommerceSettings(this: HTMLElement, ev: Event) {
    document.getElementById("PreferredMarket").value = PreferredMarketOrg;
}
function saveCommerceSettings(this: HTMLElement, ev: Event){
    let form = this.closest("form") as HTMLFormElement;
    const url = form.getAttribute("action") ?? "";
    const formData = new FormData(form);

    let data = {} as any;
    data["PreferredMarket"] = formData.get("PreferredMarket");

    let status = document.getElementById("mt-commerce-settings-status")!;
    toggleButtonLoadingIndicator(this, true);

    axios.post(url, data)
        .then((result) => {
            toggleButtonLoadingIndicator(this, false);
            status?.style.setProperty("display", "block");
            status.innerHTML = result.data;
            setTimeout(() => {
                status?.style.setProperty("display", "none");
            }, 2000);
            PreferredMarketOrg=data["PreferredMarket"];
        })
        .catch((error) => {
            toggleButtonLoadingIndicator(this, false);
            status?.style.setProperty("display", "block");
            status.innerHTML = error.data;
            setTimeout(() => {
                status?.style.setProperty("display", "none");
            }, 2000);
        });
}

const toggleButtonLoadingIndicator = ($el: HTMLElement, isLoading: boolean) => {
    let button = $el as HTMLButtonElement;
    if (!button) {
        return;
    }

    if (isLoading && !button.disabled) {
        button.classList.add("oui-button--loading");
        button.disabled = true;

        const indicator = document.createElement("div");
        indicator.setAttribute("data-test-selection", "button-spinner");
        indicator.setAttribute("class", "oui-spinner oui-spinner--tiny");
        button.insertBefore(indicator, button.lastChild);
    } else if (!isLoading && button.disabled) {
        button.classList.remove("oui-button--loading");
        button.disabled = false;
        button.firstChild?.remove();
    }
}