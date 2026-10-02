document.addEventListener('DOMContentLoaded', function () {

    // --- Color picker update ---
    const qrColor = document.getElementById('qrColor');
    const qrBgColor = document.getElementById('qrBgColor');

    function updateQRPreview() {
        const color = qrColor.value;
        const bgColor = qrBgColor.value;
        console.log('QR colors updated:', { color, bgColor });
    }

    qrColor?.addEventListener('input', updateQRPreview);
    qrBgColor?.addEventListener('input', updateQRPreview);


    // --- Logo upload preview ---
    const uploadArea = document.getElementById('uploadArea');
    const logoInput = document.getElementById('logoInput');
    const logoPreview = document.getElementById('logoPreview');

    uploadArea?.addEventListener('click', function () {
        logoInput.click();
    });

    logoInput?.addEventListener('change', function (e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (event) {
                logoPreview.style.display = 'block';
                logoPreview.querySelector('img').src = event.target.result;
            };
            reader.readAsDataURL(file);
        }
    });


    // --- Size selector ---
    const sizeOptions = document.querySelectorAll('.size-option');
    sizeOptions.forEach(option => {
        option.addEventListener('click', function () {
            sizeOptions.forEach(opt => opt.classList.remove('active'));
            this.classList.add('active');

            const size = this.dataset.size;
            document.getElementById('QRCodeSize').value = size;
            console.log('QR size selected:', size);
        });
    });


    // --- Copy link buttons ---
    const copyButtons = document.querySelectorAll('.link-action-btn:not(.qr)');
    copyButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault(); // جلوی submit یا refresh
            const originalIcon = this.innerHTML;

            // Copy لینک به clipboard
            const linkItem = this.closest('.link-item');
            const shortLink = linkItem.querySelector('.link-short').textContent;
            navigator.clipboard.writeText(shortLink.trim());

            // تغییر آیکون و رنگ
            this.innerHTML = '<i class="fas fa-check"></i>';
            this.style.color = 'var(--success)';

            setTimeout(() => {
                this.innerHTML = originalIcon;
                this.style.color = '';
            }, 2000);
        });
    });


    // --- Create QR from link ---
    const qrButtons = document.querySelectorAll('.link-action-btn.qr');
    qrButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const linkItem = this.closest('.link-item');
            const shortLink = linkItem.querySelector('.link-short').textContent;
            alert(`A QR code will be created for ${shortLink} ساخته خواهد شد`);
        });
    });


    // --- Download QR ---
    const downloadButtons = document.querySelectorAll('.qr-btn:not(.delete)');
    downloadButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            alert('QR code downloaded successfully');
        });
    });


    // --- Delete QR ---
    const deleteButtons = document.querySelectorAll('.qr-btn.delete');
    deleteButtons.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            if (confirm('Delete this QR code?')) {
                const card = this.closest('.qr-card');
                card.remove();
            }
        });
    });

    const select = document.getElementById("linkSelect");
    const input = select.querySelector(".select-input");
    const options = select.querySelectorAll(".select-option");
    input.addEventListener("click", () => {
        select.classList.toggle("active");
    });

    options.forEach(option => {
        option.addEventListener("click", () => {
            input.childNodes[0].nodeValue = option.innerText;
            select.classList.remove("active");
        });
    });

    document.addEventListener("click", (e) => {
        if (!select.contains(e.target)) {
            select.classList.remove("active");
        }
    });
    const select = document.getElementById('linkSelect');
    const input = select.querySelector('.search-input');
    const dropdown = select.querySelector('.select-dropdown');
    const options = Array.from(dropdown.querySelectorAll('.select-option'));

    input.addEventListener('focus', () => {
        select.classList.add('active');
        dropdown.style.display = 'block';
    });

    input.addEventListener('input', () => {
        const val = input.value.toLowerCase();
        options.forEach(opt => {
            const text = opt.textContent.toLowerCase();
            opt.style.display = text.includes(val) ? 'block' : 'none';
        });
    });

    options.forEach(opt => {
        opt.addEventListener('click', () => {
            input.value = opt.textContent;
            select.classList.remove('active');
            dropdown.style.display = 'none';
        });
    });

    document.addEventListener('click', e => {
        if (!select.contains(e.target)) {
            select.classList.remove('active');
            dropdown.style.display = 'none';
        }
    });
});
//document.addEventListener('DOMContentLoaded', function () {
//    const searchInput = document.getElementById('linkSearch');
//    const dropdown = document.getElementById('linkDropdown');

//    // آرایه لینک‌ها از داده‌ی سرور یا DOM
//    const links = Array.from(document.querySelectorAll('.link-item')).map(item => ({
//        short: item.querySelector('.link-short').innerText,
//        original: item.querySelector('.link-original').innerText
//    }));

//    searchInput.addEventListener('input', function () {
//        const query = this.value.trim().toLowerCase();
//        dropdown.innerHTML = ''; // پاک کردن پیشنهادهای قبلی

//        if (!query) {
//            dropdown.style.display = 'none';
//            return;
//        }

//        const matches = links.filter(link =>
//            link.short.toLowerCase().includes(query) || link.original.toLowerCase().includes(query)
//        ).slice(0, 10); // حداکثر ۱۰ مورد نمایش داده میشه

//        if (matches.length === 0) {
//            dropdown.style.display = 'none';
//            return;
//        }

//        matches.forEach(link => {
//            const div = document.createElement('div');
//            div.className = 'select-option';
//            div.innerText = link.short;
//            div.dataset.url = link.original;

//            div.addEventListener('click', () => {
//                searchInput.value = link.short;
//                dropdown.style.display = 'none';
//            });

//            dropdown.appendChild(div);
//        });

//        dropdown.style.display = 'block';
//    });

//    // بستن dropdown وقتی input blur میشه
//    searchInput.addEventListener('blur', () => {
//        setTimeout(() => dropdown.style.display = 'none', 200);
//    });
//});