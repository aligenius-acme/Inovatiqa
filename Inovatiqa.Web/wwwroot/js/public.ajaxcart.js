var AjaxCart = {
    loadWaiting: false,
    usepopupnotifications: false,
    topcartselector: '.cart-qty',
    topwishlistselector: '',
    flyoutcartselector: '',
    localized_data: false,

    isPublicList: false,
    suspendedItemIds: [],
    // flyout cart implementation of on-the-go quantity changing functionality
    quantityWaiting: false,
    changeQuantityIds: new Array(),
    quantities: new Array(),

    init: function (usepopupnotifications, topcartselector, topwishlistselector, flyoutcartselector, localized_data) {
        this.loadWaiting = false;
        this.usepopupnotifications = usepopupnotifications;
        this.topcartselector = topcartselector;
        this.topwishlistselector = topwishlistselector;
        this.flyoutcartselector = flyoutcartselector;
        this.localized_data = localized_data;
        this.quantityWaiting = false;
        this.changeQuantityIds = [];
    },

    setLoadWaiting: function (display) {
        displayAjaxLoading(display);
        this.loadWaiting = display;
    },

    //add a product to the cart/wishlist from the catalog pages
    addproducttocart_catalog: function (urladd, typeId) {
        if (this.loadWaiting !== false) {
            return;
        }

        if (typeId === 2) {
            var wishListBox = $("#wish-list-box");
            if (wishListBox.attr("data-urladd") === undefined) {
                var productId = urladd.split('/')[3].trim();
                if (isProductHasAttributes(productId) === false) {
                    if (isUserLoggedIn() === true) {
                        wishListBox.modal();
                        $("#everyone").attr("checked", "checked");
                        setWishListForm(wishListBox, urladd, typeId, null, productId);
                        populateWishListDropDown("WishListBox");
                        return;
                    }
                }
            }
        }

        this.setLoadWaiting(true);

        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });

        $(".close").click();
    },

    //add a product to the cart/wishlist from the product details page
    addproducttocart_details: function (urladd, formselector, typeId) {
        if (this.loadWaiting !== false) {
            return;
        }
   
        if (typeId === 2) {
            var wishListBox = $("#wish-list-box");
            if (wishListBox.attr("data-urladd") === undefined) {
                var productId = urladd.split('/')[3].trim();
                if (isUserLoggedIn() === true) {
                    wishListBox.modal({
                        backdrop: 'static',
                        keyboard: false
                    });
                    $("#everyone").attr("checked", "checked");
                    setWishListForm(wishListBox, urladd, typeId, formselector, productId);
                    populateWishListDropDown("WishListBox");
                    return;
                }
            }
        }
    
        var data;
        if (formselector != "#product-details-form" && formselector.search("#product-details-form") == 0) {
            data = $(formselector).closest('tr').find('input').serializeArray();
            data.push({ name: $(formselector).closest('tr').find('select').attr('name'), value: $(formselector).closest('tr').find('select').val() });
        }
        else {
            data = $(formselector).serialize();
        }
        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: urladd,
            data: data,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });

        
        $(".close").click();
        // by hamza
        //location.reload();
   
    },


    //add a product to the cart/wishlist from the product details page
    updateproducttocart_details: function (urladd, formselector, typeId) {
        if (this.loadWaiting !== false) {
            return;
        }

        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: urladd,
            data: $(formselector).serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });

        $(".close").click();
    },

    addproducttocart_reorder: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var qty = $(event.target).closest('tr').find('input.qty').val();
        var n1 = urladd.substring(0, urladd.lastIndexOf("/") + 1);
        urladd = n1 + qty;
        $.ajax({
            cache: false,
            url: urladd,
            data: $(event.target).closest('tr').find('select.uom').serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addproducttocart_productlisting: function (urladd, productId) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var qty = $(`.qty_${productId}`).val();
        var n1 = urladd.substring(0, urladd.lastIndexOf("/") + 1);
        urladd = n1 + qty;
        $.ajax({
            cache: false,
            url: urladd,
            data: $(`.uom_${productId}`).serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addproducttowishlist_productlisting: function (urladd) {
        var wishListBox = $("#wish-list-box");
        if (wishListBox.attr("data-urladd") === undefined) {
            var productId = urladd.split('/')[3].trim();
            if (isProductHasAttributes(productId) === false) {
                if (isUserLoggedIn() === true) {
                    wishListBox.modal();
                    $("#everyone").attr("checked", "checked");
                    setWishListForm(wishListBox, urladd, typeId, null, productId);
                    populateWishListDropDown("WishListBox");
                    return;
                }
            }
        }
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var qty = $(event.target).closest('tr').find('input.qty').val();
        var n1 = urladd.substring(0, urladd.lastIndexOf("/") + 1);
        urladd = n1 + qty;
        $.ajax({
            cache: false,
            url: urladd,
            data: $(event.target).closest('tr').find('select.uom').serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addbulkproducttocart_reorder: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var form = "";//$('#reorder-guide').DataTable().$('tr').find('input:checked').closest('tr').find('select.uom').serializeArray();
        var orderIds = []; var orderItemIds = []; var quantities = []
        $('#reorder-guide input:checked').each(function () {
            orderIds.push($(this).attr('orderId'));
            orderItemIds.push($(this).attr('orderItemId'));
            quantities.push($(this).closest('tr').find('input.qty').val());
        });
        //form.push({ name: 'orderIds', value: orderIds.join() });
        //form.push({ name: 'orderItemIds', value: orderItemIds.join() });
        var form = $('#reorder-guide input:checked').closest('tr').find('select').serializeArray();
        form.push({ name: 'orderIds', value: orderIds.join() });
        form.push({ name: 'orderItemIds', value: orderItemIds.join() });
        form.push({ name: 'quantities', value: quantities.join() });
        $.ajax({
            async: false,
            cache: false,
            url: urladd,
            data: form,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addbulkproducttocart_details: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var form = "";
        var orderIds = []; var orderItemIds = []; var quantities = []
        $('#tblAllItems input:checked').each(function () {
            orderIds.push($(this).attr('orderId'));
            orderItemIds.push($(this).attr('orderItemId'));
            quantities.push($(this).closest('tr').find('.qty').text());
        });
        var form = [];
        form.push({ name: 'orderIds', value: orderIds.join() });
        form.push({ name: 'orderItemIds', value: orderItemIds.join() });
        form.push({ name: 'quantities', value: quantities.join() });
        $.ajax({
            async: false,
            cache: false,
            url: urladd,
            data: form,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    chkboxreorderitem: function () {
        var countOfSelectedCheckBoxes = $("input[name='chkaddTocart']:checked").length;
        if (countOfSelectedCheckBoxes > 0) {
            $('#addToCartList').attr("disabled", false);
            //$('#addToCartList').addClass("btn_green");
            $('#addToCartList').removeClass("btn_disable");
        }
        else {
            $('#addToCartList').attr("disabled", true);
            $('#addToCartList').addClass("btn_disable");
            //$('#addToCartList').removeClass("btn_green");
        }
        $('#addToCartList').html(`Add Selected to Cart (${countOfSelectedCheckBoxes})`);
    },
    //add a product to compare list
    addproducttocomparelist: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        $.ajax({
            cache: false,
            url: urladd,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addorremovefromcomaprelist(prodId) {
        if ($(event.currentTarget).hasClass("compare-checkbox")) {
            var num = parseInt(document.getElementsByClassName('comp-button')[0].innerText.match(/\d+/, 10))
            if (event.target.checked) {
                num = isNaN(num) ? num = 1 : num + 1;
                AjaxCart.addproducttocomparelist('/compareproducts/add/' + prodId);
            }
            else {
                num = isNaN(num) ? num = 0 : num - 1;
                $.ajax({
                    type: "POST",
                    data: { "redirectToCompareList": false },
                    url: '/compareproducts/removeproduct/' + prodId,
                    success: this.success_process,
                    complete: this.resetLoadWaiting,
                    error: this.ajaxFailure
                });
            }
            if (num > 0)
                $('.comp-button').html(' Compare Selected Items ( ' + num+ ' )');
            else
                $('.comp-button').html(' Compare Selected Items');
        }


    },
    clearcomparelistwithoutredirect: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        if ($('.comp-button').html().match(/\d+/) != null) {
            this.setLoadWaiting(true);
            $.ajax({
                cache: false,
                url: urladd,
                type: "POST",
                success: function (d) {
                    if (d.success) {
                        // displayBarNotification(response.message, 'success', 3500);
                        $('.comp-button').html(' Compare Selected Items');
                        $('.compare-checkbox').each(function () {
                            this.checked = false;
                        });
                    }
                    else
                        displayPopupNotification(response.message, 'error', true);
                },
                complete: this.resetLoadWaiting,
                error: this.ajaxFailure
            });
        }
    },

    success_process: function (response) {
        console.log(response);
        AjaxCart.displayProductInflyout(response.flyout);
        if (window.location.href.indexOf("cart") != -1)
            location.reload();
        if (response.updatetopcartsectionhtml) {
            $(AjaxCart.topcartselector).html(response.updatetopcartsectionhtml);
            $('#SubtotalPrice').html(` CART ${response.totalprice}`);
        }
        if (response.updatetopwishlistsectionhtml) {
            $(AjaxCart.topwishlistselector).html(response.updatetopwishlistsectionhtml);
        }
        if (response.updateflyoutcartsectionhtml) {
            $(AjaxCart.flyoutcartselector).replaceWith(response.updateflyoutcartsectionhtml);
        }
        if (response.message) {
            //display notification
            if (response.success === true) {
                //success
                if (AjaxCart.usepopupnotifications === true) {
                    displayPopupNotification(response.message, 'success', true);
                }
                else if (response.message == "Redirect to cart") {
                    location.href = "/cart";
                } else {
                    //specify timeout for success messages
                    displayBarNotification(response.message, 'success', 3500);
                }
                if (response.message == "Redirect to cart")
                    location.href = "/cart";
            }
            else {
                //error
                if (AjaxCart.usepopupnotifications === true) {
                    displayPopupNotification(response.message, 'error', true);
                }
                else {
                    //no timeout for errors
                    displayBarNotification(response.message, 'error', 0);
                }
            }
            if (!response.flyout) {
                location.reload();
            }
            return false;
        }
        if (response.samePageRedirect) {
            window.location.reload();
        }
        if (response.redirect) {
            location.href = response.redirect;
            return true;
        }
        
        return false;
    },

    resetLoadWaiting: function () {
        AjaxCart.setLoadWaiting(false);
    },

    ajaxFailure: function () {
        alert(this.localized_data.AjaxCartFailure);
    },

    cancelList: $("#cancelList").click(function (event) {
        event.stopImmediatePropagation();
        resetForm();
        $(".close").click();
    }),

    saveList: $("#saveList").click(function (event) {
        event.stopImmediatePropagation();

        var wishListBox = $("#wish-list-box");

        var selectedWishListId = $("#wishListDDL").val();

        $("#wish-list-box").modal('hide');

        if (selectedWishListId !== "0") {
            if (wishListBox.attr("data-formselector") !== undefined && wishListBox.attr("data-urladd") !== undefined && selectedWishListId !== "0") {
                var urlAdd = wishListBox.attr("data-urladd");
                urlAdd = setURLWishListId(urlAdd, selectedWishListId);
                AjaxCart.addproducttocart_details(urlAdd, wishListBox.attr("data-formselector"), wishListBox.attr("data-typeId"));
                resetForm();
                return;
            }
            else if (wishListBox.attr("data-formselector") === undefined && wishListBox.attr("data-urladd") !== undefined && selectedWishListId !== "0") {
                var urlAdd = wishListBox.attr("data-urladd");
                urlAdd = setURLWishListId(urlAdd, selectedWishListId);
                AjaxCart.addproducttocart_catalog(urlAdd, wishListBox.attr("data-typeId"));
                resetForm();
                return;
            }
            else {
                resetForm();
                return;
            }
        }

        var newWishListName = $("#newListName").val().trim();

        var wishListNameAlreadyExists = false;

        $("#wishListDDL > option").each(function () {
            if (this.text.trim() === newWishListName) {
                wishListNameAlreadyExists = true;
            }
        });

        if (newWishListName === "") {
            alert("Please provide wishlist name or select wishlist name from the list.");
            return;
        }

        if (wishListNameAlreadyExists === true) {
            alert("Wishlist name already exists.");
            return;
        }

        if ($("#everyone").is(":checked")) {
            isPublicList = true;
        }
        else if ($("#justMe").is(":checked")) {
            isPublicList = false;
        }

        setLoadWaiting(true);

        $.ajax({
            cache: false,
            data: { "wishListName": newWishListName, "isPublic": isPublicList },
            type: "GET",
            url: "/ShoppingCart/SaveWishlistName",
            success: function (data, textStatus, jqXHR) {
                setLoadWaiting(false);
                if (data.id > 0) {
                    $("#wishListDDL").append($("<option></option>").val(data.id).html(data.listName));
                    sortDropDownListByText("wishListDDL");

                    if (wishListBox.attr("data-formselector") !== undefined && wishListBox.attr("data-urladd") !== undefined && data.id !== 0) {
                        var urlAdd = wishListBox.attr("data-urladd");
                        urlAdd = setURLWishListId(urlAdd, data.id);
                        AjaxCart.addproducttocart_details(urlAdd, wishListBox.attr("data-formselector"), wishListBox.attr("data-typeId"));
                        resetForm();
                        $(".close").click();
                        return;
                    }
                    else if (wishListBox.attr("data-formselector") === undefined && wishListBox.attr("data-urladd") !== undefined && data.id !== 0) {
                        var urlAdd = wishListBox.attr("data-urladd");
                        urlAdd = setURLWishListId(urlAdd, data.id);
                        AjaxCart.addproducttocart_catalog(urlAdd, wishListBox.attr("data-typeId"));
                        resetForm();
                        $(".close").click();
                        return;
                    }
                    else {
                        resetForm();
                        $(".close").click();
                        return;
                    }

                    $(".close").click();
                }
                else
                    alert("Something went wrong. Please try again.");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                setLoadWaiting(false);
                alert("Something went wrong. Please try again.");
            }
        });
    }),

    wishListBoxDDL: $("#wishListDDL").change(function (event) {
        event.stopImmediatePropagation();
        if ($(this).val() !== "0") {
            disableNewWishListFields();
        }
        else
            enableNewWishListFields();
    }),

    wishListBoxNewListNameTXT: $("#newListName").keypress(function () {
        if ($("#wishListDDL").val() !== "0")
            $("#wishListDDL").val("0");
    }),

    createWishList: $("#createWishList").click(function (event) {
        event.stopImmediatePropagation();
        if (!isUserLoggedIn()) {
            alert("Please Login to Create a Wishlist.");
            return;
        }
        var newWishListName = $("#newListName").val().trim();

        var wishListNameAlreadyExists = false;

        if (newWishListName === "") {
            alert("Please provide wishlist name or select wishlist name from the list.");
            return;
        }

        if (wishListNameAlreadyExists === true) {
            alert("Wishlist name already exists.");
            return;
        }
        var isPublicList = false;
        if ($("#ListSharedType").val() === 'shared' ) {
            isPublicList = true;
        }

        setLoadWaiting(true);

        $.ajax({
            cache: false,
            data: { "wishListName": newWishListName, "isPublic": isPublicList },
            type: "GET",
            url: "/ShoppingCart/SaveWishlistName",
            success: function (data, textStatus, jqXHR) {
                setLoadWaiting(false);
              
                if (data.id > 0) {
                    alert("Wish List successfully created!");
                    window.location.href = "/wishlist?wishlistid="+data.id;
                }
                else if (!data.success) {
                    alert(data.message);
                }
                else
                    alert("Something went wrong. Please try again.");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                setLoadWaiting(false);
                alert("Something went wrong. Please try again.");
            }
        });
    }),
    addproducttocart_wishlist: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var qty = $(event.target).closest('tr').find('input.qty').val();
        var n1 = urladd.substring(0, urladd.lastIndexOf("/") + 1);
        urladd = n1 + qty;
        $.ajax({
            cache: false,
            url: urladd,
            data: $(event.target).closest('tr').find('select.uom,input.itemId').serialize(),
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    addbulkproducttocart_wishlist: function (urladd) {
        if (this.loadWaiting !== false) {
            return;
        }
        this.setLoadWaiting(true);
        var form = "";//$('#reorder-guide').DataTable().$('tr').find('input:checked').closest('tr').find('select.uom').serializeArray();
        var quantities = []
        $('#tblwishlist input:checked').each(function () {
            quantities.push($(this).closest('tr').find('input.qty').val());
        });
        var form = $('#tblwishlist input:checked').closest('tr').find('select.uom,input.itemId').serializeArray();
        form.push({ name: 'quantities', value: quantities.join() });
        $.ajax({
            async: false,
            cache: false,
            url: urladd,
            data: form,
            type: "POST",
            success: this.success_process,
            complete: this.resetLoadWaiting,
            error: this.ajaxFailure
        });
    },
    //region start new/quick order 

    quickopenproductmodal: function (Url, productId, unitPrice, shoppingCartTypeId, editing ) {
        displayAjaxLoading(true);
        $.ajax({
            cache: false,
            async: true,
            data: { 'productId': productId, unitprice: unitPrice, shoppingCartTypeId: shoppingCartTypeId, editing: editing ?? false },
            type: "GET",
            url: Url,
            success: function (data, textStatus, jqXHR) {
                if (Url.search("LoadQuickProduct") > -1) {
                    $("#quick-product-view-box").html(data.update_section.html);
                    $("#quick-product-view").modal();
                }
                else
                    return;
                displayAjaxLoading(false);
            },
            error: function (jqXHR, textStatus, errorThrown) { displayAjaxLoading(false); }
        });
        return false;
    },
    postItemToQuickCart: function (cartId) {

        if (document.getElementById("product_enteredQuantity_Id").value == "") {
            document.getElementById("product_enteredQuantity_Id").value = 1;
        }
        if (!$('#quick_addprod').prop('checked')) { // check if checkbox is checked or not
            AjaxCart.quickopenproductmodal('/checkout/LoadQuickProductView/', $('#small-searchterms-cart').val());
        }
        else if ($.isNumeric($('#small-searchterms-cart').val())) {
            AjaxCart.addproducttocart_details('/addproducttocartusingSKU/details/' + $('#small-searchterms-cart').val() + '/' + cartId + '/0', '#Quick-product-details-form :input', cartId);
        }
        else if ($('#product_enteredQuantity_Id').val() != "" && $('#small-searchterms-cart').val() != "") {
            $('#product_enteredQuantity_Id').attr("name", $('#product_enteredQuantity_Id').attr("name").replace("Id", $('#selectItem').val()));
            AjaxCart.addproducttocart_details('/addproducttocart/details/' + $('#selectItem').val() + '/' + cartId + '/0', '#Quick-product-details-form', cartId);
        }
    },
    opensuspendshoppingcartmodal: function (urladd, shoppingCartTable) {
        var suspendShoppingCartModal = $("#suspend-shopping-cart-box");
        suspendShoppingCartModal.modal();
        $(shoppingCartTable + " > tbody > tr").each(function () {
            AjaxCart.suspendedItemIds.push($(this).attr('id'))
        });
    },

    createSuspendedCart: $("#suspendCart").click(function (event) {
        event.stopImmediatePropagation();

        var suspendedCartName = $("#suspended-shopping-cart-control").val();
        var suspendedCartEmail = $("#suspended-shopping-cart-control-email").val();
        var suspendedCartComment = $("#suspended-shopping-cart-control-comment").val();

        if (suspendedCartName.trim() == "") {
            alert("Please provide PO / Suspended cart name.");
            return;
        }
        if (suspendedCartEmail.trim() == "") {
            alert("Please provide an Email Address.");
            return;
        }
        if (suspendedCartEmail.trim() != "") {
            const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
            var val = re.test(suspendedCartEmail);
            if (val == false) {
                alert("Please provide a valid Email Address.");
                return;
            }
        }


        if (suspendedCartComment.trim() == "") {
            alert("Please provide Comment.");
            return;
        }



        setLoadWaiting(true);

        $.ajax({
            cache: false,
            contentType: "application/json; charset=utf-8",
            data: {
                'suspendedItemIds': AjaxCart.suspendedItemIds.toString(), 'suspendedCartName': suspendedCartName,
                'suspendedCartEmail': suspendedCartEmail, 'suspendedCartComment': suspendedCartComment
            },
            type: "GET",
            url: "/ShoppingCart/SaveSuspendedShoppingCart",
            success: function (data, textStatus, jqXHR) {
                setLoadWaiting(false);
                if (data.id > 0) {
                    alert("Shopping cart suspended successfully!");
                    window.location.href = "/suspendedcarts";
                }
                else
                    alert("Something went wrong. Please try again.");
            },
            error: function (jqXHR, textStatus, errorThrown) {
                setLoadWaiting(false);
                alert("Something went wrong. Please try again.");
            }
        });
    }),
    deleteSuspendedCart: $(".delete-suspended-cart-button").click(function (event) {
        event.stopImmediatePropagation();
        //var ids = $('#suspended-cart input:checked').closest('tr').attr('id');
        var ids = [];
        $('#suspended-cart input:checked').each(function () {
            $this = $(this);
            if ($this.is(':checked')) {
                ids.push($this.attr('id'));
            }
        });
        ids = ids.filter(function (el) { return el != undefined });
        console.log(ids);
        if (ids != undefined) {
            setLoadWaiting(true);

            $.ajax({
                cache: false,
                contentType: "application/json; charset=utf-8",
                data: { 'suspendedCartIds': ids.toString() },
                type: "GET",
                url: "/ShoppingCart/DeleteSuspendedShoppingCart",
                success: function (data, textStatus, jqXHR) {
                    setLoadWaiting(false);
                    if (data.success) {
                        alert("Shopping cart(s) deleted successfully!");
                        window.location.href = "/suspendedcarts";
                    }
                    else
                        alert("Something went wrong. Please try again.");
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    setLoadWaiting(false);
                    alert("Something went wrong. Please try again.");
                }
            });
        }
        else {
            alert("You must select at least one Suspended Cart to delete.");
        }
    }),

    MarkDeleteAll: function () {
        if (confirm('This will remove all items and delete this instance of your Working Cart')) {
            $('input[name="removefromcart"]').attr("checked", "checked");
            $('#updateSingleItemCart').click();
            return true;
        }
        else {
            $('input[name="removefromcart"]').attr("checked", "false");
            return false;
        }
    },
    markDeleteSingle: function () {
        if (confirm('Are you sure you want to delete this item from you cart?')) {
            const component = $(event.currentTarget).closest('tr').find('input[name="removefromcart"]');
            component.attr("checked", "checked");
            $('#updateSingleItemCart').click();
        }
    },
    editSingleEntry: function (id, unitPrice, shoppingCartTypeId, editing ) {
        displayAjaxLoading(true);
        console.log("check");
        AjaxCart.quickopenproductmodal('/checkout/LoadQuickProductView/', id, unitPrice, shoppingCartTypeId, editing);
        $('#UpdatedShoppingCartItemId').val(id);
        var cartId = $(event.currentTarget).closest('tr').attr('id');
        $("#product_cartId").attr("name", $("#product_cartId").attr("name").replace("Id", id));
        $("#product_cartId").val(cartId);
        $('#btns_update').show();
        $('#btns_create').hide();
        displayAjaxLoading();
    },
    changeQuantity: function (ProductId, action, element) {
        // parse quantity to integer
        var qty = parseInt($(element).text());
        this.changeQuantityIds.push(ProductId);
        this.changeQuantityIds = [...new Set(this.changeQuantityIds)];
        qty = (action == 'i') ? (qty + 1) : (qty - 1);
        qty = qty > 0 ? qty : 1;
        const allElement = document.querySelectorAll(element);
        allElement.forEach(elm => {
            elm.innerText = qty;
        });
        
        this.quantities[this.changeQuantityIds.indexOf(ProductId)] = qty;
        // below is a trigger machanism implemented by ALi Ahmad, which will not allow a lot of requests in case user quickly adds quantity,
        // instead it will hold a moment untill user stops clicking and sends the request to the server to make change in quantity

        displayAjaxLoading(true);
        if (!this.quantityWaiting) {
            this.quantityWaiting = true;
            setTimeout(() => {
                this.quantityWaiting = false;
                // sending request to backend
                $.ajax({
                    cache: false,
                    contentType: "application/json; charset=utf-8",
                    data: { ProductIds: String(this.changeQuantityIds), Quantities: String(this.quantities) },
                    type: "GET",
                    url: "/ShoppingCart/ChangeQuantities",
                    success: function (data, textStatus, jqXHR) {
                        displayBarNotification('Quantity changed successfully.', 'success', 2000);
                        setTimeout(() => { document.location.reload(); }, 500);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        displayBarNotification('An error occured while changing quantity. Please reload and try again.', 'error', 2000);
                    }
                });
                this.changeQuantityIds = [];
                displayAjaxLoading(false);
            }, 2000);
        }
        else {
            return;
        }
        
    },
    displayProductInflyout: function (data) {

        try {
            data = data.value;
            if (data.shouldDisplay) {
                const item = `<div class="cart_box">
                            <img src="${data.pictureURL}" class="cart_list_img">
                            <div>
                                <h4>${data.name}</h4>
                                <div class="flex_cart_box">
                                    <div>
                                        <div class="form-group custom-qty-box">
                                            <a href="javascript:void(0)" class="border-button-flyoyt" onclick="AjaxCart.changeQuantity(${data.productId}, 'd', '#qty_${data.productId}')"><img class="sign-img" src="/images/UIComponents/minus.svg" alt=""></a>
                                            <span class="qty-text" id="qty_${data.productId}">${data.quantity}</span>
                                            <a href="javascript:void(0)" class="border-button-flyoyt" onclick="AjaxCart.changeQuantity(${data.productId}, 'i', '#qty_${data.productId}')"><img class="sign-img" src="/images/UIComponents/plud.svg" alt=""></a>
                                        </div>
                                    </div>
                                    <div>
                                        <h2>${data.price.replace('$', '')} USD</h2>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <hr />`;
                $('.cart_body').append(item);
            }
            else {
                $(`#qty_${data.productId}`).text(data.quantity);
            }
            $('#flyoutCount').text(`${data.totalItems} items in your cart`);
        }
        catch (ex) { }


    }
};

$(".suspended-add-to-cart").click(function (event) {
    event.stopImmediatePropagation();

    //var id = $(this).attr("id");
    var newids = [];
    $('#suspended-cart input:checked').each(function () {
        newids.push($(this).closest('tr').attr('id'));
    });
    newids = newids.filter(function (el) { return el != undefined });
    if (newids.length > 0) {
        $.ajax({
            cache: false,
            contentType: "application/json; charset=utf-8",
            data: { 'suspendedShoppingCartIds': newids.join() },
            type: "GET",
            url: "/ShoppingCart/ConvertSuspendedCartToShoppingCart",
            success: function (data, textStatus, jqXHR) {
                setLoadWaiting(false);
                if (data.success) {
                    window.location.href = "/cart";
                }
                else
                    alert("Something went wrong. Please try again.");
            },
            error: function (jqXHR, textStatus, errorThrown) {
            }
        });
    }
    else {
        alert("You must select at least one Suspended Cart to merge into your current working cart.");
    }
})

function isUserLoggedIn() {
    var isUserLoggedIn = false;
    $.ajax({
        cache: false,
        async: false,
        type: "GET",
        url: "/Customer/IsUserLoggedIn",
        success: function (data, textStatus, jqXHR) {
            isUserLoggedIn = data;
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });
    return isUserLoggedIn;
}

function isProductHasAttributes(productId) {
    var returnedValue = false;
    $.ajax({
        cache: false,
        async: false,
        data: { 'productId': productId },
        type: "GET",
        url: "/ShoppingCart/IsProductHasAttributes",
        success: function (data, textStatus, jqXHR) {
            returnedValue = data;
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });
    return returnedValue;
}

function populateWishListDropDown(page) {
    $.ajax({
        cache: false,
        type: "GET",
        url: "/ShoppingCart/PopulateWishList",
        success: function (data, textStatus, jqXHR) {
            if (data != null) {
                if (page == "WishListBox") {
                    $("#wishListDDL").empty();
                    $("#wishListDDL").append($("<option></option>").val("0").html("Select one"));
                    for (i in data) {
                        $("#wishListDDL").append($("<option></option>").val(data[i].id).html(data[i].listName));
                    }
                    sortDropDownListByText("wishListDDL");
                }
                else {
                    optionGroup = $("<optgroup/>", {
                        label: "Shared",
                        value: "sharedGroup"
                    });

                    for (i in data) {
                        if (data[i].isSharedList) {
                            option = $("<option/>", {
                                value: data[i].id,
                                text: data[i].listName
                            });
                            optionGroup.append(option);
                        }
                    }
                    $("#wishListDDLWishlist").append(optionGroup);

                    optionGroup = $("<optgroup/>", {
                        label: "Personal",
                        value: "personalGroup"
                    });
                    for (i in data) {
                        if (data[i].isSharedList === false) {
                            var option = $("<option/>", {
                                value: data[i].id,
                                text: data[i].listName
                            });
                            optionGroup.append(option);
                        }
                    }
                    $("#wishListDDLWishlist").append(optionGroup);

                    searchParams = new URLSearchParams(window.location.search);
                    if (searchParams.has('wishListId'));
                    {
                        var wishListId = searchParams.get('wishListId');
                        if (wishListId !== null)
                            $("#wishListDDLWishlist").val(wishListId);
                    }
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) { }
    });
}

function sortDropDownListByText(selectId) {
    var foption = $('#' + selectId + ' option:first');
    var soptions = $('#' + selectId + ' option:not(:first)').sort(function (a, b) {
        return a.text == b.text ? 0 : a.text < b.text ? -1 : 1
    });
    $('#' + selectId).html(soptions).prepend(foption);
};

function setWishListForm(wishListBox, urladd, typeId, formselector, productId) {
    var wishListBox = $("#wish-list-box");

    wishListBox.attr("data-urladd", urladd);
    wishListBox.attr("data-typeId", typeId);
    if (formselector !== null)
        wishListBox.attr("data-formselector", formselector);
    wishListBox.attr("data-productId", productId);
}

function setLoadWaiting(display) {
    displayAjaxLoading(display);
    this.loadWaiting = display;
}

function resetForm() {
    $("#newListName").val("");
    $("#wishListDDL").val(0);

    var wishListBox = $("#wish-list-box");
    wishListBox.removeAttr("data-urladd");
    wishListBox.removeAttr("data-typeId");
    wishListBox.removeAttr("data-productId");
    if (wishListBox.attr("data-formselector") !== null || wishListBox.attr("data-formselector") !== undefined)
        wishListBox.removeAttr("data-formselector");
}

function setURLWishListId(urlAdd, wishListId) {
    if (urlAdd !== "") {
        urlAdd = urlAdd.slice(0, -1) + wishListId;
    }

    return urlAdd;
}

function disableNewWishListFields() {
    $('#newListName').val("");
    $('#newListName').attr("disabled", "disabled");
    $('input[name=wishListType]').attr("disabled", "disabled");
}

function enableNewWishListFields() {
    $('#newListName').removeAttr("disabled");
    $('input[name=wishListType]').removeAttr("disabled");
}