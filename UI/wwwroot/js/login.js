import Fetcher from "./Fetcher.js";

export default class LoginController {
    constructor() {
        var _t = this;
        this._fetcher = new Fetcher();

        $("#btnLogin").click(async function () {
            await _t.login();
        });

        $("#txtPassword").keydown(async function (e) {
            if (e.keyCode === 13) {
                await _t.login();
            }
        });

        $(document).ready(function () {
            $("#txtUsername").focus();
        });
    }

    async login() {
        var _t = this;
        var username = $("#txtUsername").val();
        if (!username) {
            showToast("Enter the user name", null, toastr.warning);
            return;
        }

        var pass = $("#txtPassword").val();
        if (!pass) {
            showToast("Enter the password", null, toastr.warning);
            return;
        }

        const response = await _t._fetcher.fetchJson(`/account/login`, {
            method: 'POST',
            body: JSON.stringify({
                username: username,
                password: pass
            })
        });
        
        if (!response.Result) {
            showToast(response);
            return;
        }

        if (response.Contents && response.Contents.returnUrl)
            window.location.href = response.Contents.returnUrl;
        else
            window.location.href = "/";
    }
}
