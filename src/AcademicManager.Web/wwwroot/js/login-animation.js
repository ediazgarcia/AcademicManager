export function init() {
    console.log("Login animation initialized");

    const body = document.querySelector("body");
    const modal = document.querySelector(".login-modal-overlay");
    const modalButton = document.querySelector(".modal-button");
    const closeButton = document.querySelector(".close-button");
    const scrollDown = document.querySelector(".scroll-down");

    let isOpened = false;

    // Helper to safely add event listener
    const addListener = (el, type, handler) => {
        if (el) {
            el.addEventListener(type, handler);
            console.log(`Added ${type} listener to`, el.className);
        } else {
            console.warn(`Element not found for ${type} listener`);
        }
    };

    const removeListener = (el, type, handler) => {
        if (el) el.removeEventListener(type, handler);
    };

    const openModal = () => {
        console.log("Opening modal...");
        if (modal) {
            modal.classList.add("is-open");
            console.log("Modal class added");
        }
        if (body) body.style.overflow = "hidden";
        if (scrollDown) scrollDown.style.display = "none";
        isOpened = true;
    };

    const closeModal = () => {
        console.log("Closing modal...");
        if (modal) modal.classList.remove("is-open");
        if (body) body.style.overflow = "initial";
        if (scrollDown) scrollDown.style.display = "flex";
        isOpened = false;
    };

    const onScroll = () => {
        const scrollY = window.scrollY || window.pageYOffset;
        const threshold = window.innerHeight / 3;

        console.log(`Scroll: ${scrollY}, Threshold: ${threshold}, IsOpened: ${isOpened}`);

        if (scrollY > threshold && !isOpened) {
            openModal();
        }
    };

    // Add event listeners
    window.addEventListener("scroll", onScroll);
    addListener(modalButton, "click", openModal);
    addListener(closeButton, "click", closeModal);

    const onKeyDown = (evt) => {
        evt = evt || window.event;
        if (evt.keyCode === 27 || evt.key === 'Escape') {
            closeModal();
        }
    };
    document.addEventListener("keydown", onKeyDown);

    // Initial check in case page is already scrolled
    setTimeout(() => {
        const scrollY = window.scrollY || window.pageYOffset;
        const threshold = window.innerHeight / 3;
        console.log(`Initial check - Scroll: ${scrollY}, Threshold: ${threshold}`);
        if (scrollY > threshold && !isOpened) {
            openModal();
        }
    }, 100);

    // Log initial state
    console.log("Modal element:", modal);
    console.log("Modal button:", modalButton);
    console.log("Close button:", closeButton);
    console.log("Scroll down:", scrollDown);

    // Return cleanup object for C# disposal
    return {
        dispose: () => {
            console.log("Login animation disposed");
            window.removeEventListener("scroll", onScroll);
            removeListener(modalButton, "click", openModal);
            removeListener(closeButton, "click", closeModal);
            document.removeEventListener("keydown", onKeyDown);
            if (body) body.style.overflow = "initial";
        }
    };
}
