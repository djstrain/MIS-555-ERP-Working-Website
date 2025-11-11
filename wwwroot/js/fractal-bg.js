(function() {
    // Fractal background using iterative condition: |||z_{i+1}|^2 - |z_i|^2| < eps
    // We'll use a Julia-set-style iteration: z_{n+1} = z_n^2 + c
    // Color mapping: escape-time or convergence by the epsilon condition.

    const canvas = document.getElementById('fractal-canvas');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    let DPR = window.devicePixelRatio || 1;
    let width = 0, height = 0;

    // Simpler Mandelbrot parameters (faster and less math-heavy)
    const maxIter = 50; // lower iterations for speed (reduced per request)
    const bailout = 4; // squared magnitude bailout threshold
    const renderScale = 0.6; // render at 60% of device resolution then upscale

    // Palette: transition from warm to cool hues depending on iteration (warmer->cooler)
    function colorFromIndex(i) {
        const t = Math.max(0, Math.min(1, i / maxIter));
        const hue = 20 + t * 180; // 20 = warm orange, 200 = cool blue
        const light = 30 + Math.floor(t * 45);
        return hslToRgba(hue, 85, light, 255);
    }

    function hslToRgba(h, s, l, a) {
        s /= 100; l /= 100;
        const k = n => (n + h / 30) % 12;
        const a_ = s * Math.min(l, 1 - l);
        const f = n => l - a_ * Math.max(Math.min(k(n) - 3, 9 - k(n), 1), -1);
        return [Math.round(255 * f(0)), Math.round(255 * f(8)), Math.round(255 * f(4)), Math.round(a * 255)];
    }

    function resize() {
        DPR = window.devicePixelRatio || 1;
        width = Math.max(1, Math.floor(window.innerWidth));
        height = Math.max(1, Math.floor(window.innerHeight));
        // CSS size (visible)
        canvas.style.width = width + 'px';
        canvas.style.height = height + 'px';
        // actual pixel buffer size (device pixels)
        canvas.width = Math.floor(width * DPR);
        canvas.height = Math.floor(height * DPR);
        // reset transform so putImageData maps to full canvas
        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.imageSmoothingEnabled = false;
        // redraw immediately
        requestAnimationFrame(draw);
    }

    // Map pixel -> complex plane and render Mandelbrot once (low iteration, lower resolution)
    function draw() {
        const pw = canvas.width; // device pixels (full)
        const ph = canvas.height;
        const sw = Math.max(1, Math.floor(pw * renderScale)); // small buffer width
        const sh = Math.max(1, Math.floor(ph * renderScale));

        // Offscreen canvas to render at lower resolution
        const off = document.createElement('canvas');
        off.width = sw;
        off.height = sh;
        const octx = off.getContext('2d');
        const imageData = octx.createImageData(sw, sh);
        const data = imageData.data;

        // scale controls zoom level relative to full-size device pixels
        const scale = 3.0 / Math.min(pw, ph);
        const offsetX = -0.5; // center
        const offsetY = 0.0;

        const ratioX = pw / sw;
        const ratioY = ph / sh;

        for (let py = 0; py < sh; py++) {
            for (let px = 0; px < sw; px++) {
                // map small-pixel to full device pixel coordinates
                const bigX = px * ratioX;
                const bigY = py * ratioY;
                const x0 = (bigX - pw / 2) * scale + offsetX;
                const y0 = (bigY - ph / 2) * scale + offsetY;

                let x = 0.0, y = 0.0;
                let iteration = 0;
                while (x*x + y*y <= 4 && iteration < maxIter) {
                    const xtemp = x*x - y*y + x0;
                    y = 2*x*y + y0;
                    x = xtemp;
                    iteration++;
                }

                const rgba = colorFromIndex(iteration);
                const idx = (py * sw + px) * 4;
                data[idx] = rgba[0];
                data[idx + 1] = rgba[1];
                data[idx + 2] = rgba[2];
                data[idx + 3] = rgba[3];
            }
        }

        octx.putImageData(imageData, 0, 0);

        // scale up to the main canvas
        ctx.imageSmoothingEnabled = true;
        ctx.clearRect(0, 0, pw, ph);
        ctx.drawImage(off, 0, 0, pw, ph);

        // apply a CSS hue-rotate animation to the canvas for subtle movement
        canvas.style.transition = 'filter 6s linear';
        canvas.classList.add('fractal-animated');
    }

    // Initial setup/resizing
    window.addEventListener('resize', debounce(resize, 150));
    resize();

    // simple debounce
    function debounce(fn, wait) {
        let t = null;
        return function () {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, arguments), wait);
        };
    }
})();
