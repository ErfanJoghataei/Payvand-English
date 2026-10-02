document.addEventListener('DOMContentLoaded', function () {
    // Back button functionality
    const backButton = document.getElementById('backButton');

    if (backButton) {
        backButton.addEventListener('click', function () {
            window.history.back();
        });
    }

    // Optional: Add animation class to shapes on scroll (not needed but nice)
    window.addEventListener('scroll', function () {
        // This is just for demonstration, no action needed
    });

    // Optional: Add random subtle movement to background shapes
    const shapes = document.querySelectorAll('.shape');
    let mouseX = 0, mouseY = 0;

    document.addEventListener('mousemove', function (e) {
        mouseX = (e.clientX / window.innerWidth - 0.5) * 20;
        mouseY = (e.clientY / window.innerHeight - 0.5) * 20;

        shapes.forEach((shape, index) => {
            const speed = (index + 1) * 0.5;
            shape.style.transform = `translate(${mouseX * speed}px, ${mouseY * speed}px)`;
        });
    });

    // Add hover effect to error code
    const errorCode = document.querySelector('.error-code');
    if (errorCode) {
        errorCode.addEventListener('mouseenter', function () {
            this.style.animation = 'none';
            this.offsetHeight; // Trigger reflow
            this.style.animation = 'pulse 2s infinite ease-in-out';
        });
    }
});