$(document).ready(function () {
    let p = sessionStorage.getItem("people_qty");
    let s = sessionStorage.getItem("start_time");
    let e = sessionStorage.getItem("end_time");
    if (p == null) {
        sessionStorage.setItem("people_qty", parseInt($('#hrm_people').val()));
    }
    if (s == null) {
        sessionStorage.setItem("start_time", new Date($('#hrm_time_from').val()));
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
});


// Clear fields
function HRMEditClear() {
    $('#hrm_etime').val('');
    $('#hrm_people').val('');
    $('#commentary').val('');
    $('#hrm_total_hours_edit').text('00.00');
    $('#hrm_hours_edit').text('00.00');
    $('#add_btn').prop('disabled', true);
}

function HRMClearCoockies() {
    sessionStorage.clear();
}

/**
 * This function will check if both fields aren't empty
 * Return object with selected dates
 */
function CheckFields() {
    let current_date = new Date(sessionStorage.getItem("start_time")); //new Date();
    let date_from = $("#hrm_time_from").val();
    let date_to = $("#hrm_time_to").val();
    let status = false;

    current_date = current_date.setSeconds(0);
    date_from = new Date(date_from);
    date_from = date_from.setSeconds(0);
    date_to = new Date(date_to);
    date_to = date_to.setSeconds(0);

    // change property if time from is not null or empty
    if (!!date_from) {
        $("#hrm_time_to").prop('disabled', false);
    } else {
        $("#hrm_time_to").prop('disabled', true);
    }
    // check if fields aren't empty
    if (!!date_from && !!date_to) {
        // reutrn check result
        const result = {
            cDate: current_date,
            dFrom: date_from,
            dTo: date_to,
            status: true
        };
        return result;
    } else {
        const result = {
            cDate: current_date,
            dFrom: date_from,
            status: status
        };
        return result;
    }
}
/**
 * This function will calculate duartion time and total hours
 * based on selected dates and number of affected people
 */
function CalculateTimes(result) {
    let people = $('#hrm_people').val();
    let startTime = new Date(result.dFrom);
    let endTime = new Date(result.dTo);
    // with affected employees
    if (result.dFrom !== null && result.dTo !== null && people > 0) {
        let difference = endTime.getTime() - startTime.getTime(); // This will give difference in milliseconds
        let resultInMinutes = Math.round(difference / 60000.00);
        let resultInHoursPeople = ((resultInMinutes * people) / 60.00).toFixed(2);
        $('#hrm_total_hours_edit').text(resultInHoursPeople);
    } else {
        $('#hrm_total_hours_edit').text('00.00');
    }
    // without affected employees
    if (result.dFrom !== null && result.dTo !== null) {
        let difference = endTime.getTime() - startTime.getTime(); // This will give difference in milliseconds
        let resultInMinutes = Math.round(difference / 60000.00);
        let resultInHours = (resultInMinutes / 60.00).toFixed(2);
        $('#hrm_hours_edit').text(resultInHours);
    } else {
        $('#hrm_hours_edit').text('00.00');
    }
}
/**
 * 06:00 - 06:30 => 06:00 && 18:00 - 18:30 => 18:00
 */
function SetStartEndTime(results) {
    const currentDate = new Date(sessionStorage.getItem("start_time")); //new Date();
    const cDate = new Date(sessionStorage.getItem("start_time")); //new Date();
    let cTime = (currentDate.getHours() * 60) + currentDate.getMinutes();
    let startTime, endTime;
    let exDate = new Date(results.dFrom);

    if (cTime >= 360 && cTime < 1080) {
        startTime = currentDate.setHours(6, 0, 0);
        endTime = currentDate.setHours(17, 59, 0);
    } else {
        startTime = currentDate.setHours(18, 0, 0);
        exDate.setDate(exDate.getDate() + 1);
        endTime = exDate.setHours(5, 59, 59);
    }
    const times = {
        cDate: cDate,
        cTime: cTime,
        startTime: new Date(startTime),
        endTime: new Date(endTime),
    };
    return times;
}
/**
 * This function will check if selected dates not exceed
 * the date range limits and -30min for starting time
 */
function CheckDates(times, results) {
    const date_from = $("#hrm_time_from");
    const date_to = $("#hrm_time_to");
    const date_to_val = date_to.val();
    const shiftEndTime = new Date();

    // check when two dates are in place
    if (!!results.dFrom && !!results.dTo) {
        // get duration
        let duration = (moment(results.dTo).diff(moment(results.dFrom), 'minutes'));
        // do checks for duration and result to date
        if (results.dFrom == results.dTo || duration < 60) {
            end60min = moment(results.dFrom).add(60, 'minutes').toDate();
            let newDateTime = formatDate(end60min);
            date_to.val(newDateTime);
            // display error message
            let message = "Minimalny czas pomiędzy rozpoczęciem,\n"
            message = message + "a zakończeniem zdarzenia, nie może być\n"
            message = message + "krótszy niż 1 godzina!\n\n"
            message = message + "Data i czas zostały zmienione na: " + newDateTime;
            sweetAlert("Coś poszło nie tak...", message, "warning");
        } else if (new Date(results.dTo) > new Date(shiftEndTime)) {
            // check if duration is higher or equal to 60 minutes
            if (duration >= 60) {
                // allowed time to add record from current time
                time30min = moment(times.cDate).subtract(30, 'minutes').toDate();
                time30min.setSeconds(0);
                if (time30min < times.startTime) {
                    time30min = times.startTime;
                } else {
                    time30min = moment(times.cDate).subtract(30, 'minutes').toDate();
                    time30min.setSeconds(0);
                }
                // check if selected start date is in allowed time range
                let d_from = moment(results.dFrom).toDate().setSeconds(0);
                d_from = moment(d_from);
                let timeDiff = d_from.diff(moment(time30min), 'minutes');
                if (timeDiff < 0) {
                    let newStartTime = formatDate(time30min);

                    date_from.val(newStartTime);
                    // display error message
                    let message = "Nie możesz wybrać daty wcześniejszej niż\n"
                    message = message + "30 minut od obecnego czasu!\n\n"
                    message = message + "Data i czas zostały zmienione na najwcześniejszą możliwą opcję: " + newStartTime;
                    sweetAlert("Coś poszło nie tak...", message, "warning");
                }
                let newDateTime = formatDate(new Date(date_to_val));
                date_to.val(newDateTime);
            } else {
                sweetAlert("Coś poszło nie tak...", "Przyszedł skrzat i nabroił coś z serwerem!", "warning");
            }
        }
        // only one date field
    } else if (!!results.dFrom) {
        // allowed time to add record from current time
        time30min = moment(times.cDate).subtract(30, 'minutes').toDate();
        time30min.setSeconds(0);
        if (time30min < times.startTime) {
            time30min = times.startTime;
        } else {
            time30min = moment(times.cDate).subtract(30, 'minutes').toDate();
            time30min.setSeconds(0);
        }
        // check if selected start date is in allowed time range
        let d_from = moment(results.dFrom).toDate().setSeconds(0);
        d_from = moment(d_from);
        let timeDiff = d_from.diff(moment(time30min), 'minutes');
        if (timeDiff < 0) {
            let newStartTime = formatDate(time30min);

            date_from.val(newStartTime);
            // display error message
            let message = "Nie możesz wybrać daty wcześniejszej niż\n"
            message = message + "30 minut od obecnego czasu!\n\n"
            message = message + "Data i czas zostały zmienione na najwcześniejszą możliwą opcję: " + newStartTime;
            sweetAlert("Coś poszło nie tak...", message, "warning");
        }
    }
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
/**
 * Main HRM Check function - add new record
 */
function HRMEdit() {
    let results = CheckFields();
    let times = SetStartEndTime(results);
    CheckDates(times, results);
    if (results.status == true) {
        // calculate duration time and total hours time
        let results = CheckFields();
        CalculateTimes(results);
    }
}