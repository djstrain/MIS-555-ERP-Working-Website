(function(){
    // Animated moving pixels overlay on gradient
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    let animationId = null;

    function setupCanvas(){
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.position = 'fixed';
        canvas.style.top = '0';
        canvas.style.left = '0';
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        canvas.style.zIndex = '0';
        canvas.style.pointerEvents = 'none';
        
        // insert before body content but after gradient
        if(document.body.firstChild){
            document.body.insertBefore(canvas, document.body.firstChild.nextSibling);
        } else {
            document.body.appendChild(canvas);
        }
    }

    const pixelSize = 8;
    const pixelColors = ['#fff', '#ffff00', '#00ffff', '#00ff00', 'rgba(255,255,255,0.7)'];
    let pixels = [];
    let time = 0;

    function initPixels(){
        pixels = [];
        const pixelCount = Math.floor((canvas.width * canvas.height) / (pixelSize * pixelSize * 20));
        for(let i = 0; i < pixelCount; i++){
            pixels.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                vx: (Math.random() - 0.5) * 3,
                vy: (Math.random() - 0.5) * 3,
                color: pixelColors[Math.floor(Math.random() * pixelColors.length)],
                life: Math.random() * 300 + 100
            });
        }
    }

    function animate(){
        // don't clear canvas — let pixels fade over the gradient
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        time++;

        for(let i = 0; i < pixels.length; i++){
            let p = pixels[i];
            
            p.x += p.vx;
            p.y += p.vy;
            p.life--;

            // wrap around edges
            if(p.x < -pixelSize) p.x = canvas.width + pixelSize;
            if(p.x > canvas.width + pixelSize) p.x = -pixelSize;
            if(p.y < -pixelSize) p.y = canvas.height + pixelSize;
            if(p.y > canvas.height + pixelSize) p.y = -pixelSize;

            // fade out
            const alpha = p.life > 0 ? Math.min(1, p.life / 50) : 0;
            ctx.globalAlpha = alpha * 0.6;
            ctx.fillStyle = p.color;
            ctx.fillRect(Math.floor(p.x), Math.floor(p.y), pixelSize, pixelSize);

            // respawn when dead
            if(p.life <= 0){
                p.x = Math.random() * canvas.width;
                p.y = Math.random() * canvas.height;
                p.vx = (Math.random() - 0.5) * 3;
                p.vy = (Math.random() - 0.5) * 3;
                p.life = Math.random() * 300 + 100;
            }
        }

        ctx.globalAlpha = 1;
        animationId = requestAnimationFrame(animate);
    }

    function start(){
        setupCanvas();
        initPixels();
        animate();
    }

    if(document.readyState === 'loading'){
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }

    // handle window resize
    window.addEventListener('resize', function(){
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
    });

})();
