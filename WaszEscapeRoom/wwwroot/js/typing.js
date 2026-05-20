document.addEventListener("DOMContentLoaded", () => {
    const audio = new Audio("assets/typing.opus");
    audio.volume = 0.2;
    audio.playbackRate = 1.5;
    audio.loop = true;
    const removeAudioListeners = () => {
        window.removeEventListener("pointerdown", startAudio);
        window.removeEventListener("keydown", startAudio);
        window.removeEventListener("touchstart", startAudio);
    };
    const startAudio = () => {
        audio
            .play()
            .then(() => {
                removeAudioListeners();
            })
            .catch(() => {});
    };
    audio.play().catch(() => {
        window.addEventListener("pointerdown", startAudio);
        window.addEventListener("keydown", startAudio);
        window.addEventListener("touchstart", startAudio);
    });
    const elements = Array.from(document.querySelectorAll("[data-typing]"));
    const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
    const fullTexts = elements.map((element) => element.textContent.trim());

    for (const element of elements) {
        element.textContent = "";
    }
    const typeElement = async (element, fullText, speed = 35) => {
        for (const character of fullText) {
            element.textContent += character;
            await sleep(speed);
        }
    };
    (async () => {
        for (let index = 0; index < elements.length; index++) {
            await typeElement(elements[index], fullTexts[index]);
            audio.pause();
            await sleep(500);
            audio.play();
        }
        removeAudioListeners();
        audio.pause();
    })();
});