/*
 * This function will populate information to WeekFrom
 */
const f_year_from = $('#year_from');
const f_year_to = $('#year_to');
const f_week_from = $('#week_from');
const f_week_to = $('#week_to');
const f_department = $('#department');
const f_claimed = $('#claimed');
const f_user = $('#userId');
const f_button = $('#apply_filters');

const disableFields = (fields, disable = true) => {
    fields.forEach(field => field.prop('disabled', disable));
};

const unlockFields = (fields, disable = false) => {
    fields.forEach(field => field.prop('disabled', disable));
};

const clearFields = (fields) => {
    fields.forEach(field => field.val(''));
};

const populateOptions = (element, options) => {
    element.empty().append('<option selected="selected" value="">Wybierz..</option>');
    options.forEach(option => {
        element.append($("<option></option>").prop("value", option).text(option));
    });
};

const fetchData = (url, data, successCallback) => {
    $.ajax({
        type: "GET",
        url: RootUrl + url,
        headers: {
            RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        data: data,
        success: successCallback
    });
};

f_year_from.change(event => {
    event.preventDefault();
    if (f_year_from.val()) {
        fetchData("Downtimes/GetYearTo/", {
            year_from: f_year_from.val()
        }, response => {
            f_year_to.prop('disabled', false);
            populateOptions(f_year_to, response);
        });
        fetchData("Downtimes/GetWeeksFrom/", {
            year_from: f_year_from.val()
        }, response => {
            f_week_from.prop('disabled', false);
            populateOptions(f_week_from, response);
        });
    } else {
        disableFields([f_week_from, f_year_to, f_week_to, f_department, f_claimed, f_user]);
        clearFields([f_week_from, f_year_to, f_week_to, f_department, f_claimed, f_user]);
    }
});

f_week_from.change(event => {
    event.preventDefault();
    if (f_year_to.val()) {
        fetchData("Downtimes/GetWeeksTo/", {
            year_from: f_year_from.val(),
            year_to: f_year_to.val(),
            week_from: f_week_from.val()
        }, response => {
            f_week_to.prop('disabled', false);
            populateOptions(f_week_to, response);
        });
    } else {
        f_week_to.val('').prop('disabled', true);
    }
});

f_year_to.change(event => {
    event.preventDefault();
    if (f_year_to.val()) {
        fetchData("Downtimes/GetWeeksTo/", {
            year_from: f_year_from.val(),
            year_to: f_year_to.val(),
            week_from: f_week_from.val()
        }, response => {
            f_week_to.prop('disabled', false);
            populateOptions(f_week_to, response);
        });
    } else {
        f_week_to.val('').prop('disabled', true);
    }
});

f_department.change(event => {
    event.preventDefault();
    if (f_department.val()) {
        fetchData("Downtimes/GetCreatedBy/", {
            year_from: f_year_from.val(),
            year_to: f_year_to.val(),
            week_from: f_week_from.val(),
            week_to: f_week_to.val(),
            claimed: f_claimed.val(),
            department: f_department.val()
        }, response => {
            f_user.empty().append('<option selected="selected" value="">Wybierz..</option>');
            response.forEach(item => {
                f_user.append($("<option></option>").prop("value", item.value).text(item.text));
            });
        });
    } else {
        clearFields([f_user]);
        populateOptions([f_user]);
    }
});

const updateOptions = () => {
    const currentDepartment = f_department.val();
    const currentUser = f_user.val();

    fetchData("Downtimes/GetCreatedBy/", {
        year_from: f_year_from.val(),
        year_to: f_year_to.val(),
        week_from: f_week_from.val(),
        week_to: f_week_to.val(),
        claimed: f_claimed.val(),
        department: f_department.val()
    }, response => {
        if (f_department.val() !== currentDepartment) {
            f_user.empty().append('<option selected="selected" value="">Wybierz..</option>');
            response.forEach(item => {
                f_user.append($("<option></option>").prop("value", item.value).text(item.text));
            });
        }
    });

    fetchData("Downtimes/GetPrimaries/", {
        year_from: f_year_from.val(),
        year_to: f_year_to.val(),
        week_from: f_week_from.val(),
        week_to: f_week_to.val(),
    }, response => {
        if (f_user.val() !== currentUser) {
            f_department.empty().append('<option selected="selected" value="">Wybierz..</option>');
            response.forEach(item => {
                f_department.append($("<option></option>").prop("value", item).text(item));
            });
        }
    });
};

function checkFieldsNotEmpty() {
    return f_year_from.val() && f_year_to.val() && f_week_from.val() && f_week_to.val();
}

const checkFilters = () => {
    if (checkFieldsNotEmpty()) {
        unlockFields([f_department, f_claimed, f_user, f_button]);
        updateOptions();
    } else {
        disableFields([f_department, f_claimed, f_user, f_button]);
        clearFields([f_department, f_claimed, f_user]);
    }
};

$('#clear_filters').click(() => {
    // clear fields
    [f_year_to, f_week_from, f_week_to, f_department, f_user].forEach(field => {
        field.empty().append('<option selected="selected" value="">Wybierz..</option>');
    });

    // disable fields
    disableFields([f_year_to, f_week_from, f_week_to, f_department, f_claimed, f_user, f_button]);

    // clear parameters
    ["yearFrom", "weekFrom", "yearTo", "weekTo", "department", "claimed", "userId"].forEach(param => {
        sessionStorage.setItem(param, "");
    });
});