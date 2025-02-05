$(document).ready(function () {
    const machine = $('#machine');
    machine.change(function (event) {
        event.preventDefault();
        let date_from = $('#date_from');
        if (machine.val() !== null) {
            date_from.prop('disabled', false);
        } else {
            date_from.prop('disabled', true);
        }
    });

    const dateFrom = $('#date_from');
    dateFrom.change(function (event) {
        event.preventDefault();
        let date_to = $('#date_to');
        if (dateFrom.val() !== null) {
            date_to.prop('disabled', false);
        } else {
            date_to.prop('disabled', true);
        }
    });
});

/**
 * This function will get parameters from url path 
 * and pass it into select fields to keep them visible
 */
function getParameters() {
    let operatingTime = $("#machine option:selected").text().match(/\((.*)\)/)[1].slice(0, -1);
    sessionStorage.setItem("operatingTime", operatingTime);
}
setParameters();
function setParameters() {
    let operatingTime = sessionStorage.getItem("operatingTime");
    if (operatingTime !== null && operatingTime.length > 0) {
        $('#machine').val(operatingTime);
    }
}

/**
 * Clear session storage coockies
 */
function DowntimesClearCookies() {
    sessionStorage.clear();
}

/**
 * This function will check dates and calculate necesarry fields to show
 * how many hours have been used between the dates accordingly to the number 
 * of users typed in. Prevent from selecting old weeks dates and future dates
 * according to setted rules
 */
function CheckDates() {
    let currentDate = new Date();
    let startDate = $('#date_from');
    let startDateD = new Date(startDate.val());
    let endDate = $('#date_to');
    let endDateD = new Date(endDate.val());
    let operatingTime = sessionStorage.getItem("operatingTime");
    let message = "";
    let employeeNumber = parseInt($('#people_affected').val());

    // Get all available peirods and date time limits
    let limits = getWeekPeriods(currentDate);

    // Check if current date is after tuesday 10:00
    let isTuesday = checkIfTuesady(currentDate, limits);

    // Create a new end date that will be applied to field when wrong end date is selected
    let newEndDate = createNewEndDate(startDateD, operatingTime, limits);

    if (startDate.val() != "") {
        switch (isTuesday) {
            // current date time is Tuesday >= 10:00:00 => allow to select only current week dates
            case true:
                // check if fields are not empty
                // start date field only
                if (startDate.val() != "") {
                    if (startDateD >= limits.currWkStart && startDateD <= limits.currWkEnd) {
                        // if both fields are not empty
                        if (startDate.val() != "" && endDate.val() != "") {
                            if (endDateD < $('#date_from').val()) {
                                endDate.val('');
                                message = "Wybierz daty z podanego zakresu:\n" + formatDate(startDateD) + " - " + formatDate(limits.currWkEnd);
                                sweetAlert("Data zakończenia jest nieodpowiednia", message, "warning");
                            } else if ($('#date_to').val() == $('#date_from').val()) {
                                endDate.val('');
                                message = "Wybierz daty z podanego zakresu:\n" + formatDate(new Date($('#date_from').val())) + " - " + formatDate(limits.currWkEnd);
                                sweetAlert("Wybrane daty są identyczne", message, "warning");
                            } else {
                                // check if end date is in periods range
                                let startD = new Date($('#date_from').val());

                                if (endDateD >= startD && endDateD <= newEndDate) {
                                } else {
                                    endDate.val(formatDate(newEndDate));
                                    message = "Wybierz daty z podanego zakresu:\n" + formatDate(new Date($('#date_from').val())) + " - " + formatDate(newEndDate);
                                    sweetAlert("Data zakończenia jest poza czasem pracy", message, "warning");
                                }
                            }
                        } else {

                        }
                    } else {
                        // dates outside of the current possible range of dates, clear field and show error message
                        startDate.val('');
                        endDate.val('');
                        message = "Wybierz datę z podanego zakresu:\n" + formatDate(limits.currWkStart) + " - " + formatDate(limits.currWkEnd);
                        sweetAlert("Wybrana data jest poza niedozwolona...", message, "warning");
                    }
                }
                if (startDate.val() != "" && endDate.val() != "" && employeeNumber > 0) {
                    CalculateUsedHours();
                }
                break;
            // current date time is Tuesday <= 10:00:00 => allow to select previous week & curremt week dates
            case false:
                // check if fields are not empty
                // start date field only
                if (startDate.val() != "") {
                    if (startDateD >= limits.prevWkStart && startDateD <= limits.currWkEnd) {
                        // if both fields are not empty
                        if (startDate.val() != "" && endDate.val() != "") {
                            if (endDateD < $('#date_from').val()) {
                                endDate.val('');
                                message = "Wybierz daty z podanego zakresu:\n" + formatDate(startDateD) + " - " + formatDate(limits.currWkEnd);
                                sweetAlert("Data zakończenia jest nieodpowiednia", message, "warning");
                            } else if ($('#date_to').val() == $('#date_from').val()) {
                                endDate.val('');
                                message = "Wybierz daty z podanego zakresu:\n" + formatDate(new Date($('#date_from').val())) + " - " + formatDate(limits.currWkEnd);
                                sweetAlert("Wybrane daty są identyczne", message, "warning");
                            } else {
                                // check if end date is in periods range
                                let startD = new Date($('#date_from').val());

                                if (endDateD >= startD && endDateD <= newEndDate) {
                                } else {
                                    endDate.val(formatDate(newEndDate));
                                    message = "Wybierz daty z podanego zakresu:\n" + formatDate(new Date($('#date_from').val())) + " - " + formatDate(newEndDate);
                                    sweetAlert("Data zakończenia jest poza czasem pracy", message, "warning");
                                }
                            }
                        } else {

                        }
                    } else {
                        // dates outside of the current possible range of dates, clear field and show error message
                        startDate.val('');
                        endDate.val('');
                        message = "Wybierz datę z podanego zakresu:\n" + formatDate(limits.prevWkStart) + " - " + formatDate(limits.currWkEnd);
                        sweetAlert("Wybrana data jest poza niedozwolona...", message, "warning");
                    }
                }
                if (startDate.val() != "" && endDate.val() != "" && employeeNumber > 0 ) {
                    CalculateUsedHours();
                }
                break;
            default:
                sweetAlert("Uwaga!", "Sprawdź czy wszystkie wymagane pola są uzupełnione", "warning");
        }
    }
}

/**
 * This function will check if current date time is tuesady after 10:00 o'clock
 * @param {any} currentDate
 * @param {any} limits
 */
function checkIfTuesady(currentDate, limits) {
    if (currentDate >= limits.tuesdaLimit) {
        return true;
    } else {
        return false;
    }
}

/**
 * This function will calculate number of hours 
 */
function CalculateUsedHours() {
    let employeeNumber = parseInt($('#people_affected').val());
    let startDateD = moment($('#date_from').val());
    let endDateD = moment($('#date_to').val());
    let duration = endDateD.diff(startDateD, 'minutes');

    if (employeeNumber > 0) {
        duration = (employeeNumber * duration) / 60.00;
    } else {
        duration = 0.00;
    }

    $('#calculated_hours').html(duration.toFixed(2));
}

/**
 * This function will set new EndDate specified by Area/Machine/Line operating time
 * @param {any} startDate
 * @param {any} operatingTime
 * @param {any} limits
 */
function createNewEndDate(startDate, operatingTime, limits) {
    let newEndDate = startDate;
    let startDateX = startDate.getHours();

    switch (parseInt(operatingTime)) {
        case 8:
            if (startDateX >= 6 && startDateX < 14) {
                newEndDate = newEndDate.setHours(13, 59, 59, 0);
            } else if (startDateX >= 14 && startDateX < 18) {
                newEndDate = newEndDate.setHours(21, 59, 59, 0);
            } else {
                newEndDate = moment(startDate).add('days', 1).set({ h: 5, m: 59, s: 59 });
            }
            break;
        case 12:
            if (startDateX >= 6 && startDateX < 18) {
                newEndDate = newEndDate.setHours(17, 59, 59, 0);
            } else {
                newEndDate = moment(startDate).add('days', 1).set({h:5,m:59,s:59});
            }
            break;
        default:
            newEndDate = limits.currWkEnd;
    }   

    return new Date(newEndDate);
}

/**
 *  This function will get limited period dates
 * @param {any} date
 */
function getWeekPeriods(date) {
    let currentWeekStartDate = new Date(date);
    currentWeekStartDate.setDate(currentWeekStartDate.getDate() - (currentWeekStartDate.getDay() + 1));
    currentWeekStartDate.setHours(6, 0, 0, 0);

    let currentWeekEndDate = new Date(currentWeekStartDate);
    currentWeekEndDate.setDate(currentWeekEndDate.getDate() + 7);
    currentWeekEndDate.setHours(5, 59, 0, 0);

    let previousWeekStartDate = new Date(currentWeekStartDate);
    previousWeekStartDate.setDate(previousWeekStartDate.getDate() - 7);

    let previousWeekEndDate = new Date(currentWeekEndDate);
    previousWeekEndDate.setDate(previousWeekEndDate.getDate() - 7);

    let limitDate = new Date(currentWeekStartDate);
    limitDate.setDate(limitDate.getDate() + 3);
    limitDate.setHours(10, 0, 0, 0);  

    const result = {
        currWkStart: currentWeekStartDate,
        currWkEnd: currentWeekEndDate,
        prevWkStart: previousWeekStartDate,
        prevWkEnd: previousWeekEndDate,
        tuesdaLimit: limitDate
    };

    return result;
}

/**
 * Extact correct datetime string format from date
 * @param {any} num
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