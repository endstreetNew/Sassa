function printPage() {
    window.print();
}
function SetIdFocus(elementId) {
    var element = document.getElementById(elementId);
    element.focus();
    /*    return element === true;*/
}
function getHTML() {
  return document.getElementById('pdf').innerHTML;
}

function jsSaveAsFile(filename, byteBase64)
{
    var link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + byteBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function triggerFileDownload(fileName, url) {
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

//function SetElementFocus(element) {
//    element.focus();
//}

//function SetIdFocus(elementId) {
//    var element = document.getElementById(elementId);
//    element.focus();
//}
//const prodTheme = createProdTheme({
//    palette: {
//        themePrimary: '#0078d4',
//        themeLighterAlt: '#eff6fc',
//        themeLighter: '#deecf9',
//        themeLight: '#c7e0f4',
//        themeTertiary: '#71afe5',
//        themeSecondary: '#2b88d8',
//        themeDarkAlt: '#106ebe',
//        themeDark: '#005a9e',
//        themeDarker: '#004578',
//        neutralLighterAlt: '#faf9f8',
//        neutralLighter: '#f3f2f1',
//        neutralLight: '#edebe9',
//        neutralQuaternaryAlt: '#e1dfdd',
//        neutralQuaternary: '#d0d0d0',
//        neutralTertiaryAlt: '#c8c6c4',
//        neutralTertiary: '#a19f9d',
//        neutralSecondary: '#605e5c',
//        neutralPrimaryAlt: '#3b3a39',
//        neutralPrimary: '#323130',
//        neutralDark: '#201f1e',
//        black: '#000000',
//        white: '#ffffff',
//    }
//});

//const qaTheme = createQaTheme({
//    palette: {
//        themePrimary: '#0d9e2a',
//        themeLighterAlt: '#f2fbf4',
//        themeLighter: '#ccefd3',
//        themeLight: '#a4e2b0',
//        themeTertiary: '#58c56e',
//        themeSecondary: '#20aa3c',
//        themeDarkAlt: '#0b8e26',
//        themeDark: '#0a7820',
//        themeDarker: '#075917',
//        neutralLighterAlt: '#faf9f8',
//        neutralLighter: '#f3f2f1',
//        neutralLight: '#edebe9',
//        neutralQuaternaryAlt: '#e1dfdd',
//        neutralQuaternary: '#d0d0d0',
//        neutralTertiaryAlt: '#c8c6c4',
//        neutralTertiary: '#a19f9d',
//        neutralSecondary: '#605e5c',
//        neutralPrimaryAlt: '#3b3a39',
//        neutralPrimary: '#323130',
//        neutralDark: '#201f1e',
//        black: '#000000',
//        white: '#ffffff',
//    }
//});
