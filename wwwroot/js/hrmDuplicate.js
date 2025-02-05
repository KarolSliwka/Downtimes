$(document).ready(function () {
    let p = sessionStorage.getItem("people_qty");
    let s = sessionStorage.getItem("start_time");
    let e = sessionStorage.getItem("end_time");
    if (p == null) {
        sessionStorage.setItem("people_qty", parseInt($('#hrm_d_people').val()));
    }
    if (s == null) {
        sessionStorage.setItem("start_time", new Date($('#hrm_d_stime').val()));
    }
    if (e == null) {
        let endDate;
        if (new Date($('#hrm_d_etime').val()) == "Invalid Date") {
            endDate = "";
        } else {
            endDate = new Date($('#hrm_d_etime').val());
        }
        sessionStorage.setItem("end_time", endDate);
    }

    history.pushState(null, null, location.href);
    window.onpopstate = function () {
        history.go(1);
    };

    $('#commentary').html('');

});

// Check end date time
function HRMEdit() {
    const s_time = new Date(sessionStorage.getItem("start_time"));
    const e_time = new Date(sessionStorage.getItem("end_time"));
    const v_endtime = $('#hrm_d_etime');
    const f_endtime = new Date(v_endtime.val());

    // no date in end date field
    if (!!e_time) {
        // new end date is before or equal to start date time
        if (f_endtime <= s_time) {
            // apply new date
            //let newEndDate = formatDate(new Date());
            //v_endtime.val(newEndDate);

            end60min = moment(s_time).add(60, 'minutes').toDate();
            let newEndDate = formatDate(end60min);
            v_endtime.val(newEndDate);

            let message = "Wybrałeś złą datę, która jest przed datą rozpoczęcia.\n " +
                "Zmieniliśmy Twoją datę na: " + newEndDate;
            sweetAlert("Coś poszło nie tak...", message, "warning");
        } else {
            // new end date is before or equal to start date time
            let selectedDate = new Date(v_endtime.val());
            if (selectedDate < new Date()) {
                // apply new date
                let newEndDate = formatDate(new Date());
                v_endtime.val(newEndDate);
                let message = "Wybrałeś złą datę, która jest przed datą rozpoczęcia.\n " +
                    "Zmieniliśmy Twoją datę na: " + newEndDate;
                sweetAlert("Coś poszło nie tak...", message, "warning");
            }
        }
    // start & end date are in 
    } else if (!!s_time && !!e_time) {
        // new end date is before or equal to start date time
        let selectedDate = $('#hrm_d_etime').val();
        if (f_endtime <= s_time || selectedDate < f_endtime) {
            // apply new date
            //let newEndDate = formatDate(new Date());
            //v_endtime.val(newEndDate);
            end60min = moment(selectedDate).add(60, 'minutes').toDate();
            let newEndDate = formatDate(end60min);
            v_endtime.val(newEndDate);
            let message = "Wybrałeś złą datę, która jest przed datą rozpoczęcia.\n " +
                "Zmieniliśmy Twoją datę na: " + newEndDate;
            sweetAlert("Coś poszło nie tak...", message, "warning");
        }
    }
    CalculateTimes();
};

// Check people qty
function HRMPeople() {
    const p_qty = parseInt(sessionStorage.getItem("people_qty"));
    const update_p_qty = $('#hrm_d_people');
    const new_p_qty = update_p_qty.val();

    if (new_p_qty >= p_qty || new_p_qty <= 0) {
        update_p_qty.val(1);
        $('#add_btn').prop('disabled', false);
        let message = "Maksymalna ilość osób pobrana z marketu nie może być równa, ani przekraczać wartości pracowników z duplikowanego rekordu (" +
            p_qty + ")\n\n"
        message = message + "Ilość pracowników została zmieniona na: 1\n";
        sweetAlert("Coś poszło nie tak...", message, "warning");
    } else {
        $('#add_btn').prop('disabled', false);
    }
    CalculateTimes();
};

// Clear fields
function HRMDuplicateClear() {
    $('#hrm_d_etime').val('');
    $('#hrm_d_people').val('');
    $('#commentary').val('');
    $('#hrm_d_total_hours_edit').text('00.00');
    $('#hrm_d_hours_edit').text('00.00');
    $('#add_btn').prop('disabled', true);
}

/**
 * Extact correct datetime string format from date
 */
function padTo2Digits(num) {
    return num.toString().padStart(2, '0');
}
function formatDate(date) {
    return (
        [
            date.getFullYear(),
            padTo2Digits(date.getMonth() + 1),
            padTo2Digits(date.getDate()),
        ].join('-') +
        ' ' +
        [
            padTo2Digits(date.getHours()),
            padTo2Digits(date.getMinutes()),
        ].join(':')
    );
}

function HRMClearCoockies() {
    sessionStorage.clear();
}

/**
 * This function will calculate duartion time and total hours
 * based on selected dates and number of affected people
 */
function CalculateTimes() {
    let people = $('#hrm_d_people').val();
    let startTime = $('#hrm_d_stime').val();
    let endTime = $('#hrm_d_etime').val();
    // with affected employees
    if (endTime !== "" && people > 0) {
        let difference = new Date(endTime).getTime() - new Date(startTime).getTime(); // This will give difference in milliseconds
        let resultInMinutes = Math.round(difference / 60000.00);
        let resultInHoursPeople = ((resultInMinutes * people) / 60.00).toFixed(2);
        $('#hrm_d_total_hours_edit').text(resultInHoursPeople);
    } else {
        $('#hrm_d_total_hours_edit').text('00.00');
    }
    // without affected employees
    if (endTime !== "") {
        let difference = new Date(endTime).getTime() - new Date(startTime).getTime(); // This will give difference in milliseconds
        let resultInMinutes = Math.round(difference / 60000.00);
        let resultInHours = (resultInMinutes / 60.00).toFixed(2);
        $('#hrm_d_hours_edit').text(resultInHours);
    } else {
        $('#hrm_d_hours_edit').text('00.00');
    }
}