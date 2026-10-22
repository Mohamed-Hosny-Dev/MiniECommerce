/* Shared front-end helpers: CSRF header for AJAX, toasts, confirm dialog, formatting. */
(function (window, $) {
    'use strict';

    var App = (window.App = window.App || {});

    var token = $('meta[name="csrf-token"]').attr('content');
    var base = ($('meta[name="app-base"]').attr('content') || '/').replace(/\/$/, '');

    // Every unsafe AJAX request automatically carries the anti-forgery token (validated by the server).
    $.ajaxPrefilter(function (options, original, jqXHR) {
        if (!/^(GET|HEAD|OPTIONS|TRACE)$/i.test(options.type || 'GET') && !options.crossDomain) {
            jqXHR.setRequestHeader('RequestVerificationToken', token);
        }
    });

    /** Builds an app-relative URL so the site also works when hosted under a sub-path. */
    App.url = function (path) {
        return base + path;
    };

    /** Same output as the server-side ToEgp() extension, e.g. "1,299.00 EGP". */
    App.formatMoney = function (value) {
        return Number(value || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' EGP';
    };

    App.errorMessage = function (xhr) {
        if (!xhr || xhr.status === 0) return 'Cannot reach the server. Check your connection and try again.';
        if (xhr.status === 400) return 'Your session expired. Please refresh the page and try again.';
        if (xhr.status === 401 || xhr.status === 403) return 'You are not allowed to do that. Please sign in again.';
        if (xhr.status === 404) return 'The requested item was not found.';
        return 'Something went wrong on our side. Please try again.';
    };

    /** Updates the cart badge in the navbar. */
    App.setCartCount = function (count) {
        var $badge = $('#cart-badge-count');
        $badge.text(count).toggleClass('d-none', !count);
        $badge.removeClass('bump');
        // force reflow so the animation can replay
        void $badge[0].offsetWidth;
        $badge.addClass('bump');
    };

    /** Bootstrap toast. Messages are inserted as text, never as HTML. */
    App.toast = function (message, type, opts) {
        type = type || 'success';
        opts = opts || {};
        var icons = { success: 'fa-circle-check', danger: 'fa-circle-exclamation', warning: 'fa-triangle-exclamation', info: 'fa-circle-info' };
        var closeClass = type === 'warning' ? 'btn-close' : 'btn-close btn-close-white';

        var $toast = $(
            '<div class="toast app-toast align-items-center border-0" role="status" aria-live="polite" aria-atomic="true">' +
            '<div class="d-flex"><div class="toast-body"><i class="fa-solid me-2"></i><span class="msg"></span></div>' +
            '<button type="button" class="' + closeClass + ' me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div></div>'
        );

        $toast.addClass('text-bg-' + type);
        $toast.find('i').addClass(icons[type] || icons.info);
        $toast.find('.msg').text(message);

        if (opts.href && opts.linkText) {
            $('<a class="toast-link"></a>').attr('href', opts.href).text(opts.linkText).appendTo($toast.find('.toast-body'));
        }

        $('#toast-container').append($toast);
        var toast = new bootstrap.Toast($toast[0], { delay: opts.delay || 3500 });
        $toast.on('hidden.bs.toast', function () { $toast.remove(); });
        toast.show();
    };

    /** Promise based replacement for window.confirm(). */
    App.confirm = function (message, opts) {
        opts = opts || {};
        return new Promise(function (resolve) {
            var el = document.getElementById('confirmModal');
            var modal = bootstrap.Modal.getOrCreateInstance(el);
            var decided = false;

            function finish(value) {
                if (decided) return;
                decided = true;
                resolve(value);
            }

            $('#confirmTitle').text(opts.title || 'Are you sure?');
            $('#confirmMessage').text(message);
            $('#confirmOk').text(opts.okText || 'Confirm').attr('class', 'btn ' + (opts.okClass || 'btn-danger'));

            $('#confirmOk').off('click.confirm').on('click.confirm', function () {
                finish(true);
                modal.hide();
            });
            $(el).off('hidden.bs.modal.confirm').on('hidden.bs.modal.confirm', function () { finish(false); });

            modal.show();
        });
    };

    // Forms / buttons that ask for confirmation:  <form data-confirm="Cancel this order?" data-confirm-ok="Yes, cancel">
    $(document).on('submit', 'form[data-confirm]', function (e) {
        var form = this;
        e.preventDefault();
        App.confirm(form.getAttribute('data-confirm'), { okText: form.getAttribute('data-confirm-ok') }).then(function (ok) {
            if (ok) form.submit(); // programmatic submit() does not re-trigger this handler
        });
    });

    // Prevent double submits (e.g. placing the same order twice).
    $(document).on('submit', 'form[data-single-submit]', function () {
        var form = this;
        if ($.fn.valid && !$(form).valid()) return;
        $(form).find('[type=submit]').prop('disabled', true).addClass('disabled');
    });

    // CSP does not allow inline onerror handlers, so broken product images are handled here (error events don't bubble -> capture).
    document.addEventListener('error', function (e) {
        var img = e.target;
        if (img && img.tagName === 'IMG' && img.getAttribute('data-fallback') && !img.getAttribute('data-fallback-used')) {
            img.setAttribute('data-fallback-used', '1');
            img.src = img.getAttribute('data-fallback');
        }
    }, true);

})(window, window.jQuery);
