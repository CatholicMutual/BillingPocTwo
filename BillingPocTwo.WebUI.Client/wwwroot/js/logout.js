window.addEventListener('beforeunload', function (e) {
    DotNet.invokeMethodAsync('BillingPocTwo.WebUI.Client', 'LogoutOnClose');
});

window.onbeforeunload = async () => {
    const token = sessionStorage.getItem("AccessToken");
    if (token) {
        await fetch("/api/logout", {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });
        sessionStorage.removeItem("AccessToken");
    }
};