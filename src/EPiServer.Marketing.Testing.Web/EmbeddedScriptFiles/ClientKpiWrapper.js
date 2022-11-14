﻿

var ActiveKpis = new Array();
var testId, itemVersion, kpiId, resultValue;

var ClientKpiConverted = document.createEvent("Event");
ClientKpiConverted.initEvent("ConvertClientKpi", true, false);
    window.addEventListener('ConvertClientKpi', function (e) {
    setTestData(e.id);
    convert();
});

function addKpiData(kId, tId, itemVersion, value) {
    var data = { kpiId: kId, testId: tId, itemVersion: itemVersion, resultValue: value }
    ActiveKpis.push(data);
}

function setTestData(id) {
    var result;
        
    result = search(id, ActiveKpis);

    testId = result.testId;
    itemVersion = result.itemVersion;
    kpiId = result.kpiId;
    resultValue = result.resultValue;
}

function search(id, arrayObj) {
    for (var i = 0; i < arrayObj.length; i++) {
        if (arrayObj[i].kpiId === id) {
            return arrayObj[i];
        }
    }
}

function convert() {
    {
        var xhttp = new XMLHttpRequest();
        var xhttpData;
        xhttp.open("POST", window.location.protocol + "//" + window.location.host + "/api/episerver/testing/SaveKpiResult", true);
        xhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
        xhttp.onreadystatechange = function () {
            {
                if (xhttp.status == 200) {
                    {
                        console.info(xhttp.status + ":" + xhttp.responseText);
                    }
                } else {
                    {
                        console.error(xhttp.status + ":" + xhttp.responseText);
                    }
                }
            }
        }
        xhttpData = "testId=" + testId + "&itemVersion=" + itemVersion + "&kpiId=" + kpiId;
        if (resultValue) {
            xhttpData += "&resultValue=" + resultValue
        }
        xhttp.send(xhttpData);
    }
};
