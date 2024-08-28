function printPage() {
    window.print();
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

function triggerFileDownload(fileName, url)
{
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
}

