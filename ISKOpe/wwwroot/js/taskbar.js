// Liquid Glass Bottom Task Bar Interactivity with Centered Search Expansion and Subtle Tilt

document.addEventListener('DOMContentLoaded', function () {
    // Pills in the group card
    const groupCard = document.querySelector('.taskbar-group-card');
    const homePill = document.querySelector('.home-pill');
    const aboutPill = document.querySelector('.about-pill');
    const profilePill = document.querySelector('.profile-pill');
    const pills = [homePill, aboutPill, profilePill];

    // Search and Add
    const searchBtn = document.querySelector('.search-btn');
    const searchInput = document.querySelector('.search-input');
    const searchContainer = document.querySelector('.taskbar-search');
    const addBtn = document.querySelector('.add-btn');
    const bottomTaskbar = document.querySelector('.bottom-taskbar');

    // Activate pill
    function activatePill(pill) {
        pills.forEach(p => p.classList.remove('active'));
        if (pill) pill.classList.add('active');
    }

    // Razor-compatible navigation paths
    function getNavPath(target) {
        switch (target) {
            case 'home': return '/Index';
            case 'about': return '/About';
            case 'profile': return '/Profile';
            case 'submit': return '/submit-item'; // assuming your Razor page is SubmitItem.cshtml
            default: return '#';
        }
    }

    // Navigation for pills
    if (homePill) {
        homePill.addEventListener('click', () => {
            activatePill(homePill);
            window.location.href = getNavPath('home');
        });
    }
    if (aboutPill) {
        aboutPill.addEventListener('click', () => {
            activatePill(aboutPill);
            window.location.href = getNavPath('about');
        });
    }
    if (profilePill) {
        profilePill.addEventListener('click', () => {
            activatePill(profilePill);
            window.location.href = getNavPath('profile');
        });
    }

    // Add post button (Razor version)
    if (addBtn) {
        addBtn.addEventListener('click', () => {
            window.location.href = getNavPath('submit');
        });
    }

    // Activate correct pill on page load
    const path = window.location.pathname.toLowerCase();
    if (path.includes('/about')) {
        activatePill(aboutPill);
    } else if (path.includes('/profile')) {
        activatePill(profilePill);
    } else {
        activatePill(homePill);
    }

    // Search bar toggle
    if (searchBtn && searchInput && searchContainer && bottomTaskbar) {
        searchBtn.addEventListener('click', () => {
            searchContainer.classList.toggle('expanded');
            bottomTaskbar.classList.toggle('search-expanded');
            if (searchContainer.classList.contains('expanded')) {
                setTimeout(() => searchInput.focus(), 120);
            } else {
                searchInput.value = '';
            }
        });

        searchInput.addEventListener('blur', () => {
            searchContainer.classList.remove('expanded');
            bottomTaskbar.classList.remove('search-expanded');
        });

        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                searchContainer.classList.remove('expanded');
                bottomTaskbar.classList.remove('search-expanded');
            }
        });
    }

    // Search input enhancements
    if (searchInput) {
        const searchLabel = document.querySelector('.search-icon-label');

        function updateSearchLabel() {
            const expanded = searchContainer.classList.contains('expanded');
            if (!expanded) {
                if (searchLabel) searchLabel.style.display = 'none';
                searchInput.style.display = 'none';
                searchInput.blur();
            } else {
                searchInput.style.display = '';
                if (searchInput.value.trim()) {
                    if (searchLabel) searchLabel.style.display = 'none';
                } else {
                    if (searchLabel) searchLabel.style.display = '';
                }
            }
        }

        updateSearchLabel();

        searchBtn.addEventListener('click', updateSearchLabel);
        searchInput.addEventListener('focus', updateSearchLabel);
        searchInput.addEventListener('input', updateSearchLabel);
        searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') {
                searchInput.value = '';
                if (searchLabel) searchLabel.style.display = '';
                updateSearchLabel();
            }
        });

        const observer = new MutationObserver(updateSearchLabel);
        observer.observe(searchContainer, { attributes: true, attributeFilter: ['class'] });

        document.addEventListener('mousedown', (e) => {
            if (
                searchContainer.classList.contains('expanded') &&
                !searchContainer.contains(e.target) &&
                !searchBtn.contains(e.target)
            ) {
                searchContainer.classList.remove('expanded');
                bottomTaskbar.classList.remove('search-expanded');
                updateSearchLabel();
            }
        });
    }

    // Subtle tilt function
    function addTiltEffect(element, maxTiltX = 2, maxTiltY = 3) {
        if (!element) return;
        element.addEventListener('mousemove', (e) => {
            const rect = element.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const px = x / rect.width;
            const py = y / rect.height;
            const tiltX = (py - 0.5) * maxTiltX * 2;
            const tiltY = (px - 0.5) * -maxTiltY * 2;
            element.style.setProperty('--tilt-x', `${tiltX}deg`);
            element.style.setProperty('--tilt-y', `${tiltY}deg`);
            element.classList.add('tilted');
        });
        element.addEventListener('mouseleave', () => {
            element.style.setProperty('--tilt-x', '0deg');
            element.style.setProperty('--tilt-y', '0deg');
            element.classList.remove('tilted');
        });
    }

    // Tilt + spotlight on hover
    function addTiltSpotlight(card, spotlightSize = 400) {
        if (!card) return;
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const px = x / rect.width;
            const py = y / rect.height;
            card.style.setProperty('--mouse-x', `${x}px`);
            card.style.setProperty('--mouse-y', `${y}px`);
            const tiltX = (py - 0.5) * 3;
            const tiltY = (px - 0.5) * -5;
            card.style.setProperty('--tilt-x', `${tiltX}deg`);
            card.style.setProperty('--tilt-y', `${tiltY}deg`);
            card.classList.add('tilted');
        });
        card.addEventListener('mouseleave', () => {
            card.style.setProperty('--mouse-x', '50%');
            card.style.setProperty('--mouse-y', '50%');
            card.style.setProperty('--tilt-x', '0deg');
            card.style.setProperty('--tilt-y', '0deg');
            card.classList.remove('tilted');
        });
    }

    // Apply tilt to group and add cards
    addTiltEffect(groupCard);
    addTiltEffect(document.querySelector('.taskbar-add-card'));

    // Apply tilt/spotlight to various UI elements
    document.querySelectorAll('.most-recent, .categories').forEach(card => addTiltSpotlight(card));
    document.querySelectorAll('.category-item').forEach(card => addTiltSpotlight(card, 200));
    document.querySelectorAll('.tilt-glow-btn').forEach(btn => addTiltSpotlight(btn, 180));
    addTiltSpotlight(document.querySelector('.about-glass-card'));
    addTiltSpotlight(document.querySelector('.profile-glass-card'));
    document.querySelectorAll('.category-glass-card, .items-grid').forEach(card => addTiltSpotlight(card, 320));
});
