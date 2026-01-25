function DrawPoints(arr, color) {
    var stats = GetStandardDeviation(arr);
    var clr = "black";
    if (color != undefined) { clr = color; }
    var canv = document.getElementById("canvGraph");
    var ctx = canv.getContext('2d');
    ctx.save();
    ctx.strokeStyle = clr;
    ctx.fillStyle = clr;
    var ratioX, ratioY, sampleX, maxValue, minValue;
    for (var len = arr.length, n = 0; n < len; n++) {
        // Verify that the value is within the standard deviation
        if (arr[n] < (stats.Average + stats.StandardDeviation) && arr[n] - (stats.Average - stats.StandardDeviation)) {
            if (maxValue < arr[n] || maxValue == undefined) {
                maxValue = arr[n];
            }
            if (minValue > arr[n] || minValue == undefined) {
                minValue = arr[n];
            }
        }
    }
    ratioX = canv.width / arr.length;
    ratioY = canv.height / maxValue;
    sampleX = (1 + (1 - ratioX));
    if (ratioX > sampleX) { sampleX = ratioX; }

    ctx.translate(0, canv.height / 2);
    ctx.scale(1, -1);
    ctx.moveTo(0, 0);
    ctx.beginPath();
    var x = 0;
    for (var len = arr.length, n = 0; n < len; n += sampleX) {
        var y = (arr[parseInt(n)] * ratioY)//(canv.height - (arr[parseInt(n)] * ratioY));
        ctx.lineTo(x, y);
        x += 1 * sampleX;
    }
    ctx.lineTo(canv.width, 0);
    ctx.lineTo(0, 0);
    ctx.fill();
    ctx.stroke();
    ctx.closePath();

    ctx.beginPath();
    ctx.strokeStyle = "orange";
    ctx.moveTo(0, ((stats.Average + stats.StandardDeviation) * ratioY));
    ctx.lineTo(canv.width, ((stats.Average + stats.StandardDeviation) * ratioY));

    ctx.moveTo(0, ((stats.Average - stats.StandardDeviation) * ratioY));
    ctx.lineTo(canv.width, ((stats.Average - stats.StandardDeviation) * ratioY));
    ctx.stroke();
    ctx.closePath();

    ctx.restore();

    return { MaximumStandard: maxValue, MinimumStandard: minValue, XRatio: ratioX, YRatio: ratioY, SampleRate: sampleX }
}
function GetStandardDeviation(arr) {
    var ave = 0;
    for (var len = arr.length, n = 0; n < arr.length; n++) {
        ave += arr[n];
    }
    ave = ave / arr.length;

    var vari = 0;
    for (var len = arr.length, n = 0; n < arr.length; n++) {
        vari += Math.pow((ave - arr[n]), 2);
    }
    vari = vari / arr.length;

    var stnd = Math.sqrt(vari);

    return { Average: ave, StandardDeviation: stnd }
}