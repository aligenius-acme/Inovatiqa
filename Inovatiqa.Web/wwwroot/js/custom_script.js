$(document).ready(function () {

    function Paginate(tableId) {
        console.log(`Paginating ${tableId}`);
    }

    new WOW().init();
    $('#select-all').click(function(event) {
        var $that = $(this);
        $(':checkbox').each(function() {
            this.checked = $that.is(':checked');
        });
    });
 
   
  $('#formcontinue').click(function() {
                var optionValue = $("#CustomerType").val();
                  
                  if (optionValue =="vendorType")
                  {
                      $('#company').show();
                      $('#companyField').show();
                    $('#account-details').show();
                    $('#passwordArea').show();
                    $('#alternate_shipping_form').show();
                  }
                  else if (optionValue == "customerType")
                  {
                      $('#company').hide();
                      $('#companyField').hide();
                    $('#alternate_shipping_form').hide();
                      $('#passwordArea').show();
                      $('#account-details').show();
                  }
                  else{
                      $('#account-details').hide();
                      $('#passwordArea').hide();
                      $('#alternate_shipping_form').hide();
                  }
        });


   $('.click-check_a').click(function() {
                 
                    $(".unchecked-hidefrom_a").slideToggle();
        });

   

    $(".shipping_details :checkbox").on("change", function() {
        $(this).parent().toggleClass("greenBackground", this.unchecked);
    });



    $("#flip").click(function(e) {
        e.preventDefault();
        $("#panel").slideToggle("slow");


 
    });
    $(".custom-control-input").change(function() {
        var ischecked = $(this).is(':checked');
        if (!ischecked) {
            $(this).closest('tr').css('background', '#fff');
        } else {
            $(this).closest('tr').css('background', '#F0FFDD');
        }
    });
     
    $(".view_all_cat").click(function() {
        $(".hidden_cat").css({
            "display": "flex"
        });
        $(".hidden_img").slideDown();
        $(this).hide();
    });
    /*$('#banner_slider').owlCarousel({
        margin: 30,
        loop: true,
        slideSpeed: 300,
        paginationSpeed: 400,
        autoplay: true,
        nav: false,
        pagination: false,
        dots: true,
        navText: ["<i class='fa fa-angle-left'></i>", "<i class='fa fa-angle-right'></i>"],
        responsive: {
            0: {
                items: 1
            },
            480: {
                items: 1
            },
            700: {
                items: 1
            },
            1000: {
                items: 1
            },
            1100: {
                items: 1
            }
        }
    });*/
    $(document).on("click", ".tab_title a", function() {
        var data_val = $(this).attr("data-val");
        $(".tab_title a").removeClass("active");
        $(this).addClass("active");
        $(".tab_body").removeClass("active");
        $(".tab_body." + data_val).addClass("active");
    });
});