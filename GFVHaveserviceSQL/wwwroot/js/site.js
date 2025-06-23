// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const recurCheckbox = document.getElementById('IsRecurring');
    const recurOptions = document.getElementById('recurrenceOptions');
    function toggleRecurrence() {
        if (recurCheckbox && recurOptions) {
            recurOptions.style.display = recurCheckbox.checked ? 'block' : 'none';
        }
    }
    if (recurCheckbox) {
        recurCheckbox.addEventListener('change', toggleRecurrence);
        toggleRecurrence();
    }

    const themeToggle = document.getElementById('theme-toggle');
    const storedTheme = localStorage.getItem('theme');
    if (storedTheme === 'dark') {
        document.body.classList.add('dark-mode');
    }
    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            document.body.classList.toggle('dark-mode');
            const theme = document.body.classList.contains('dark-mode') ? 'dark' : 'light';
            localStorage.setItem('theme', theme);
        });
    }
});
