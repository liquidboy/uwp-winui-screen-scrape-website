// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        doWork();
    };
    alert('js injected from UWP & function created in webview2');
} else {
    alert('js has already been injected into webview2');
    doWork();
};

function doWork() {
    var foundFromTos = document.querySelectorAll('input[aria-label="Currency Amount Field"]');
    var foundFromTosCurrencyTypes = document.querySelectorAll('select[aria-label="Currency Type"]');
    

    //alert(`${foundFromTos.length} text inputs found`);

    var input = foundFromTos[0];
    input.focus();
    input.setAttribute("value", "5.99");

    var inputCurrencyType = foundFromTosCurrencyTypes[0];
    inputCurrencyType.focus();
    inputCurrencyType.value = -1;
    inputCurrencyType.value = "/m/0kz1h";
    inputCurrencyType.dispatchEvent(new MouseEvent("mousedown", { bubbles: true, cancelable: true, view: window }));
    //inputCurrencyType.dispatchEvent(new MouseEvent("mouseup", { bubbles: true, cancelable: true, view: window }))



}


// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};