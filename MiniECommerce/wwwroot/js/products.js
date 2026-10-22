/* Products page: live search / filter / sort / paging with AJAX, keeping the URL and the back button in sync. */
(function (window, $) {
    'use strict';

    var App = window.App;
    var $form = $('#filterForm');
    var $container = $('#productsContainer');

    if (!$form.length) return;

    var request = null;
    var timer = null;

    function queryString(page) {
        var params = $form.serializeArray().filter(function (p) { return p.name !== 'pageNumber' && p.value !== ''; });
        if (page && page > 1) params.push({ name: 'pageNumber', value: page });
        return $.param(params);
    }

    function load(page, options) {
        options = options || {};
        var qs = queryString(page);

        if (request) request.abort(); // ignore slow, outdated responses
        $container.addClass('is-loading');

        request = $.ajax({
            url: $form.attr('action') + (qs ? '?' + qs : ''),
            type: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).done(function (html) {
            $container.html(html);

            if (options.push !== false) {
                history.pushState({ filters: true }, '', $form.attr('action') + (qs ? '?' + qs : ''));
            }
            if (options.scroll) {
                $('html, body').animate({ scrollTop: Math.max(0, $container.offset().top - 150) }, 200);
            }
        }).fail(function (xhr, status) {
            if (status !== 'abort') App.toast(App.errorMessage(xhr), 'danger');
        }).always(function () {
            $container.removeClass('is-loading');
        });
    }

    // typing (debounced)
    $form.find('#searchInput').on('input', function () {
        clearTimeout(timer);
        timer = setTimeout(function () { load(1); }, 350);
    });

    // dropdowns
    $form.find('select').on('change', function () { load(1); });

    // pressing Enter / the search button
    $form.on('submit', function (e) {
        e.preventDefault();
        clearTimeout(timer);
        load(1);
    });

    // reset
    $('#resetFilterBtn').on('click', function () {
        $form[0].reset();
        $form.find('#searchInput').val('');
        $form.find('select').val('');
        load(1);
    });

    // pager links are real links (work without JS); with JS they load via AJAX
    $(document).on('click', '#productsContainer .js-page-link', function (e) {
        var page = parseInt($(this).data('page'), 10);
        if (!page || $(this).closest('.page-item').is('.disabled, .active')) {
            e.preventDefault();
            return;
        }
        e.preventDefault();
        load(page, { scroll: true });
    });

    // browser back / forward
    window.addEventListener('popstate', function () {
        var params = new URLSearchParams(window.location.search);
        $form.find('#searchInput').val(params.get('search') || '');
        $form.find('#categorySelect').val(params.get('categoryId') || '');
        $form.find('#sortSelect').val(params.get('sort') || '');

        $container.addClass('is-loading');
        $.ajax({
            url: $form.attr('action') + window.location.search,
            type: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).done(function (html) {
            $container.html(html);
        }).always(function () {
            $container.removeClass('is-loading');
        });
    });

})(window, window.jQuery);
