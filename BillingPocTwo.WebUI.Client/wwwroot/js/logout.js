window.addEventListener('beforeunload', function (e) {
    DotNet.invokeMethodAsync('BillingPocTwo.WebUI.Client', 'LogoutOnClose');
});