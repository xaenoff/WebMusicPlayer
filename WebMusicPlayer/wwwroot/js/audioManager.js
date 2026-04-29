
window.audioManager = {
    init: function (dotNetHelper) {
        const audio = document.getElementById('global-audio');
        if (!audio) {
            console.error(" Audio element not found");
            return;
        }

        audio.volume = 0.10; 

        audio.addEventListener('timeupdate', () => {
            const time = isNaN(audio.currentTime) ? 0 : audio.currentTime;
            const dur = isNaN(audio.duration) ? 0 : audio.duration;
            const percent = (dur > 0) ? (time / dur) * 100 : 0;

            // Вызываем C# методы
            dotNetHelper.invokeMethodAsync('OnTimeUpdate', time, dur);
            dotNetHelper.invokeMethodAsync('UpdateProgressVisual', percent);
        });

        audio.addEventListener('ended', () => dotNetHelper.invokeMethodAsync('OnTrackEnded'));
        audio.addEventListener('play', () => dotNetHelper.invokeMethodAsync('OnPlayStateChange', true));
        audio.addEventListener('pause', () => dotNetHelper.invokeMethodAsync('OnPlayStateChange', false));
        audio.addEventListener('error', (e) => console.error("Audio error:", audio.error));
    },

    play: async function () {
        const audio = document.getElementById('global-audio');
        if (audio) await audio.play();
    },

    pause: function () {
        const audio = document.getElementById('global-audio');
        if (audio) audio.pause();
    },

    seek: function (time) {
        const audio = document.getElementById('global-audio');
        if (audio) audio.currentTime = time;
    },

    setVolume: function (value) {
        const audio = document.getElementById('global-audio');
        if (audio) audio.volume = value / 100;
    },

    setSource: function (src) {
        const audio = document.getElementById('global-audio');
        if (audio) {
            audio.pause();
            audio.src = src;
            audio.load();
        }
    },

    loadAndPlay: async function (src) {
        const audio = document.getElementById('global-audio');
        if (!audio) return;

        audio.pause();
        audio.volume = 0.10;
        audio.src = src;
        audio.load();

        // Ждём готовности или таймаут
        await new Promise(resolve => {
            const onReady = () => {
                audio.removeEventListener('canplaythrough', onReady);
                resolve();
            };
            audio.addEventListener('canplaythrough', onReady, { once: true });
            setTimeout(resolve, 1500);
        });

        try {
            await audio.play();
        } catch (err) {
            console.warn("Autoplay blocked:", err.message);
        }
    },

    setProgressValue: function (percent) {
        const slider = document.getElementById('progress-slider');
        if (slider) slider.value = percent;
    }
};