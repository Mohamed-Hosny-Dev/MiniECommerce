/* Admin area: orders list, products list, categories and the product form - all AJAX, no page reloads. */
(function (window, $) {
    'use strict';

    var App = window.App;

    function jsonPost(path, data) {
        return $.ajax({ url: App.url(path), type: 'POST', dataType: 'json', data: data });
    }

    // ===================== Orders =====================
    var $orders = $('#ordersContainer');
    if ($orders.length) {
        var ordersPage = 1;
        var ordersXhr = null;
        var ordersTimer = null;

        var loadOrders = function (page) {
            ordersPage = page || 1;
            if (ordersXhr) ordersXhr.abort();
            $orders.addClass('is-loading');

            ordersXhr = $.ajax({
                url: App.url('/Admin/Orders'),
                type: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                data: { search: $('#orderSearchInput').val(), status: $('#orderStatusFilter').val(), pageNumber: ordersPage }
            }).done(function (html) {
                $orders.html(html);
            }).fail(function (xhr, status) {
                if (status !== 'abort') App.toast(App.errorMessage(xhr), 'danger');
            }).always(function () {
                $orders.removeClass('is-loading');
            });
        };

        $('#orderSearchInput').on('input', function () {
            clearTimeout(ordersTimer);
            ordersTimer = setTimeout(function () { loadOrders(1); }, 350);
        });
        $('#orderStatusFilter').on('change', function () { loadOrders(1); });
        $('#resetOrderFilter').on('click', function () {
            $('#orderSearchInput').val('');
            $('#orderStatusFilter').val('');
            loadOrders(1);
        });

        $(document).on('click', '#ordersContainer .js-page-link', function (e) {
            e.preventDefault();
            var page = parseInt($(this).data('page'), 10);
            if (page && !$(this).closest('.page-item').is('.disabled, .active')) loadOrders(page);
        });

        $(document).on('change', '.js-status-select', function () {
            var $select = $(this);
            var orderId = $select.data('order-id');
            var previous = String($select.data('current'));
            var newStatus = $select.val();
            var isCancel = $select.find('option:selected').data('cancel') === 1 || $select.find('option:selected').data('cancel') === '1';

            function send() {
                $select.prop('disabled', true);
                jsonPost('/Admin/UpdateOrderStatus', { orderId: orderId, status: newStatus }).done(function (res) {
                    App.toast(res.message, res.success ? 'success' : 'danger');
                    loadOrders(ordersPage); // re-render: status badges + the next allowed options
                }).fail(function (xhr) {
                    App.toast(App.errorMessage(xhr), 'danger');
                    $select.val(previous).prop('disabled', false);
                });
            }

            if (isCancel) {
                App.confirm('Cancelling returns the items to stock and cannot be undone.', { title: 'Cancel order #' + orderId + '?', okText: 'Cancel order' })
                    .then(function (ok) { if (ok) send(); else $select.val(previous); });
            } else {
                send();
            }
        });
    }

    // ===================== Products (admin list) =====================
    var $products = $('#adminProductsContainer');
    if ($products.length) {
        var productsPage = 1;
        var productsXhr = null;
        var productsTimer = null;

        var loadProducts = function (page) {
            productsPage = page || 1;
            if (productsXhr) productsXhr.abort();
            $products.addClass('is-loading');

            productsXhr = $.ajax({
                url: App.url('/Admin/Products'),
                type: 'GET',
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                data: { search: $('#adminProductSearch').val(), pageNumber: productsPage }
            }).done(function (html) {
                $products.html(html);
            }).fail(function (xhr, status) {
                if (status !== 'abort') App.toast(App.errorMessage(xhr), 'danger');
            }).always(function () {
                $products.removeClass('is-loading');
            });
        };

        $('#adminProductSearch').on('input', function () {
            clearTimeout(productsTimer);
            productsTimer = setTimeout(function () { loadProducts(1); }, 350);
        });

        $(document).on('click', '#adminProductsContainer .js-page-link', function (e) {
            e.preventDefault();
            var page = parseInt($(this).data('page'), 10);
            if (page && !$(this).closest('.page-item').is('.disabled, .active')) loadProducts(page);
        });

        $(document).on('click', '[data-toggle-product]', function () {
            var $btn = $(this).prop('disabled', true);
            jsonPost('/Products/ToggleActiveStatus', { id: $btn.data('product-id') }).done(function (res) {
                App.toast(res.message, res.success ? 'success' : 'danger');
                loadProducts(productsPage);
            }).fail(function (xhr) {
                App.toast(App.errorMessage(xhr), 'danger');
                $btn.prop('disabled', false);
            });
        });
        $(document).on('click', '[data-delete-product]', function () {
            var $btn = $(this);
            var productId = $btn.data('product-id');
            var productName = $btn.data('product-name');

            // استخدام App.confirm الخاص بك للحفاظ على نفس شكل الـ UI
            App.confirm('Are you sure you want to delete the product "' + productName + '"?', { title: 'Delete product', okText: 'Delete' }).then(function (ok) {
                if (!ok) return;
                
                // إضافة حالة تحميل (Disabling) للزر لمنع الضغط المزدوج
                $btn.prop('disabled', true);

                jsonPost('/Products/Delete', { id: productId }).done(function (res) {
                    if (res.success) {
                        App.toast(res.message, 'success');
                        loadProducts(productsPage); 
                    } else {
                        App.toast(res.message, 'danger', { delay: 5000 });
                        $btn.prop('disabled', false);
                    }
                }).fail(function (xhr) {
                    App.toast(App.errorMessage(xhr), 'danger');
                    $btn.prop('disabled', false);
                });
            });
        });

    }

    // ===================== Categories =====================
    var $categoryModal = $('#categoryModal');
    if ($categoryModal.length) {
        var modal = bootstrap.Modal.getOrCreateInstance($categoryModal[0]);

        $('#btnAddCategory').on('click', function () {
            $('#categoryModalTitle').text('Add category');
            $('#categoryId').val('');
            $('#categoryName').val('');
            $('#categoryDescription').val('');
            $('#categoryError').addClass('d-none').text('');
            modal.show();
        });

        $(document).on('click', '[data-category-edit]', function () {
            var $b = $(this);
            $('#categoryModalTitle').text('Edit category');
            $('#categoryId').val($b.data('id'));
            $('#categoryName').val($b.data('name'));
            $('#categoryDescription').val($b.data('description') || '');
            $('#categoryError').addClass('d-none').text('');
            modal.show();
        });

        $('#categoryForm').on('submit', function (e) {
            e.preventDefault();
            var $save = $('#categorySave').prop('disabled', true);

            jsonPost('/Admin/SaveCategory', {
                id: $('#categoryId').val(),
                name: $('#categoryName').val(),
                description: $('#categoryDescription').val()
            }).done(function (res) {
                if (res.success) {
                    window.location.reload();
                } else {
                    $('#categoryError').removeClass('d-none').text(res.message);
                }
            }).fail(function (xhr) {
                $('#categoryError').removeClass('d-none').text(App.errorMessage(xhr));
            }).always(function () {
                $save.prop('disabled', false);
            });
        });

        $(document).on('click', '[data-category-delete]', function () {
            var $b = $(this);
            App.confirm('Delete the category "' + $b.data('name') + '"?', { title: 'Delete category', okText: 'Delete' }).then(function (ok) {
                if (!ok) return;
                jsonPost('/Admin/DeleteCategory', { id: $b.data('id') }).done(function (res) {
                    if (res.success) {
                        $b.closest('tr').fadeOut(200, function () { window.location.reload(); });
                        App.toast(res.message, 'success');
                    } else {
                        App.toast(res.message, 'danger', { delay: 5000 });
                    }
                }).fail(function (xhr) {
                    App.toast(App.errorMessage(xhr), 'danger');
                });
            });
        });
    }

    // ===================== Product form: live image preview =====================
    var $imageUrl = $('#ImageUrl');
    if ($imageUrl.length) {
        var $preview = $('#imagePreview');
        var placeholder = $preview.data('placeholder');
        var updatePreview = function () {
            var value = ($imageUrl.val() || '').trim();
            $preview.attr('src', value || placeholder);
        };
        $imageUrl.on('input', updatePreview);
    }

})(window, window.jQuery);
