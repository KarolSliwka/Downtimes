$(document).ready(function () {
    // Tooltip activate 
    $('[data-bs-toggle=tooltip]').tooltip();

    // Toggle hamburger button
    const navToggle = document.getElementById('btn-hamburger');
    navToggle.addEventListener('click', () => {
        navToggle.classList.toggle('open')
    });

    $(".is-corrected").change(function () {
        let label = $(this).next();
        if (this.checked == false) {
            label.html("Nie");
        }
        else {
            label.html("Tak");
        }
    });
});

/*
    * This function will remove disabled attribute 
    * from second dorpodwon select list 
    */
const export_year = $('#export_year');
export_year.change(function (event) {
    event.preventDefault();
    if (export_year.val() !== null) {
        const week_from = $("#export_week_from");
        const week_to = $("#export_week_to");
        week_from.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        week_to.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        week_from.prop('disabled', false);
        $.ajax({
            type: "GET",
            url: RootUrl + "Home/GetWeeks/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { year: export_year.val() },
            success: function (response) {
                week_from.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    week_from.append($("<option></option>").prop("value", this).prop("text", this));
                });
                week_to.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    week_to.append($("<option></option>").prop("value", this).prop("text", this));
                });
            },
        });
    }
    else {
        week_from.empty().append('<option selected="selected" value="">Wybierz..</option>');
        week_to.empty().append('<option selected="selected" value="">Wybierz..</option>');
        week_from.prop('disabled', true);
        week_to.prop('disabled', true);
    }
});

/**
    * Chabge disabled proeprty on field change
    */
const week_from = $("#export_week_from");
const week_to = $("#export_week_to");
week_from.change(function (event) {
    event.preventDefault();
    if (week_from.val != null || week_from.val != "") {
        week_to.prop('disabled', false);
    }
    else {
        week_to.prop('disabled', true);
    }
});

/**
    * This function will pass filtered customer into 
    * customers dropdown filed based on selected primary organization 
*/
const department = $('#department_create');
department.change(function (event) {
    event.preventDefault();
    if (department.val() !== null) {
        var customers = $("#customer");
        customers.prop('disabled', false);
        customers.removeClass("selected-style");
        customers.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetCustomers/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { department: department.val() },
            success: async function (response) {
                customers.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    customers.append($("<option></option>").prop("value", this).prop("text", this));
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
const category = $('#category_create');
category.change(function (event) {
    event.preventDefault();
    if (category.val() !== null ) {
        var reason = $("#reason");
        reason.prop('disabled', false);
        reason.removeClass("selected-style");
        reason.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetReasons/",
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
    else {
        reason.empty().append('<option selected="selected" value="">Select..</option>');
        reason.prop('disabled', true);
    }
});

/**
* This function will pass filtered claims status into
* claim filed based on selected category and reason
*/
const reason = $('#reason');
const claims = $('#claims');
const claims_hidden = $('#claims_hidden');
reason.change(function (event) {
    event.preventDefault();
    if (reason.val() !== null) {
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetStatus/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { category: category.val(), reason: reason.val() },
            success: async function (response) {
                claims.empty();
                $.each(response, function () {
                    if (this == "NO") {
                        claims.append($("<option></option>").prop("value", this).prop("text", "Nie"));
                        claims_hidden.val(false);
                    } else {
                        claims.append($("<option></option>").prop("value", this).prop("text", "Tak"));
                        claims_hidden.val(true);
                    }
                });
            },
        });
    }
    else {
        claims.empty().append('<option selected="selected" value="">Błąd..</option>');
    }
});

/**
    * This function will prevent user from using tab 
    * and carret in commentary field in Create record
    */
$('textarea').on('keyup keypress', function (e) {
    var keyCode = e.keyCode || e.which;
    if (keyCode === 13 || keyCode === 39 || keyCode === 186 || e.key === ";") {
        e.preventDefault();
        return false;
    }
});

/*
    * This function will add disabled attribute to 
    * export week select field when form is cleared
    */
$('#export_clear_btn').click(function () {
    $('#export_week_from').prop('disabled', true);
    $('#export_week_to').prop('disabled', true);
    $('select').removeClass('selected-style');
});

/*
    * This function will add class to not empty select field in forms
    */
function checkEmpty(elem) {
    if (elem.val() != '') {
        elem.addClass('selected-style');
    } else {
        elem.removeClass('selected-style');
    }
}

$('input, select, textarea').change(function () {
    checkEmpty($(this));
});

/*
    * This fucntion will reset style when clicekd on clear button 
    */
$('#create_reset').click(function () {
    let reason = $("#reason");
    let customer = $("#customer");
    let type = $("#type");
    let date_from = $('#date_from');
    let date_to = $('#date_to');

    reason.empty().append('<option selected="selected" value="">Wybierz..</option>');
    customer.empty().append('<option selected="selected" value="">Wybierz..</option>');
    reason.prop("disabled", true);
    customer.prop("disabled", true);
    type.prop("disabled", true);
    date_from.prop("disabled", true);
    date_to.prop("disabled", true);

    $('select').removeClass('selected-style');
    $('input').removeClass('selected-style');
    $('textarea').removeClass('selected-style');
    $('#customer_list').prop('disabled', true);
    $('#reason_list').prop('disabled', true);
    $('#machine').prop('disabled', true);
    $('#add_btn').prop('disabled', false);
    $('#calculated_hours').text('00.00');
    $('#claims').empty('?');
    sessionStorage.clear();
});

/*
    * This function will set cookie 
    * based on choosen machine/line
    */
const machine = $('#machine');
machine.change(function (event) {
    event.preventDefault();
    if (machine.val() !== null) {
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetCookie/",
            data: { machineCookie: machine.val() },
            success: function (response) {
                $.each(response, function () {
                    sessionStorage.setItem("op_time", response);
                });
            },
        });
    }
    else {
        sessionStorage.setItem("op_time","");
    }
});

let btnUserCretea = $("#userCreateClear");
btnUserCretea.click(function () {
    $("#isHrm").nextElementSibling.innerHTML = "No";
});

/**
 * This function will add active link class to an element
 */
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.nav-link').forEach(link => {
        if (!!link.getAttribute('href')) {
            if (link.getAttribute('href').toLowerCase() === location.pathname.toLowerCase()) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        }
    });
});

/**
 * This function will get parameters from url path 
 * and pass it into select fields to keep them visible
 */
function getParameters() {
    let yearFrom = $("#year_from").val();
    let weekFrom = $("#week_from").val();
    let yearTo = $("#year_to").val();
    let weekTo = $("#week_to").val();
    let claimed = $("#claimed").val();
    let department = $("#department").val();
    let userId = $("#userId").val();
    sessionStorage.setItem("yearFrom", yearFrom);
    sessionStorage.setItem("weekFrom", weekFrom);
    sessionStorage.setItem("yearTo", yearTo);
    sessionStorage.setItem("weekTo", weekTo);
    sessionStorage.setItem("department", department);
    sessionStorage.setItem("claimed", claimed);
    sessionStorage.setItem("userId", userId);
}

setParameters();

function setParameters() {
    let yearFrom = sessionStorage.getItem("yearFrom");
    let weekFrom = sessionStorage.getItem("weekFrom");
    let yearTo = sessionStorage.getItem("yearTo");
    let weekTo = sessionStorage.getItem("weekTo");
    let department = sessionStorage.getItem("department");
    let claimed = sessionStorage.getItem("claimed");
    let userId = sessionStorage.getItem("userId");

    if (yearFrom !== null && yearFrom.length > 0) {
        $('#year_from').val(yearFrom);
        $('#week_from').val(weekFrom).prop('disabled', false);
        $('#year_to').val(yearTo).prop('disabled', false);
        $('#week_to').val(weekTo).prop('disabled', false);
        $('#department').val(department).prop('disabled', false);
        $('#claimed').val(claimed).prop('disabled', false);
        $('#userId').val(userId).prop('disabled', false);
    }
}

/**
 * This function will export data from table in List page
 * excel file in user friendly format
 */
function ExcelExport(type) {
    let data = $('#data_result tr:has(td)').map(function (i, v) {
        let $td = $('td', this);
        return {
            'Year & Week': $td.eq(17).text() + "_W" + $td.eq(18).text(),
            'Evenet Start Time': $td.eq(4).text(),
            'Event End Time': $td.eq(5).text(),
            'Primary Organization': $td.eq(6).text(),
            'Customer': $td.eq(7).text(),
            'Category': $td.eq(9).text(),
            'Reason': $td.eq(10).text(),
            'People Affected': Number($td.eq(11).text()),
            'Total Hours': parseFloat($td.eq(12).text()),
            'Comments': $td.eq(13).text(),
            'Created By': $td.eq(19).text(),
            'Creation Date': $td.eq(20).text(),
        }
    }).get();

    let ws = XLSX.utils.json_to_sheet(data);
    let wb = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(wb, ws, "Downtimes");
    XLSX.writeFile(wb, "ExcelExport.xlsx");
};

/**
 * This function will export data from table in List page 
 * excel file prepared for Raptor input upload
 */
function RaptorExport(type) {
    let data = $('#data_result tr:has(td)').map(function (i, v) {
        let $td = $('td', this);
        return {
            'Site': $td.eq(0).text(),
            'Date': $td.eq(2).text(),
            'Time': $td.eq(3).text(),
            'Primary Organization': $td.eq(6).text(),
            'Customer': $td.eq(7).text(),
            'Category': $td.eq(9).text(),
            'Reason': $td.eq(10).text(),
            'Total Hours': $td.eq(12).text(),
            'Comments': $td.eq(13).text(),
            'To-Be Claimed': $td.eq(21).text(),
            'Machine': "",
            'Approver Email/ADID':"",
            '':'',
        }
    }).get();

    let ws = XLSX.utils.json_to_sheet(data);
    let wb = XLSX.utils.book_new();

    XLSX.utils.book_append_sheet(wb, ws, "DowntimesRaptor");
    XLSX.writeFile(wb, "DowntimesRaptor.csv", {FS:";"});
};

/**
 * This function will pass filtered values into machines/lines 
 * based on selected primary orgniazation & type
 * on Primary Organization field
*/
function GetMachines() {
    let type = $('#type');
    let dep = $('#department_create');
    let machine = $("#machine");

    if (type.val() !== null & dep.val() !== null ) {
        machine.prop('disabled', false);
        type.prop('disabled', false);
        machine.removeClass("selected-style");
        machine.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetMachines/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { department: dep.val(), type: type.val() },
            success: async function (response) {
                machine.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    machine.append($("<option></option>").prop("value", Object.values(this)[4]).prop("text", Object.values(this)[3]));
                });
            },
        });
    }
    else {
        machine.empty().append('<option selected="selected" value="">Wybierz..</option>');
        machine.prop('disabled', true);
    }
};

/**
 * This function will pass filtered values into machines/lines 
 * based on selected primary orgniazation & type
 * on Primary Organization field
*/
function GetTypes() {
    let type = $('#type');
    let dep = $('#department_create');
    if (dep.val() !== null) {
        type.removeClass("selected-style");
        type.prop('disabled', false);
        type.empty().append('<option selected="selected" value="" disabled="disabled">Wybierz..</option>');
        $.ajax({
            type: "GET",
            url: RootUrl + "Downtimes/GetTypes/",
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            data: { department: dep.val() },
            success: async function (response) {
                type.empty().append('<option selected="selected" value="">Wybierz..</option>');
                $.each(response, function () {
                    type.append($("<option></option>").prop("value", Object.values(this)[0]).prop("text", Object.values(this)[1]));
                });
            },
        });
    }
    else {
        type.empty().append('<option selected="selected" value="">Wybierz..</option>');
        type.prop('disabled', true);
    }

    GetMachines();
};
