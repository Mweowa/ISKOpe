document.addEventListener('DOMContentLoaded', function () {
    // --- Profile Image Upload Preview ---
    const profileUploadInput = document.getElementById('profile-upload');
    const profilePictureContainer = document.querySelector('.profile-picture-container');

    if (profileUploadInput && profilePictureContainer) {
        profileUploadInput.addEventListener('change', (event) => {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    profilePictureContainer.style.backgroundImage = `url('${e.target.result}')`;
                    profilePictureContainer.style.backgroundSize = 'cover';
                    profilePictureContainer.style.backgroundPosition = 'center';
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // --- Submit Item Image Upload Preview ---
    const itemUploadInput = document.getElementById('item-image');
    const itemImageContainer = document.querySelector('.image-upload');

    if (itemUploadInput && itemImageContainer) {
        itemUploadInput.addEventListener('change', (event) => {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    itemImageContainer.style.backgroundImage = `url('${e.target.result}')`;
                    itemImageContainer.style.backgroundSize = 'cover';
                    itemImageContainer.style.backgroundPosition = 'center';
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // --- Student Login Validation ---
    const loginForm = document.getElementById('student-login-form');
    if (loginForm) {
        loginForm.addEventListener('submit', function (event) {
            event.preventDefault();
            const studentNumber = document.getElementById('student-number').value.trim();
            const password = document.getElementById('student-password').value;
            if (studentNumber === '2022-08302-MN-0' && password === 'Emmanuelfaigal') {
                window.location.href = '/Index';
            } else {
                alert('Invalid student number or password.');
            }
        });
    }

    // --- Call toggleFields if lost/found exist ---
    if (document.getElementById('lost') || document.getElementById('found')) {
        toggleFields();

        // Re-bind radio button toggle
        const lostRadio = document.getElementById('lost');
        const foundRadio = document.getElementById('found');
        if (lostRadio) lostRadio.addEventListener('change', toggleFields);
        if (foundRadio) foundRadio.addEventListener('change', toggleFields);
    }
});

// --- Show/hide Lost/Found Fields ---
function toggleFields() {
    const isLost = document.getElementById("lost")?.checked ?? false;

    const showIf = (id, show) => {
        const el = document.getElementById(id);
        if (el) {
            el.style.display = 'block';
            el.style.visibility = show ? 'visible' : 'hidden';
            el.style.height = show ? 'auto' : '0';
        }
    };

    showIf('lost-location-label', isLost);
    showIf('lost-location', isLost);

    showIf('found-location-label', !isLost);
    showIf('found-location', !isLost);
    showIf('pickup-location-label', !isLost);
    showIf('pickup-location', !isLost);

    showIf('found-time-section', isLost);
}

// --- Logout Confirmation Modal ---
function showLogoutPopup() {
    const popup = document.getElementById("logoutPopup");
    if (popup) popup.style.display = "flex";
}

function closePopup() {
    const popup = document.getElementById("logoutPopup");
    if (popup) popup.style.display = "none";
}

function logout() {
    window.location.href = '/Login';
}

// --- Profile Image Preview ---
function previewImage(event) {
    const input = event.target;
    const preview = document.getElementById('profile-preview');
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            if (preview) {
                preview.src = e.target.result;
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}
