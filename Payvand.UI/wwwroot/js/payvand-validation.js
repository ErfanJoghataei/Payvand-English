(function () {
    const phoneRegex = /^09\d{9}$/;
    const usernameRegex = /^[\u0600-\u06FFa-zA-Z0-9_ ]{3,30}$/;

    const messages = {
        required: 'This field is required.',
        phone: 'Enter an 11-digit mobile number starting with 09.',
        password: 'Password must be at least 8 characters.',
        confirm: 'Passwords do not match.',
        otp: 'Verification code must be six digits.',
        username: 'Username must be 3 to 30 characters and use letters, numbers, spaces, or underscores.',
        email: 'Enter a valid email address.',
        min2: 'Enter at least 2 characters.',
        min3: 'Enter at least 3 characters.',
        min10: 'Enter at least 10 characters.',
        url: 'Enter a valid URL.',
        qr: 'Choose a short link or enter a valid URL.',
        logo: 'Logo must be smaller than 1 MB.',
        invalid: 'Complete the form correctly.'
    };

    function ensureStyles() {
        if (document.getElementById('payvand-validation-style')) return;

        const style = document.createElement('style');
        style.id = 'payvand-validation-style';
        style.textContent = `
            .pv-global-loader {
                position: fixed;
                inset: 0;
                display: grid;
                place-items: center;
                padding: 24px;
                background: rgba(248, 249, 250, 0.78);
                backdrop-filter: blur(10px);
                -webkit-backdrop-filter: blur(10px);
                z-index: 9999;
                opacity: 0;
                pointer-events: none;
                transition: opacity 180ms ease;
            }
            .pv-global-loader.show {
                opacity: 1;
                pointer-events: auto;
            }
            .pv-loader-card {
                width: min(320px, 92vw);
                border: 1px solid rgba(67, 97, 238, 0.16);
                border-radius: 18px;
                padding: 24px 22px 20px;
                background:
                    radial-gradient(circle at 20% 20%, rgba(76, 201, 240, 0.18), transparent 34%),
                    radial-gradient(circle at 80% 0%, rgba(114, 9, 183, 0.14), transparent 32%),
                    #ffffff;
                box-shadow: 0 24px 70px rgba(31, 41, 55, 0.18);
                text-align: center;
                direction: rtl;
            }
            .pv-loader-title {
                margin: 14px 0 0;
                color: #212529;
                font-size: 0.95rem;
                font-weight: 800;
                line-height: 1.8;
            }
            .pv-loader-subtitle {
                margin: 4px 0 0;
                color: #6c757d;
                font-size: 0.78rem;
                line-height: 1.7;
            }
            .pv-link-loader {
                position: relative;
                width: 178px;
                height: 82px;
                margin: 0 auto;
            }
            .pv-link-loader::before,
            .pv-link-loader::after {
                content: '';
                position: absolute;
                top: 38px;
                width: 66px;
                height: 6px;
                border-radius: 999px;
                background: linear-gradient(90deg, rgba(67, 97, 238, 0.18), rgba(76, 201, 240, 0.7), rgba(114, 9, 183, 0.18));
                background-size: 200% 100%;
                animation: pvChainFlow 1.05s linear infinite;
            }
            .pv-link-loader::before {
                right: 30px;
                transform: rotate(-15deg);
            }
            .pv-link-loader::after {
                left: 30px;
                transform: rotate(15deg);
                animation-delay: 140ms;
            }
            .pv-loader-node {
                position: absolute;
                width: 42px;
                height: 42px;
                border-radius: 50%;
                background: #ffffff;
                border: 3px solid #4361ee;
                box-shadow: 0 8px 24px rgba(67, 97, 238, 0.22), inset 0 0 0 7px rgba(67, 97, 238, 0.08);
                animation: pvNodePulse 1.25s ease-in-out infinite;
            }
            .pv-loader-node::after {
                content: '';
                position: absolute;
                inset: 10px;
                border-radius: 50%;
                background: linear-gradient(135deg, #4361ee, #4cc9f0);
            }
            .pv-loader-node.one {
                right: 0;
                top: 22px;
            }
            .pv-loader-node.two {
                right: 68px;
                top: 0;
                border-color: #4cc9f0;
                animation-delay: 140ms;
            }
            .pv-loader-node.two::after {
                background: linear-gradient(135deg, #4cc9f0, #2ecc71);
            }
            .pv-loader-node.three {
                left: 0;
                top: 22px;
                border-color: #7209b7;
                animation-delay: 280ms;
            }
            .pv-loader-node.three::after {
                background: linear-gradient(135deg, #7209b7, #4361ee);
            }
            .pv-inline-loader-host {
                position: relative;
            }
            .pv-inline-loader {
                position: absolute;
                inset: 0;
                display: grid;
                place-items: center;
                border-radius: inherit;
                background: rgba(255, 255, 255, 0.72);
                backdrop-filter: blur(6px);
                -webkit-backdrop-filter: blur(6px);
                opacity: 0;
                pointer-events: none;
                transition: opacity 160ms ease;
                z-index: 5;
            }
            .pv-inline-loader.show {
                opacity: 1;
                pointer-events: auto;
            }
            .pv-inline-loader .pv-link-loader {
                transform: scale(0.64);
                transform-origin: center;
            }
            .pv-inline-loader-text {
                margin-top: -10px;
                color: #495057;
                font-size: 0.78rem;
                font-weight: 800;
            }
            .pv-submit-loading {
                position: relative;
                color: transparent !important;
                pointer-events: none;
            }
            .pv-submit-loading::after {
                content: '';
                position: absolute;
                width: 22px;
                height: 22px;
                inset: 0;
                margin: auto;
                border-radius: 50%;
                border: 3px solid rgba(255, 255, 255, 0.4);
                border-top-color: #ffffff;
                animation: pvSpin 700ms linear infinite;
            }
            @keyframes pvChainFlow {
                to {
                    background-position: 200% 0;
                }
            }
            @keyframes pvNodePulse {
                0%, 100% {
                    transform: translateY(0) scale(1);
                    box-shadow: 0 8px 24px rgba(67, 97, 238, 0.18), inset 0 0 0 7px rgba(67, 97, 238, 0.08);
                }
                50% {
                    transform: translateY(-5px) scale(1.06);
                    box-shadow: 0 14px 34px rgba(67, 97, 238, 0.28), inset 0 0 0 7px rgba(67, 97, 238, 0.12);
                }
            }
            @keyframes pvSpin {
                to {
                    transform: rotate(360deg);
                }
            }
            .pv-invalid {
                border-color: #e74c3c !important;
                box-shadow: 0 0 0 3px rgba(231, 76, 60, 0.12) !important;
            }
            .pv-valid {
                border-color: #2ecc71 !important;
            }
            .pv-field-error {
                display: block;
                margin-top: 6px;
                color: #e74c3c;
                font-size: 0.82rem;
                line-height: 1.7;
            }
            .pv-form-error {
                display: none;
                margin-bottom: 12px;
                padding: 10px 12px;
                border-radius: 10px;
                background: #fee2e2;
                color: #991b1b;
                font-size: 0.9rem;
                font-weight: 700;
            }
            .pv-form-error.show {
                display: block;
            }
            button.pv-disabled,
            .submit-button.pv-disabled,
            .save-btn.pv-disabled {
                opacity: 0.58;
                cursor: not-allowed !important;
                transform: none !important;
            }
        `;
        document.head.appendChild(style);
    }

    function loaderMarkup(message, compact) {
        return `
            <div class="pv-link-loader" aria-hidden="true">
                <span class="pv-loader-node one"></span>
                <span class="pv-loader-node two"></span>
                <span class="pv-loader-node three"></span>
            </div>
            ${compact ? `<div class="pv-inline-loader-text">${message}</div>` : `
                <p class="pv-loader-title">${message}</p>
                <p class="pv-loader-subtitle">Connecting securely...</p>
            `}
        `;
    }

    function ensureGlobalLoader() {
        ensureStyles();
        let loader = document.getElementById('pvGlobalLoader');
        if (loader) return loader;

        loader = document.createElement('div');
        loader.id = 'pvGlobalLoader';
        loader.className = 'pv-global-loader';
        loader.setAttribute('role', 'status');
        loader.setAttribute('aria-live', 'polite');
        loader.innerHTML = `<div class="pv-loader-card">${loaderMarkup('Please wait a moment', false)}</div>`;
        document.body.appendChild(loader);
        return loader;
    }

    function showGlobalLoader(message) {
        const loader = ensureGlobalLoader();
        const card = loader.querySelector('.pv-loader-card');
        if (card) card.innerHTML = loaderMarkup(message || 'Please wait a moment', false);
        requestAnimationFrame(() => loader.classList.add('show'));
    }

    function hideGlobalLoader() {
        document.getElementById('pvGlobalLoader')?.classList.remove('show');
    }

    function showInlineLoader(host, message) {
        if (!host) return null;
        ensureStyles();
        host.classList.add('pv-inline-loader-host');
        let loader = host.querySelector(':scope > .pv-inline-loader');
        if (!loader) {
            loader = document.createElement('div');
            loader.className = 'pv-inline-loader';
            loader.setAttribute('role', 'status');
            loader.setAttribute('aria-live', 'polite');
            host.appendChild(loader);
        }
        loader.innerHTML = loaderMarkup(message || 'Preparing...', true);
        requestAnimationFrame(() => loader.classList.add('show'));
        return loader;
    }

    function hideInlineLoader(host) {
        host?.querySelector(':scope > .pv-inline-loader')?.classList.remove('show');
    }

    window.PayvandLoader = {
        show: showGlobalLoader,
        hide: hideGlobalLoader,
        showInline: showInlineLoader,
        hideInline: hideInlineLoader
    };

    function fieldContainer(field) {
        return field.closest('.form-group') || field.parentElement;
    }
    function errorFor(field) {
        const container = fieldContainer(field);
        let error = container?.querySelector(`.pv-field-error[data-for="${field.name || field.id}"]`);
        if (!error) {
            error = document.createElement('span');
            error.className = 'pv-field-error';
            error.dataset.for = field.name || field.id;
            container?.appendChild(error);
        }
        return error;
    }

    function setFieldState(field, valid, message) {
        if (!field || field.type === 'hidden') return;

        field.classList.toggle('pv-invalid', !valid);
        field.classList.toggle('pv-valid', valid && Boolean(field.value?.trim?.()));

        const error = errorFor(field);
        error.textContent = valid ? '' : message;
    }

    function setGroupState(fields, valid, message) {
        fields.forEach(field => setFieldState(field, valid, message));
    }

    function isValidUrl(value) {
        if (!value) return false;

        value = value.trim();

        if (!value.startsWith("http://") &&
            !value.startsWith("https://")) {
            value = "https://" + value;
        }

        try {
            const url = new URL(value);
            return (url.protocol === "http:" || url.protocol === "https:") &&
                url.hostname.includes(".");
        } catch {
            return false;
        }
    }

    function isRequired(field) {
        return field.hasAttribute('required') || field.dataset.pvRequired === 'true';
    }

    function validateTextField(field, form) {
        const value = field.value.trim();

        if (isRequired(field) && !value) {
            return messages.required;
        }

        if (!value && !isRequired(field)) {
            return '';
        }

        // Email Validation (جایگزین شماره موبایل)
        if (field.dataset.pvPhone === 'true' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value))
            return messages.email;

        if (field.dataset.pvUrl === 'true' && !isValidUrl(value))
            return messages.url;

        if (field.dataset.pvUsername === 'true' && !usernameRegex.test(value))
            return messages.username;

        if (field.dataset.pvEmail === 'true' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value))
            return messages.email;

        if (field.dataset.pvMin && value.length < Number(field.dataset.pvMin))
            return messages[`min${field.dataset.pvMin}`] || `Enter at least ${field.dataset.pvMin} characters.`;

        if (field.dataset.pvPassword === 'true' && value.length < 8)
            return messages.password;

        if (field.dataset.pvConfirmFor) {
            const password = form.querySelector(field.dataset.pvConfirmFor);
            if (password && value !== password.value)
                return messages.confirm;
        }

        return '';
    }
    function validateOtpForm(form) {
        const hidden = form.querySelector('#CodeHidden,[name="Code"]');
        const otpInputs = Array.from(form.querySelectorAll('.otp-input'));
        if (!hidden || otpInputs.length === 0) return true;

        hidden.value = otpInputs.map(input => input.value.replace(/\D/g, '')).join('');
        const valid = /^\d{6}$/.test(hidden.value);
        setGroupState(otpInputs, valid, messages.otp);
        return valid;
    }

    function normalizeUrl(value) {
        value = (value || '').trim();
        if (!value) return value;
        if (!value.startsWith('http://') && !value.startsWith('https://')) {
            value = 'https://' + value;
        }
        return value;
    }

    function validateQrForm(form, showMessages) {
        if (form.dataset.pvQr !== 'true') return true;

        const select = form.querySelector('[name="Qrcdoe.ShortenedLinkId"]');
        const link = form.querySelector('[name="Qrcdoe.Link"]');
        const logo = form.querySelector('[name="Qrcdoe.Logo"]');

        const hasSelectedLink = Boolean(select?.value);
        const rawLinkValue = link?.value.trim() || '';
        const normalizedLinkValue = normalizeUrl(rawLinkValue);

        // Write back the normalized URL so client & server work with the same value
        if (link && rawLinkValue && rawLinkValue !== normalizedLinkValue) {
            link.value = normalizedLinkValue;
        }

        const hasValidDirectLink = Boolean(normalizedLinkValue) && isValidUrl(normalizedLinkValue);
        const valid = hasSelectedLink || hasValidDirectLink;
        const hasAnyLinkValue = hasSelectedLink || Boolean(rawLinkValue);
        const shouldShowState = Boolean(showMessages) || hasAnyLinkValue;

        if (select) setFieldState(select, shouldShowState ? valid : true, shouldShowState ? messages.qr : '');
        if (link) setFieldState(link, shouldShowState ? valid : true, shouldShowState ? messages.qr : '');

        if (logo?.files?.[0] && logo.files[0].size > 1024 * 1024) {
            setFieldState(logo, false, messages.logo);
            return false;
        }

        return valid;
    }

    function formError(form) {
        let error = form.querySelector(':scope > .pv-form-error');
        if (!error) {
            error = document.createElement('div');
            error.className = 'pv-form-error';
            form.prepend(error);
        }
        return error;
    }

    async function checkUnique(field, form) {
        const url = field.dataset.pvUniqueUrl;
        if (!url || !field.value.trim() || field.classList.contains('pv-invalid')) return true;

        const requestUrl = `${url}${url.includes('?') ? '&' : '?'}displayName=${encodeURIComponent(field.value.trim())}`;
        field.dataset.pvPending = 'true';

        try {
            const response = await fetch(requestUrl, { credentials: 'same-origin' });
            const result = await response.json();
            const available = Boolean(result.available);
            field.dataset.pvUniqueValid = available ? 'true' : 'false';
            setFieldState(field, available, result.message || 'This username is already taken.');
            return available;
        } catch {
            field.dataset.pvUniqueValid = 'true';
            return true;
        } finally {
            field.dataset.pvPending = 'false';
            updateSubmitState(form);
        }
    }

    function syncOtpInputs(form) {
        const hidden = form.querySelector('#CodeHidden,[name="Code"]');
        const inputs = Array.from(form.querySelectorAll('.otp-input'));
        if (!hidden || inputs.length === 0) return;
        hidden.value = inputs.map(input => input.value.replace(/\D/g, '')).join('');
    }

    function validateForm(form, showMessages) {
        syncOtpInputs(form);

        let valid = true;
        const fields = Array.from(form.querySelectorAll('input, textarea, select'))
            .filter(field => !field.disabled && field.type !== 'hidden' && field.type !== 'submit' && field.type !== 'button');

        fields.forEach(field => {
            if (field.classList.contains('otp-input')) return;
            if (field.type === 'file' && !field.dataset.pvRequired) return;
            if (field.type === 'color') return;

            const message = validateTextField(field, form);
            if (message) valid = false;
            if (showMessages || field.value.trim()) setFieldState(field, !message, message);
        });

        valid = validateOtpForm(form) && valid;
        valid = validateQrForm(form, showMessages) && valid;

        const uniqueFields = fields.filter(field => field.dataset.pvUniqueUrl);
        uniqueFields.forEach(field => {
            if (field.dataset.pvUniqueValid === 'false' || field.dataset.pvPending === 'true') {
                valid = false;
            }
        });

        return valid;
    }

    function updateSubmitState(form) {
        const submit = form.querySelector('button[type="submit"], input[type="submit"]');
        if (!submit || form.dataset.pvDisableSubmit !== 'true') return;

        const valid = validateForm(form, false);
        submit.disabled = !valid;
        submit.classList.toggle('pv-disabled', !valid);
    }

    function startSubmitLoading(form) {
        const submit = form.querySelector('button[type="submit"], input[type="submit"]');
        if (submit) {
            submit.dataset.pvOriginalDisabled = submit.disabled ? 'true' : 'false';
            submit.disabled = true;
            submit.classList.add('pv-submit-loading');
        }

        if (form.dataset.pvNoLoading !== 'true') {
            showGlobalLoader(form.dataset.pvLoadingText || 'Sending...');
        }
    }

    function stopSubmitLoading(form) {
        const submit = form.querySelector('button[type="submit"], input[type="submit"]');
        if (submit) {
            submit.disabled = submit.dataset.pvOriginalDisabled === 'true';
            submit.classList.remove('pv-submit-loading');
        }
        hideGlobalLoader();
    }

    function wireForm(form) {
        if (form.dataset.pvReady === 'true') return;
        form.dataset.pvReady = 'true';

        const fields = Array.from(form.querySelectorAll('input, textarea, select'));
        fields.forEach(field => {
            const eventName = field.tagName === 'SELECT' || field.type === 'file' ? 'change' : 'input';
            field.addEventListener(eventName, () => {
                if (field.classList.contains('otp-input')) {
                    field.value = field.value.replace(/\D/g, '').slice(0, 1);
                }

                const message = validateTextField(field, form);
                setFieldState(field, !message, message);

                if (field.dataset.pvUniqueUrl) {
                    clearTimeout(field._pvUniqueTimer);
                    field.dataset.pvUniqueValid = 'false';
                    field._pvUniqueTimer = setTimeout(() => checkUnique(field, form), 350);
                }

                validateOtpForm(form);
                validateQrForm(form, false);
                updateSubmitState(form);
            });

            field.addEventListener('blur', () => {
                const message = validateTextField(field, form);
                setFieldState(field, !message, message);
                if (field.dataset.pvUniqueUrl) checkUnique(field, form);
                updateSubmitState(form);
            });
        });

        form.addEventListener('submit', async event => {
            if (form.dataset.pvSubmitting === 'true') return;
            event.preventDefault();
            event.stopPropagation();

            const uniqueFields = fields.filter(field => field.dataset.pvUniqueUrl);
            for (const field of uniqueFields) {
                await checkUnique(field, form);
            }

            if (!validateForm(form, true)) {
                const error = formError(form);
                error.textContent = form.dataset.pvError || messages.invalid;
                error.classList.add('show');
                form.querySelector('.pv-invalid')?.focus();
                updateSubmitState(form);
                stopSubmitLoading(form);
                return false;
            }

            startSubmitLoading(form);
            form.dataset.pvSubmitting = 'true';
            HTMLFormElement.prototype.submit.call(form);
        });

        updateSubmitState(form);
    }

    function wirePlainFormLoading(form) {
        if (form.dataset.pvLoadingReady === 'true') return;
        form.dataset.pvLoadingReady = 'true';

        form.addEventListener('submit', event => {
            if (event.defaultPrevented || form.dataset.pvValidate === 'true') return;
            const method = (form.getAttribute('method') || 'get').toLowerCase();
            if (method === 'get' || form.dataset.pvNoLoading === 'true') return;

            if (typeof form.checkValidity === 'function' && !form.checkValidity()) return;
            startSubmitLoading(form);
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        ensureStyles();
        document.querySelectorAll('form[data-pv-validate="true"]').forEach(wireForm);
        document.querySelectorAll('form').forEach(wirePlainFormLoading);
        window.addEventListener('pageshow', () => hideGlobalLoader());
    });
})();
