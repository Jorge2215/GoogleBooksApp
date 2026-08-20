// GoogleBooksApp - Client-side enhancements

(function() {
    'use strict';

    // ==================== Dark Mode Toggle ====================
    
    function initThemeToggle() {
        var themeToggle = document.getElementById('themeToggle');
        var themeIcon = document.getElementById('themeIcon');
        
        if (!themeToggle || !themeIcon) return;

        function getCurrentTheme() {
            return document.documentElement.getAttribute('data-theme') || 'light';
        }

        function updateThemeIcon(theme) {
            themeIcon.textContent = theme === 'dark' ? '☀️' : '🌙';
            themeToggle.setAttribute('aria-label', 
                theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'
            );
        }

        function setTheme(theme) {
            if (theme === 'dark') {
                document.documentElement.setAttribute('data-theme', 'dark');
            } else {
                document.documentElement.removeAttribute('data-theme');
            }
            localStorage.setItem('theme', theme);
            updateThemeIcon(theme);
        }

        // Initialize icon on page load
        updateThemeIcon(getCurrentTheme());

        // Toggle theme on button click
        themeToggle.addEventListener('click', function() {
            var currentTheme = getCurrentTheme();
            var newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            setTheme(newTheme);
        });
    }

    // ==================== Loading Spinner ====================
    
    function initLoadingSpinner() {
        var searchForm = document.getElementById('searchForm');
        var spinner = document.getElementById('search-spinner');
        
        if (!searchForm || !spinner) return;

        // Show spinner on form submit
        searchForm.addEventListener('submit', function() {
            spinner.classList.add('is-visible');
            spinner.setAttribute('aria-hidden', 'false');
        });

        // Show spinner on pagination link clicks
        var paginationLinks = document.querySelectorAll('.pagination__link:not(.pagination__link--disabled)');
        paginationLinks.forEach(function(link) {
            link.addEventListener('click', function(e) {
                // Only show spinner if it's an actual navigation (not a disabled link)
                if (!link.classList.contains('pagination__link--disabled')) {
                    spinner.classList.add('is-visible');
                    spinner.setAttribute('aria-hidden', 'false');
                }
            });
        });
    }

    // ==================== Initialize on DOM ready ====================
    
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', function() {
            initThemeToggle();
            initLoadingSpinner();
        });
    } else {
        initThemeToggle();
        initLoadingSpinner();
    }
})();
