function showToast(data, title, toastType = toastr.error, options = null) {
    var m = "";
    var t;
    if (title)
        t = title;

    if (!t) {
        if (toastType !== toastr.error) {
            if (toastType === toastr.success || toastType === toastr.info)
                t = "info";
            else
                t = "warning";
        } else
            t = "error";
    }

    if (!data) {
        m = "Called showToast without a message";
    }
    else {
        if (typeof data === "string") {
            m = data;
        }
        else {
            if (data.IsResponsePageResult) {
                PopupAjaxError(data.Message); // Message contains the HTML of the respose page
                return;
            }
            if (!data.Message)
                m = "Called ShowToast with a response object that has no message";
            else
                m = data.Message;
        }
    }
    if (!options)
        options = {};
    if (!options.positionClass)
        options["positionClass"] = 'toast-bottom-right';

    if (toastType === toastr.error) {
        options["preventDuplicates"] = true;
        options["closeButton"] = true;
        options["timeOut"] = 10000;
        options["extendedTimeOut"] = 0;
    }

    if (m && m.length > 300)
        m = m.substring(0, 300) + "...";

    while (m.indexOf("\r\n") >= 0)
        m = m.replace("\r\n", "<br/>");

    toastType(m, t, options);
}

function PopupAjaxError(html) {
    var w = window.open('');
    if (w)
        w.document.write(html);
    else
        alert("Disable popup blockers");
};
