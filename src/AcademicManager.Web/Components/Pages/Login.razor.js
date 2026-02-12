export function init() {
    const body = document.querySelector("body");
    const modal = document.querySelector(".modal");
    const modalButton = document.querySelector(".modal-button");
    const closeButton = document.querySelector(".close-button");
    const scrollDown = document.querySelector(".scroll-down");
    let isOpened = false;

    const openModal = () => {
        modal.classList.add("is-open");
        body.style.overflow = "hidden";
        if (scrollDown) scrollDown.style.display = "none";
        isOpened = true;
    };

    const closeModal = () => {
        modal.classList.remove("is-open");
        body.style.overflow = "initial";
        isOpened = false;
        // Optional: restore scrollDown if needed, but original code didn't
    };

    const onScroll = () => {
        if (window.scrollY > window.innerHeight / 3 && !isOpened) {
            openModal();
        }
    };

    window.addEventListener("scroll", onScroll);

    if (modalButton) modalButton.addEventListener("click", openModal);
    if (closeButton) closeButton.addEventListener("click", closeModal);

    document.onkeydown = evt => {
        evt = evt || window.event;
        if (evt.keyCode === 27) closeModal();
    };

    // Return cleanup function
    return {
        dispose: () => {
            window.removeEventListener("scroll", onScroll);
            if (modalButton) modalButton.removeEventListener("click", openModal);
            if (closeButton) closeButton.removeEventListener("click", closeModal);
            document.onkeydown = null;
            body.style.overflow = "initial";
        }
    };
}
