/**
 * Real Estate Management System — Modern Client-Side UI & Validation Engine
 * Author: Maitri437-sketch <maitriadroja437@gmail.com>
 */

document.addEventListener('DOMContentLoaded', function () {
    initPasswordStrength();
    initPasswordMatch();
    initImagePreviews();
    initAppointmentDateConstraints();
    initPriceRangeValidation();
    initQuickQuestionChips();
    initCharCounters();
    initBackToTop();
    initFormSubmitProtection();
});

/* ==========================================================================
   1. Real-time Password Strength Meter
   ========================================================================== */
function initPasswordStrength() {
    const pwdInputs = document.querySelectorAll('input[type="password"]#Password, input[type="password"]#NewPassword, [data-password-strength]');
    
    pwdInputs.forEach(input => {
        const container = input.closest('.mb-3') || input.parentElement;
        if (!container) return;

        // Check if meter already exists
        let meterWrapper = container.querySelector('.password-strength-container');
        if (!meterWrapper) {
            meterWrapper = document.createElement('div');
            meterWrapper.className = 'password-strength-container';
            meterWrapper.innerHTML = `
                <div class="password-strength-meter">
                    <div class="password-strength-bar"></div>
                </div>
                <div class="password-feedback-text">
                    <span class="strength-label text-muted">Password strength</span>
                    <span class="strength-score text-muted">Not entered</span>
                </div>
            `;
            container.appendChild(meterWrapper);
        }

        const bar = meterWrapper.querySelector('.password-strength-bar');
        const scoreLabel = meterWrapper.querySelector('.strength-score');

        input.addEventListener('input', function () {
            const val = input.value;
            if (!val) {
                bar.className = 'password-strength-bar';
                scoreLabel.textContent = 'Not entered';
                scoreLabel.className = 'strength-score text-muted';
                return;
            }

            let score = 0;
            if (val.length >= 6) score++;
            if (val.length >= 10) score++;
            if (/[a-z]/.test(val) && /[A-Z]/.test(val)) score++;
            if (/\d/.test(val)) score++;
            if (/[^A-Za-z0-9]/.test(val)) score++;

            bar.className = 'password-strength-bar';

            if (score <= 1) {
                bar.classList.add('strength-weak');
                scoreLabel.textContent = 'Weak (Needs more variety & length)';
                scoreLabel.className = 'strength-score text-danger';
            } else if (score === 2) {
                bar.classList.add('strength-fair');
                scoreLabel.textContent = 'Fair (Add uppercase or numbers)';
                scoreLabel.className = 'strength-score text-warning';
            } else if (score === 3 || score === 4) {
                bar.classList.add('strength-good');
                scoreLabel.textContent = 'Good (Secure password)';
                scoreLabel.className = 'strength-score text-primary';
            } else {
                bar.classList.add('strength-strong');
                scoreLabel.textContent = 'Very Strong (Excellent!)';
                scoreLabel.className = 'strength-score text-success';
            }
        });
    });
}

/* ==========================================================================
   2. Password Confirmation Match Validation
   ========================================================================== */
function initPasswordMatch() {
    const confirmInputs = document.querySelectorAll('#ConfirmPassword, #ConfirmNewPassword');

    confirmInputs.forEach(confirmInput => {
        const isProfile = confirmInput.id === 'ConfirmNewPassword';
        const mainInput = document.getElementById(isProfile ? 'NewPassword' : 'Password');
        if (!mainInput) return;

        function checkMatch() {
            if (!confirmInput.value && !mainInput.value) {
                confirmInput.classList.remove('is-valid', 'is-invalid');
                return;
            }
            if (confirmInput.value === mainInput.value && mainInput.value.length >= 6) {
                confirmInput.classList.remove('is-invalid');
                confirmInput.classList.add('is-valid');
            } else if (confirmInput.value.length > 0) {
                confirmInput.classList.remove('is-valid');
                confirmInput.classList.add('is-invalid');
            }
        }

        confirmInput.addEventListener('input', checkMatch);
        mainInput.addEventListener('input', checkMatch);
    });
}

/* ==========================================================================
   3. File Upload Live Validation & Instant Image Preview
   ========================================================================== */
function initImagePreviews() {
    const fileInputs = document.querySelectorAll('input[type="file"][accept*="image"], #ProfilePicture, #Images');

    fileInputs.forEach(fileInput => {
        const isMultiple = fileInput.hasAttribute('multiple');
        const maxFileSizeMB = isMultiple ? 10 : 5;
        const maxSizeBytes = maxFileSizeMB * 1024 * 1024;
        const allowedExtensions = ['jpg', 'jpeg', 'png', 'webp'];

        // Create or find preview container
        let previewBox = fileInput.parentElement.querySelector('.image-preview-wrapper');
        if (!previewBox) {
            previewBox = document.createElement('div');
            previewBox.className = 'image-preview-wrapper d-none';
            fileInput.parentElement.appendChild(previewBox);
        }

        // Create error message container
        let errorMsg = fileInput.parentElement.querySelector('.file-validation-msg');
        if (!errorMsg) {
            errorMsg = document.createElement('div');
            errorMsg.className = 'file-validation-msg small text-danger mt-1 d-none';
            fileInput.parentElement.appendChild(errorMsg);
        }

        fileInput.addEventListener('change', function () {
            previewBox.innerHTML = '';
            errorMsg.classList.add('d-none');
            errorMsg.textContent = '';
            fileInput.classList.remove('is-invalid', 'is-valid');

            const files = fileInput.files;
            if (!files || files.length === 0) {
                previewBox.classList.add('d-none');
                return;
            }

            let isValid = true;
            let errorMessage = '';

            for (let i = 0; i < files.length; i++) {
                const file = files[i];
                const ext = file.name.split('.').pop().toLowerCase();

                if (!allowedExtensions.includes(ext)) {
                    isValid = false;
                    errorMessage = `"${file.name}" has an unsupported format. Only JPG, PNG, and WEBP are allowed.`;
                    break;
                }

                if (file.size > maxSizeBytes) {
                    isValid = false;
                    errorMessage = `"${file.name}" exceeds the maximum limit of ${maxFileSizeMB}MB.`;
                    break;
                }
            }

            if (!isValid) {
                fileInput.value = '';
                fileInput.classList.add('is-invalid');
                errorMsg.textContent = errorMessage;
                errorMsg.classList.remove('d-none');
                previewBox.classList.add('d-none');
                return;
            }

            // Valid files -> render live image previews
            fileInput.classList.add('is-valid');
            previewBox.classList.remove('d-none');

            Array.from(files).forEach((file, index) => {
                const reader = new FileReader();
                reader.onload = function (e) {
                    const previewItem = document.createElement('div');
                    previewItem.className = 'image-preview-item';
                    previewItem.innerHTML = `
                        <img src="${e.target.result}" alt="Preview ${index + 1}" />
                        <span class="image-preview-badge">#${index + 1}</span>
                    `;
                    previewBox.appendChild(previewItem);
                };
                reader.readAsDataURL(file);
            });
        });
    });
}

/* ==========================================================================
   4. Appointment Date & Business Hours Validation
   ========================================================================== */
function initAppointmentDateConstraints() {
    const dateInput = document.querySelector('input[type="datetime-local"]#AppointmentDate, input[name="AppointmentDate"]');
    if (!dateInput) return;

    // Set minimum date to current date + 1 hour
    const now = new Date();
    now.setHours(now.getHours() + 1);
    now.setMinutes(0);
    const minIso = now.toISOString().slice(0, 16);
    dateInput.setAttribute('min', minIso);

    // Live validation on change
    dateInput.addEventListener('change', function () {
        const selected = new Date(dateInput.value);
        const day = selected.getDay(); // 0 = Sunday
        const hours = selected.getHours();

        dateInput.classList.remove('is-invalid', 'is-valid');

        let warningNotice = dateInput.parentElement.querySelector('.appt-time-warning');
        if (!warningNotice) {
            warningNotice = document.createElement('div');
            warningNotice.className = 'appt-time-warning small mt-1';
            dateInput.parentElement.appendChild(warningNotice);
        }

        if (day === 0) {
            dateInput.classList.add('is-invalid');
            warningNotice.className = 'appt-time-warning small text-danger mt-1';
            warningNotice.innerHTML = '<i class="bi bi-exclamation-circle me-1"></i> Our offices are closed on Sundays. Please select Monday through Saturday.';
        } else if (hours < 9 || hours >= 18) {
            dateInput.classList.add('is-invalid');
            warningNotice.className = 'appt-time-warning small text-danger mt-1';
            warningNotice.innerHTML = '<i class="bi bi-clock me-1"></i> Viewing hours are 9:00 AM to 6:00 PM. Please choose a time within business hours.';
        } else {
            dateInput.classList.add('is-valid');
            warningNotice.className = 'appt-time-warning small text-success mt-1';
            warningNotice.innerHTML = '<i class="bi bi-check2-circle me-1"></i> Selected tour time is during regular business hours.';
        }
    });
}

/* ==========================================================================
   5. Price Range Cross-Field Validation
   ========================================================================== */
function initPriceRangeValidation() {
    const minPriceInput = document.querySelector('input[name="MinPrice"], #MinPrice');
    const maxPriceInput = document.querySelector('input[name="MaxPrice"], #MaxPrice');

    if (!minPriceInput || !maxPriceInput) return;

    function validatePrices() {
        const minVal = parseFloat(minPriceInput.value);
        const maxVal = parseFloat(maxPriceInput.value);

        if (!isNaN(minVal) && !isNaN(maxVal) && minVal > maxVal) {
            minPriceInput.classList.add('is-invalid');
            maxPriceInput.classList.add('is-invalid');
            minPriceInput.title = 'Min Price cannot exceed Max Price';
        } else {
            minPriceInput.classList.remove('is-invalid');
            maxPriceInput.classList.remove('is-invalid');
            minPriceInput.removeAttribute('title');
        }
    }

    minPriceInput.addEventListener('input', validatePrices);
    maxPriceInput.addEventListener('input', validatePrices);
}

/* ==========================================================================
   6. Quick Preset Chips for Inquiries
   ========================================================================== */
function initQuickQuestionChips() {
    const chips = document.querySelectorAll('.quick-chip');
    const messageBox = document.querySelector('#Message, textarea[name="Message"]');
    const subjectBox = document.querySelector('#Subject, input[name="Subject"]');

    if (!chips.length || !messageBox) return;

    chips.forEach(chip => {
        chip.addEventListener('click', function () {
            const questionText = chip.getAttribute('data-question') || chip.textContent.trim();
            const subjectText = chip.getAttribute('data-subject');

            if (subjectText && subjectBox && !subjectBox.value) {
                subjectBox.value = subjectText;
                subjectBox.classList.add('is-valid');
            }

            if (messageBox.value.trim().length === 0) {
                messageBox.value = questionText;
            } else if (!messageBox.value.includes(questionText)) {
                messageBox.value += '\n' + questionText;
            }

            chip.classList.toggle('active');
            messageBox.dispatchEvent(new Event('input'));
            messageBox.focus();
        });
    });
}

/* ==========================================================================
   7. Live Character Counter for Textareas
   ========================================================================== */
function initCharCounters() {
    const textareas = document.querySelectorAll('textarea[maxlength], textarea[data-max-chars]');

    textareas.forEach(textarea => {
        const max = parseInt(textarea.getAttribute('maxlength') || textarea.getAttribute('data-max-chars') || '1000');
        
        let counter = textarea.parentElement.querySelector('.char-counter');
        if (!counter) {
            counter = document.createElement('div');
            counter.className = 'char-counter';
            textarea.parentElement.appendChild(counter);
        }

        function updateCount() {
            const currentLen = textarea.value.length;
            counter.textContent = `${currentLen} / ${max} characters`;

            if (currentLen >= max) {
                counter.className = 'char-counter text-danger fw-bold';
            } else if (currentLen >= max * 0.85) {
                counter.className = 'char-counter text-warning fw-semibold';
            } else {
                counter.className = 'char-counter text-muted';
            }
        }

        textarea.addEventListener('input', updateCount);
        updateCount();
    });
}

/* ==========================================================================
   8. Floating Back-to-Top Button
   ========================================================================== */
function initBackToTop() {
    let btn = document.querySelector('.back-to-top-btn');
    if (!btn) {
        btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'back-to-top-btn';
        btn.title = 'Back to top';
        btn.setAttribute('aria-label', 'Back to top');
        btn.innerHTML = '<i class="bi bi-chevron-up"></i>';
        document.body.appendChild(btn);
    }

    window.addEventListener('scroll', function () {
        if (window.scrollY > 300) {
            btn.classList.add('visible');
        } else {
            btn.classList.remove('visible');
        }
    });

    btn.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
}

/* ==========================================================================
   9. Form Submit Protection & Spinner State
   ========================================================================== */
function initFormSubmitProtection() {
    const forms = document.querySelectorAll('form:not([data-no-protect])');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            // Check HTML5 validity
            if (!form.checkValidity()) {
                return;
            }

            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                setTimeout(() => {
                    submitBtn.disabled = true;
                    submitBtn.dataset.originalHtml = submitBtn.innerHTML;
                    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span> Processing...';
                }, 10);
            }
        });
    });
}
