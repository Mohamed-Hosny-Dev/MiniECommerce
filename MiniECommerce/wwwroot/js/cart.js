/* Add-to-cart (home, listing, details) and the cart page: quantity updates + removal, all via AJAX. */
(function (window, $) {
    'use strict';

    var App = window.App;
    var MAX_QTY = 20;

    function clamp(value, min, max) {
        return Math.min(Math.max(value, min), max);
    }

    // ---- Add to cart -------------------------------------------------------
    $(document).on('click', '[data-add-to-cart]', function () {
        var $btn = $(this);
        if ($btn.prop('disabled')) return;

        var qtySelector = $btn.data('quantity-input');
        var quantity = qtySelector ? parseInt($(qtySelector).val(), 10) || 1 : 1;
        var originalHtml = $btn.html();

        $btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');

        $.ajax({
            url: App.url('/Cart/AddToCart'),
            type: 'POST',
            dataType: 'json',
            data: { productId: $btn.data('product-id'), quantity: quantity }
        }).done(function (res) {
            if (res && res.success) {
                App.setCartCount(res.cartCount);
                App.toast(res.message, 'success', { href: App.url('/Cart'), linkText: 'View cart' });
            } else {
                App.toast((res && res.message) || 'Could not add the product.', 'warning');
            }
        }).fail(function (xhr) {
            App.toast(App.errorMessage(xhr), 'danger');
        }).always(function () {
            $btn.prop('disabled', false).html(originalHtml);
        });
    });

    // ---- Quantity stepper (+ / -) ------------------------------------------
    $(document).on('click', '[data-qty-step]', function () {
        var $input = $(this).closest('.qty-stepper').find('input[type=number]');
        var min = parseInt($input.attr('min'), 10) || 1;
        var max = parseInt($input.attr('max'), 10) || MAX_QTY;
        var current = parseInt($input.val(), 10) || min;
        var next = clamp(current + parseInt($(this).data('qty-step'), 10), min, max);

        if (next !== current) {
            $input.val(next).trigger('change');
        }
    });

    // Keep typed values in range on the product details page
    $(document).on('change', '#quantitySelect', function () {
        var min = parseInt($(this).attr('min'), 10) || 1;
        var max = parseInt($(this).attr('max'), 10) || MAX_QTY;
        $(this).val(clamp(parseInt($(this).val(), 10) || min, min, max));
    });

    // ---- Cart page ---------------------------------------------------------
    function applyTotals(res) {
        $('#cart-total').text(App.formatMoney(res.cartTotal));
        App.setCartCount(res.cartCount);
    }

    var timers = {};

    $(document).on('change', 'input[data-cart-qty]', function () {
        var $input = $(this);
        var productId = $input.data('product-id');
        var max = parseInt($input.attr('max'), 10) || MAX_QTY;
        var value = clamp(parseInt($input.val(), 10) || 1, 1, max);

        $input.val(value);

        // small debounce so quickly clicking "+" several times sends one request
        clearTimeout(timers[productId]);
        timers[productId] = setTimeout(function () {
            var $row = $('#cart-row-' + productId).addClass('is-loading');

            $.ajax({
                url: App.url('/Cart/UpdateQuantity'),
                type: 'POST',
                dataType: 'json',
                data: { productId: productId, quantity: value }
            }).done(function (res) {
                // The server's answer wins: if the change was rejected (stock!), the input goes back to the real value.
                if (!res.removed) {
                    $input.val(res.quantity);
                    $('#subtotal-' + productId).text(App.formatMoney(res.itemSubtotal));
                }
                applyTotals(res);
                if (!res.success) App.toast(res.message, 'warning');
            }).fail(function (xhr) {
                App.toast(App.errorMessage(xhr), 'danger');
            }).always(function () {
                $row.removeClass('is-loading');
            });
        }, 300);
    });

    $(document).on('click', '[data-remove-from-cart]', function () {
        var $btn = $(this);
        var productId = $btn.data('product-id');
        var name = $btn.data('product-name') || 'this item';

        App.confirm('Remove "' + name + '" from your cart?', { okText: 'Remove', title: 'Remove item' }).then(function (ok) {
            if (!ok) return;

            $.ajax({
                url: App.url('/Cart/RemoveFromCart'),
                type: 'POST',
                dataType: 'json',
                data: { productId: productId }
            }).done(function (res) {
                if (!res.success) return;
                applyTotals(res);
                $('#cart-row-' + productId).fadeOut(200, function () {
                    $(this).remove();
                    if (!$('[data-cart-row]').length) {
                        window.location.reload(); // show the empty-cart state
                    }
                });
                App.toast('Item removed from your cart.', 'info');
            }).fail(function (xhr) {
                App.toast(App.errorMessage(xhr), 'danger');
            });
        });
    });

})(window, window.jQuery);
