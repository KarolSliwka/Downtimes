$(document).ready(function () {
    /**
     * This function will pass filtered customer into 
     * customers dropdown filed based on selected primary organization 
    */
    const department = $('#hrm_department');
    department.change(function (event) {
        event.preventDefault();
        if (department.val() !== null) {
            var customers = $("#hrm_customer");
            customers.prop('disabled', false);
            customers.removeClass("selected-style");
            customers.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
            $.ajax({
                type: "GET",
                url: RootUrl + "Hrms/GetCustomers/",
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                data: { department: department.val() },
                success: async function (response) {
                    customers.empty().append('<option selected="selected" value="">Wybierz..</option>');
                    $.each(response, function () {
                        let innerText = Object.values(this)[1];
                        customers.append($("<option></option>").prop("value", Object.values(this)[0]).prop("text", innerText));
                    });
                },
            });
        }
        else {
            customers.empty().append('<option selected="selected" value="">Wybierz..</option>');
            customers.prop('disabled', true);
        }
    });
    /**
     * This function will pass filtered customer into
     * customers dropdown filed based on selected primary organization 
    */
    const category = $('#hrm_category');
    category.change(function (event) {
        event.preventDefault();
        if (category.val() !== null ) {
            var reason = $("#hrm_reason");
            reason.prop('disabled', false);
            reason.removeClass("selected-style");
            reason.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
            $.ajax({
                type: "GET",
                url: RootUrl + "Hrms/GetReasons/",
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                },
                data: { category: category.val() },
                success: async function (response) {
                    reason.empty().append('<option selected="selected" value="">Wybierz..</option>');
                    console.log(response);
                    $.each(response, function () {
                        let innerText = Object.values(this)[0] + " (" + Object.values(this)[1] + ")";
                        reason.append($("<option></option>").prop("value", Object.values(this)[0]).prop("text", innerText));
                    });
                },
            });
        }
        else {
            reason.empty().append('<option selected="selected" value="">Wybierz..</option>');
            reason.prop('disabled', true);
        }
    });
});

/**
 * This function will make fields disabled on form clear
 */
function FormClear() {
    $("[name='Customer']").prop('disabled', true);
    $("[name='Reason']").prop('disabled', true);
    $("#hrm_time_to").prop('disabled', true);
    $('#hrm_total_hours').text('00.00');
    $('#hrm_hours').text('00.00');
    $(".is-corrected").next().html("Nie");
}

/**
 * 
 */
function HrmEdit() {
    const category = $('#hrm_category');
    if (category.val() !== null) {
        var reason = $("#hrm_reason");
        reason.prop('disabled', false);
        reason.removeClass("selected-style");
        reason.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        $.ajax({
            type: "GET",
            url: RootUrl + "Hrms/GetReasons/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { category: category.val() },
            success: async function (response) {
                reason.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    let innerText = Object.values(this)[0] + " (" + Object.values(this)[1] + ")";
                    reason.append($("<option></option>").prop("value", Object.values(this)[0]).prop("text", innerText));
                });
            },
        });
    }
}