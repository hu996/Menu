(() => {
    const body = document.body;
    const sidebar = document.querySelector('[data-sidebar-toggle]')?.closest('.admin-layout')?.querySelector('.sidebar');
    const storageKey = 'restaurant-menu-platform.sidebar-collapsed';
    const mobileQuery = window.matchMedia('(max-width: 1079px)');
    let drawerReturnFocus = null;

    const iconPaths = {
        activity: '<path d="M3 12h4l2-7 4 14 2-7h6"/>',
        'book-open': '<path d="M3 5.5A2.5 2.5 0 0 1 5.5 3H11v17H5.5A2.5 2.5 0 0 0 3 22V5.5Z"/><path d="M21 5.5A2.5 2.5 0 0 0 18.5 3H13v17h5.5A2.5 2.5 0 0 1 21 22V5.5Z"/>',
        building: '<path d="M4 21V5l8-3 8 3v16M8 9h1M15 9h1M8 13h1M15 13h1M8 17h1M15 17h1M11 21v-4h2v4"/>',
        briefcase: '<rect x="3" y="7" width="18" height="13" rx="2"/><path d="M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2M3 12h18M10 12v3h4v-3"/>',
        'chevron-left': '<path d="m15 18-6-6 6-6"/>',
        clipboard: '<rect x="5" y="4" width="14" height="17" rx="2"/><path d="M9 4.5V3h6v1.5M8 9h8M8 13h8M8 17h5"/>',
        'credit-card': '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="M3 10h18M7 15h3"/>',
        globe: '<circle cx="12" cy="12" r="9"/><path d="M3 12h18M12 3a14 14 0 0 1 0 18M12 3a14 14 0 0 0 0 18"/>',
        grid: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><rect x="14" y="14" width="7" height="7" rx="1"/>',
        history: '<path d="M3 12a9 9 0 1 0 3-6.7"/><path d="M3 4v5h5M12 7v5l3 2"/>',
        home: '<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10M9 20v-6h6v6"/>',
        image: '<rect x="3" y="4" width="18" height="16" rx="2"/><circle cx="8.5" cy="9" r="1.5"/><path d="m4 17 5-5 3 3 2-2 6 6"/>',
        layers: '<path d="m12 3 9 5-9 5-9-5 9-5Z"/><path d="m3 12 9 5 9-5M3 16l9 5 9-5"/>',
        leaf: '<path d="M20 4C11 4 5 8 5 15c0 3 2 5 5 5 7 0 10-6 10-16Z"/><path d="M4 20c3-4 6-6 11-9"/>',
        list: '<path d="M8 6h13M8 12h13M8 18h13"/><path d="M3 6h.01M3 12h.01M3 18h.01"/>',
        lock: '<rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',
        'log-out': '<path d="M10 17l5-5-5-5M15 12H3M21 19V5a2 2 0 0 0-2-2h-5"/>',
        menu: '<path d="M4 7h16M4 12h16M4 17h16"/>',
        'more-horizontal': '<circle cx="5" cy="12" r="1" fill="currentColor" stroke="none"/><circle cx="12" cy="12" r="1" fill="currentColor" stroke="none"/><circle cx="19" cy="12" r="1" fill="currentColor" stroke="none"/>',
        'map-pin': '<path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z"/><circle cx="12" cy="10" r="2.5"/>',
        'qr-code': '<rect x="4" y="4" width="6" height="6"/><rect x="14" y="4" width="6" height="6"/><rect x="4" y="14" width="6" height="6"/><path d="M14 14h3v3h-3zM17 17h3v3h-3zM14 20h3"/>',
        shield: '<path d="M12 3 20 6v5c0 5-3.3 8.4-8 10-4.7-1.6-8-5-8-10V6l8-3Z"/><path d="m9 12 2 2 4-4"/>',
        sliders: '<path d="M4 6h16M4 12h16M4 18h16"/><circle cx="8" cy="6" r="2"/><circle cx="15" cy="12" r="2"/><circle cx="10" cy="18" r="2"/>',
        tag: '<path d="M20 13 13 20l-9-9V4h7l9 9Z"/><circle cx="8" cy="8" r="1"/>',
        users: '<path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8ZM22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/>'
    };

    const renderIcon = icon => {
        const path = iconPaths[icon.dataset.icon];
        if (!path) return;
        icon.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${path}</svg>`;
    };
    document.querySelectorAll('[data-icon]').forEach(renderIcon);

    document.querySelectorAll('img.product-card-image').forEach(image => image.addEventListener('error', () => {
        if (image.dataset.fallbackHandled) return;
        image.dataset.fallbackHandled = 'true';
        const fallback = document.createElement('div');
        fallback.className = 'product-card-image product-card-placeholder';
        fallback.innerHTML = '<span class="placeholder-icon" data-icon="image" aria-hidden="true"></span><strong>No image yet</strong><small>Open editor to add</small>';
        image.replaceWith(fallback);
        renderIcon(fallback.querySelector('[data-icon]'));
    }));

    document.querySelectorAll('.upload-dropzone').forEach(zone => {
        const input = zone.querySelector('input[type="file"]');
        if (!input) return;
        ['dragenter', 'dragover'].forEach(eventName => zone.addEventListener(eventName, event => {
            event.preventDefault();
            zone.classList.add('is-dragging');
        }));
        ['dragleave', 'drop'].forEach(eventName => zone.addEventListener(eventName, event => {
            event.preventDefault();
            zone.classList.remove('is-dragging');
        }));
        zone.addEventListener('drop', event => {
            if (!event.dataTransfer?.files?.length) return;
            try {
                input.files = event.dataTransfer.files;
                input.dispatchEvent(new Event('change', { bubbles: true }));
            } catch { /* browsers that protect the file input remain click-to-select */ }
        });
    });

    const readCollapsed = () => {
        try { return window.localStorage.getItem(storageKey) === 'true'; } catch { return false; }
    };

    const writeCollapsed = value => {
        try { window.localStorage.setItem(storageKey, String(value)); } catch { /* private browsing */ }
    };

    const syncSidebar = () => {
        if (!sidebar || !body.classList.contains('admin-shell')) return;
        const mobileOpen = body.classList.contains('sidebar-mobile-open');
        if (mobileQuery.matches) {
            body.classList.remove('sidebar-collapsed');
            sidebar.classList.toggle('is-open', mobileOpen);
            body.classList.toggle('sidebar-mobile-open', mobileOpen);
        } else {
            body.classList.remove('sidebar-mobile-open');
            sidebar.classList.remove('is-open');
            body.classList.toggle('sidebar-collapsed', readCollapsed());
        }
        const drawerHidden = mobileQuery.matches && !mobileOpen;
        sidebar.toggleAttribute('inert', drawerHidden);
        sidebar.setAttribute('aria-hidden', String(drawerHidden));
        document.querySelectorAll('[data-sidebar-toggle]').forEach(button => {
            button.setAttribute('aria-expanded', String(mobileQuery.matches ? mobileOpen : !body.classList.contains('sidebar-collapsed')));
        });
    };

    if (sidebar) {
        const drawerFocusables = () => [...sidebar.querySelectorAll('a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])')]
            .filter(element => !element.hidden && element.getAttribute('aria-hidden') !== 'true');
        const focusDrawer = () => {
            const target = drawerFocusables()[0];
            target?.focus({ preventScroll: true });
        };
        const restoreDrawerFocus = () => {
            const target = drawerReturnFocus;
            drawerReturnFocus = null;
            target?.focus?.({ preventScroll: true });
        };
        syncSidebar();
        document.querySelectorAll('[data-sidebar-toggle]').forEach(button => button.addEventListener('click', () => {
            if (mobileQuery.matches) {
                const opening = !body.classList.contains('sidebar-mobile-open');
                if (opening) drawerReturnFocus = document.activeElement;
                body.classList.toggle('sidebar-mobile-open');
                syncSidebar();
                if (opening) window.requestAnimationFrame(focusDrawer);
                else restoreDrawerFocus();
            } else {
                writeCollapsed(!body.classList.contains('sidebar-collapsed'));
                syncSidebar();
            }
        }));
        document.querySelectorAll('[data-sidebar-close]').forEach(button => button.addEventListener('click', () => {
            body.classList.remove('sidebar-mobile-open');
            syncSidebar();
            restoreDrawerFocus();
        }));
        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && body.classList.contains('sidebar-mobile-open')) {
                body.classList.remove('sidebar-mobile-open');
                syncSidebar();
                restoreDrawerFocus();
                return;
            }
            if (event.key !== 'Tab' || !mobileQuery.matches || !body.classList.contains('sidebar-mobile-open')) return;
            const focusables = drawerFocusables();
            if (!focusables.length) return;
            const first = focusables[0];
            const last = focusables[focusables.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        });
        mobileQuery.addEventListener?.('change', syncSidebar);
    }

    const markCurrentNavigation = () => {
        const current = new URL(window.location.href);
        document.querySelectorAll('[data-nav-link]').forEach(link => {
            const target = new URL(link.href, window.location.origin);
            const samePath = target.pathname === current.pathname;
            const sameHash = target.hash ? target.hash === current.hash : !current.hash;
            const active = samePath && sameHash;
            link.classList.toggle('is-active', active);
            if (active) link.setAttribute('aria-current', 'page');
            else link.removeAttribute('aria-current');
        });
    };
    markCurrentNavigation();
    window.addEventListener('hashchange', markCurrentNavigation);

    const isArabic = document.documentElement.lang?.toLowerCase().startsWith('ar');
    const copy = {
        notice: isArabic ? '\u0625\u0634\u0639\u0627\u0631' : 'Notice',
        success: isArabic ? '\u062a\u0645\u062a \u0627\u0644\u0639\u0645\u0644\u064a\u0629' : 'Completed',
        error: isArabic ? '\u062a\u0639\u0630\u0631 \u062a\u0646\u0641\u064a\u0630 \u0627\u0644\u0639\u0645\u0644\u064a\u0629' : 'Action failed',
        required: isArabic ? '\u0647\u0630\u0627 \u0627\u0644\u062d\u0642\u0644 \u0645\u0637\u0644\u0648\u0628.' : 'This field is required.',
        invalid: isArabic ? '\u0623\u062f\u062e\u0644 \u0642\u064a\u0645\u0629 \u0635\u062d\u064a\u062d\u0629.' : 'Enter a valid value.',
        review: isArabic ? '\u064a\u0631\u062c\u0649 \u0645\u0631\u0627\u062c\u0639\u0629 \u0627\u0644\u062d\u0642\u0648\u0644 \u0627\u0644\u0645\u0644\u0648\u0646\u0629.' : 'Review the highlighted fields.',
        selection: isArabic ? '\u0627\u062e\u062a\u0631 \u0639\u0646\u0635\u0631\u064b\u0627 \u0648\u0627\u062d\u062f\u064b\u0627 \u0639\u0644\u0649 \u0627\u0644\u0623\u0642\u0644.' : 'Select at least one item.',
        confirm: isArabic ? '\u062a\u0623\u0643\u064a\u062f' : 'Confirm',
        cancel: isArabic ? '\u0625\u0644\u063a\u0627\u0621' : 'Cancel',
        close: isArabic ? '\u0625\u063a\u0644\u0627\u0642' : 'Close',
        loading: isArabic ? '\u062c\u0627\u0631\u064d \u0627\u0644\u062a\u0646\u0641\u064a\u0630...' : 'Working…'
    };

    const toastStack = document.querySelector('[data-toast-stack]');
    const showToast = (message, options = {}) => {
        if (!toastStack || !message) return;
        const toast = document.createElement('div');
        const tone = options.tone || 'info';
        toast.className = 'toast';
        toast.dataset.tone = tone;
        toast.setAttribute('role', tone === 'danger' ? 'alert' : 'status');
        const title = document.createElement('strong');
        title.className = 'toast-title';
        title.textContent = options.title || (tone === 'success' ? copy.success : tone === 'danger' ? copy.error : copy.notice);
        const text = document.createElement('span');
        text.className = 'toast-message';
        text.textContent = message;
        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'toast-close';
        close.setAttribute('aria-label', copy.close);
        close.textContent = '×';
        close.addEventListener('click', () => toast.remove());
        toast.append(title, text, close);
        toastStack.appendChild(toast);
        window.setTimeout(() => toast.remove(), options.duration || 5200);
    };

    const modal = document.querySelector('[data-modal]');
    const modalDialog = modal?.querySelector('.modal-dialog');
    const modalTitle = modal?.querySelector('#modal-title');
    const modalBody = modal?.querySelector('[data-modal-body]');
    const modalConfirm = modal?.querySelector('[data-modal-confirm]');
    let modalReturnFocus = null;
    let modalAction = null;
    const modalFocusables = () => modalDialog ? [...modalDialog.querySelectorAll('button,[href],input,select,textarea,[tabindex]:not([tabindex="-1"])')].filter(x => !x.disabled) : [];
    const closeModal = () => {
        if (!modal) return;
        modal.hidden = true;
        modal.setAttribute('aria-hidden', 'true');
        document.body.classList.remove('modal-open');
        modal.classList.remove('is-danger', 'is-warning', 'is-info');
        modalAction = null;
        modalReturnFocus?.focus();
        modalReturnFocus = null;
    };
    const openModal = (options = {}) => {
        if (!modal) return;
        modalReturnFocus = document.activeElement;
        modalAction = options.onConfirm || null;
        if (modalTitle) modalTitle.textContent = options.title || copy.confirm;
        if (modalBody) {
            modalBody.textContent = '';
            const message = document.createElement('p');
            message.textContent = options.message || '';
            modalBody.appendChild(message);
            if (options.details) {
                const details = document.createElement('small');
                details.className = 'modal-details';
                details.textContent = options.details;
                modalBody.appendChild(details);
            }
        }
        modal.classList.toggle('is-danger', options.tone === 'danger');
        modal.classList.toggle('is-warning', options.tone === 'warning');
        modal.classList.toggle('is-info', options.tone === 'info');
        modalDialog?.setAttribute('role', options.tone === 'danger' || options.tone === 'warning' ? 'alertdialog' : 'dialog');
        if (modalConfirm) {
            modalConfirm.classList.remove('btn-primary', 'btn-danger', 'btn-warning');
            modalConfirm.classList.add(options.tone === 'danger' ? 'btn-danger' : options.tone === 'warning' ? 'btn-warning' : 'btn-primary');
            modalConfirm.textContent = options.confirmLabel || copy.confirm;
            modalConfirm.disabled = false;
            modalConfirm.setAttribute('aria-busy', 'false');
            modalConfirm.onclick = async () => {
                if (modalConfirm.disabled) return;
                modalConfirm.disabled = true;
                modalConfirm.setAttribute('aria-busy', 'true');
                try { await modalAction?.(); } finally { closeModal(); }
            };
        }
        modal.hidden = false;
        modal.setAttribute('aria-hidden', 'false');
        document.body.classList.add('modal-open');
        (modalFocusables()[0] || modalConfirm)?.focus();
    };
    document.querySelectorAll('[data-modal-close]').forEach(button => button.addEventListener('click', closeModal));
    document.querySelectorAll('[data-modal-open]').forEach(button => button.addEventListener('click', () => openModal({
        title: button.dataset.modalTitle,
        message: button.dataset.modalMessage,
        details: button.dataset.modalDetails,
        tone: button.dataset.modalTone,
        confirmLabel: button.dataset.modalConfirmLabel
    })));
    document.addEventListener('keydown', event => {
        if (!modal || modal.hidden) return;
        if (event.key === 'Escape') { closeModal(); return; }
        if (event.key !== 'Tab') return;
        const focusables = modalFocusables();
        if (!focusables.length) return;
        const first = focusables[0];
        const last = focusables[focusables.length - 1];
        if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
        else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
    });

    const validationSummary = form => {
        let summary = form.querySelector('[data-validation-summary],[data-valmsg-summary]');
        if (!summary) {
            summary = document.createElement('div');
            summary.className = 'form-summary';
            summary.setAttribute('role', 'alert');
            summary.hidden = true;
            form.prepend(summary);
        }
        summary.dataset.validationSummary = 'true';
        summary.classList.add('form-summary');
        summary.setAttribute('role', 'alert');
        return summary;
    };
    const inputFor = (form, key) => {
        if (!key) return null;
        if (key.startsWith('#')) return form.querySelector(key);
        const name = key.replace(/\.(\d+)\./g, '[$1].');
        return form.querySelector(`[name="${CSS.escape(key)}"], [name="${CSS.escape(name)}"], #${CSS.escape(key.replace(/[.\[\]]/g, '_'))}`);
    };
    const fieldMessage = (input, message) => {
        const container = input?.closest('.form-field,.choice-list,.upload-form') || input?.parentElement;
        if (!container) return null;
        let target = container.querySelector(`[data-validation-message="${CSS.escape(input.name || input.id)}"]`);
        if (!target) {
            target = container.querySelector('.field-validation,[data-valmsg-for]');
        }
        if (!target) {
            target = document.createElement('span');
            target.className = 'field-validation';
            target.dataset.validationMessage = input.name || input.id;
            input.insertAdjacentElement('afterend', target);
        }
        target.textContent = message;
        target.hidden = !message;
        target.classList.toggle('is-visible', Boolean(message));
        return target;
    };
    const clearValidation = form => {
        form.querySelectorAll('[aria-invalid="true"]').forEach(input => {
            input.removeAttribute('aria-invalid');
            input.classList.remove('is-invalid');
            const describedBy = (input.getAttribute('aria-describedby') || '').split(' ').filter(id => !id.startsWith('validation-')).join(' ');
            if (describedBy) input.setAttribute('aria-describedby', describedBy); else input.removeAttribute('aria-describedby');
        });
        form.querySelectorAll('.field-validation,[data-valmsg-for]').forEach(message => { message.textContent = ''; message.hidden = true; message.classList.remove('is-visible'); });
        const summary = form.querySelector('[data-validation-summary],[data-valmsg-summary]');
        if (summary) { summary.hidden = true; summary.replaceChildren(); }
    };
    const addError = (form, key, message, errors) => {
        const input = inputFor(form, key);
        if (!input) { errors.push({ key, message }); return null; }
        input.setAttribute('aria-invalid', 'true');
        input.classList.add('is-invalid');
        const target = fieldMessage(input, message);
        if (target) {
            if (!target.id) target.id = `validation-${(input.name || input.id).replace(/[^a-zA-Z0-9_-]/g, '-')}`;
            const ids = new Set((input.getAttribute('aria-describedby') || '').split(' ').filter(Boolean));
            ids.add(target.id);
            input.setAttribute('aria-describedby', [...ids].join(' '));
        }
        errors.push({ input, key, message });
        return input;
    };
    const showErrors = (form, errors) => {
        const summary = validationSummary(form);
        summary.replaceChildren();
        const heading = document.createElement('strong');
        heading.textContent = copy.review;
        summary.appendChild(heading);
        const list = document.createElement('ul');
        errors.forEach(error => {
            const item = document.createElement('li');
            if (error.input) {
                const link = document.createElement('a');
                link.href = `#${error.input.id || ''}`;
                link.textContent = error.message;
                link.addEventListener('click', () => error.input.focus());
                item.appendChild(link);
            } else item.textContent = error.message;
            list.appendChild(item);
        });
        summary.appendChild(list);
        summary.hidden = false;
        const first = errors.find(x => x.input)?.input;
        first?.focus({ preventScroll: false });
    };
    const clientErrors = form => {
        const errors = [];
        if (form.dataset.requireSelection === 'true' && !form.querySelector('input[type="checkbox"]:checked'))
            errors.push({ key: '', message: form.dataset.selectionMessage || copy.selection });
        form.querySelectorAll('input,select,textarea').forEach(input => {
            if (input.disabled || input.type === 'hidden' || input.type === 'submit' || input.type === 'button') return;
            const value = input.type === 'checkbox' || input.type === 'radio' ? (input.checked ? input.value : '') : input.value.trim();
            const required = input.required || input.dataset.valRequired !== undefined;
            let message = '';
            if (required && !value) message = input.dataset.valRequired || copy.required;
            else if (value && input.type === 'email' && !/^\S+@\S+\.\S+$/.test(value)) message = input.dataset.valEmail || copy.invalid;
            else if (value && input.type === 'url' && !/^(https?:\/\/|mailto:)/i.test(value)) message = input.dataset.valUrl || copy.invalid;
            else if (value && input.dataset.valLengthMax && value.length > Number(input.dataset.valLengthMax)) message = input.dataset.valLength || copy.invalid;
            else if (value && input.dataset.valLengthMin && value.length < Number(input.dataset.valLengthMin)) message = input.dataset.valLength || copy.invalid;
            else if (value && input.dataset.valRegexPattern) {
                try { if (!(new RegExp(input.dataset.valRegexPattern)).test(value)) message = input.dataset.valRegex || copy.invalid; }
                catch { message = copy.invalid; }
            }
            else if (value && input.dataset.valRangeMax && Number(value) > Number(input.dataset.valRangeMax)) message = input.dataset.valRange || copy.invalid;
            else if (value && input.dataset.valRangeMin && Number(value) < Number(input.dataset.valRangeMin)) message = input.dataset.valRange || copy.invalid;
            else if (value && input.type === 'number' && Number.isNaN(Number(value))) message = input.dataset.valNumber || copy.invalid;
            if (message) addError(form, input.name || input.id, message, errors);
        });
        form.querySelectorAll('[data-val-equalto]').forEach(input => {
            const otherName = input.dataset.valEqualtoOther;
            const other = inputFor(form, otherName);
            if (other && input.value !== other.value) addError(form, input.name || input.id, input.dataset.valEqualto || copy.invalid, errors);
        });
        return errors;
    };
    const readServerErrors = () => {
        const payload = document.querySelector('[data-model-state]');
        if (!payload) return {};
        try { return JSON.parse(payload.textContent || '{}'); } catch { return {}; }
    };
    const stopLoading = form => {
        form.removeAttribute('aria-busy');
        form.dataset.submitting = 'false';
        form.querySelectorAll('[data-loading-control]').forEach(button => {
            button.disabled = false;
            button.removeAttribute('aria-busy');
            if (button.dataset.originalLabel) button.textContent = button.dataset.originalLabel;
        });
    };
    const startLoading = form => {
        if (form.dataset.submitting === 'true') return false;
        form.dataset.submitting = 'true';
        form.setAttribute('aria-busy', 'true');
        form.querySelectorAll('button[type="submit"]:not([data-allow-repeat]),input[type="submit"]:not([data-allow-repeat])').forEach(button => {
            button.dataset.loadingControl = 'true';
            button.dataset.originalLabel = button.textContent?.trim() || button.value;
            button.disabled = true;
            button.setAttribute('aria-busy', 'true');
            const label = button.dataset.loadingLabel || form.dataset.loadingLabel || copy.loading;
            if (button.tagName === 'INPUT') button.value = label; else button.textContent = label;
        });
        return true;
    };
    const serverErrors = readServerErrors();
    document.querySelectorAll('form').forEach(form => {
        if (form.matches('[data-public-ajax-form],[data-public-cart-form]')) return;
        const method = (form.getAttribute('method') || 'get').toLowerCase();
        const shouldValidate = method === 'post' || form.dataset.validate === 'true';
        if (shouldValidate) {
            const matching = Object.entries(serverErrors).filter(([key]) => inputFor(form, key) || !key);
            if (matching.length) {
                const errors = [];
                matching.forEach(([key, messages]) => (Array.isArray(messages) ? messages : [messages]).forEach(message => addError(form, key, message, errors)));
                showErrors(form, errors);
            }
        }
        form.addEventListener('submit', event => {
            if (form.dataset.confirm && form.dataset.confirmed !== 'true') {
                event.preventDefault();
                openModal({
                    title: form.dataset.confirmTitle || copy.confirm,
                    message: form.dataset.confirmMessage || copy.confirm,
                    details: form.dataset.confirmDetails,
                    tone: form.dataset.confirmTone || 'warning',
                    confirmLabel: form.dataset.confirmLabel || copy.confirm,
                    onConfirm: () => { form.dataset.confirmed = 'true'; if (event.submitter) form.requestSubmit(event.submitter); else form.requestSubmit(); }
                });
                return;
            }
            if (shouldValidate) {
                clearValidation(form);
                const errors = clientErrors(form);
                if (errors.length) { event.preventDefault(); showErrors(form, errors); stopLoading(form); return; }
            }
            if (!startLoading(form)) event.preventDefault();
        });
    });
    document.querySelectorAll('[data-image-input],[data-cover-input]').forEach(input => {
        const preview = input.closest('.form-field')?.querySelector('[data-preview-logo],[data-preview-cover]');
        if (!preview || input.dataset.previewBound === 'true') return;
        input.dataset.previewBound = 'true';
        input.addEventListener('change', () => {
            preview.replaceChildren();
            const file = input.files?.[0];
            if (!file || !file.type.startsWith('image/')) return;
            const image = document.createElement('img');
            image.alt = file.name;
            const objectUrl = URL.createObjectURL(file);
            image.src = objectUrl;
            image.addEventListener('load', () => URL.revokeObjectURL(objectUrl), { once: true });
            preview.appendChild(image);
        });
    });
    document.querySelectorAll('[data-server-feedback] [data-feedback]').forEach(node => showToast(node.textContent.trim(), { tone: node.dataset.tone, title: node.dataset.title }));
    window.MenuPlatform = window.MenuPlatform || {};
    window.MenuPlatform.toast = showToast;
    window.MenuPlatform.openModal = openModal;
    window.MenuPlatform.stopLoading = stopLoading;
    window.MenuPlatform.validate = form => { clearValidation(form); const errors = clientErrors(form); if (errors.length) showErrors(form, errors); return errors.length === 0; };

})();

/* Public ordering interactions deliberately sit outside the admin form
   behavior above. The server still owns every price, modifier rule and basket
   mutation; this layer only keeps the menu feeling immediate. */
(() => {
    const publicForms = document.querySelectorAll('[data-public-add-form],[data-public-cart-form]');
    const isArabic = document.documentElement.lang?.toLowerCase().startsWith('ar');
    const copy = {
        working: isArabic ? 'جارٍ التحديث…' : 'Updating…',
        required: isArabic ? 'يرجى اختيار الاختيارات المطلوبة.' : 'Please complete the required choices.',
        tooMany: isArabic ? 'تم الوصول إلى الحد الأقصى للاختيارات.' : 'You have reached the selection limit.',
        quantity: isArabic ? 'يجب أن تكون الكمية بين 1 و20.' : 'Quantity must be between 1 and 20.',
        added: isArabic ? 'تمت إضافة المنتج إلى السلة.' : 'Added to basket.',
        failed: isArabic ? 'تعذر تحديث السلة.' : 'The basket could not be updated.'
    };
    const publicImages = document.querySelectorAll('[data-public-image]');
    publicImages.forEach(image => image.addEventListener('error', () => {
        const frame = image.closest('[data-image-frame]');
        if (!frame) {
            image.hidden = true;
            return;
        }
        const fallback = document.createElement('span');
        fallback.className = 'public-image-placeholder public-image-error';
        fallback.setAttribute('role', 'img');
        fallback.setAttribute('aria-label', image.dataset.fallbackLabel || (isArabic ? 'صورة المنتج' : 'Product image'));
        const mark = document.createElement('span');
        mark.setAttribute('aria-hidden', 'true');
        mark.textContent = '◎';
        const label = document.createElement('small');
        label.textContent = image.dataset.fallbackLabel || (isArabic ? 'صورة المنتج' : 'Product image');
        fallback.append(mark, label);
        frame.replaceChildren(fallback);
    }, { once: true }));
    if (!publicForms.length && !document.querySelector('[data-public-product-page]') && !publicImages.length) return;

    const toast = message => {
        window.MenuPlatform?.toast?.(message, { tone: 'danger' });
    };
    const basketAmount = data => {
        const total = Number(data?.total ?? 0);
        const currency = data?.currency || '';
        return `${total.toFixed(2)} ${currency}`.trim();
    };
    const updateBasketUI = data => {
        if (!data) return;
        document.querySelectorAll('[data-basket-count]').forEach(node => node.textContent = String(data.itemCount ?? 0));
        document.querySelectorAll('[data-basket-total]').forEach(node => node.textContent = basketAmount(data));
        document.querySelectorAll('[data-public-basket-bar]').forEach(bar => bar.dataset.hasItems = String(Number(data.itemCount ?? 0) > 0));
    };
    const setWorking = (form, working) => {
        form.dataset.publicSubmitting = working ? 'true' : 'false';
        form.setAttribute('aria-busy', String(working));
        form.querySelectorAll('button[type="submit"]').forEach(button => {
            button.disabled = working;
            button.classList.toggle('is-loading', working);
            button.setAttribute('aria-busy', String(working));
        });
    };
    const responseJson = async response => {
        const payload = await response.json().catch(() => ({}));
        if (!response.ok || payload.ok === false) throw new Error(payload.error || copy.failed);
        return payload;
    };
    const submitPublicForm = async (form, onSuccess) => {
        if (form.dataset.publicSubmitting === 'true') return;
        setWorking(form, true);
        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
            });
            const payload = await responseJson(response);
            updateBasketUI(payload);
            onSuccess?.(payload);
        } catch (error) {
            toast(error.message || copy.failed);
        } finally {
            setWorking(form, false);
        }
    };
    const showGroupError = (group, message) => {
        const target = group.querySelector('[data-group-error]');
        if (!target) return;
        target.textContent = message;
        target.hidden = !message;
        group.classList.toggle('has-error', Boolean(message));
    };
    const validateProductForm = form => {
        let valid = true;
        const quantity = form.querySelector('[data-quantity-input]');
        const quantityValue = Number(quantity?.value);
        const quantityError = form.querySelector('[data-quantity-error]');
        if (!Number.isInteger(quantityValue) || quantityValue < 1 || quantityValue > 20) {
            if (quantityError) { quantityError.textContent = copy.quantity; quantityError.hidden = false; }
            quantity?.setAttribute('aria-invalid', 'true');
            valid = false;
        } else {
            if (quantityError) quantityError.hidden = true;
            quantity?.removeAttribute('aria-invalid');
        }
        form.querySelectorAll('[data-modifier-group]').forEach(group => {
            const min = Number(group.dataset.groupMin || 0);
            const max = Number(group.dataset.groupMax || 1);
            const count = group.querySelectorAll('[data-modifier-option]:checked').length;
            const label = group.dataset.groupLabel || (isArabic ? 'الاختيارات' : 'Choices');
            if (count < min) {
                showGroupError(group, `${label}: ${copy.required}`);
                valid = false;
            } else if (count > max) {
                showGroupError(group, `${label}: ${copy.tooMany}`);
                valid = false;
            } else showGroupError(group, '');
        });
        if (!valid) form.querySelector('[aria-invalid="true"],[data-group-error]:not([hidden])')?.scrollIntoView({ block: 'center', behavior: 'smooth' });
        return valid;
    };
    const updateProductTotal = form => {
        if (!form) return;
        const base = Number(form.dataset.basePrice || 0);
        const quantity = Number(form.querySelector('[data-quantity-input]')?.value || 1);
        const modifiers = [...form.querySelectorAll('[data-modifier-option]:checked')].reduce((sum, input) => sum + Number(input.dataset.priceAdjustment || 0), 0);
        const total = Math.max(0, (base + modifiers) * Math.max(1, quantity));
        form.querySelectorAll('[data-item-total]').forEach(node => node.textContent = total.toFixed(2));
    };
    const updateQuantity = (form, direction) => {
        const input = form.querySelector('[data-quantity-input]');
        if (!input) return;
        const current = Number(input.value || 1);
        input.value = String(Math.min(20, Math.max(1, current + direction)));
        const output = form.querySelector('[data-quantity-value]');
        if (output) output.textContent = input.value;
        updateProductTotal(form);
    };
    document.querySelectorAll('[data-quantity-action]').forEach(button => button.addEventListener('click', () => {
        const form = button.closest('[data-public-product-form]');
        updateQuantity(form, button.dataset.quantityAction === 'increase' ? 1 : -1);
    }));
    document.querySelectorAll('[data-quantity-input]').forEach(input => input.addEventListener('input', () => {
        const form = input.closest('[data-public-product-form]');
        const output = form?.querySelector('[data-quantity-value]');
        if (output) output.textContent = input.value || '0';
        updateProductTotal(form);
    }));
    document.querySelectorAll('[data-modifier-option]').forEach(input => input.addEventListener('change', () => {
        const group = input.closest('[data-modifier-group]');
        const max = Number(group?.dataset.groupMax || 1);
        const selected = group ? [...group.querySelectorAll('[data-modifier-option]:checked')] : [];
        if (selected.length > max) {
            input.checked = false;
            showGroupError(group, `${group.dataset.groupLabel || (isArabic ? 'الاختيارات' : 'Choices')}: ${copy.tooMany}`);
        } else showGroupError(group, '');
        updateProductTotal(input.closest('[data-public-product-form]'));
    }));
    document.querySelectorAll('[data-gallery-thumb]').forEach(button => button.addEventListener('click', () => {
        const gallery = button.closest('[data-public-gallery]');
        const primary = gallery?.querySelector('[data-gallery-primary]');
        if (!primary) return;
        primary.src = button.dataset.src;
        primary.alt = button.dataset.alt || '';
        gallery.querySelectorAll('[data-gallery-thumb]').forEach(item => item.classList.remove('is-active'));
        button.classList.add('is-active');
    }));
    document.querySelectorAll('[data-public-card][data-product-href]').forEach(card => {
        const open = () => { if (card.dataset.productHref) window.location.href = card.dataset.productHref; };
        card.addEventListener('click', event => { if (!event.target.closest('a,button,form,input,select,textarea')) open(); });
        card.addEventListener('keydown', event => {
            if ((event.key === 'Enter' || event.key === ' ') && event.target === card) { event.preventDefault(); open(); }
        });
    });

    document.querySelectorAll('[data-public-add-form]').forEach(form => form.addEventListener('submit', event => {
        event.preventDefault();
        if (form.matches('[data-public-product-form]') && !validateProductForm(form)) return;
        submitPublicForm(form, data => {
            const feedback = form.querySelector('[data-form-feedback]');
            if (feedback) {
                feedback.textContent = data.message || copy.added;
                feedback.classList.add('is-visible');
            }
            const card = form.closest('[data-public-card]');
            if (card && data.lineKey) {
                const state = card.querySelector('[data-card-quantity]');
                const value = state?.querySelector('[data-card-quantity-value]');
                if (state) {
                    state.dataset.cartKey = data.lineKey;
                    state.hidden = false;
                    if (value) value.textContent = String(data.lineQuantity || 1);
                }
                form.hidden = true;
            }
        });
    }));

    const updateCardQuantity = async (state, quantity) => {
        const card = state.closest('[data-public-card]');
        const token = card?.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const formData = new FormData();
        formData.append('__RequestVerificationToken', token || '');
        formData.append('key', state.dataset.cartKey || '');
        formData.append('quantity', String(quantity));
        state.classList.add('is-loading');
        try {
            const response = await fetch(state.dataset.cartUpdateUrl, { method: 'POST', body: formData, headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' } });
            const data = await responseJson(response);
            updateBasketUI(data);
            const form = card?.querySelector('[data-public-add-form]');
            if (quantity <= 0) {
                state.hidden = true;
                if (form) form.hidden = false;
            } else {
                const value = state.querySelector('[data-card-quantity-value]');
                if (value) value.textContent = String(quantity);
            }
        } catch (error) { toast(error.message || copy.failed); }
        finally { state.classList.remove('is-loading'); }
    };
    document.querySelectorAll('[data-card-quantity-action]').forEach(button => button.addEventListener('click', () => {
        const state = button.closest('[data-card-quantity]');
        const value = Number(state?.querySelector('[data-card-quantity-value]')?.textContent || 0);
        updateCardQuantity(state, button.dataset.cardQuantityAction === 'increase' ? Math.min(20, value + 1) : Math.max(0, value - 1));
    }));

    document.querySelectorAll('[data-public-cart-form]').forEach(form => {
        form.addEventListener('click', event => {
            const button = event.target.closest('[data-cart-step]');
            if (!button) return;
            const input = form.querySelector('input[name="quantity"]');
            if (!input) return;
            const current = Number(input.value || 0);
            input.value = String(button.dataset.cartStep === 'increase' ? Math.min(20, current + 1) : Math.max(0, current - 1));
        });
        form.addEventListener('submit', event => {
            event.preventDefault();
            submitPublicForm(form, () => window.location.reload());
        });
    });
})();
