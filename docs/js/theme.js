(function () {
    'use strict';

    const cookieName = 'payvand-theme';
    const root = document.documentElement;

    function readThemeCookie() {
        const prefix = `${cookieName}=`;
        const item = document.cookie
            .split(';')
            .map(value => value.trim())
            .find(value => value.startsWith(prefix));

        if (!item) return null;

        const value = decodeURIComponent(item.slice(prefix.length));
        return value === 'dark' || value === 'light' ? value : null;
    }

    function preferredTheme() {
        return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        root.dataset.theme = theme;
        root.style.colorScheme = theme;

        const themeColor = document.querySelector('meta[name="theme-color"]');
        themeColor?.setAttribute('content', theme === 'dark' ? '#0b1020' : '#4361ee');

        document.querySelectorAll('[data-theme-toggle]').forEach(button => {
            const isDark = theme === 'dark';
            button.setAttribute('aria-pressed', String(isDark));
            button.setAttribute('aria-label', isDark ? 'Enable light mode' : 'Enable dark mode');
            button.setAttribute('title', isDark ? 'Light mode' : 'Dark mode');
            const label = button.querySelector('.theme-toggle-label');
            if (label) label.textContent = isDark ? 'Light' : 'Dark';
        });
    }

    function saveTheme(theme) {
        const secure = location.protocol === 'https:' ? '; Secure' : '';
        document.cookie = `${cookieName}=${encodeURIComponent(theme)}; Max-Age=31536000; Path=/; SameSite=Lax${secure}`;
    }

    applyTheme(readThemeCookie() || preferredTheme());

    function initializeThemeControls() {
        applyTheme(root.dataset.theme || preferredTheme());

        document.querySelectorAll('[data-theme-toggle]').forEach(button => {
            button.addEventListener('click', () => {
                const nextTheme = root.dataset.theme === 'dark' ? 'light' : 'dark';
                applyTheme(nextTheme);
                saveTheme(nextTheme);
            });
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeThemeControls, { once: true });
    } else {
        initializeThemeControls();
    }
})();
