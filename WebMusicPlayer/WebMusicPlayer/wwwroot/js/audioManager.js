
//window.audioManager = {
//    init: function (dotNetHelper) {
//        const audio = document.getElementById('global-audio');
//        const slider = document.getElementById('progress-slider');
//        if (!audio || !slider) return;

//        let isDragging = false;

//        slider.addEventListener('input', (e) => {
//            isDragging = true;
//            slider.value = e.target.value;
//        });

//        slider.addEventListener('change', (e) => {
//            isDragging = false;
//            const percent = parseFloat(e.target.value);
//            if (audio.duration > 0 && !isNaN(audio.duration)) {
//                this.seek((percent / 100) * audio.duration);
//            }
//        });

//        audio.addEventListener('timeupdate', () => {
//            if (isDragging) return;
//            if (!audio.duration || isNaN(audio.duration)) return;

//            const percent = (audio.currentTime / audio.duration) * 100;
//            if (Math.abs(parseFloat(slider.value) - percent) > 0.5) {
//                slider.value = percent.toFixed(2);
//            }

//            dotNetHelper.invokeMethodAsync('OnTimeUpdate', audio.currentTime, audio.duration);
//        });

//        audio.addEventListener('ended', () => dotNetHelper.invokeMethodAsync('OnTrackEnded'));
//        audio.addEventListener('play', () => dotNetHelper.invokeMethodAsync('OnPlayStateChange', true));
//        audio.addEventListener('pause', () => dotNetHelper.invokeMethodAsync('OnPlayStateChange', false));
//        audio.addEventListener('error', (e) => console.error("Audio error:", audio.error));
//    },

//    play: async function () {
//        const audio = document.getElementById('global-audio');
//        if (audio) await audio.play();
//    },

//    pause: function () {
//        const audio = document.getElementById('global-audio');
//        if (audio) audio.pause();
//    },

//    seek: function (time) {
//        const audio = document.getElementById('global-audio');
//        if (!audio) return;

//        if (audio.readyState < 2) {
//            audio.addEventListener('loadedmetadata', () => {
//                audio.currentTime = Math.min(time, audio.duration);
//            }, { once: true });
//            return;
//        }

//        audio.currentTime = Math.min(time, audio.duration);
//    },

//    setVolume: function (value) {
//        const audio = document.getElementById('global-audio');
//        if (audio) audio.volume = value / 100;
//    },

//    setSource: function (src) {
//        const audio = document.getElementById('global-audio');
//        if (audio) {
//            audio.pause();
//            audio.src = src;
//            audio.load();
//        }
//    },

//    loadAndPlay: async function (src) {
//        const audio = document.getElementById('global-audio');
//        if (!audio) return;

//        audio.pause();
//        audio.volume = 0.10;
//        audio.src = src;
//        audio.load();

//        // Ждём готовности или таймаут
//        await new Promise(resolve => {
//            const onReady = () => {
//                audio.removeEventListener('canplaythrough', onReady);
//                resolve();
//            };
//            audio.addEventListener('canplaythrough', onReady, { once: true });
//            setTimeout(resolve, 1500);
//        });

//        try {
//            await audio.play();
//        } catch (err) {
//            console.warn("Autoplay blocked:", err.message);
//        }
//    },

//    setProgressValue: function (percent) {
//        const slider = document.getElementById('progress-slider');
//        if (slider) slider.value = percent;
//    }
//};

window.audioManager = {
    dotNetHelper: null,

    init: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper; // 🔹 Сохраняем ссылку на C# компонент

        const audio = document.getElementById('global-audio');
        const progressSlider = document.getElementById('progress-slider');
        const volumeSlider = document.getElementById('volume-slider');

        if (!audio) {
            console.error("❌ Audio element #global-audio not found");
            return;
        }
        if (!progressSlider) {
            console.error("❌ Progress slider #progress-slider not found");
        }
        if (!volumeSlider) {
            console.error("❌ Volume slider #volume-slider not found");
        }

        // 🔹 Инициализация громкости
        if (volumeSlider) {
            volumeSlider.addEventListener('input', (e) => {
                const vol = parseInt(e.target.value);
                audio.volume = vol / 100;
            });
        }

        // 🔹 Обработка перемотки (только progress-slider)
        if (progressSlider) {
            let isDragging = false;

            progressSlider.addEventListener('input', (e) => {
                isDragging = true;
                progressSlider.value = e.target.value;
            });

            progressSlider.addEventListener('change', (e) => {
                isDragging = false;
                const percent = parseFloat(e.target.value);
                if (audio.duration > 0 && !isNaN(audio.duration)) {
                    this.seek((percent / 100) * audio.duration);
                }
            });
        }

        // 🔹 Обновление прогресса из audio -> JS -> Blazor
        audio.addEventListener('timeupdate', () => {
            if (!progressSlider) return;

            // Не обновляем ползунок, если пользователь его тащит
            const isSliderDragging = progressSlider.matches(':active');
            if (!isSliderDragging && audio.duration > 0) {
                const percent = (audio.currentTime / audio.duration) * 100;
                progressSlider.value = percent.toFixed(2);
            }

            // 🔹 Отправляем время в C# для обновления таймера
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnTimeUpdateFromJs', audio.currentTime, audio.duration)
                    .catch(err => console.warn("JS->C# timeupdate failed:", err));
            }
        });

        // 🔹 События состояния для Blazor
        audio.addEventListener('play', () => {
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnPlayStateChange', true)
                    .catch(err => console.warn("JS->C# play failed:", err));
            }
        });

        audio.addEventListener('pause', () => {
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnPlayStateChange', false)
                    .catch(err => console.warn("JS->C# pause failed:", err));
            }
        });

        audio.addEventListener('ended', () => {
            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('OnTrackEnded')
                    .catch(err => console.warn("JS->C# ended failed:", err));
            }
        });

        audio.addEventListener('error', (e) => {
            console.error("🎵 Audio error:", audio.error);
        });
    },

    play: async function () {
        const audio = document.getElementById('global-audio');
        if (audio) {
            try {
                await audio.play();
            } catch (err) {
                console.warn("Autoplay blocked:", err.message);
            }
        }
    },

    pause: function () {
        const audio = document.getElementById('global-audio');
        if (audio) audio.pause();
    },

    seek: function (time) {
        const audio = document.getElementById('global-audio');
        if (!audio) return;

        if (audio.readyState < 2) {
            audio.addEventListener('loadedmetadata', () => {
                audio.currentTime = Math.min(time, audio.duration);
            }, { once: true });
            return;
        }
        audio.currentTime = Math.min(time, audio.duration);
    },

    setVolume: function (value) {
        const audio = document.getElementById('global-audio');
        const slider = document.getElementById('volume-slider');
        if (audio) audio.volume = value / 100;
        if (slider) slider.value = value;
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
    }
};