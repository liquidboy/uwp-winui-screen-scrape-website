// injected js from uwp host into webview2
if (!!window['injectedFunction'] === false) {
    function injectedFunction() {
        doWork();
    };
    //alert('js injected from UWP & function created in webview2');
} else {
    //alert('js has already been injected into webview2');
    doWork();
};

function doWork() {
    let name = document.querySelector('h2').firstChild.nodeValue;
    let code = document.querySelector('small').innerText;
    let logo = document.querySelector('img').attributes['src'].nodeValue;

    //https://developer.mozilla.org/en-US/docs/Web/CSS/Attribute_selectors
    let price = parseFloat(document.querySelectorAll('div[class*="priceValue"]')[0].innerText.substring(1).replace(/,/g, '')); 
    let circulatingSupply = parseFloat(document.querySelectorAll('div[class*="statsValue"]')[4].innerText.replace(/,/g, ''));
    let maxSupply = parseFloat(document.querySelectorAll('div[class*="maxSupplyValue"]')[0].innerText.replace(/,/g, ''));
    let totalSupply = parseFloat(document.querySelectorAll('div[class*="maxSupplyValue"]')[1].innerText.replace(/,/g, ''));
        
    let result = {
        "name": name, "code": code, "logo": logo,
        "price": price,
        "circulatingSupply": circulatingSupply, "maxSupply": maxSupply, "totalSupply": totalSupply
    }; 
    window.chrome.webview.postMessage( JSON.stringify(result));
}

// execute injected function
if (!!window['injectedFunction'] === true) {
    window['injectedFunction']();
};